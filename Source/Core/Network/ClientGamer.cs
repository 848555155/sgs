using System.IO;
using ProtoBuf;
using System.Net.Sockets;
using System.Diagnostics;

namespace Sanguosha.Core.Network;

#pragma warning disable CA1001 // 具有可释放字段的类型应该是可释放的
public class ClientGamer
#pragma warning restore CA1001 // 具有可释放字段的类型应该是可释放的
{
    public ClientGamer()
    {
        sema = new Semaphore(0, 1);
        semPause = new Semaphore(0, 1);
        isStopped = false;
    }

    public TcpClient TcpClient { get; set; }
    public Stream DataStream { get; set; }

    private readonly Semaphore sema;
    private readonly Semaphore semPause;
    private bool isStopped;
    public GameDataPacket Receive()
    {
        Trace.Assert(!isStopped);
        if (isStopped) return null;
        var packet = Serializer.DeserializeWithLengthPrefix<GameDataPacket>(DataStream, PrefixStyle.Base128);
        if (packet is EndOfGameNotification)
        {
            isStopped = true;
            DataStream.Close();
            if (TcpClient != null) TcpClient.Close();
            return null;
        }
        Trace.TraceInformation("Packet type {0} received", packet.GetType().Name);
        return packet;
    }

    public bool Send(GameDataPacket packet)
    {
        try
        {
            Serializer.SerializeWithLengthPrefix<GameDataPacket>(DataStream, packet, PrefixStyle.Base128);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
