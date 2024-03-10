using System.IO;

namespace Sanguosha.Core.Utils;

public class RecordTakingInputStream : Stream
{
#pragma warning disable CA2213 // 应释放可释放的字段
    private Stream _inputStream;
#pragma warning restore CA2213 // 应释放可释放的字段

    public Stream InputStream
    {
        get { return _inputStream; }
        set { _inputStream = value; }
    }
#pragma warning disable CA2213 // 应释放可释放的字段
    private Stream _recordStream;
#pragma warning restore CA2213 // 应释放可释放的字段

    public Stream RecordStream
    {
        get { return _recordStream; }
        set { _recordStream = value; }
    }

    public RecordTakingInputStream()
    {

    }

    public RecordTakingInputStream(Stream inputStream, Stream recordStream)
    {
        _inputStream = inputStream;
        _recordStream = recordStream;
    }

    public override bool CanRead
    {
        get { return true; }
    }

    public override bool CanSeek
    {
        get { return false; }
    }

    public override bool CanWrite
    {
        get 
        {
            return true; 
        }
    }

    public override void Flush()
    {
        if (RecordStream != null)
        {
            RecordStream.Flush();
        }
    }

    public override long Length
    {
        get { return InputStream.Length; }
    }

    public override long Position
    {
        get
        {
            return InputStream.Position;
        }
        set
        {
            InputStream.Position = value;
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int bytesRead = InputStream.Read(buffer, offset, count);
        if (RecordStream != null)
        {
            RecordStream.Write(buffer, offset, bytesRead);
            RecordStream.Flush();
        }
        return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        InputStream.Write(buffer, offset, count);
    }
}
