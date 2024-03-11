namespace Sanguosha.Core.Cards;

[Serializable]    
public class DeckType
{
    private static readonly Dictionary<string, DeckType> registeredDeckTypes = [];

    public static DeckType Register(string name) => Register(name, name);

    public static DeckType Register(string name, string shortName)
    {
        if (!registeredDeckTypes.TryGetValue(shortName, out var value))
        {
            value = new DeckType(name, shortName);
            registeredDeckTypes.Add(shortName, value);
        }
        return value;
    }

    protected DeckType(string name, string shortName)
    {
        Name = name;
        AbbriviatedName = shortName;
    }

    public override int GetHashCode() => Name.GetHashCode();

    public string Name { get; private set; }

    /// <summary>
    /// Sets/gets abbreviated name used to uniquely identify and serialize this DeckType.
    /// </summary>
    public string AbbriviatedName{get;private set;}

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(obj, this))
        {
            return true;
        }
        if (obj is not DeckType)
        {
            return false;
        }
        DeckType type2 = (DeckType)obj;
        return Name.Equals(type2.Name);
    }

    public static bool operator ==(DeckType a, DeckType b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if ((a is null) || (b is null))
        {
            return false;
        }

        return a.Name.Equals(b.Name);
    }

    public static bool operator !=(DeckType a, DeckType b)
    {
        return !(a == b);
    }


    public static readonly DeckType Dealing = Register(nameof(Dealing), "0");
    public static readonly DeckType Discard = Register(nameof(Discard), "1");
    public static readonly DeckType Compute = Register(nameof(Compute), "2");
    public static readonly DeckType Hand = Register(nameof(Hand), "3");
    public static readonly DeckType Equipment = Register(nameof(Equipment), "4");
    public static readonly DeckType DelayedTools = Register(nameof(DelayedTools), "5");
    public static readonly DeckType JudgeResult = Register(nameof(JudgeResult), "6");
    public static readonly DeckType GuHuo = Register(nameof(GuHuo), "7");
    public static readonly DeckType None = Register(nameof(None), "8");
    public static readonly DeckType Heroes = Register(nameof(Heroes), "9");
    public static readonly DeckType SpecialHeroes = Register(nameof(SpecialHeroes), "A");

    public override string ToString() => Name;
}
