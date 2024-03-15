using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.UI;

namespace Sanguosha.Expansions.Basic.Cards;


public class CardWrapper : CardTransformSkill
{
    public override VerifierResult TryTransform(List<Card> cards, List<Player> arg, out CompositeCard card, bool isPlay)
    {
        card = new CompositeCard
        {
            Type = handler,
            Subcards = new List<Card>(cards),
            Owner = Owner
        };
        return VerifierResult.Success;
    }

    private readonly CardHandler handler;
    private readonly bool withoutNotify;

    public CardWrapper(Player p, CardHandler h, bool Notify = true)
    {
        Owner = p;
        handler = h;
        withoutNotify = !Notify;
    }

    public override void NotifyAction(Player source, List<Player> targets, CompositeCard card)
    {
        if (withoutNotify) return;
        var log = new ActionLog
        {
            GameAction = GameAction.None,
            CardAction = card,
            Source = source,
            SpecialEffectHint = GenerateSpecialEffectHintIndex(source, targets, card)
        };
        Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
    }
}
