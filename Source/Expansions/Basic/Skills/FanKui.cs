using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;
using System.Diagnostics;

namespace Sanguosha.Expansions.Basic.Skills;

/// <summary>
/// 反馈-每当你受到一次伤害后，你可以获得伤害来源的一张牌。
/// </summary>
public class FanKui : TriggerSkill
{
    public void OnAfterDamageInflicted(Player owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        NotifySkillUse(new List<Player>() { eventArgs.Source });
        List<DeckPlace> deck = [new DeckPlace(eventArgs.Source, DeckType.Hand), new DeckPlace(eventArgs.Source, DeckType.Equipment)];
        List<int> max = [1];
        List<List<Card>> result;
        List<string> deckname = ["FanKui choice"];

        if (!Game.CurrentGame.UiProxies[Owner].AskForCardChoice(new CardChoicePrompt("FanKui", eventArgs.Source), deck, deckname, max, new RequireOneCardChoiceVerifier(true), out result))
        {
            Trace.TraceInformation("Invalid choice for FanKui");
            result = [Game.CurrentGame.PickDefaultCardsFrom(deck)];
        }
        Game.CurrentGame.HandleCardTransferToHand(eventArgs.Source, owner, result[0]);
    }

    public FanKui()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            OnAfterDamageInflicted,
            TriggerCondition.OwnerIsTarget | TriggerCondition.SourceHasCards
        )
        { IsAutoNotify = false };
        Triggers.Add(GameEvent.AfterDamageInflicted, trigger);
        IsAutoInvoked = false;
    }

}
