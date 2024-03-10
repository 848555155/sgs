namespace Sanguosha.Core.Players;

public class PlayerAttribute
{
    private PlayerAttribute(string attrName, bool autoReset, bool isAMark, bool isStatus)
    {
        Name = attrName;
        AutoReset = autoReset;
        IsMark = isAMark;
        IsStatus = isStatus;
    }

    public string Name { get; set; }

    public bool AutoReset { get; set; }

    public bool IsStatus { get; set; }

    public bool IsMark { get; set; }

    private static Dictionary<string, PlayerAttribute> _attributeNames;

    public PlayerAttribute this[Player key]
    {
        get
        {
            if (key == null) return this;
            if (!key.AssociatedPlayerAttributes.ContainsKey(this))
            {
                var attribute = new PlayerAttribute(Name, AutoReset, IsMark, IsStatus);
                key.AssociatedPlayerAttributes.Add(this, attribute);
            }
            return key.AssociatedPlayerAttributes[this];
        }
    }

    public static PlayerAttribute Register(string attributeName, bool autoReset = false, bool isAMark = false, bool isStatus = false)
    {
        _attributeNames ??= [];
        if (_attributeNames.ContainsKey(attributeName))
        {
            return _attributeNames[attributeName];
            //throw new DuplicateAttributeKeyException(attributeName);
        }
        var attr = new PlayerAttribute(attributeName, autoReset, isAMark, isStatus);
        _attributeNames.Add(attributeName, attr);
        return attr;
    }
}
