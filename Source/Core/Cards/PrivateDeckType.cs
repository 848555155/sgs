namespace Sanguosha.Core.Cards;

[Serializable]
public class PrivateDeckType(string name, bool pv = false) : DeckType(name, name)
{
    public bool PubliclyVisible { get; set; } = pv;
}
