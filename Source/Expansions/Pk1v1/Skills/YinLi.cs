using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;

namespace Sanguosha.Expansions.Pk1v1.Skills;

/// <summary>
/// 姻礼-对手的回合内，当其失去的装备牌进入弃牌堆时，你可以获得之。
/// </summary>
public class YinLi : TriggerSkill
{
    private void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        var args = eventArgs as DiscardCardEventArgs;
        if (Game.CurrentGame.CurrentPlayer == Owner || args.Source == null || args.Source == Owner || (args.Reason != DiscardReason.Discard))
        {
            return;
        }
        var cardsToProcess = new List<Card>(
                             from c in eventArgs.Cards
                             where c.Type.BaseCategory() == CardCategory.Equipment && (c.Place.DeckType == DeckType.Hand || c.Place.DeckType == DeckType.Equipment)
                             select c);
        if (cardsToProcess.Count() > 0)
        {
            CardsMovement temp = new CardsMovement();
            temp.Cards = new List<Card>(cardsToProcess);
            temp.To = new DeckPlace(null, DeckType.Discard);
            foreach (Card cc in cardsToProcess)
            {
                cc.PlaceOverride = new DeckPlace(null, DeckType.Discard);
            }
            Game.CurrentGame.NotificationProxy.NotifyCardMovement(new List<CardsMovement>() { temp });
        }
        else return;
        List<OptionPrompt> prompts = new List<OptionPrompt>();
        if (cardsToProcess.Count > 1)
        {
            prompts.Add(Prompt.NoChoice);
            prompts.Add(new OptionPrompt("YinLiQuanBu"));
            prompts.Add(new OptionPrompt("YinLiBuFen"));
        }
        else prompts.AddRange(Prompt.YesNoChoices);
        int choiceIndex = 0;
        Owner.AskForMultipleChoice(new MultipleChoicePrompt(Prompt.SkillUseYewNoPrompt, this), prompts, out choiceIndex);
        if (choiceIndex == 0) return;
        if (choiceIndex == 1) NotifySkillUse();
        foreach (var c in cardsToProcess)
        {
            var prompt = new MultipleChoicePrompt("YinLi", c);
            int answer = 0;
            if (choiceIndex == 1 || Owner.AskForMultipleChoice(prompt, Prompt.YesNoChoices, out answer) && answer == 1)
            {
                if (choiceIndex == 2) NotifySkillUse();
                c.Log = new ActionLog();
                c.Log.SkillAction = this;
                CardsMovement temp = new CardsMovement();
                temp.Cards.Add(c);
                temp.To = new DeckPlace(null, YinLiDeck);
                c.PlaceOverride = new DeckPlace(null, DeckType.Discard);
                Game.CurrentGame.NotificationProxy.NotifyCardMovement(new List<CardsMovement>() { temp });
                c.Log = new ActionLog();
                Game.CurrentGame.HandleCardTransferToHand(c.Owner, Owner, new List<Card>() { c }, new MovementHelper() { IsFakedMove = true, AlwaysShowLog = true });
                eventArgs.Cards.Remove(c);
            }
        }
    }

    private static readonly DeckType YinLiDeck = DeckType.Register("YinLi");


    public YinLi()
    {
        var trigger2 = new AutoNotifyPassiveSkillTrigger(
            this,
            Run,
            TriggerCondition.Global
        )
        { IsAutoNotify = false, AskForConfirmation = false };
        Triggers.Add(GameEvent.CardsEnteringDiscardDeck, trigger2);
    }

}