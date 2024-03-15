using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Sanguosha.Core.Network;
using Sanguosha.Core.Utils;
using Sanguosha.Lobby.Core;

namespace Sanguosha.Lobby.Server;

[Authorize]
public class LobbyHub(
    AccountContext accountContext,
    LobbyManager lobbyManager,
    IPasswordHasher<Account> passwordHasher,
    ILogger<LobbyHub> logger
    ) : Hub<IGameClient>, ILobbyService
{
    private readonly AccountContext accountContext = accountContext;
    private readonly LobbyManager lobbyManager = lobbyManager;
    private readonly IPasswordHasher<Account> passwordHasher = passwordHasher;
    private readonly ILogger<LobbyHub> logger = logger;

    public static bool CheatEnabled { get; set; } = false;
    public static IPAddress HostingIp { get; set; } = IPAddress.Any;
    public static IPAddress PublicIp { get; set; } = IPAddress.Any;

    [AllowAnonymous]
    public async Task<LoginResult> Login(string username, string password)
    {
        LobbyPlayer currentAccount;
        string? reconnectionString = null;
        var reconnectionToken = new LoginToken();
        var authenticatedAccount = await accountContext.Accounts
            .Where(account => account.UserName.Contains(username))
            .FirstOrDefaultAsync();
        if (authenticatedAccount is null || passwordHasher.VerifyHashedPassword(authenticatedAccount, authenticatedAccount.Password, password) != PasswordVerificationResult.Success)
            return LoginResult.InvalidUsernameAndPassword;
        if (lobbyManager.loggedInAccounts.TryGetValue(username, out var disconnected))
        {
            try
            {
                if (await Clients.Client(disconnected.ConnectedId).Ping())
                    return LoginResult.InvalidUsernameAndPassword;
            }
            catch (Exception)
            {

            }
            currentAccount = disconnected;
            var room = disconnected.CurrentRoom;
            if (room is not null)
            {
                if (room.Room.State == RoomState.Gaming
                    && !disconnected.Account.IsDead)
                {
                    reconnectionString = $"{room.Room.IpAddress}:{room.Room.IpPort}";
                    reconnectionToken = disconnected.Account.LoginToken;
                }
                else
                {
                    disconnected.CurrentRoom = null;
                }
            }
        }
        else
        {
            var acc = new LobbyPlayer(authenticatedAccount, Context.ConnectionId);
            lobbyManager.loggedInAccounts.TryAdd(username, acc);
            currentAccount = acc;
            var roomresult = lobbyManager.rooms.Values.Where(r => r.Room.Seats.Any(st => st.Account == authenticatedAccount));
            if (roomresult.Any())
            {
                acc.CurrentRoom = roomresult.First();
            }
        }
        logger.LogInformation("{username} logged in", username);

        return LoginResult.Success(authenticatedAccount, reconnectionString, reconnectionToken);
    }

    public async Task Logout()
    {
        var currentAccount = lobbyManager.loggedInAccounts[Context.ConnectionId];
        logger.LogInformation("{UserName} logged out", currentAccount.Account.UserName);
        await _Logout(currentAccount);
    }

    public IEnumerable<Room> GetRooms(bool notReadyRoomsOnly)
    {
        var currentAccount = lobbyManager.loggedInAccounts[Context.ConnectionId];
        return from r in lobbyManager.rooms.Values
               where !notReadyRoomsOnly || r.Room.State == RoomState.Waiting
               select r.Room;
    }

    public async Task<Room?> CreateRoom(RoomSettings settings, string? password = null)
    {
        var currentAccount = lobbyManager.loggedInAccounts[Context.ConnectionId];
        if (currentAccount.CurrentRoom is not null)
            return null;

        var newRoomId = Guid.NewGuid().ToString();
        var room = new Room();
        int maxSeats = settings.GameType == GameType.Pk1V1 ? 2 : 8;
        for (int i = 0; i < maxSeats; i++)
        {
            room.Seats.Add(new Seat() { State = SeatState.Empty });
        }
        room.Seats[0].Account = currentAccount.Account;
        room.Seats[0].State = SeatState.Host;
        room.Id = newRoomId;
        room.Settings = settings;
        var srvRoom = lobbyManager.rooms.GetOrAdd(newRoomId, new ServerRoom(room));
        currentAccount.CurrentRoom = srvRoom;
        currentAccount.LastAction = DateTime.Now;
        await Groups.AddToGroupAsync(Context.ConnectionId, newRoomId);
        logger.LogInformation("created room {NewRoomId}", newRoomId);
        return room;
    }

    public async Task<EnterRoomResult> EnterRoom(string roomId, bool spectate, string password)
    {
        var currentAccount = lobbyManager.loggedInAccounts[Context.ConnectionId];
        logger.LogInformation("{UserName} Enter room {RoomId}", roomId, currentAccount.Account.UserName);
        if (currentAccount.CurrentRoom is not null)
            return EnterRoomResult.Locked;

        if (!lobbyManager.rooms.TryGetValue(roomId, out var serverRoom))
            return EnterRoomResult.Invalid;
        var clientRoom = serverRoom.Room;
        if (clientRoom.IsEmpty || clientRoom.State == RoomState.Gaming)
            return EnterRoomResult.Locked;
        int seatNo = 0;
        foreach (var seat in clientRoom.Seats)
        {
            logger.LogInformation("Testing seat {SeatNo}", seatNo);
            if (seat.Account is null && seat.State == SeatState.Empty)
            {
                currentAccount.CurrentRoom = serverRoom;
                currentAccount.LastAction = DateTime.Now;
                seat.Account = currentAccount.Account;
                seat.State = SeatState.GuestTaken;
                await _NotifyRoomLayoutChanged(clientRoom);
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                logger.LogInformation("Seat {SeatNo}", seatNo);
                _Unspectate(currentAccount);
                return EnterRoomResult.Success(clientRoom);
            }
            seatNo++;
        }
        logger.LogInformation("Full");
        return EnterRoomResult.Full;
    }

    public Task<RoomOperationResult> ExitRoom()
    {
        var currentAccount = lobbyManager.loggedInAccounts[Context.ConnectionId];
        return _ExitRoom(currentAccount);
    }

    public async Task<RoomOperationResult> ChangeSeat(int newSeat)
    {
        var currentAccount = lobbyManager.loggedInAccounts[Context.ConnectionId];
        if (currentAccount.CurrentRoom is null)
            return RoomOperationResult.Invalid;
        var room = currentAccount.CurrentRoom.Room;

        if (room.State == RoomState.Gaming)
            return RoomOperationResult.Locked;
        if (newSeat < 0 || newSeat >= room.Seats.Count)
            return RoomOperationResult.Invalid;
        var seat = room.Seats[newSeat];
        if (seat.Account is null && seat.State == SeatState.Empty)
        {
            foreach (var remove in room.Seats)
            {
                if (remove.Account == currentAccount.Account)
                {
                    currentAccount.LastAction = DateTime.Now;
                    if (remove == seat)
                        return RoomOperationResult.Invalid;
                    seat.State = remove.State;
                    seat.Account = remove.Account;
                    remove.Account = null;
                    remove.State = SeatState.Empty;
                    await _NotifyRoomLayoutChanged(room);
                    await Groups.AddToGroupAsync(Context.ConnectionId, room.Id);
                    return RoomOperationResult.Success;
                }
            }
        }
        logger.LogInformation("Full");
        return RoomOperationResult.Full;
    }

    public async Task<RoomOperationResult> StartGame()
    {
        var currentAccount = lobbyManager.loggedInAccounts[Context.ConnectionId];
        if (currentAccount.CurrentRoom is null)
            return RoomOperationResult.Invalid;
        var room = currentAccount.CurrentRoom;
        var total = room.Room.Seats.Count(pl => pl.Account is not null);
        var initiator = room.Room.Seats.FirstOrDefault(pl => pl.Account == currentAccount.Account);
        if (room.Room.State == RoomState.Gaming)
            return RoomOperationResult.Invalid;
        if (total <= 1)
            return RoomOperationResult.Invalid;
        if (initiator is null || initiator.State != SeatState.Host)
            return RoomOperationResult.Invalid;
        if (room.Room.Seats.Any(cs => cs.Account is not null && cs.State != SeatState.Host && cs.State != SeatState.GuestReady))
            return RoomOperationResult.Invalid;
        room.Room.State = RoomState.Gaming;
        foreach (var unready in room.Room.Seats)
        {
            if (unready.State == SeatState.GuestReady)
                unready.State = SeatState.Gaming;
        }
        var gs = new GameSettings()
        {
            TimeOutSeconds = room.Room.Settings.TimeOutSeconds,
            TotalPlayers = total,
            CheatEnabled = CheatEnabled,
            DualHeroMode = room.Room.Settings.IsDualHeroMode,
            NumHeroPicks = room.Room.Settings.NumHeroPicks,
            NumberOfDefectors = room.Room.Settings.NumberOfDefectors == 2 ? 2 : 1,
            GameType = room.Room.Settings.GameType,
        };

        // Load pakcages.
        if (gs.GameType == GameType.RoleGame)
        {
            gs.PackagesEnabled.Add("Sanguosha.Expansions.BasicExpansion");
            gs.PackagesEnabled.Add("Sanguosha.Expansions.BattleExpansion");
            if ((room.Room.Settings.EnabledPackages & EnabledPackages.Wind) != 0)
                gs.PackagesEnabled.Add("Sanguosha.Expansions.WindExpansion");
            if ((room.Room.Settings.EnabledPackages & EnabledPackages.Fire) != 0)
                gs.PackagesEnabled.Add("Sanguosha.Expansions.FireExpansion");
            if ((room.Room.Settings.EnabledPackages & EnabledPackages.Woods) != 0)
                gs.PackagesEnabled.Add("Sanguosha.Expansions.WoodsExpansion");
            if ((room.Room.Settings.EnabledPackages & EnabledPackages.Hills) != 0)
                gs.PackagesEnabled.Add("Sanguosha.Expansions.HillsExpansion");
            if ((room.Room.Settings.EnabledPackages & EnabledPackages.Sp) != 0)
            {
                gs.PackagesEnabled.Add("Sanguosha.Expansions.SpExpansion");
                gs.PackagesEnabled.Add("Sanguosha.Expansions.StarSpExpansion");
            }
            if ((room.Room.Settings.EnabledPackages & EnabledPackages.OverKnightFame) != 0)
            {
                gs.PackagesEnabled.Add("Sanguosha.Expansions.OverKnightFame11Expansion");
                gs.PackagesEnabled.Add("Sanguosha.Expansions.OverKnightFame12Expansion");
                gs.PackagesEnabled.Add("Sanguosha.Expansions.OverKnightFame13Expansion");
            }
            if ((room.Room.Settings.EnabledPackages & EnabledPackages.Others) != 0)
            {
                gs.PackagesEnabled.Add("Sanguosha.Expansions.AssasinExpansion");
            }
        }
        if (gs.GameType == GameType.Pk1V1)
        {
            gs.PackagesEnabled.Add("Sanguosha.Expansions.Pk1v1Expansion");
        }

        foreach (var addconfig in room.Room.Seats)
        {
            var account = addconfig.Account;
            if (account != null)
            {
                account.LoginToken = new LoginToken() { TokenString = Guid.NewGuid() };
                account.IsDead = false;
                gs.Accounts.Add(account);
            }
        }
        GameService.StartGameService(HostingIp, gs, room.Room.Id, _OnGameEnds, out int portNumber);
        room.Room.IpAddress = PublicIp.ToString();
        room.Room.IpPort = portNumber;
        await _NotifyGameStart(room.Room.Id, PublicIp, portNumber);
        currentAccount.LastAction = DateTime.Now;
        return RoomOperationResult.Success;
    }

    public async Task<RoomOperationResult> Ready()
    {
        var currentAccount = lobbyManager.loggedInAccounts[Context.ConnectionId];
        if (currentAccount.CurrentRoom is null)
            return RoomOperationResult.Invalid;
        var room = currentAccount.CurrentRoom.Room;
        {
            var seat = room.Seats.FirstOrDefault(s => s.Account == currentAccount.Account);
            if (seat is null)
                return RoomOperationResult.Invalid;
            if (seat.State != SeatState.GuestTaken)
                return RoomOperationResult.Invalid;
            seat.State = SeatState.GuestReady;
            await _NotifyRoomLayoutChanged(room);
            currentAccount.LastAction = DateTime.Now;
            return RoomOperationResult.Success;
        }
    }

    public async Task<RoomOperationResult> CancelReady()
    {
        var currentAccount = lobbyManager.loggedInAccounts[Context.ConnectionId];
        if (currentAccount.CurrentRoom is null)
            return RoomOperationResult.Invalid;
        var room = currentAccount.CurrentRoom.Room;
        var seat = room.Seats.FirstOrDefault(s => s.Account == currentAccount.Account);
        if (seat is null)
            return RoomOperationResult.Invalid;
        if (seat.State != SeatState.GuestReady)
            return RoomOperationResult.Invalid;
        seat.State = SeatState.GuestTaken;
        await _NotifyRoomLayoutChanged(room);
        currentAccount.LastAction = DateTime.Now;
        return RoomOperationResult.Success;
    }

    public async Task<RoomOperationResult> Kick(int seatNo)
    {
        var currentAccount = lobbyManager.loggedInAccounts[Context.ConnectionId];
        if (currentAccount.CurrentRoom is null)
            return RoomOperationResult.Invalid;
        var room = currentAccount.CurrentRoom.Room;
        var seat = room.Seats.FirstOrDefault(s => s.Account == currentAccount.Account);
        if (seat == null) return RoomOperationResult.Invalid;
        if (seat.State != SeatState.Host) return RoomOperationResult.Invalid;
        if (seatNo < 0 || seatNo >= room.Seats.Count) return RoomOperationResult.Invalid;
        if (room.Seats[seatNo].State == SeatState.GuestReady || room.Seats[seatNo].State == SeatState.GuestTaken)
        {
            var kicked = room.Seats[seatNo].Account;

            if (kicked is null || !lobbyManager.loggedInAccounts.TryGetValue(kicked.UserName, out LobbyPlayer? value) ||
                await _ExitRoom(value, true) == RoomOperationResult.Invalid)
            {
                // zombie occured?
                room.Seats[seatNo].State = SeatState.Empty;
                room.Seats[seatNo].Account = null;
            }
            else
            {
                try
                {
                    await Clients.User(value.ConnectedId).NotifyKicked();
                }
                catch (Exception)
                {
                }
            }
            return RoomOperationResult.Success;
        }
        else
        {
            room.Seats[seatNo].State = SeatState.Empty;
            room.Seats[seatNo].Account = null;
        }
        return RoomOperationResult.Invalid;
    }

    public async Task<RoomOperationResult> OpenSeat(int seatNo)
    {
        var currentAccount = lobbyManager.loggedInAccounts[Context.ConnectionId];
        if (currentAccount.CurrentRoom is null)
            return RoomOperationResult.Invalid;
        var room = currentAccount.CurrentRoom.Room;
        var seat = room.Seats.FirstOrDefault(s => s.Account == currentAccount.Account);
        if (seat == null)
            return RoomOperationResult.Invalid;
        if (seat.State != SeatState.Host)
            return RoomOperationResult.Invalid;
        if (seatNo < 0 || seatNo >= room.Seats.Count)
            return RoomOperationResult.Invalid;
        if (room.Seats[seatNo].State != SeatState.Closed)
            return RoomOperationResult.Invalid;
        room.Seats[seatNo].State = SeatState.Empty;
        await _NotifyRoomLayoutChanged(room);
        currentAccount.LastAction = DateTime.Now;
        return RoomOperationResult.Success;
    }

    public async Task<RoomOperationResult> CloseSeat(int seatNo)
    {
        var currentAccount = lobbyManager.loggedInAccounts[Context.ConnectionId];
        if (currentAccount.CurrentRoom is null)
            return RoomOperationResult.Invalid;
        var room = currentAccount.CurrentRoom.Room;
        var seat = room.Seats.FirstOrDefault(s => s.Account == currentAccount.Account);
        if (seat == null)
            return RoomOperationResult.Invalid;
        if (seat.State != SeatState.Host)
            return RoomOperationResult.Invalid;
        if (seatNo < 0 || seatNo >= room.Seats.Count)
            return RoomOperationResult.Invalid;
        if (room.Seats[seatNo].State != SeatState.Empty)
            return RoomOperationResult.Invalid;
        room.Seats[seatNo].State = SeatState.Closed;
        await _NotifyRoomLayoutChanged(room);
        currentAccount.LastAction = DateTime.Now;
        return RoomOperationResult.Success;
    }

    public async Task<RoomOperationResult> Chat(string message)
    {
        var currentAccount = lobbyManager.loggedInAccounts[Context.ConnectionId];
        if (message.Length > Misc.MaxChatLength)
            return RoomOperationResult.Invalid;

        // @todo: No global chat
        if (currentAccount.CurrentRoom == null && currentAccount.CurrentSpectatingRoom == null)
        {
            return RoomOperationResult.Invalid;
        }

        var thread = Task.Run(async () =>
        {
            var room = currentAccount.CurrentRoom ?? currentAccount.CurrentSpectatingRoom;
            if (room is null)
                return;
            foreach (var seat in room.Room.Seats)
            {
                if (seat.Account != null)
                {
                    try
                    {
                        if (lobbyManager.loggedInAccounts.TryGetValue(seat.Account.UserName, out var player))
                        {
                            await Clients.User(player.ConnectedId).NotifyChat(currentAccount.Account, message);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            foreach (var sp in room.Spectators)
            {
                try
                {
                    if (lobbyManager.loggedInAccounts.TryGetValue(sp, out var player))
                    {
                        await Clients.User(player.ConnectedId).NotifyChat(currentAccount.Account, message);
                    }
                }
                catch (Exception)
                {
                }
            }
        });
        thread.Start();
        await Task.CompletedTask;
        currentAccount.LastAction = DateTime.Now;
        return RoomOperationResult.Success;
    }

    public async Task<RoomOperationResult> Spectate(string roomId)
    {
        var currentAccount = lobbyManager.loggedInAccounts[Context.ConnectionId];
        if (currentAccount.CurrentRoom is not null)
            return RoomOperationResult.Invalid;
        if (!lobbyManager.rooms.TryGetValue(roomId, out var room))
            return RoomOperationResult.Invalid;
        if (room.Room.State != RoomState.Gaming)
            return RoomOperationResult.Invalid;
        _Unspectate(currentAccount);
        lock (room.Spectators)
        {
            if (!room.Spectators.Contains(currentAccount.Account.UserName))
            {
                room.Spectators.Add(currentAccount.Account.UserName);
            }
        }
        currentAccount.CurrentSpectatingRoom = room;
        await Clients.User(Context.ConnectionId).NotifyGameStart(room.Room.IpAddress + ":" + room.Room.IpPort, new LoginToken() { TokenString = new Guid() });
        return RoomOperationResult.Success;
    }

    [AllowAnonymous]
    public async Task<LoginStatus> CreateAccount(string userName, string p)
    {
        var result = from a in accountContext.Accounts where a.UserName.Equals(userName) select a;
        if (await result.AnyAsync())
        {
            return LoginStatus.InvalidUsernameAndPassword;
        }
        await accountContext.Accounts.AddAsync(new Account() { UserName = userName, Password = p });
        await accountContext.SaveChangesAsync();
        return LoginStatus.Success;
    }

    public void SubmitBugReport(Stream s)
    {
        throw new NotImplementedException();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var currentAccount = lobbyManager.loggedInAccounts[Context.ConnectionId];
            if (currentAccount.CurrentRoom?.Room.State == RoomState.Gaming)
                return;
            await _Logout(currentAccount);

        }
        catch (Exception)
        {
        }
    }

    private async Task _Logout(LobbyPlayer account, bool forced = false)
    {
        logger.LogInformation("{UserName} logged out", account.Account.UserName);
        if (account.CurrentRoom is not null)
        {
            if ((await _ExitRoom(account, forced)) != RoomOperationResult.Success)
            {
                try
                {
                    await Clients.Caller.NotifyCloseConnection();
                }
                catch (Exception)
                {
                    Context.Abort();
                }
                return;
            }
        }
        Trace.Assert(lobbyManager.loggedInAccounts.ContainsKey(account.Account.UserName));
        if (!lobbyManager.loggedInAccounts.ContainsKey(account.Account.UserName))
            return;
        account.CurrentSpectatingRoom = null;
        lobbyManager.loggedInAccounts.Remove(account.Account.UserName, out var _);
        try
        {
            await Clients.Caller.NotifyCloseConnection();
        }
        catch (Exception)
        {
            Context.Abort();
        }
    }

    private async Task<RoomOperationResult> _ExitRoom(LobbyPlayer account, bool forced = false)
    {
        var room = account.CurrentRoom;
        if (room is null)
            return RoomOperationResult.Invalid;

        var seat = room.Room.Seats.FirstOrDefault(s => s.Account == account.Account);
        if (seat is null)
            return RoomOperationResult.Invalid;

        if (!forced && room.Room.State == RoomState.Gaming && !account.Account.IsDead)
            return RoomOperationResult.Locked;
        var findAnotherHost = seat.State == SeatState.Host;
        seat.Account = null;
        seat.State = SeatState.Empty;
        account.CurrentRoom = null;
        account.LastAction = DateTime.Now;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Room.Id);

        if (_DestroyRoomIfEmpty(room))
            return RoomOperationResult.Success;

        if (findAnotherHost)
        {
            foreach (var host in room.Room.Seats)
            {
                if (host.Account is not null)
                {
                    host.State = SeatState.Host;
                    break;
                }
            }
        }
        if (room is not null)
            await _NotifyRoomLayoutChanged(room.Room);
        return RoomOperationResult.Success;
    }

    private async Task _NotifyRoomLayoutChanged(Room room)
    {
        await Clients.Group(room.Id).NotifyRoomUpdate(room.Id, room);
    }

    private bool _DestroyRoomIfEmpty(ServerRoom room)
    {
        if (!room.Room.IsEmpty)
        {
            return false;
        }
        else
        {
            _DestroyRoom(room.Room.Id);
            return true;
        }
    }

    private void _DestroyRoom(string roomId)
    {
        if (!lobbyManager.rooms.Remove(roomId, out var room))
            return;
        foreach (var sp in room.Spectators)
        {
            if (lobbyManager.loggedInAccounts.TryGetValue(sp, out var spectators))
            {
                spectators.CurrentSpectatingRoom = null;
            }
        }
        room.Spectators.Clear();
        foreach (var st in room.Room.Seats)
        {
            st.Account = null;
            st.State = SeatState.Closed;
        }
    }

    private static void _Unspectate(LobbyPlayer account)
    {
        if (account is null || account.Account is null) return;

        var room = account.CurrentSpectatingRoom;
        if (room is not null)
        {
            room.Spectators.Remove(account.Account.UserName);
            account.CurrentSpectatingRoom = null;
        }
    }

    private async Task _OnGameEnds(string roomId)
    {
        if (accountContext is not null)
        {
            try
            {
                await accountContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                var crashReport = new StreamWriter(FileRotator.CreateFile("./Crash", "crash", ".dmp", 1000));
                crashReport.WriteLine(e);
                crashReport.WriteLine(e.Message);
                crashReport.Close();
            }
        }
        if (!lobbyManager.rooms.TryGetValue(roomId, out var room))
            return;
        Trace.Assert(room != null);
        room.Room.State = RoomState.Waiting;
        foreach (var seat in room.Room.Seats)
        {
            if (seat.Account is null)
                continue;
            if (lobbyManager.loggedInAccounts.TryGetValue(seat.Account.UserName, out var seatAccount))
            {
                try
                {
                    await Clients.Client(seatAccount.ConnectedId).Ping();
                }
                catch (Exception)
                {
                    await _Logout(seatAccount, true);
                    seat.Account = null;
                    seat.State = SeatState.Empty;
                    continue;
                }
            }
            else
            {
                seat.State = SeatState.Empty;
                seat.Account = null;
            }

            if (seat.State != SeatState.Host)
                seat.State = SeatState.GuestTaken;

            if (seat.Account is not null && lobbyManager.loggedInAccounts.TryGetValue(seat.Account.UserName, out var lobbyPlayer) && lobbyPlayer.CurrentRoom != lobbyManager.rooms[roomId])
            {
                seat.Account = null;
                seat.State = SeatState.Empty;
            }
        }

        if (_DestroyRoomIfEmpty(room))
        {
            return;
        }
        if (!room.Room.Seats.Any(st => st.State == SeatState.Host))
        {
            room.Room.Seats.First(st => st.State == SeatState.GuestTaken).State = SeatState.Host;
        }
        Trace.Assert(room != null);
        await _NotifyRoomLayoutChanged(room.Room);
    }

    private async Task _NotifyGameStart(string roomId, IPAddress ip, int port)
    {
        if (!lobbyManager.rooms.TryGetValue(roomId, out var room))
            return;
        foreach (var notify in room.Room.Seats)
        {
            if (notify.Account is not null)
            {
                try
                {
                    await Clients.User(lobbyManager.loggedInAccounts[notify.Account.UserName].ConnectedId).NotifyGameStart(ip.ToString() + ":" + port, notify.Account.LoginToken);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}