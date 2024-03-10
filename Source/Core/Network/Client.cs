﻿using System.Net;
using System.Net.Sockets;
using Sanguosha.Core.Games;
using System.IO;
using Sanguosha.Core.UI;
using Sanguosha.Lobby.Core;
using Sanguosha.Core.Utils;

namespace Sanguosha.Core.Network;

public class Client
{
    private ClientGamer gamer;

    public int SelfId { get; set; }

    public string IpString { get; set; }

    public int PortNumber { get; set; }

    public Client()
    {

    }

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
        IPEndPoint ep = new IPEndPoint(IPAddress.Parse(IpString), PortNumber);
        TcpClient client = new TcpClient();
        client.Connect(ep);
        NetworkStream stream = client.GetStream();
        gamer = new ClientGamer();
        gamer.TcpClient = client;
        gamer.DataStream = new RecordTakingInputStream(stream, recordStream);
        if (token != null)
        {
            gamer.Send(new ConnectionRequest() { Token = (LoginToken)token });
        }
    }

    public ReplayController ReplayController { get; set; }

    public void StartReplay(Stream replayStream)
    {
        this.replayStream = replayStream;
        gamer = new ClientGamer();
        gamer.DataStream = new NullOutputStream(replayStream);
        ReplayController = new Utils.ReplayController();
        ReplayController.EvenDelays = true;
    }

    private Stream replayStream;

    public Stream ReplayStream
    {
        get { return replayStream; }
    }

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
            if (pkt is StatusSync)
            {
                return ((StatusSync)pkt).Status;
            }
            else if (pkt is CardSync)
            {
                return ((CardSync)pkt).Item.ToCard(SelfId);
            }
            else if (pkt is CardRearrangementNotification)
            {
                Game.CurrentGame.NotificationProxy.NotifyCardChoiceCallback((pkt as CardRearrangementNotification).CardRearrangement);
                continue;
            }
            else if (pkt is SeedSync)
            {
                return pkt;
            }
            else if (pkt is UIStatusHint)
            {
                Game.CurrentGame.IsUiDetached = (pkt as UIStatusHint).IsDetached;
                continue;
            }
            else if (pkt is MultiCardUsageResponded)
            {
                Game.CurrentGame.NotificationProxy.NotifyMultipleCardUsageResponded((pkt as MultiCardUsageResponded).PlayerItem.ToPlayer());
                continue;
            }
            else if (pkt is OnlineStatusUpdate)
            {
                var osu = pkt as OnlineStatusUpdate;
                if (Game.CurrentGame.Players.Count > osu.PlayerId)
                {
                    Game.CurrentGame.Players[osu.PlayerId].OnlineStatus = osu.OnlineStatus;
                }
                continue;
            }
            else
            {
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
