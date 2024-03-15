using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Sanguosha.Lobby.Core;
using System.Threading.Channels;

namespace Sanguosha.Lobby.Server;

public class LobbyPlayer(Account account, ServerCallContext context): IGameClient
{
    public ServerCallContext ServerCallContext { get; set; } = context;
    public Account Account { get; set; } = account;
    public ServerRoom? CurrentRoom { get; set; }
    public ServerRoom? CurrentSpectatingRoom { get; set; }
    public DateTime LastAction { get; set; }

    public Channel<NotifyMessage> NotifyChannel { get; } = Channel.CreateBounded<NotifyMessage>(10);

    public async Task NotifyChat(Account account, string message)
    {
        await NotifyChannel.Writer.WriteAsync(new NotifyMessage()
        {
            Chat = new NotifyChatReply()
            {
                Account = account,
                Message = message
            }
        });
    }

    public async Task NotifyGameStart(string connectionString, LoginToken token)
    {
        await NotifyChannel.Writer.WriteAsync(new NotifyMessage()
        {
            GameStart = new NotifyGameStartReply()
            {
                ConnectionString = connectionString,
                Token = token.TokenString.ToString()
            }
        });
    }

    public async Task NotifyKicked()
    {
        await NotifyChannel.Writer.WriteAsync(new NotifyMessage()
        {
            Kicked = new Empty()
        });
    }

    public async Task NotifyRoomUpdate(string id, Room room)
    {
        await NotifyChannel.Writer.WriteAsync(new NotifyMessage()
        {
            RoomUpdate = new NotifyRoomUpdateReply()
            {
                Id = id,
                Room = room
            }
        });
    }

    public Task<bool> Ping()
    {
        throw new NotImplementedException();
    }
}
