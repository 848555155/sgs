using Sanguosha.Core.Games;
using Sanguosha.Core.UI;
using Sanguosha.Core.Utils;
using Sanguosha.Lobby.Core;
using System.Net;
using System.Net.Sockets;

namespace Sanguosha.Core.Network;

public class Client
{
    private ClientGamer gamer;

    public int SelfId { get; set; }

    public string IpString { get; set; }

    public int PortNumber { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isReplay">Set true if this is client is connected to a replayFile</param>
    /// <param name="replayStream"></param>
    /// <exception cref="System.ArgumentOutOfRangeException" />
    /// <exception cref="System.Net.Sockets.SocketException" />
    public void Start(Stream recordStream, LoginToken? token = null)
    {
        RecordStream = recordStream;
        var ep = new IPEndPoint(IPAddress.Parse(IpString), PortNumber);
        var client = new TcpClient();
        client.Connect(ep);
        NetworkStream stream = client.GetStream();
        gamer = new ClientGamer
        {
            TcpClient = client,
            DataStream = new RecordTakingInputStream(stream, recordStream)
        };
        if (token != null)
        {
            gamer.Send(new ConnectionRequest() { Token = (LoginToken)token });
        }
    }

    public ReplayController ReplayController { get; set; }

    public void StartReplay(Stream replayStream)
    {
        this.ReplayStream = replayStream;
        gamer = new ClientGamer
        {
            DataStream = new NullOutputStream(replayStream)
        };
        ReplayController = new ReplayController
        {
            EvenDelays = true
        };
    }

    public Stream ReplayStream { get; private set; }

    public Stream RecordStream { get; set; }

    public void MoveHandCard(int from, int to)
    {
        gamer.Send(new HandCardMovementNotification() { From = from, To = to, PlayerItem = PlayerItem.Parse(SelfId) });
    }

    public void CardChoiceCallBack(CardRearrangement arrange)
    {
        gamer.Send(new CardRearrangementNotification() { CardRearrangement = arrange });
    }

    public void Send(GameDataPacket p)
    {
        gamer.Send(p);
    }

    public object Receive()
    {
        while (true)
        {
            var pkt = gamer.Receive();
            switch (pkt)
            {
                case StatusSync sync:
                    return sync.Status;
                case CardSync cardSync:
                    return cardSync.Item.ToCard(SelfId);
                case CardRearrangementNotification cardRearrangementNotification:
                    Game.CurrentGame.NotificationProxy.NotifyCardChoiceCallback(cardRearrangementNotification.CardRearrangement);
                    continue;
                case SeedSync:
                    return pkt;
                case UIStatusHint uIStatusHint:
                    Game.CurrentGame.IsUiDetached = uIStatusHint.IsDetached;
                    continue;
                case MultiCardUsageResponded multiCardUsageResponded:
                    Game.CurrentGame.NotificationProxy.NotifyMultipleCardUsageResponded(multiCardUsageResponded.PlayerItem.ToPlayer());
                    continue;
                case OnlineStatusUpdate osu:
                    if (Game.CurrentGame.Players.Count > osu.PlayerId)
                    {
                        Game.CurrentGame.Players[osu.PlayerId].OnlineStatus = osu.OnlineStatus;
                    }
                    continue;
                default:
                    return pkt;
            }
        }
    }

    public void Stop()
    {
        gamer.Receive();
        if (RecordStream != null)
        {
            RecordStream.Flush();
            RecordStream.Close();
        }
    }
}
