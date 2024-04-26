using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.SignalR;
using Sanguosha.Lobby.Core;

namespace Sanguosha.Lobby.Server;

public partial class LobbyService
{
    public override async Task Notify(Empty request, IServerStreamWriter<NotifyMessage> responseStream, ServerCallContext context)
    {
        var user = context.GetHttpContext().User.Identity?.Name ?? throw new RpcException(Status.DefaultCancelled, "should authorized");
        if (lobbyManager.loggedInAccounts.TryGetValue(user, out var clientAccount))
        {
            await foreach (var message in clientAccount.NotifyChannel.Reader.ReadAllAsync())
            {
                if (!context.CancellationToken.IsCancellationRequested)
                    await responseStream.WriteAsync(message);
            }
        }
        else
        {
            throw new RpcException(Status.DefaultCancelled, "should login in before subscribe");
        }
    }
}
