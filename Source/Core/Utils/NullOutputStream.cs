using System.IO;

namespace Sanguosha.Core.Utils;

public class NullOutputStream(Stream stream) : Stream
{
    public Stream InputStream { get; set; } = stream;

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override void Flush()
    {
    }

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
    }
}
