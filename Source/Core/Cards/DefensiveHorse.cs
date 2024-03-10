using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Core.Cards;


public class DefensiveHorse(string name) : Equipment
{
    public override object Clone()
    {
        return Activator.CreateInstance(this.GetType(), this.HorseName);            
    }

    protected override void RegisterEquipmentTriggers(Player p)
    {
        p[Player.RangePlus]++;
    }

    protected override void UnregisterEquipmentTriggers(Player p)
    {
        p[Player.RangePlus]--;
    }

    public override CardCategory Category => CardCategory.DefensiveHorse;

    protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
    {
        throw new NotImplementedException();
    }

    public string HorseName { get; set; } = name;

    public override string Name => HorseName;
}
