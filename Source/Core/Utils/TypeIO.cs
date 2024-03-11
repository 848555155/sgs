using System.IO;

namespace Sanguosha.Core.Utils;

public class TypeIO
{
    private delegate void WriterDlgt(BinaryWriter bw, object val);
    private delegate object ReaderDlgt(BinaryReader br);

    private class IODelegate(TypeIO.WriterDlgt writer, TypeIO.ReaderDlgt reader)
    {
        protected WriterDlgt writer = writer;
        protected ReaderDlgt reader = reader;

        public WriterDlgt Writer
        {
            get { return writer; }
        }

        public ReaderDlgt Reader
        {
            get { return reader; }
        }
    }

    private readonly Dictionary<Type, IODelegate> typeToDelegateMap;

    public TypeIO()
    {
        typeToDelegateMap = new Dictionary<Type, IODelegate>
        {
            [typeof(bool)] = new IODelegate(new WriterDlgt(BoolWriter), new ReaderDlgt(BoolReader)),
            [typeof(byte)] = new IODelegate(new WriterDlgt(ByteWriter), new ReaderDlgt(ByteReader)),
            [typeof(byte[])] = new IODelegate(new WriterDlgt(ByteArrayWriter), new ReaderDlgt(ByteArrayReader)),
            [typeof(char)] = new IODelegate(new WriterDlgt(CharWriter), new ReaderDlgt(CharReader)),
            [typeof(char[])] = new IODelegate(new WriterDlgt(CharArrayWriter), new ReaderDlgt(CharArrayReader)),
            [typeof(decimal)] = new IODelegate(new WriterDlgt(DecimalWriter), new ReaderDlgt(DecimalReader)),
            [typeof(double)] = new IODelegate(new WriterDlgt(DoubleWriter), new ReaderDlgt(DoubleReader)),
            [typeof(short)] = new IODelegate(new WriterDlgt(ShortWriter), new ReaderDlgt(ShortReader)),
            [typeof(int)] = new IODelegate(new WriterDlgt(IntWriter), new ReaderDlgt(IntReader)),
            [typeof(long)] = new IODelegate(new WriterDlgt(LongWriter), new ReaderDlgt(LongReader)),
            [typeof(sbyte)] = new IODelegate(new WriterDlgt(SByteWriter), new ReaderDlgt(SByteReader)),
            [typeof(float)] = new IODelegate(new WriterDlgt(FloatWriter), new ReaderDlgt(FloatReader)),
            [typeof(string)] = new IODelegate(new WriterDlgt(StringWriter), new ReaderDlgt(StringReader)),
            [typeof(ushort)] = new IODelegate(new WriterDlgt(UShortWriter), new ReaderDlgt(UShortReader)),
            [typeof(uint)] = new IODelegate(new WriterDlgt(UIntWriter), new ReaderDlgt(UIntReader)),
            [typeof(ulong)] = new IODelegate(new WriterDlgt(ULongWriter), new ReaderDlgt(ULongReader)),
            [typeof(DateTime)] = new IODelegate(new WriterDlgt(DateTimeWriter), new ReaderDlgt(DateTimeReader))
        };
    }

    public bool Write(BinaryWriter bw, object val)
    {
        bool success = false;
        Type t = val.GetType();

        if (typeToDelegateMap.ContainsKey(t))
        {
            typeToDelegateMap[t].Writer(bw, val);
            success = true;
        }

        return success;
    }

    public object Read(BinaryReader br, Type t, out bool success)
    {
        object ret = null;
        success = false;

        if (typeToDelegateMap.ContainsKey(t))
        {
            ret = typeToDelegateMap[t].Reader(br);
            success = true;
        }

        return ret;
    }

    protected void BoolWriter(BinaryWriter bw, object val)
    {
        bw.Write((bool)val);
    }

    protected object BoolReader(BinaryReader br)
    {
        return br.ReadBoolean();
    }

    protected void ByteWriter(BinaryWriter bw, object val)
    {
        bw.Write((byte)val);
    }

    protected object ByteReader(BinaryReader br)
    {
        return br.ReadByte();
    }

    protected void ByteArrayWriter(BinaryWriter bw, object val)
    {
        bw.Write(((byte[])val).Length);
        bw.Write((byte[])val);
    }

    protected object ByteArrayReader(BinaryReader br)
    {
        int count = br.ReadInt32();
        return br.ReadBytes(count);
    }

    protected void CharWriter(BinaryWriter bw, object val)
    {
        bw.Write((char)val);
    }

    protected object CharReader(BinaryReader br)
    {
        return br.ReadChar();
    }

    protected void CharArrayWriter(BinaryWriter bw, object val)
    {
        bw.Write(((char[])val).Length);
        bw.Write((char[])val);
    }

    protected object CharArrayReader(BinaryReader br)
    {
        int count = br.ReadInt32();
        return br.ReadChars(count);
    }

    protected void DecimalWriter(BinaryWriter bw, object val)
    {
        bw.Write((decimal)val);
    }

    protected object DecimalReader(BinaryReader br)
    {
        return br.ReadDecimal();
    }

    protected void DoubleWriter(BinaryWriter bw, object val)
    {
        bw.Write((double)val);
    }

    protected object DoubleReader(BinaryReader br)
    {
        return br.ReadDouble();
    }

    protected void ShortWriter(BinaryWriter bw, object val)
    {
        bw.Write((short)val);
    }

    protected object ShortReader(BinaryReader br)
    {
        return br.ReadInt16();
    }

    protected void IntWriter(BinaryWriter bw, object val)
    {
        bw.Write((int)val);
    }

    protected object IntReader(BinaryReader br)
    {
        return br.ReadInt32();
    }

    protected void LongWriter(BinaryWriter bw, object val)
    {
        bw.Write((long)val);
    }

    protected object LongReader(BinaryReader br)
    {
        return br.ReadInt64();
    }

    protected void SByteWriter(BinaryWriter bw, object val)
    {
        bw.Write((sbyte)val);
    }

    protected object SByteReader(BinaryReader br)
    {
        return br.ReadSByte();
    }

    protected void FloatWriter(BinaryWriter bw, object val)
    {
        bw.Write((float)val);
    }

    protected object FloatReader(BinaryReader br)
    {
        return br.ReadSingle();
    }

    protected void StringWriter(BinaryWriter bw, object val)
    {
        bw.Write((string)val);
    }

    protected object StringReader(BinaryReader br)
    {
        return br.ReadString();
    }

    protected void UShortWriter(BinaryWriter bw, object val)
    {
        bw.Write((ushort)val);
    }

    protected object UShortReader(BinaryReader br)
    {
        return br.ReadUInt16();
    }

    protected void UIntWriter(BinaryWriter bw, object val)
    {
        bw.Write((uint)val);
    }

    protected object UIntReader(BinaryReader br)
    {
        return br.ReadUInt32();
    }

    protected void ULongWriter(BinaryWriter bw, object val)
    {
        bw.Write((ulong)val);
    }

    protected object ULongReader(BinaryReader br)
    {
        return br.ReadUInt64();
    }

    protected void DateTimeWriter(BinaryWriter bw, object val)
    {
        bw.Write(((DateTime)val).Ticks);
    }

    protected object DateTimeReader(BinaryReader br)
    {
        long ticks = br.ReadInt64();
        return new DateTime(ticks);
    }
}
