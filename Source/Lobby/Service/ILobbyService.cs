using ProtoBuf;
using System.Reflection;
using System.ServiceModel;

namespace Sanguosha.Lobby.Core;

[Serializable]
[ProtoContract]
public struct LoginToken
{
    [ProtoMember(1)]
    public Guid TokenString { get; set; }
}

public class LoginResult
{
    public LoginStatus Status { get; set; }

    public Account? Account { get; set; }

    public string? ReconnectionString { get; set; }

    public LoginToken ReconnectionToken { get; set; }

    public static LoginResult InvalidUsernameAndPassword => new()
    {
        Status = LoginStatus.InvalidUsernameAndPassword
    };

    public static LoginResult Success(Account account, string? reconnectionString, LoginToken reconnectionToken) => new()
    {
        Status = LoginStatus.Success,
        Account = account,
        ReconnectionString = reconnectionString,
        ReconnectionToken = reconnectionToken
    };
}

public class EnterRoomResult
{
    public RoomOperationResult Status { get; set; }

    public Room? Room { get; set; }

    public static EnterRoomResult Success(Room room) => new()
    {
        Status = RoomOperationResult.Success,
        Room = room
    };

    public static EnterRoomResult Locked => new()
    {
        Status = RoomOperationResult.Locked
    };

    public static EnterRoomResult Invalid => new()
    {
        Status = RoomOperationResult.Invalid
    };

    public static EnterRoomResult Full => new()
    {
        Status = RoomOperationResult.Full
    };
}


[ServiceKnownType("GetKnownTypes", typeof(Helper))]
[ServiceContract(Namespace = "", CallbackContract = typeof(IGameClient), SessionMode = SessionMode.Required)]
public interface ILobbyService
{
    [OperationContract(IsInitiating = true)]
    Task<LoginResult> Login(string username, string password);

    [OperationContract(IsInitiating = false)]
    Task Logout();

    [OperationContract]
    IEnumerable<Room> GetRooms(bool notReadyRoomsOnly);

    [OperationContract]
    Task<Room?> CreateRoom(RoomSettings settings, string? password = null);

    [OperationContract]
    Task<EnterRoomResult> EnterRoom(string roomId, bool spectate, string password);

    [OperationContract]
    Task<RoomOperationResult> ExitRoom();

    [OperationContract]
    Task<RoomOperationResult> ChangeSeat(int newSeat);

    [OperationContract]
    Task<RoomOperationResult> StartGame();

    [OperationContract]
    Task<RoomOperationResult> Ready();

    [OperationContract]
    Task<RoomOperationResult> CancelReady();

    [OperationContract]
    Task<RoomOperationResult> Kick(int seatNo);

    [OperationContract]
    Task<RoomOperationResult> OpenSeat(int seatNo);

    [OperationContract]
    Task<RoomOperationResult> CloseSeat(int seatNo);

    [OperationContract]
    Task<RoomOperationResult> Chat(string message);

    [OperationContract]
    Task<RoomOperationResult> Spectate(string roomId);

    [OperationContract]
    Task<LoginStatus> CreateAccount(string userName, string p);

    [OperationContract]
    void SubmitBugReport(Stream s);
}

public interface IGameClient
{
    Task NotifyRoomUpdate(string id, Room room);

    Task NotifyKicked();

    Task NotifyGameStart(string connectionString, LoginToken token);

    Task NotifyChat(Account account, string message);

    Task<bool> Ping();

}
internal static class Helper
{
    public static IEnumerable<Type> GetKnownTypes(ICustomAttributeProvider provider)
    {
        return [
                // Add any types to include here.
                typeof(Room),
                typeof(Seat),
                typeof(Account),
            ];
    }
}
