using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Players;

namespace Sanguosha.Core.Cards;

public class CardAttribute
{
    public static readonly CardAttribute TargetRequireTwoResponses = Register(nameof(TargetRequireTwoResponses));

    public static readonly CardAttribute SourceRequireTwoResponses = Register(nameof(SourceRequireTwoResponses));

    private CardAttribute(string attrName) => Name = attrName;

    public string Name { get; set; }

    private static Dictionary<string, CardAttribute> _attributeNames;


    public CardAttribute this[Player key]
    {
        get
        {
            if (key is null) return this;
            if (!key.AssociatedCardAttributes.ContainsKey(this))
            {
                key.AssociatedCardAttributes.Add(this, new(Name));
            }
            return key.AssociatedCardAttributes[this];
        }
    }

    public static CardAttribute Register(string attributeName)
    {
        _attributeNames ??= [];
        if (_attributeNames.ContainsKey(attributeName))
        {
            throw new DuplicateAttributeKeyException(attributeName);
        }
        var attr = new CardAttribute(attributeName);
        _attributeNames.Add(attributeName, attr);
        return attr;
    }
}
