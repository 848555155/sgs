using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;

namespace Sanguosha.Core.Cards;


public abstract class CardHandler : ICloneable
{
    private Dictionary<DeckPlace, List<Card>> deckBackup;
    private List<Card> cardsOnHold;
    public virtual object Clone()
    {
        return Activator.CreateInstance(GetType());
    }

    public abstract CardCategory Category{get;}

    /// <summary>
    /// 临时将卡牌提出，verify时使用
    /// </summary>
    /// <param name="cards">卡牌</param>
    /// <remarks>第二次调用将会摧毁第一次调用时临时区域的所有卡牌</remarks>
    public virtual void HoldInTemp(List<Card> cards)
    {
        deckBackup = [];
        foreach (var c in cards)
        {
            if (c.Place.DeckType == DeckType.None) continue;
            Trace.Assert(c.Type != null);
            if ((c.Type is Equipment e) && c.Place.DeckType == DeckType.Equipment)
            {
                e.UnregisterTriggers(c.Place.Player);
            }
            if (!deckBackup.ContainsKey(c.Place))
            {
                deckBackup.Add(c.Place, new List<Card>(Game.CurrentGame.Decks[c.Place]));
            }
            Game.CurrentGame.Decks[c.Place].Remove(c);
        }
        cardsOnHold = cards;
    }

    /// <summary>
    /// 回复临时区域的卡牌到原来位置
    /// </summary>
    public virtual void ReleaseHoldInTemp()
    {
        foreach (var c in cardsOnHold)
        {
            if (c.Place.DeckType == DeckType.None) continue;
            Trace.Assert(c.Type != null);
            if ((c.Type is Equipment e) && c.Place.DeckType == DeckType.Equipment)
            {
                e.RegisterTriggers(c.Place.Player);
            }
        }
        foreach (DeckPlace p in deckBackup.Keys)
        {
            Game.CurrentGame.Decks[p].Clear();
            Game.CurrentGame.Decks[p].AddRange(deckBackup[p]);
        }
        deckBackup = null;
        cardsOnHold = null;
    }

    public void NotifyCardUse(Player source, List<Player> dests, List<Player> secondary, ICard card, GameAction action)
    {
        List<Player> logTargets = ActualTargets(source, dests, card);
        var log = new ActionLog
        {
            Source = source,
            Targets = logTargets,
            SecondaryTargets = secondary,
            GameAction = action,
            CardAction = card
        };
        Game.CurrentGame.NotificationProxy.NotifySkillUse(log);

        if (card is Card terminalCard)
        {
            terminalCard.Log ??= new ActionLog();
            terminalCard.Log.Source = source;
            terminalCard.Log.Targets = dests;
            terminalCard.Log.SecondaryTargets = secondary;
            terminalCard.Log.CardAction = card;
            terminalCard.Log.GameAction = action;
        }
        else if (card is CompositeCard compositeCard)
        {
            foreach (var s in compositeCard.Subcards)
            {
                s.Log ??= new ActionLog();
                s.Log.Source = source;
                s.Log.Targets = dests;
                s.Log.SecondaryTargets = secondary;
                s.Log.CardAction = card;
                s.Log.GameAction = action;
            }
        }
    }

    public virtual void TagAndNotify(Player source, List<Player> dests, ICard card, GameAction action = GameAction.Use)
    {
        NotifyCardUse(source, dests, [], card, action);
    }

    protected virtual bool IgnoreDeath => true;

    public virtual void Process(GameEventArgs handlerArgs)
    {
        var source = handlerArgs.Source;
        var dests = handlerArgs.Targets;
        var readonlyCard = handlerArgs.ReadonlyCard;
        var inResponseTo = handlerArgs.InResponseTo;
        var card = handlerArgs.Card;
        ICard attributeCard = new Card();
        Game.CurrentGame.SortByOrderOfComputation(Game.CurrentGame.CurrentPlayer, dests);
        foreach (var player in dests)
        {
            if (player.IsDead && IgnoreDeath) continue;
            var args = new GameEventArgs
            {
                Source = source,
                Targets = [player],
                Card = card,
                ReadonlyCard = readonlyCard
            };
            try
            {
                Game.CurrentGame.Emit(GameEvent.CardUsageTargetValidating, args);
            }
            catch (TriggerResultException e)
            {
                Trace.Assert(e.Status == TriggerResult.End);
                Game.CurrentGame.NotificationProxy.NotifyLogEvent(new LogEvent("CardInvalid", this, player), args.Targets, false);
                continue;
            }
            try
            {
                Game.CurrentGame.Emit(GameEvent.CardUsageBeforeEffected, args);
            }
            catch (TriggerResultException e)
            {
                Trace.Assert(e.Status == TriggerResult.End);
                continue;
            }
            if (player.IsDead) continue;
            var newCard = new ReadOnlyCard(readonlyCard);
            Process(source, player, card, newCard, inResponseTo);
            if (newCard.Attributes is not null)
            {
                foreach (var attr in newCard.Attributes)
                {
                    attributeCard[attr.Key] = attr.Value;
                }
            }
        }
        if (attributeCard.Attributes is not null)
        {
            foreach (var attr in attributeCard.Attributes)
            {
                readonlyCard[attr.Key] = attr.Value;
            }
        }
    }

    protected abstract void Process(Player source, Player dest, ICard card, ReadOnlyCard readonlyCard, GameEventArgs inResponseTo);

    public virtual VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> targets)
    {
        return VerifyHelper(source, skill, cards, targets, IsReforging(source, skill, cards, targets));
    }

    public virtual List<Player> ActualTargets(Player source, List<Player> targets, ICard card)
    {
        return targets;
    }

    public virtual bool IsReforging(Player source, ISkill skill, List<Card> cards, List<Player> targets)
    {
        return false;
    }
    /// <summary>
    /// 卡牌UI合法性检查
    /// </summary>
    /// <param name="source"></param>
    /// <param name="skill"></param>
    /// <param name="cards"></param>
    /// <param name="targets"></param>
    /// <param name="notReforging">不是重铸中，检查PlayerCanUseCard</param>
    /// <returns></returns>
    protected VerifierResult VerifyHelper(Player source, ISkill skill, List<Card> cards, List<Player> targets, bool isReforging)
    {
        ICard card;
        if (skill is not null)
        {
            CompositeCard c;
            if (skill is CardTransformSkill s)
            {
                VerifierResult r = s.TryTransform(cards, targets, out c);
                if (c is not null && c.Type is not null && !GetType().IsAssignableFrom(c.Type.GetType()))
                {
                    return VerifierResult.Fail;
                }
                if (r != VerifierResult.Success)
                {
                    return r;
                }
                if (!isReforging)
                {
                    if (!Game.CurrentGame.PlayerCanUseCard(source, c))
                    {
                        return VerifierResult.Fail;
                    }
                }
                HoldInTemp(c.Subcards);
                card = c;
            }
            else
            {
                return VerifierResult.Fail;
            }
        }
        else
        {
            if (cards == null || cards.Count != 1)
            {
                return VerifierResult.Fail;
            }
            card = cards[0];
            if (!(GetType().IsAssignableFrom(card.Type.GetType())))
            {
                return VerifierResult.Fail;
            }

            if (!isReforging)
            {
                if (!Game.CurrentGame.PlayerCanUseCard(source, card))
                {
                    return VerifierResult.Fail;
                }
            }
            HoldInTemp(cards);
        }

        var targetCheck = ActualTargets(source, targets, card);
        if (targetCheck is not null && targetCheck.Count != 0)
        {
            if (!isReforging)
            {
                if (!Game.CurrentGame.PlayerCanBeTargeted(source, targetCheck, card))
                {
                    ReleaseHoldInTemp();
                    return VerifierResult.Fail;
                }
            }
        }
        var ret = Verify(source, card, targets);
        ReleaseHoldInTemp();
        return ret;
    }

    public abstract VerifierResult Verify(Player source, ICard card, List<Player> targets, bool isLooseVerify = false);

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Used by UI Only!</remarks>
    public virtual string Name => GetType().Name;

}
