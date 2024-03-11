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


[ServiceKnownType("GetKnownTypes", typeof(Helper))]
[ServiceContract(Namespace = "", CallbackContract = typeof(IGameClient), SessionMode = SessionMode.Required)]
public interface ILobbyService
{
    [OperationContract(IsInitiating = true)]
    LoginStatus Login(int version, string username, string hash, out Account retAccount, out string reconnectionString, out LoginToken reconnectionToken);

    [OperationContract(IsInitiating = false)]
    void Logout();

    [OperationContract]
    IEnumerable<Room> GetRooms(bool notReadyRoomsOnly);

    [OperationContract]
    Room CreateRoom(RoomSettings settings, string password = null);

    [OperationContract]
    RoomOperationResult EnterRoom(int roomId, bool spectate, string password, out Room room);

    [OperationContract]
    RoomOperationResult ExitRoom();

    [OperationContract]
    RoomOperationResult ChangeSeat(int newSeat);

    [OperationContract]
    RoomOperationResult StartGame();

    [OperationContract]
    RoomOperationResult Ready();

    [OperationContract]
    RoomOperationResult CancelReady();

    [OperationContract]
    RoomOperationResult Kick(int seatNo);

    [OperationContract]
    RoomOperationResult OpenSeat(int seatNo);

    [OperationContract]
    RoomOperationResult CloseSeat(int seatNo);

    [OperationContract]
    RoomOperationResult Chat(string message);

    [OperationContract]
    RoomOperationResult Spectate(int roomId);

    [OperationContract]
    LoginStatus CreateAccount(string userName, string p);

    [OperationContract]
    void SubmitBugReport(Stream s);
}

public interface IGameClient
{
    void NotifyRoomUpdate(int id, Room room);

    void NotifyKicked();

    void NotifyGameStart(string connectionString, LoginToken token);

    void NotifyChat(Account account, string message);
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
