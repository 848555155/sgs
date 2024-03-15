using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;
using System.Diagnostics;

namespace Sanguosha.Expansions.Basic.Cards;


public abstract class ShunChai : CardHandler
{
    protected abstract string ResultDeckName { get; }

    protected abstract string ChoicePrompt { get; }

    protected abstract DeckPlace ShunChaiDest(Player source, Player dest);

    protected override void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo)
    {
        IPlayerProxy ui = Game.CurrentGame.UiProxies[source];
        if (source.IsDead) return;
        if (dest.HandCards().Count + dest.Equipments().Count + dest.DelayedTools().Count == 0) return; // ShunChai -> WuXie(from target) -> WuXie(soemone else) -> target has no card
        List<DeckPlace> places =
        [
            new DeckPlace(dest, DeckType.Hand),
            new DeckPlace(dest, DeckType.Equipment),
            new DeckPlace(dest, DeckType.DelayedTools),
        ];
        List<string> resultDeckPlace = [ResultDeckName];
        List<int> resultDeckMax = [1];
        List<List<Card>> answer;
        if (!ui.AskForCardChoice(new CardChoicePrompt(ChoicePrompt), places, resultDeckPlace, resultDeckMax, new RequireOneCardChoiceVerifier(true), out answer))
        {
            Trace.TraceInformation("Player {0} Invalid answer", source.Id);
            answer = new List<List<Card>>();
            answer.Add(Game.CurrentGame.PickDefaultCardsFrom(places));
        }
        Card theCard = answer[0][0];

        if (ShunChaiDest(source, dest).DeckType == DeckType.Discard)
        {
            Game.CurrentGame.HandleCardDiscard(dest, new List<Card>() { theCard });
        }
        else
        {
            Game.CurrentGame.HandleCardTransferToHand(dest, source, new List<Card>() { theCard });
        }
    }

    protected abstract bool ShunChaiAdditionalCheck(Player source, Player dest, ICard card);

    public override VerifierResult Verify(Player source, ICard card, List<Player> targets, bool isLooseVerify)
    {
        if (targets == null || targets.Count == 0)
        {
            return VerifierResult.Partial;
        }
        if (!isLooseVerify && targets.Count > 1)
        {
            return VerifierResult.Fail;
        }

        foreach (var player in targets)
        {
            if (player == source)
            {
                return VerifierResult.Fail;
            }
            if (!ShunChaiAdditionalCheck(source, player, card))
            {
                return VerifierResult.Fail;
            }
            if (Game.CurrentGame.Decks[player, DeckType.Hand].Count == 0 &&
                Game.CurrentGame.Decks[player, DeckType.DelayedTools].Count == 0 &&
                Game.CurrentGame.Decks[player, DeckType.Equipment].Count == 0)
            {
                return VerifierResult.Fail;
            }
        }
        return VerifierResult.Success;
    }

    public override CardCategory Category => CardCategory.ImmediateTool;
}
