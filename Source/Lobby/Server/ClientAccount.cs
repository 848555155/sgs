using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Sanguosha.Lobby.Core;
using System.ServiceModel;
using System.Threading.Channels;

namespace Sanguosha.Lobby.Server;

public class ClientAccount : IGameClient
{
    public ServerCallContext OpContext { get; set; }
    public Account Account { get; set; }
    public ServerRoom CurrentRoom { get; set; }
    public ServerRoom CurrentSpectatingRoom { get; set; }
    public IGameClient CallbackChannel { get; set; }
    public LobbyService LobbyService { get; set; }
    public DateTime LastAction { get; set; }

    internal readonly Channel<NotifyChatReply> notifyChat = Channel.CreateBounded<NotifyChatReply>(5);
    internal readonly Channel<NotifyGameStartReply> notifyGameStart = Channel.CreateBounded<NotifyGameStartReply>(5);
    internal readonly Channel<Empty> notifyKicked = Channel.CreateBounded<Empty>(5);
    internal readonly Channel<NotifyRoomUpdateReply> notifyRoomUpdate = Channel.CreateBounded<NotifyRoomUpdateReply>(5);

    public void NotifyChat(Account account, string message)
    {
        notifyChat.Writer.TryWrite(new NotifyChatReply
        {
            Account = account,
            Message = message
        });
    }

    public void NotifyGameStart(string connectionString, LoginToken token)
    {
        notifyGameStart.Writer.TryWrite(new NotifyGameStartReply
        {
            ConnectionString = connectionString,
            Token = token.TokenString.ToString()
        });
    }

    public void NotifyKicked()
    {
        notifyKicked.Writer.TryWrite(new Empty());
    }

    public void NotifyRoomUpdate(int id, Room room)
    {
        notifyRoomUpdate.Writer.TryWrite(new NotifyRoomUpdateReply { Id = id, Room = room });
    }
}
