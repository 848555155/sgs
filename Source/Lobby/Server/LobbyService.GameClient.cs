using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Sanguosha.Lobby.Core;
using System.Threading.Channels;

namespace Sanguosha.Lobby.Server;

public partial class LobbyService : IGameClient
{
    public override async Task NotifyChat(Empty request, IServerStreamWriter<NotifyChatReply> responseStream, ServerCallContext context)
    {
        var channel = Channel.CreateBounded<NotifyChatReply>(5);
        await foreach (var message in channel.Reader.ReadAllAsync())
        {
            await responseStream.WriteAsync(message);
        }
    }

    public override Task NotifyGameStart(Empty request, IServerStreamWriter<NotifyGameStartReply> responseStream, ServerCallContext context)
    {
        return base.NotifyGameStart(request, responseStream, context);
    }

    public override Task NotifyKicked(Empty request, IServerStreamWriter<Empty> responseStream, ServerCallContext context)
    {
        return base.NotifyKicked(request, responseStream, context);
    }

    public override Task NotifyRoomUpdate(Empty request, IServerStreamWriter<NotifyRoomUpdateReply> responseStream, ServerCallContext context)
    {
        return base.NotifyRoomUpdate(request, responseStream, context);
    }

    public void NotifyChat(Account account, string message)
    {
        throw new NotImplementedException();
    }

    public void NotifyGameStart(string connectionString, LoginToken token)
    {
        throw new NotImplementedException();
    }


    public void NotifyKicked()
    {
        throw new NotImplementedException();
    }

    public void NotifyRoomUpdate(int id, Room room)
    {
        throw new NotImplementedException();
    }
}
