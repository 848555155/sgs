namespace Sanguosha.Core.Utils;

public class RecordTakingInputStream(Stream inputStream, Stream recordStream) : Stream
{
    public Stream InputStream { get; set; } = inputStream;

    public Stream RecordStream { get; set; } = recordStream;

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override void Flush() => RecordStream?.Flush();

    public override long Length => InputStream.Length;

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
