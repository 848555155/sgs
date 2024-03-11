using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;

namespace Sanguosha.Core.Skills;

public abstract class EnforcedCardTransformSkill : TriggerSkill
{
    public List<DeckType> Decks { get; set; }

    protected abstract bool CardVerifier(ICard card);

    protected abstract void TransfromAction(Player Owner, ICard card);

    public EnforcedCardTransformSkill()
    {
        Decks = [];
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return a.Card != null && Decks.Contains(a.Card.Place.DeckType) && CardVerifier(a.Card); },
            (p, e, a) => { TransfromAction(p, a.Card); },
            TriggerCondition.OwnerIsSource
        ) { IsAutoNotify = false };
        Triggers.Add(GameEvent.EnforcedCardTransform, trigger);

        var notify = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) =>
            {
                return a.Card is Card card && Decks.Contains(card.HistoryPlace1.DeckType) && CardVerifier(GameEngine.CardSet[card.Id]);
            },
            (p, e, a) =>
            {
                Game.CurrentGame.NotificationProxy.NotifyLogEvent(
                    new LogEvent("EnforcedCardTransform", Owner, GameEngine.CardSet[(a.Card as Card).Id], a.Card),
                    [Owner],
                    true,
                    false
                );
            },
            TriggerCondition.OwnerIsSource
        );
        Triggers.Add(GameEvent.PlayerUsedCard, notify);
        IsEnforced = true;
    }
}
