using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sanguosha.Core.Utils;
using Sanguosha.Lobby.Core;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.ServiceModel;
using static Sanguosha.Lobby.Core.Lobby;

namespace Sanguosha.Lobby.Server;

[Authorize]
public partial class LobbyService(
    AccountContext accountContext,
    LobbyManager lobbyManager,
    ILogger<LobbyService> logger
    ) : LobbyBase
{
    private readonly AccountContext accountContext = accountContext;
    private readonly LobbyManager lobbyManager = lobbyManager;
    private readonly ILogger<LobbyService> logger = logger;



    public static IPAddress HostingIp { get; set; }

    public static IPAddress PublicIp { get; set; }

    public static bool CheatEnabled { get; set; }


    private LobbyPlayer? currentAccount;

    private async Task<Account> Authenticate(string username, string password)
    {
        var result = from a in accountContext.Accounts where a.UserName.Equals(username) select a;
        if (!await result.AnyAsync())
            return null;
        var account = await result.FirstAsync();
        if (!account.Password.Equals(password))
            return null;
        return await result.FirstAsync();
    }

    [AllowAnonymous]
    public override async Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
    {
        int version = request.Version;
        string username = request.Username;
        string hash = request.Hash;
        Account retAccount = null;

        var reconnectionToken = new LoginToken();
        string reconnectionString = null;
        if (version != Misc.ProtocolVersion)
        {
            retAccount = null;
            return Result(LoginStatus.OutdatedVersion, retAccount);
        }

        var authenticatedAccount = await Authenticate(username, hash);
        if (authenticatedAccount == null)
        {
            retAccount = null;
            return Result(LoginStatus.InvalidUsernameAndPassword, retAccount);
        }
        LobbyPlayer disconnected = null;
        if (lobbyManager.loggedInAccounts.ContainsKey(username))
        {
            disconnected = lobbyManager.loggedInAccounts[username];
            try
            {
                // if (ping.Ping())
                // {
                //     reconnectionString = null;
                //     retAccount = null;
                //     return Result(LoginStatus.InvalidUsernameAndPassword, retAccount);
                // }
                
            }
            catch (Exception)
            {
            }
            currentAccount = disconnected;
            var room = disconnected.CurrentRoom;
            if (room != null)
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
            var acc = new LobbyPlayer(authenticatedAccount, context);
            lobbyManager.loggedInAccounts.TryAdd(username, acc);
            currentAccount = acc;
            // hack
            
            var roomresult = lobbyManager.rooms.Values
                .Where(r => r.Room.Seats.Any(st => st.Account == authenticatedAccount));
            if (roomresult.Any())
            {
                acc.CurrentRoom = roomresult.First();
            }
        }
        logger.LogInformation("{username} logged in", username);
        var faultHandler = () =>
        {
            try
            {
                if (currentAccount.CurrentRoom.Room.State == RoomState.Gaming) return;
                _Logout(currentAccount);
            }
            catch (Exception)
            {
            }
        };

        context.CancellationToken.Register(faultHandler);

        retAccount = currentAccount.Account;
        _Unspectate(currentAccount);
        currentAccount.OpContext = context;
        currentAccount.LastAction = DateTime.Now;
        return Result(LoginStatus.Success, retAccount, reconnectionString, reconnectionToken.TokenString.ToString());
    }


    private void _Logout(LobbyPlayer account, bool forced = false)
    {
        logger.LogInformation("{UserName} logged out", account.Account.UserName);
        if (account == null) return;
        if (account.CurrentRoom != null)
        {
            if (_ExitRoom(account, forced) != RoomOperationResult.Success)
            {
                try
                {
                    account.NotifyChannel.Writer.Complete();
                }
                catch (Exception)
                {
                    // account.OpContext.Channel.Abort();
                }
                return;
            }
        }
        Trace.Assert(lobbyManager.loggedInAccounts.ContainsKey(account.Account.UserName));
        if (!lobbyManager.loggedInAccounts.ContainsKey(account.Account.UserName)) return;
        account.CurrentSpectatingRoom = null;
        lobbyManager.loggedInAccounts.Remove(account.Account.UserName, out var _);
        try
        {
            account.NotifyChannel.Writer.Complete();
            // account.OpContext.Channel.Close();
        }
        catch (Exception)
        {
            // account.OpContext.Channel.Abort();
        }
    }

    public override async Task<Empty> Logout(Empty request, ServerCallContext context)
    {
        if (currentAccount == null) 
            return new Empty();
        logger.LogTrace("{UserName} logged out", currentAccount.Account.UserName);
        _Logout(currentAccount);
        currentAccount = null;
        return new Empty();
    }


    public override async Task<RoomsReply> GetRooms(BoolValue request, ServerCallContext context)
    {
        var notReadyRoomsOnly = request.Value;
        if (currentAccount == null) return null;
        var result = new RoomsReply();
        result.Rooms.AddRange((from r in lobbyManager.rooms.Values
                               where !notReadyRoomsOnly || r.Room.State == RoomState.Waiting
                               select r.Room).ToList());
        return result;
    }

    public override async Task<Room> CreateRoom(CreateRoomRequest request, ServerCallContext context)
    {
        var settings = request.Settings;
        if (currentAccount == null) return null;
        if (currentAccount.CurrentRoom != null)
        {
            return null;
        }

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
        var srvRoom = new ServerRoom(room);
        lobbyManager.rooms.TryAdd(newRoomId, srvRoom);
        currentAccount.CurrentRoom = srvRoom;
        currentAccount.LastAction = DateTime.Now;
        logger.LogInformation("created room {NewRoomId}", newRoomId);
        return room;
    }

    public override async Task<EnterRoomReply> EnterRoom(EnterRoomRequest request, ServerCallContext context)
    {
        string roomId = request.RoomId;
        //bool spectate, string password
        Room room = null;
        if (currentAccount == null)
            return Result(RoomOperationResult.NotAutheticated, room);
        logger.LogInformation("{UserName} Enter room {RoomId}", roomId, currentAccount.Account.UserName);
        if (currentAccount.CurrentRoom != null)
        {
            return Result(RoomOperationResult.Locked, room);
        }

        ServerRoom serverRoom = null;
        Room clientRoom = null;
        if (!lobbyManager.rooms.ContainsKey(roomId))
        {
            return Result(RoomOperationResult.Invalid, room);
        }
        else
        {
            serverRoom = lobbyManager.rooms[roomId];
            clientRoom = serverRoom.Room;
        }

        if (clientRoom.IsEmpty || clientRoom.State == RoomState.Gaming)
            return Result(RoomOperationResult.Locked, room);
        int seatNo = 0;
        foreach (var seat in clientRoom.Seats)
        {
            logger.LogInformation("Testing seat {SeatNo}", seatNo);
            if (seat.Account == null && seat.State == SeatState.Empty)
            {
                currentAccount.CurrentRoom = serverRoom;
                currentAccount.LastAction = DateTime.Now;
                seat.Account = currentAccount.Account;
                seat.State = SeatState.GuestTaken;
                _NotifyRoomLayoutChanged(clientRoom);
                logger.LogInformation("Seat {SeatNo}", seatNo);
                _Unspectate(currentAccount);
                room = clientRoom;
                return Result(RoomOperationResult.Success, room);
            }
            seatNo++;
        }
        logger.LogInformation("Full");
        return Result(RoomOperationResult.Full, room);
    }

    private void _DestroyRoom(string roomId)
    {
        if (!lobbyManager.rooms.TryGetValue(roomId, out var room))
            return;
        lobbyManager.rooms.Remove(roomId, out var _);
        foreach (var sp in room.Spectators)
        {
            if (lobbyManager.loggedInAccounts.TryGetValue(sp, out var clientAccount))
            {
                clientAccount.CurrentSpectatingRoom = null;
            }
        }
        room.Spectators.Clear();
        foreach (var st in room.Room.Seats)
        {
            st.Account = null;
            st.State = SeatState.Closed;
        }
    }

    private RoomOperationResult _ExitRoom(LobbyPlayer account, bool forced = false)
    {
        if (account == null)
            return RoomOperationResult.Invalid;
        var room = account.CurrentRoom;
        if (room == null)
            return RoomOperationResult.Invalid;

        var seat = room.Room.Seats.FirstOrDefault(s => s.Account == account.Account);
        if (seat == null) return RoomOperationResult.Invalid;

        if (!forced && room.Room.State == RoomState.Gaming
             && !account.Account.IsDead)
        {
            return RoomOperationResult.Locked;
        }

        bool findAnotherHost = false;
        if (seat.State == SeatState.Host)
        {
            findAnotherHost = true;
        }
        seat.Account = null;
        seat.State = SeatState.Empty;
        account.CurrentRoom = null;
        account.LastAction = DateTime.Now;

        if (_DestroyRoomIfEmpty(room))
        {
            return RoomOperationResult.Success;
        }

        if (findAnotherHost)
        {
            foreach (var host in room.Room.Seats)
            {
                if (host.Account != null)
                {
                    host.State = SeatState.Host;
                    break;
                }
            }
        }
        if (room != null) _NotifyRoomLayoutChanged(room.Room);
        return RoomOperationResult.Success;
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

    public override Task<RoomOperationResultReplay> ExitRoom(Empty request, ServerCallContext context)
    {
        return Task.FromResult(Result(_ExitRoom(currentAccount)));
    }

    private async Task _NotifyRoomLayoutChanged(Room room)
    {
        if (room == null) return;
        foreach (var notify in room.Seats)
        {
            if (notify.Account != null)
            {
                try
                {
                    await lobbyManager.loggedInAccounts[notify.Account.UserName].NotifyRoomUpdate(room.Id, room);

                }
                catch (Exception)
                {
                }
            }
        }
    }

    private async Task _NotifyGameStart(string roomId, IPAddress ip, int port)
    {
        var room = lobbyManager.rooms[roomId];
        if (room == null || room.Room == null) return;

        int i = 0;
        foreach (var notify in room.Room.Seats)
        {
            if (notify.Account != null)
            {
                try
                {
                    await lobbyManager.loggedInAccounts[notify.Account.UserName].NotifyGameStart(ip.ToString() + ":" + port, notify.Account.LoginToken);
                }
                catch (Exception)
                {
                }
                i++;
            }
        }
    }

    public override async Task<RoomOperationResultReplay> ChangeSeat(Int32Value request, ServerCallContext context)
    {
        var newSeat = request.Value;
        if (currentAccount == null)
            return Result(RoomOperationResult.NotAutheticated);
        if (currentAccount.CurrentRoom == null)
            return Result(RoomOperationResult.Invalid);
        var room = currentAccount.CurrentRoom.Room;
        if (room.State == RoomState.Gaming)
        {
            return Result(RoomOperationResult.Locked);
        }
        if (newSeat < 0 || newSeat >= room.Seats.Count)
            return Result(RoomOperationResult.Invalid);
        var seat = room.Seats[newSeat];
        if (seat.Account == null && seat.State == SeatState.Empty)
        {
            foreach (var remove in room.Seats)
            {
                if (remove.Account == currentAccount.Account)
                {
                    currentAccount.LastAction = DateTime.Now;
                    if (remove == seat)
                        return Result(RoomOperationResult.Invalid);
                    seat.State = remove.State;
                    seat.Account = remove.Account;
                    remove.Account = null;
                    remove.State = SeatState.Empty;
                    await _NotifyRoomLayoutChanged(room);

                    return Result(RoomOperationResult.Success);
                }
            }
        }
        logger.LogInformation("Full");
        return Result(RoomOperationResult.Full);
    }

    private async Task _OnGameEnds(string roomId)
    {
        if (accountContext != null)
        {
            try
            {
                accountContext.SaveChanges();
            }
            catch (Exception e)
            {
                var crashReport = new StreamWriter(FileRotator.CreateFile("./Crash", "crash", ".dmp", 1000));
                crashReport.WriteLine(e);
                crashReport.WriteLine(e.Message);
                crashReport.Close();
                // todo change logic
                //accountContext = new AccountContext();
            }
        }
        ServerRoom room = null;
        if (!lobbyManager.rooms.ContainsKey(roomId)) return;
        room = lobbyManager.rooms[roomId];
        Trace.Assert(room != null);
        if (room == null) return;
        room.Room.State = RoomState.Waiting;
        foreach (var seat in room.Room.Seats)
        {
            if (seat.Account == null) continue;
            if (lobbyManager.loggedInAccounts.ContainsKey(seat.Account.UserName))
            {
                try
                {
                    // change to check status
                    //loggedInAccounts[seat.Account.UserName].CallbackChannel.Ping();
                }
                catch (Exception)
                {
                    _Logout(lobbyManager.loggedInAccounts[seat.Account.UserName], true);
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

            if (seat.State != SeatState.Host) seat.State = SeatState.GuestTaken;

            if (seat.Account != null && lobbyManager.loggedInAccounts.ContainsKey(seat.Account.UserName) && lobbyManager.loggedInAccounts[seat.Account.UserName].CurrentRoom != lobbyManager.rooms[roomId])
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
            var f = room.Room.Seats.First(st => st.State == SeatState.GuestTaken);
            f.State = SeatState.Host;
        }
        Trace.Assert(room != null);
        await _NotifyRoomLayoutChanged(room.Room);
    }

    public override async Task<RoomOperationResultReplay> StartGame(Empty request, ServerCallContext context)
    {

        if (currentAccount == null)
            return Result(RoomOperationResult.NotAutheticated);
        if (currentAccount.CurrentRoom == null)
            return Result(RoomOperationResult.Invalid);
        int portNumber;
        var room = currentAccount.CurrentRoom;
        var total = room.Room.Seats.Count(pl => pl.Account != null);
        var initiator = room.Room.Seats.FirstOrDefault(pl => pl.Account == currentAccount.Account);
        if (room.Room.State == RoomState.Gaming)
            return Result(RoomOperationResult.Invalid);
        if (total <= 1)
            return Result(RoomOperationResult.Invalid);
        if (initiator == null || initiator.State != SeatState.Host)
            return Result(RoomOperationResult.Invalid);
        if (room.Room.Seats.Any(cs => cs.Account != null && cs.State != SeatState.Host && cs.State != SeatState.GuestReady))
            return Result(RoomOperationResult.Invalid);
        room.Room.State = RoomState.Gaming;
        foreach (var unready in room.Room.Seats)
        {
            if (unready.State == SeatState.GuestReady) unready.State = SeatState.Gaming;
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
            if ((room.Room.Settings.EnabledPackages & EnabledPackages.Wind) != 0) gs.PackagesEnabled.Add("Sanguosha.Expansions.WindExpansion");
            if ((room.Room.Settings.EnabledPackages & EnabledPackages.Fire) != 0) gs.PackagesEnabled.Add("Sanguosha.Expansions.FireExpansion");
            if ((room.Room.Settings.EnabledPackages & EnabledPackages.Woods) != 0) gs.PackagesEnabled.Add("Sanguosha.Expansions.WoodsExpansion");
            if ((room.Room.Settings.EnabledPackages & EnabledPackages.Hills) != 0) gs.PackagesEnabled.Add("Sanguosha.Expansions.HillsExpansion");
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
        GameService.StartGameService(HostingIp, gs, room.Room.Id, _OnGameEnds, out portNumber);
        room.Room.IpAddress = PublicIp.ToString();
        room.Room.IpPort = portNumber;
        await _NotifyGameStart(room.Room.Id, PublicIp, portNumber);
        currentAccount.LastAction = DateTime.Now;
        return Result(RoomOperationResult.Success);
    }

    public override async Task<RoomOperationResultReplay> Ready(Empty request, ServerCallContext context)
    {
        if (currentAccount == null)
            return Result(RoomOperationResult.NotAutheticated);
        if (currentAccount.CurrentRoom == null)
            return Result(RoomOperationResult.Invalid);
        var room = currentAccount.CurrentRoom.Room;
        var seat = room.Seats.FirstOrDefault(s => s.Account == currentAccount.Account);
        if (seat == null)
            return Result(RoomOperationResult.Invalid);
        if (seat.State != SeatState.GuestTaken)
            return Result(RoomOperationResult.Invalid);
        seat.State = SeatState.GuestReady;
        await _NotifyRoomLayoutChanged(room);
        currentAccount.LastAction = DateTime.Now;
        return Result(RoomOperationResult.Success);
    }

    public override async Task<RoomOperationResultReplay> CancelReady(Empty request, ServerCallContext context)
    {
        if (currentAccount == null)
            return Result(RoomOperationResult.NotAutheticated);
        if (currentAccount.CurrentRoom == null)
            return Result(RoomOperationResult.Invalid);
        var room = currentAccount.CurrentRoom.Room;
        var seat = room.Seats.FirstOrDefault(s => s.Account == currentAccount.Account);
        if (seat == null)
            return Result(RoomOperationResult.Invalid);
        if (seat.State != SeatState.GuestReady)
            return Result(RoomOperationResult.Invalid);
        seat.State = SeatState.GuestTaken;
        await _NotifyRoomLayoutChanged(room);
        currentAccount.LastAction = DateTime.Now;
        return Result(RoomOperationResult.Success);
    }

    public override async Task<RoomOperationResultReplay> Kick(Int32Value request, ServerCallContext context)
    {
        var seatNo = request.Value;
        if (currentAccount == null)
            return Result(RoomOperationResult.NotAutheticated);
        if (currentAccount.CurrentRoom == null)
            return Result(RoomOperationResult.Invalid);
        var room = currentAccount.CurrentRoom.Room;
        var seat = room.Seats.FirstOrDefault(s => s.Account == currentAccount.Account);
        if (seat == null)
            return Result(RoomOperationResult.Invalid);
        if (seat.State != SeatState.Host)
            return Result(RoomOperationResult.Invalid);
        if (seatNo < 0 || seatNo >= room.Seats.Count)
            return Result(RoomOperationResult.Invalid);
        if (room.Seats[seatNo].State == SeatState.GuestReady || room.Seats[seatNo].State == SeatState.GuestTaken)
        {
            var kicked = room.Seats[seatNo].Account;

            if (kicked == null || !lobbyManager.loggedInAccounts.TryGetValue(kicked.UserName, out var clientAccount) ||
                _ExitRoom(clientAccount, true) == RoomOperationResult.Invalid)
            {
                // zombie occured?
                room.Seats[seatNo].State = SeatState.Empty;
                room.Seats[seatNo].Account = null;
            }
            else
            {
                try
                {
                   await  clientAccount.NotifyKicked();
                }
                catch (Exception)
                {
                }
            }
            return Result(RoomOperationResult.Success);
        }
        else
        {
            room.Seats[seatNo].State = SeatState.Empty;
            room.Seats[seatNo].Account = null;
        }
        return Result(RoomOperationResult.Invalid);
    }

    public override async Task<RoomOperationResultReplay> OpenSeat(Int32Value request, ServerCallContext context)
    {
        var seatNo = request.Value;
        if (currentAccount == null)
            return Result(RoomOperationResult.NotAutheticated);
        if (currentAccount.CurrentRoom == null)
            return Result(RoomOperationResult.Invalid);
        var room = currentAccount.CurrentRoom.Room;
        var seat = room.Seats.FirstOrDefault(s => s.Account == currentAccount.Account);
        if (seat == null)
            return Result(RoomOperationResult.Invalid);
        if (seat.State != SeatState.Host)
            return Result(RoomOperationResult.Invalid);
        if (seatNo < 0 || seatNo >= room.Seats.Count)
            return Result(RoomOperationResult.Invalid);
        if (room.Seats[seatNo].State != SeatState.Closed)
            return Result(RoomOperationResult.Invalid);
        room.Seats[seatNo].State = SeatState.Empty;
        await _NotifyRoomLayoutChanged(room);
        currentAccount.LastAction = DateTime.Now;
        return Result(RoomOperationResult.Success);
    }

    public override async Task<RoomOperationResultReplay> CloseSeat(Int32Value request, ServerCallContext context)
    {
        var seatNo = request.Value;
        if (currentAccount == null)
            return Result(RoomOperationResult.NotAutheticated);
        if (currentAccount.CurrentRoom == null)
            return Result(RoomOperationResult.Invalid);
        var room = currentAccount.CurrentRoom.Room;
        var seat = room.Seats.FirstOrDefault(s => s.Account == currentAccount.Account);
        if (seat == null)
            return Result(RoomOperationResult.Invalid);
        if (seat.State != SeatState.Host)
            return Result(RoomOperationResult.Invalid);
        if (seatNo < 0 || seatNo >= room.Seats.Count)
            return Result(RoomOperationResult.Invalid);
        if (room.Seats[seatNo].State != SeatState.Empty)
            return Result(RoomOperationResult.Invalid);
        room.Seats[seatNo].State = SeatState.Closed;
        await _NotifyRoomLayoutChanged(room);
        currentAccount.LastAction = DateTime.Now;
        return Result(RoomOperationResult.Success);

    }

    public override async Task<RoomOperationResultReplay> Chat(StringValue request, ServerCallContext context)
    {
        var message = request.Value;
        if (currentAccount == null)
            return Result(RoomOperationResult.NotAutheticated);
        if (message.Length > Misc.MaxChatLength)
            return Result(RoomOperationResult.Invalid);

        // @todo: No global chat
        if (currentAccount.CurrentRoom == null && currentAccount.CurrentSpectatingRoom == null)
        {
            return Result(RoomOperationResult.Invalid);
        }

        var task = new Task(async () =>
        {
            var room = currentAccount.CurrentRoom;
            if (room == null && currentAccount.CurrentSpectatingRoom != null)
                room = currentAccount.CurrentSpectatingRoom;
            foreach (var seat in room.Room.Seats)
            {
                if (seat.Account != null)
                {
                    try
                    {
                        if (lobbyManager.loggedInAccounts.ContainsKey(seat.Account.UserName))
                        {
                            await lobbyManager.loggedInAccounts[seat.Account.UserName].NotifyChat(currentAccount.Account, message);
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
                    if (lobbyManager.loggedInAccounts.ContainsKey(sp))
                    {
                        await lobbyManager.loggedInAccounts[sp].NotifyChat(currentAccount.Account, message);
                    }
                }
                catch (Exception)
                {
                }
            }
        });
        task.Start();
        currentAccount.LastAction = DateTime.Now;
        await Task.CompletedTask;
        return Result(RoomOperationResult.Success);
    }

    private static void _Unspectate(LobbyPlayer account)
    {
        if (account == null || account.Account == null) return;

        var room = account.CurrentSpectatingRoom;
        if (room != null)
        {
            room.Spectators.Remove(account.Account.UserName);
            account.CurrentSpectatingRoom = null;
        }
    }

    public override async Task<RoomOperationResultReplay> Spectate(StringValue request, ServerCallContext context)
    {
        string roomId = request.Value;
        if (currentAccount == null)
            return Result(RoomOperationResult.NotAutheticated);
        if (currentAccount.CurrentRoom != null)
            return Result(RoomOperationResult.Invalid);
        if (!lobbyManager.rooms.TryGetValue(roomId, out var room))
            return Result(RoomOperationResult.Invalid);
        if (room.Room.State != RoomState.Gaming)
            return Result(RoomOperationResult.Invalid);
        _Unspectate(currentAccount);
        if (!room.Spectators.Contains(currentAccount.Account.UserName))
        {
            room.Spectators.Add(currentAccount.Account.UserName);
        }
        currentAccount.CurrentSpectatingRoom = room;
        await currentAccount.NotifyGameStart(room.Room.IpAddress + ":" + room.Room.IpPort, new LoginToken() { TokenString = new Guid() });
        await Task.CompletedTask;
        return Result(RoomOperationResult.Success);
    }

    // todo change to task, should remove this method
    public void WipeDatabase()
    {
        accountContext.Database.EnsureDeleted();
        //accountContext.Database.EnsureDeleted();
    }

    // move to web api
    [AllowAnonymous]
    public override async Task<LoginStatusReply> CreateAccount(CreateAccountRequest request, ServerCallContext context)
    {
        var userName = request.UserName;
        var p = request.P;
        var result = accountContext.Accounts
               .Where(account => account.UserName.Equals(userName));
        if (await result.AnyAsync())
        {
            return Result(LoginStatus.InvalidUsernameAndPassword);
        }
        await accountContext.Accounts.AddAsync(new Account() { UserName = userName, Password = p });
        await accountContext.SaveChangesAsync();
        return Result(LoginStatus.Success);
    }

    public override async Task<Empty> SubmitBugReport(IAsyncStreamReader<BytesValue> requestStream, ServerCallContext context)
    {
        try
        {
            var file = FileRotator.CreateFile("./Reports", "crashdmp", ".rpt", 1000);
            await foreach (var bytes in requestStream.ReadAllAsync())
            {
                await file.WriteAsync(bytes.Value.ToByteArray());
            }
            await file.FlushAsync();
            file.Close();
        }
        catch (Exception)
        {
        }
        return new Empty();
    }

    private LoginStatusReply Result(LoginStatus loginStatus) => new() { LoginStatus = loginStatus };
    private RoomOperationResultReplay Result(RoomOperationResult roomOperationResult) => new() { RoomOperationResult = roomOperationResult };

    private EnterRoomReply Result(RoomOperationResult roomOperationResult, Room room = null) => new() { RoomOperationResult = roomOperationResult, Room = room };

    private LoginReply Result(LoginStatus loginStatus, Account retAccount, string connectionString = "", string tokenString = "") => new()
    {
        Status = loginStatus,
        RetAccount = retAccount,
        ReconnectionString = connectionString,
        TokenString = tokenString
    };
}
