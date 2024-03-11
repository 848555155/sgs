using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Expansions.OverKnightFame13.Skills;

public class JueCe : TriggerSkill
{
    public JueCe()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) =>
            {
                var ret = a.Cards.Any(c => c[Card.IsLastHandCard] == 1) && Game.CurrentGame.CurrentPlayer == p;
                if (ret)
                {
                    var card = a.Cards.FirstOrDefault(c => c[Card.IsLastHandCard] == 1);
                    if (card != null && card.HistoryPlace1.Player != null && !card.HistoryPlace1.Player.IsDead)
                        return true;
                }
                return false;
            },
            (p, e, a) =>
            {
                var card = a.Cards.FirstOrDefault(c => c[Card.IsLastHandCard] == 1);
                if (card != null && card.HistoryPlace1.Player != null)
                    Game.CurrentGame.DoDamage(p, card.HistoryPlace1.Player, 1, DamageElement.None, null, null);
            },
            TriggerCondition.Global
        )
        { };
        Triggers.Add(GameEvent.CardsLost, trigger);
        IsAutoInvoked = null;
    }
}
