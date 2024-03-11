﻿using ProtoBuf;
using Sanguosha.Core.Games;
using Sanguosha.Core.Utils;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Sanguosha.Core.Network;

public delegate void GamePacketHandler(GameDataPacket request);
public delegate void GamerDisconnectedHandler(ServerGamer gamer);

[ProtoContract]
public enum OnlineStatus
{
    [ProtoEnum]
    Online,
    [ProtoEnum]
    Offline,
    [ProtoEnum]
    Trusted,
    [ProtoEnum]
    Away,
    [ProtoEnum]
    Quit
}
public class ServerGamer
{
    public ServerGamer()
    {
        semaPakArrival = new Semaphore(0, Int32.MaxValue);
        semaPakToSend = new Semaphore(0, Int32.MaxValue);
        receiverLock = new object();
        senderLock = new object();
        DataStream = new RecordTakingOutputStream();
        sendQueue = new BlockingCollection<GameDataPacket>(new ConcurrentQueue<GameDataPacket>());
    }

    private Thread receiveThread;
    private Thread sendThread;
    private readonly Semaphore semaPakArrival;
    private readonly Semaphore semaPakToSend;
    private readonly object receiverLock;
    private readonly object senderLock;
    private readonly BlockingCollection<GameDataPacket> sendQueue;

    public Game Game { get; set; }
    public OnlineStatus OnlineStatus { get; set; }

    public bool IsConnected
    {
        get
        {
            return OnlineStatus != OnlineStatus.Offline && OnlineStatus != OnlineStatus.Quit;
        }
    }

    public RecordTakingOutputStream DataStream
    {
        get;
        private set;
    }

    public bool IsStopped
    {
        get
        {
            return sendThread == null && receiveThread == null;
        }
    }

    public void StartSender()
    {
        lock (this)
        {
            if (sendThread == null)
            {
                sendThread = new Thread(SendLoop) { IsBackground = true };
                sendThread.Start();
            }
        }
    }

    public void StartReceiver()
    {
        lock (this)
        {
            if (!IsSpectator && receiveThread == null)
            {
                receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
                receiveThread.Start();
            }
        }
    }

    public void Stop()
    {
        SendAsync(new EndOfGameNotification());
        if (!IsSpectator && receiveThread != null)
        {
#pragma warning disable SYSLIB0006 // 类型或成员已过时
            receiveThread.Abort();
#pragma warning restore SYSLIB0006 // 类型或成员已过时
            receiveThread = null;
        }
    }

    public void Abort()
    {
        if (!IsSpectator && receiveThread != null)
        {
#pragma warning disable SYSLIB0006 // 类型或成员已过时
            receiveThread.Abort();
#pragma warning restore SYSLIB0006 // 类型或成员已过时
            receiveThread = null;
        }
        if (sendThread != null)
        {
            // We need to acquire sender lock to ensure that all packets
            // are properly written to cache for reconnection.
            lock (senderLock)
            {
#pragma warning disable SYSLIB0006 // 类型或成员已过时
                sendThread.Abort();
#pragma warning restore SYSLIB0006 // 类型或成员已过时
                sendThread = null;
            }
        }
    }

    public GameDataPacket Receive()
    {
        GameDataPacket packet;
        lock (this)
        {
            lock (receiverLock)
            {
                packet = null;
                try
                {
                    packet = Serializer.DeserializeWithLengthPrefix<GameDataPacket>(DataStream, PrefixStyle.Base128);
                }
                catch (IOException)
                {
                    OnlineStatus = OnlineStatus.Offline;
                    var handler = OnDisconnected;
                    if (handler != null)
                    {
                        OnDisconnected(this);
                    }
                    return null;
                }
                catch (Exception e)
                {
                    Trace.Assert(e != null);
                }
            }
        }
        return packet;
    }


    private void ReceiveLoop()
    {
        Game.RegisterCurrentThread();
        while (true)
        {
            GameDataPacket packet = Receive();
            if (packet == null) break;
            try
            {
                if (packet is GameResponse) semaPakArrival.WaitOne();
                var handler2 = OnGameDataPacketReceived;
                if (handler2 != null)
                {
                    handler2(packet);
                }
            }
            catch (Exception e)
            {
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                }

                Trace.TraceError(e.StackTrace);
                Trace.Assert(false, e.StackTrace);

                var crashReport = new StreamWriter(FileRotator.CreateFile("./Crash", "crash", ".dmp", 1000));
                crashReport.WriteLine(e);
                crashReport.Close();
                break;
            }
        }
        receiveThread = null;
    }

    private void SendLoop()
    {
        GameDataPacket packet;
        while (true)
        {
            packet = sendQueue.Take();
            Send(packet);
            if (packet is EndOfGameNotification)
            {
                break;
            }
        }
        sendThread = null;
    }

    public event GamerDisconnectedHandler OnDisconnected;
    public void Send(GameDataPacket packet, bool doRecord = true)
    {

        lock (senderLock)
        {
            DataStream.IsRecordEnabled = doRecord;
            Trace.TraceInformation("ServerGamer : Send {0}({1}) to client {2}", packet.GetType().Name, packet.GetHashCode(), this.GetHashCode());
            Serializer.SerializeWithLengthPrefix<GameDataPacket>(DataStream, packet, PrefixStyle.Base128);
            DataStream.Flush();
        }
        if (!DataStream.IsLastWriteSuccessful && !IsSpectator)
        {
            OnlineStatus = OnlineStatus.Offline;
            var handler = OnDisconnected;
            if (handler != null)
            {
                try
                {
                    OnDisconnected(this);
                }
                catch (Exception) { }
            }
        }
    }

    public void SendAsync(GameDataPacket packet)
    {
        sendQueue.Add(packet);
    }

    public void ReceiveAsync()
    {
        semaPakArrival.Release();
    }

    public event GamePacketHandler OnGameDataPacketReceived;

    public void Flush()
    {
        if (DataStream != null)
        {
            DataStream.Flush();
        }
    }

    public bool IsSpectator
    {
        get;
        set;
    }

    public void AddStream(Stream newStream)
    {
        if (DataStream == null)
        {
            DataStream = new RecordTakingOutputStream();
            DataStream.AddStream(newStream, true);
            return;
        }
        if (sendThread != null)
        {
            while (sendQueue.Count > 0)
            {
                Thread.Sleep(10);
            }
        }
        lock (receiverLock)
        {
            lock (senderLock)
            {
                try
                {
                    var uiDetach = new UIStatusHint() { IsDetached = true };
                    var uiAttach = new UIStatusHint() { IsDetached = false };
                    Serializer.SerializeWithLengthPrefix<GameDataPacket>(newStream, uiDetach, PrefixStyle.Base128);
                    DataStream.AddStream(newStream, true);
                    Serializer.SerializeWithLengthPrefix<GameDataPacket>(newStream, uiAttach, PrefixStyle.Base128);
                    newStream.Flush();
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
