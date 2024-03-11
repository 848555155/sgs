using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Core.Cards;


public class OffensiveHorse(string name) : Equipment
{
    public override object Clone()
    {
        return Activator.CreateInstance(GetType(), HorseName);
    }

    protected override void RegisterEquipmentTriggers(Player p) => p[Player.RangeMinus]--;

    protected override void UnregisterEquipmentTriggers(Player p) => p[Player.RangeMinus]++;

    public override CardCategory Category => CardCategory.OffensiveHorse;

    protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
    {
        throw new NotImplementedException();
    }

    public string HorseName { get; set; } = name;

    public override string Name => HorseName;
}
