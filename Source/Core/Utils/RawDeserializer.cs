using System.Runtime.InteropServices;

namespace Sanguosha.Core.Utils;

public class RawDeserializer(Stream input)
{
    protected BinaryReader br = new(input);

    /// <summary>
    /// Helper instance for reading value types.
    /// </summary>
    protected TypeIO tr = new();

    public bool DeserializeBool() => br.ReadBoolean();

    public byte DeserializeByte() => br.ReadByte();

    public byte[] DeserializeBytes()
    {
        int count = br.ReadInt32();
        return br.ReadBytes(count);
    }

    public char DeserializeChar() => br.ReadChar();

    public char[] DeserializeChars()
    {
        int count = br.ReadInt32();
        return br.ReadChars(count);
    }

    public decimal DeserializeDecimal() => br.ReadDecimal();

    public double DeserializeDouble() => br.ReadDouble();

    public short DeserializeShort() => br.ReadInt16();

    public int DeserializeInt() => br.ReadInt32();

    public long DeserializeLong() => br.ReadInt64();

    public sbyte DeserializeSByte() => br.ReadSByte();

    public float DeserializeFloat() => br.ReadSingle();

    public string DeserializeString() => br.ReadString();

    public ushort DeserializeUShort() => br.ReadUInt16();

    public uint DeserializeUInt() => br.ReadUInt32();

    public ulong DeserializeULong() => br.ReadUInt64();

    public Guid DeserializeGuid() => (Guid)Deserialize(typeof(Guid));

    public DateTime DeserializeDateTime() => (DateTime)Deserialize(typeof(DateTime));

    #region NullableTypes

    // Nullable value type support.

    public bool? DeserializeNBool()
    {
        bool? ret = null;

        if (br.ReadByte() == 2)
        {
            ret = br.ReadBoolean();
        }

        return ret;
    }

    public byte? DeserializeNByte()
    {
        byte? ret = null;

        if (br.ReadByte() == 2)
        {
            ret = br.ReadByte();
        }

        return ret;
    }

    public char? DeserializeNChar()
    {
        char? ret = null;

        if (br.ReadByte() == 2)
        {
            ret = br.ReadChar();
        }

        return ret;
    }

    public decimal? DeserializeNDecimal()
    {
        decimal? ret = null;

        if (br.ReadByte() == 2)
        {
            ret = br.ReadDecimal();
        }

        return ret;
    }

    public double? DeserializeNDouble()
    {
        double? ret = null;

        if (br.ReadByte() == 2)
        {
            ret = br.ReadDouble();
        }

        return ret;
    }

    public short? DeserializeNShort()
    {
        short? ret = null;

        if (br.ReadByte() == 2)
        {
            ret = br.ReadInt16();
        }

        return ret;
    }

    public int? DeserializeNInt()
    {
        int? ret = null;

        if (br.ReadByte() == 2)
        {
            ret = br.ReadInt32();
        }

        return ret;
    }

    public long? DeserializeNLong()
    {
        long? ret = null;

        if (br.ReadByte() == 2)
        {
            ret = br.ReadInt64();
        }

        return ret;
    }

    public sbyte? DeserializeNSByte()
    {
        sbyte? ret = null;

        if (br.ReadByte() == 2)
        {
            ret = br.ReadSByte();
        }

        return ret;
    }

    public float? DeserializeNFloat()
    {
        float? ret = null;

        if (br.ReadByte() == 2)
        {
            ret = br.ReadSingle();
        }

        return ret;
    }

    public ushort? DeserializeNUShort()
    {
        ushort? ret = null;

        if (br.ReadByte() == 2)
        {
            ret = br.ReadUInt16();
        }

        return ret;
    }

    public uint? DeserializeNUInt()
    {
        uint? ret = null;

        if (br.ReadByte() == 2)
        {
            ret = br.ReadUInt32();
        }

        return ret;
    }

    public ulong? DeserializeNULong()
    {
        ulong? ret = null;

        if (br.ReadByte() == 2)
        {
            ret = br.ReadUInt64();
        }

        return ret;
    }

    public DateTime? DeserializeNDateTime()
    {
        DateTime? ret = null;

        if (br.ReadByte() == 2)
        {
            ret = (DateTime?)Deserialize(typeof(DateTime));
        }

        return ret;
    }

    public Guid? DeserializeNGuid()
    {
        Guid? ret = null;

        if (br.ReadByte() == 2)
        {
            ret = (Guid?)Deserialize(typeof(Guid));
        }

        return ret;
    }

    #endregion NullableTypes

    public object Deserialize(Type type)
    {
        bool success;
        object ret = tr.Read(br, type, out success);

        if (!success)
        {
            if (type.IsValueType)
            {
                int count = br.ReadInt32();
                byte[] data = br.ReadBytes(count);
                ret = Deserialize(data, type);
            }
            else
            {
                throw new RawSerializerException("Cannot deserialize " + type.AssemblyQualifiedName);
            }
        }

        return ret;
    }

    public object DeserializeNullable(Type type)
    {
        object ret = null;

        byte code = br.ReadByte();

        if (code == 0)
        {
            ret = DBNull.Value;
        }
        else if (code == 1)
        {
            ret = null;
        }
        else if (code == 2)
        {
            ret = Deserialize(type);
        }
        else
        {
            throw new RawSerializerException("Expected a code byte during deserialization of " + type.Name);
        }

        return ret;
    }

    protected virtual object Deserialize(byte[] bytes, Type type)
    {
        object structure = null;

        try
        {
            GCHandle h = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            structure = Marshal.PtrToStructure(Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0), type);
            h.Free();
        }
        catch (Exception e)
        {
            throw new RawSerializerException(e.Message);
        }

        return structure;
    }
}
