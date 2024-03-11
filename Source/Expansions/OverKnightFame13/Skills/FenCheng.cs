using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;

namespace Sanguosha.Expansions.OverKnightFame13.Skills;

public class FenCheng : ActiveSkill
{
    public FenCheng()
    {
        Helper.HasNoConfirmation = true;
        IsSingleUse = true;
    }

    private static readonly PlayerAttribute FenChengUsed = PlayerAttribute.Register("FenChengUsed", false);

    public override VerifierResult Validate(GameEventArgs arg)
    {
        if (Owner[FenChengUsed] != 0)
        {
            return VerifierResult.Fail;
        }
        if (arg.Cards != null && arg.Cards.Count != 0)
        {
            return VerifierResult.Fail;
        }
        if (arg.Targets != null && arg.Targets.Count != 0)
        {
            return VerifierResult.Fail;
        }
        return VerifierResult.Success;
    }

    public override bool Commit(GameEventArgs arg)
    {
        Owner[FenChengUsed] = 1;
        var toProcess = Game.CurrentGame.AlivePlayers;
        toProcess.Remove(Owner);
        foreach (Player target in toProcess)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (target.IsDead) break;
            int answer = 0;
            if (answer == 0 && Math.Max(1, target.Equipments().Count) <= target.HandCards().Count &&
                Game.CurrentGame.UiProxies[target].AskForCardUsage(new CardUsagePrompt("FenCheng"), new FenChengVerifier(Math.Max(1, target.Equipments().Count)),
                out skill, out cards, out players))
            {
                Game.CurrentGame.HandleCardDiscard(target, cards);
            }
            else
            {
                Game.CurrentGame.DoDamage(null, target, 1, DamageElement.Fire, null, null);
            }
        }
        return true;
    }

    private class FenChengVerifier : CardsAndTargetsVerifier
    {
        public FenChengVerifier(int X)
        {
            MaxCards = X;
            MinCards = X;
            MaxPlayers = 0;
            MinPlayers = 0;
            Discarding = true;
        }
        protected override bool VerifyPlayer(Player source, Player player)
        {
            return true;
        }
        protected override bool VerifyCard(Player source, Card card)
        {
            return true;
        }
    }
}
