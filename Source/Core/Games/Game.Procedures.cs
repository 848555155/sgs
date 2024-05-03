﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;
using Sanguosha.Core.Utils;
using System.Diagnostics;


namespace Sanguosha.Core.Games;

public abstract partial class Game
{
    /// <summary>
    /// 造成伤害
    /// </summary>
    /// <param name="source">伤害来源</param>
    /// <param name="dest">伤害目标</param>
    /// <param name="originalTarget">最初的伤害目标</param>
    /// <param name="magnitude">伤害点数</param>
    /// <param name="elemental">伤害属性</param>
    /// <param name="cards">造成伤害的牌</param>
    public void DoDamage(Player source, Player dest, Player originalTarget, int magnitude, DamageElement elemental, ICard card, ReadOnlyCard readonlyCard)
    {
        if (dest.IsDead) return;
        var damageArgs = new DamageEventArgs() { Source = source, OriginalTarget = originalTarget, Targets = new List<Player>(), Magnitude = magnitude, Element = elemental };
        HealthChangedEventArgs healthChangedArgs;
        int ironShackledDamage = 0;
        DamageElement ironShackledDamageElement = DamageElement.None;
        readonlyCard ??= new ReadOnlyCard(new Card() { Place = new DeckPlace(null, null) });
        damageArgs.ReadonlyCard = readonlyCard;
        if (card is CompositeCard compositeCard)
        {
            if (compositeCard.Subcards != null)
            {
                damageArgs.Cards = new List<Card>(compositeCard.Subcards);
            }
        }
        else if (card is Card)
        {
            damageArgs.Cards = [card as Card];
        }
        else
        {
            damageArgs.Cards = [];
        }
        damageArgs.Targets.Add(dest);
        damageArgs.Card = card;

        try
        {
            //伤害来源与基数、属性的确定发生在伤害结算前，连环，以及转移的伤害不会重新确定来源与基数，所以不会多次触发【裸衣】，以及【酒】
            while (damageArgs.ReadonlyCard[SourceAndElementIsConfirmed] == 0)
            {
                Emit(GameEvent.DamageSourceConfirmed, damageArgs);
                Emit(GameEvent.DamageElementConfirmed, damageArgs);
                damageArgs.ReadonlyCard[SourceAndElementIsConfirmed] = 1;
                break;
            }
            Emit(GameEvent.BeforeDamageComputing, damageArgs);
            Emit(GameEvent.DamageComputingStarted, damageArgs);
            Emit(GameEvent.DamageCaused, damageArgs);
            Emit(GameEvent.DamageInflicted, damageArgs);
            if (damageArgs.Magnitude == 0)
            {
                Trace.TraceInformation("Damage is 0, aborting");
                return;
            }
            if (damageArgs.Targets[0].IsIronShackled && damageArgs.Element != DamageElement.None)
            {
                ironShackledDamage = damageArgs.Magnitude;
                Trace.TraceInformation("IronShackled damage {0}", ironShackledDamage);
                ironShackledDamageElement = damageArgs.Element;
                damageArgs.Targets[0].IsIronShackled = false;
                // if this is TieSuo damage already, prevent itself from spreading...
                if (readonlyCard[IsIronShackleDamage] == 1) ironShackledDamage = 0;
            }
            healthChangedArgs = new HealthChangedEventArgs(damageArgs);
            Emit(GameEvent.BeforeHealthChanged, healthChangedArgs);
            damageArgs.Magnitude = -healthChangedArgs.Delta;
        }
        catch (TriggerResultException e)
        {
            if (e.Status == TriggerResult.End)
            {
                //伤害结算完毕事件应该总是被触发
                //受到伤害的角色如果存活能发动的技能/会执行的技能效果：【酒诗②】、执行【天香】摸牌的效果。
                Emit(GameEvent.DamageComputingFinished, damageArgs);
                Trace.TraceInformation("Damage Aborted");
                return;
            }
            Trace.Assert(false);
            return;
        }

        Trace.Assert(damageArgs.Targets.Count == 1);
        damageArgs.Targets[0].Health -= damageArgs.Magnitude;
        Trace.TraceInformation("Player {0} Lose {1} hp, @ {2} hp", damageArgs.Targets[0].Id, damageArgs.Magnitude, damageArgs.Targets[0].Health);
        NotificationProxy.NotifyDamage(damageArgs.Source, damageArgs.Targets[0], damageArgs.Magnitude, damageArgs.Element);
        GameDelays.Delay(GameDelays.Damage);

        try
        {
            Emit(GameEvent.AfterHealthChanged, healthChangedArgs);
        }
        catch (TriggerResultException)
        {
        }
        Emit(GameEvent.AfterDamageCaused, damageArgs);
        Emit(GameEvent.AfterDamageInflicted, damageArgs);
        Emit(GameEvent.DamageComputingFinished, damageArgs);
        if (ironShackledDamage != 0)
        {
            List<Player> toProcess = new List<Player>(AlivePlayers);
            SortByOrderOfComputation(CurrentPlayer, toProcess);
            foreach (var p in toProcess)
            {
                if (p.IsIronShackled)
                {
                    readonlyCard[IsIronShackleDamage] = 1;
                    DoDamage(damageArgs.Source, p, originalTarget, ironShackledDamage, ironShackledDamageElement, card, readonlyCard);
                }
            }
        }
    }

    private static readonly CardAttribute IsIronShackleDamage = CardAttribute.Register(nameof(IsIronShackleDamage));
    private static readonly CardAttribute SourceAndElementIsConfirmed = CardAttribute.Register(nameof(SourceAndElementIsConfirmed));

    public void DoDamage(Player source, Player dest, int magnitude, DamageElement elemental, ICard card, ReadOnlyCard readonlyCard)
    {
        DoDamage(source, dest, dest, magnitude, elemental, card, readonlyCard);
    }

    public void PlayerAcquireAdditionalSkill(Player p, ISkill skill, Hero tag, bool undeletable = false)
    {
        if (p.IsDead) return;
        p.AcquireAdditionalSkill(skill, tag, undeletable);
        var args = new SkillSetChangedEventArgs
        {
            Source = p,
            Skills = [skill],
            IsLosingSkill = false
        };
        Emit(GameEvent.PlayerSkillSetChanged, args);
        _ResetCards(p);
    }

    public void PlayerLoseAdditionalSkill(Player p, ISkill skill, bool undeletable = false)
    {
        if (!undeletable && !p.AdditionalSkills.Contains(skill)) return;
        p.LoseAdditionalSkill(skill, undeletable);
        var args = new SkillSetChangedEventArgs
        {
            Source = p,
            Skills = [skill],
            IsLosingSkill = true
        };
        Emit(GameEvent.PlayerSkillSetChanged, args);
        _ResetCards(p);
    }

    public void HandleGodHero(Player p)
    {
        if (p.Allegiance == Allegiance.God)
        {
            UiProxies[p].AskForMultipleChoice(new MultipleChoicePrompt("ChooseAllegiance"), Prompt.AllegianceChoices, out var answer);
            p.Allegiance = answer switch
            {
                0 => p.Allegiance = Allegiance.Qun,
                1 => p.Allegiance = Allegiance.Shu,
                2 => p.Allegiance = Allegiance.Wei,
                3 => p.Allegiance = Allegiance.Wu,
                _ => p.Allegiance = Allegiance.Unknown
            };
        }
    }


    public ReadOnlyCard Judge(Player player, ISkill skill = null, ICard handler = null, JudgementResultSucceed del = null)
    {
        var log = new ActionLog
        {
            SkillAction = skill,
            CardAction = handler,
            Source = player,
            GameAction = GameAction.Judge
        };
        var move = new CardsMovement();
        Card c;
        int initCount = Decks[player, DeckType.JudgeResult].Count;
        SyncImmutableCardAll(PeekCard(0));
        c = DrawCard();
        c.Log = log;
        move = new CardsMovement
        {
            Cards = [c],
            To = new DeckPlace(player, DeckType.JudgeResult)
        };
        MoveCards(move, false, GameDelays.None);
        var args = new GameEventArgs
        {
            Source = player
        };
        if (triggers.TryGetValue(GameEvent.PlayerJudgeBegin, out var value) && value.Count > 0)
        {
            NotifyIntermediateJudgeResults(player, log, del);
        }
        Emit(GameEvent.PlayerJudgeBegin, args);
        c = Decks[player, DeckType.JudgeResult].Last();
        args.ReadonlyCard = new ReadOnlyCard(c);
        args.Cards = [c];
        args.Skill = skill;
        args.Card = handler;
        bool? succeed = null;
        if (del != null)
        {
            succeed = del(args.ReadonlyCard);
        }

        var uiCard = new Card(args.ReadonlyCard)
        {
            Id = args.ReadonlyCard.Id
        };
        uiCard.Log ??= new ActionLog();
        uiCard.Log = log;
        NotificationProxy.NotifyJudge(player, uiCard, log, succeed);
        Emit(GameEvent.PlayerJudgeDone, args);
        Trace.Assert(args.Source == player);
        Trace.Assert(args.ReadonlyCard is ReadOnlyCard);

        if (Decks[player, DeckType.JudgeResult].Count > initCount)
        {
            c = Decks[player, DeckType.JudgeResult].Last();
            move = new CardsMovement
            {
                Cards = [c]
            };
            var backup = new List<Card>(move.Cards);
            move.To = new DeckPlace(null, DeckType.Discard);
            move.Helper = new MovementHelper();
            PlayerAboutToDiscardCard(player, move.Cards, DiscardReason.Judge);
            MoveCards(move, false, GameDelays.None);
            PlayerDiscardedCard(player, backup, DiscardReason.Judge);
        }
        GameDelays.Delay(GameDelays.JudgeEnd);
        return args.ReadonlyCard;
    }

    public void RecoverHealth(Player source, Player target, int magnitude)
    {
        if (target.IsDead) return;
        if (target.Health >= target.MaxHealth)
        {
            return;
        }
        var args = new HealthChangedEventArgs() { Source = source, Delta = magnitude };
        args.Targets.Add(target);

        Emit(GameEvent.BeforeHealthChanged, args);

        Trace.Assert(args.Targets.Count == 1);
        if (args.Targets[0].Health + args.Delta > args.Targets[0].MaxHealth)
        {
            args.Targets[0].Health = args.Targets[0].MaxHealth;
        }
        else
        {
            args.Targets[0].Health += args.Delta;
        }

        Trace.TraceInformation("Player {0} gain {1} hp, @ {2} hp", args.Targets[0].Id, args.Delta, args.Targets[0].Health);
        NotificationProxy.NotifyRecoverHealth(args.Targets[0], args.Delta);

        try
        {
            Emit(GameEvent.AfterHealthChanged, args);
        }
        catch (TriggerResultException)
        {
        }
    }

    public void LoseHealth(Player source, int magnitude)
    {
        if (source.IsDead) return;
        var args = new HealthChangedEventArgs() { Source = null, Delta = -magnitude };
        args.Targets.Add(source);

        Emit(GameEvent.BeforeHealthChanged, args);

        Trace.Assert(args.Targets.Count == 1);
        args.Targets[0].Health += args.Delta;
        Trace.TraceInformation("Player {0} lose {1} hp, @ {2} hp", args.Targets[0].Id, -args.Delta, args.Targets[0].Health);
        NotificationProxy.NotifyLoseHealth(args.Targets[0], -args.Delta);
        GameDelays.Delay(GameDelays.Damage);

        try
        {
            Emit(GameEvent.AfterHealthChanged, args);
        }
        catch (TriggerResultException)
        {
        }

    }

    public void LoseMaxHealth(Player source, int magnitude)
    {
        if (source.IsDead) return;
        int result = source.MaxHealth - magnitude;
        bool trigger = false;
        if (source.Health > result)
        {
            source.Health = result;
            trigger = true;
        }
        source.MaxHealth = result;
        CurrentGame.NotificationProxy.NotifyLoseMaxHealth(source, magnitude);
        if (source.MaxHealth <= 0) Emit(GameEvent.GameProcessPlayerIsDead, new GameEventArgs() { Source = null, Targets = new List<Player>() { source } });
        if (trigger && !source.IsDead) CurrentGame.Emit(GameEvent.AfterHealthChanged, new HealthChangedEventArgs() { Source = null, Delta = 0, Targets = new List<Player>() { source } });
    }

    /// <summary>
    /// 处理玩家打出卡牌事件。
    /// </summary>
    /// <param name="source"></param>
    /// <param name="c"></param>
    public void PlayerPlayedCard(Player source, List<Player> targets, ICard c)
    {
        Trace.Assert(c != null);
        try
        {
            var arg = new GameEventArgs
            {
                Source = source,
                Targets = targets,
                Card = c,
                ReadonlyCard = new ReadOnlyCard(c)
            };

            Emit(GameEvent.PlayerPlayedCard, arg);
        }
        catch (TriggerResultException)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 处理玩家打出卡牌
    /// </summary>
    /// <param name="p"></param>
    /// <param name="skill"></param>
    /// <param name="cards"></param>
    /// <param name="targets"></param>
    /// <returns></returns>
    public bool HandleCardPlay(Player p, ISkill skill, List<Card> cards, List<Player> targets)
    {
        Trace.Assert(cards != null);
        var m = new CardsMovement();
        bool status = CommitCardTransform(p, skill, cards, out var result, targets, true);
        if (!status)
        {
            return false;
        }
        if (skill != null)
        {
            var r = result as CompositeCard;
            Trace.Assert(r != null);
            cards.Clear();
            cards.AddRange(r.Subcards);
        }
        m.Cards = new List<Card>(cards);
        m.To = new DeckPlace(null, DeckType.Discard);
        Player isDoingAFavor = p;
        foreach (var checkFavor in m.Cards)
        {
            if (checkFavor.Owner != p)
            {
                Trace.TraceInformation("Acting on behalf of others");
                isDoingAFavor = checkFavor.Owner;
                break;
            }
        }
        result.Type.TagAndNotify(p, targets, result, GameAction.Play);
        var backup = new List<Card>(m.Cards);
        if (isDoingAFavor != p)
        {
            PlayerAboutToDiscardCard(isDoingAFavor, m.Cards, DiscardReason.Play);
            MoveCards(m, false, GameDelays.PlayerAction);
            PlayerLostCard(p, m.Cards);
            PlayerPlayedCard(isDoingAFavor, targets, result);
            PlayerPlayedCard(p, targets, result);
            PlayerDiscardedCard(isDoingAFavor, backup, DiscardReason.Play);
        }
        else
        {
            PlayerAboutToDiscardCard(p, m.Cards, DiscardReason.Play);
            MoveCards(m, false, GameDelays.PlayerAction);
            PlayerLostCard(p, m.Cards);
            PlayerPlayedCard(p, targets, result);
            PlayerDiscardedCard(p, backup, DiscardReason.Play);
        }
        CurrentGame.LastAction = skill;
        return true;
    }

    public void PlayerDiscardedCard(Player p, List<Card> cards, DiscardReason reason)
    {
        try
        {
            var arg = new DiscardCardEventArgs
            {
                Source = p,
                Targets = null,
                Cards = cards,
                Reason = reason
            };
            Emit(GameEvent.CardsEnteredDiscardDeck, arg);
        }
        catch (TriggerResultException)
        {
            throw new NotImplementedException();
        }
    }

    public void PlayerAboutToDiscardCard(Player p, List<Card> cards, DiscardReason reason)
    {
        SyncCardsAll(cards);
        try
        {
            var arg = new DiscardCardEventArgs
            {
                Source = p,
                Targets = null,
                Cards = cards,
                Reason = reason
            };
            Emit(GameEvent.CardsEnteringDiscardDeck, arg, true);
        }
        catch (TriggerResultException)
        {
            throw new NotImplementedException();
        }
    }

    public void PlayerLostCard(Player p, List<Card> cards)
    {
        try
        {
            var arg = new GameEventArgs
            {
                Source = p,
                Targets = null,
                Cards = cards
            };
            Emit(GameEvent.CardsLost, arg);
        }
        catch (TriggerResultException)
        {
            throw new NotImplementedException();
        }
    }

    public void PlayerAcquiredCard(Player p, List<Card> cards)
    {
        try
        {
            var arg = new GameEventArgs
            {
                Source = p,
                Targets = null,
                Cards = cards
            };
            Emit(GameEvent.CardsAcquired, arg);
        }
        catch (TriggerResultException)
        {
            throw new NotImplementedException();
        }
    }

    public void HandleCardDiscard(Player p, List<Card> cards, DiscardReason reason = DiscardReason.Discard)
    {
        cards = new List<Card>(cards);
        var move = new CardsMovement
        {
            Cards = new List<Card>(cards)
        };
        foreach (var c in cards)
        {
            c.Log.Source = p;
            if (reason == DiscardReason.Discard)
                c.Log.GameAction = GameAction.Discard;
            else if (reason == DiscardReason.Play)
                c.Log.GameAction = GameAction.Play;
            else if (reason == DiscardReason.Use)
                c.Log.GameAction = GameAction.Use;
        }
        var backup = new List<Card>(move.Cards);
        move.To = new DeckPlace(null, DeckType.Discard);
        PlayerAboutToDiscardCard(p, move.Cards, reason);
        MoveCards(move, false, GameDelays.Discard);
        if (p != null)
        {
            PlayerLostCard(p, move.Cards);
            PlayerDiscardedCard(p, backup, reason);
        }
    }

    public void HandleCardTransferToHand(Player from, Player to, List<Card> cards, MovementHelper helper = null)
    {
        cards = new List<Card>(cards);
        if (to.IsDead)
        {
            if (cards.Any(cd => cd.Place.DeckType != DeckType.Hand && cd.Place.DeckType != DeckType.Equipment && cd.Place.DeckType != DeckType.DelayedTools))
            {
                var move1 = new CardsMovement
                {
                    Cards = new List<Card>(cards),
                    To = new DeckPlace(null, DeckType.Discard)
                };
                MoveCards(move1);
                PlayerLostCard(from, cards);
            }
            return;
        }
        var move = new CardsMovement
        {
            Cards = new List<Card>(cards),
            To = new DeckPlace(to, DeckType.Hand)
        };
        if (helper != null)
        {
            move.Helper = helper;
        }
        MoveCards(move);
        EnterAtomicContext();
        PlayerLostCard(from, cards);
        PlayerAcquiredCard(to, cards);
        ExitAtomicContext();
    }

    public void HandleCardTransfer(Player from, Player to, DeckType target, List<Card> cards, Hero tag = null)
    {
        cards = new List<Card>(cards);
        if (to.IsDead)
        {
            if (cards.Any(cd => cd.Place.DeckType != DeckType.Hand && cd.Place.DeckType != DeckType.Equipment && cd.Place.DeckType != DeckType.DelayedTools))
            {
                var move1 = new CardsMovement
                {
                    Cards = new List<Card>(cards),
                    To = new DeckPlace(null, DeckType.Discard)
                };
                MoveCards(move1);
                PlayerLostCard(from, cards);
            }
            return;
        }
        var move = new CardsMovement
        {
            Cards = new List<Card>(cards),
            To = new DeckPlace(to, target),
            Helper = new MovementHelper()
        };
        move.Helper.PrivateDeckHeroTag = tag;
        MoveCards(move);
        bool triggerAcquiredCard = target == DeckType.Hand || target == DeckType.Equipment;
        EnterAtomicContext();
        PlayerLostCard(from, cards);
        if (triggerAcquiredCard) PlayerAcquiredCard(to, cards);
        ExitAtomicContext();
    }


    public void ForcePlayerDiscard(Player player, NumberOfCardsToForcePlayerDiscard numberOfCards, bool canDiscardEquipment, bool atOnce = true)
    {
        if (player.IsDead) return;
        Trace.TraceInformation("Player {0} discard.", player);
        int cannotBeDiscarded = 0;
        int numberOfCardsDiscarded = 0;
        while (true)
        {
            int handCardCount = Decks[player, DeckType.Hand].Count; // 玩家手牌数
            int equipCardCount = Decks[player, DeckType.Equipment].Count; // 玩家装备牌数
            int toDiscard = numberOfCards(player, numberOfCardsDiscarded);
            // Have we finished discarding everything?
            // We finish if 
            //      玩家手牌数 小于等于 我们要强制弃掉的数目
            //  或者玩家手牌数 (小于)等于 不可弃的牌的数目（此时装备若可弃，必须弃光）
            if (toDiscard == 0 || (handCardCount <= cannotBeDiscarded && (!canDiscardEquipment || equipCardCount == 0)))
            {
                break;
            }
            Trace.Assert(UiProxies.ContainsKey(player));
            IPlayerProxy proxy = UiProxies[player];
            List<Card> cards;
            cannotBeDiscarded = 0;
            foreach (var c in Decks[player, DeckType.Hand])
            {
                if (!PlayerCanDiscardCard(player, c))
                {
                    cannotBeDiscarded++;
                }
            }
            int totalCards = (canDiscardEquipment ? equipCardCount : 0) + handCardCount;
            int numCanBeDiscarded = totalCards - cannotBeDiscarded;
            //如果玩家无法达到弃牌要求 则 摊牌
            bool status = cannotBeDiscarded == 0 || (numCanBeDiscarded >= toDiscard);
            SyncConfirmationStatus(ref status);
            if (!status)
            {
                SyncImmutableCardsAll(Decks[player, DeckType.Hand]);
                ShowHandCards(player, Decks[player, DeckType.Hand]);
                if (CurrentGame.IsClient)
                {
                    //刷新所有客户端该玩家不可弃掉的牌的数目
                    cannotBeDiscarded = 0;
                    foreach (Card c in Decks[player, DeckType.Hand])
                    {
                        if (!PlayerCanDiscardCard(player, c))
                        {
                            cannotBeDiscarded++;
                        }
                    }
                }
            }
            int minimum;
            int numShouldDiscard = status ? toDiscard : numCanBeDiscarded;
            minimum = !atOnce ? 1 : numShouldDiscard;
            bool answered = false;
            cards = [];
            if (minimum < numCanBeDiscarded)
            {
                var v = new PlayerForceDiscardVerifier(numShouldDiscard, canDiscardEquipment, minimum);
                answered = proxy.AskForCardUsage(new Prompt(Prompt.DiscardPhasePrompt, toDiscard),
                                                 v, out var skill, out cards, out var players);
            }

            if (!answered)
            {
                //玩家没有回应(default)
                Trace.TraceInformation("Invalid answer, choosing for you");
                int cardsDiscarded = 0;
                var chooseFrom = new List<Card>(Decks[player, DeckType.Hand]);
                if (canDiscardEquipment)
                {
                    chooseFrom.AddRange(Decks[player, DeckType.Equipment]);
                }
                foreach (Card c in chooseFrom)
                {
                    if (PlayerCanDiscardCard(player, c))
                    {
                        cards.Add(c);
                        cardsDiscarded++;
                    }
                    if (cardsDiscarded == toDiscard)
                    {
                        SyncCardsAll(cards);
                        break;
                    }
                }
            }
            numberOfCardsDiscarded += cards.Count;
            HandleCardDiscard(player, cards);
        }
    }


    public void InsertBeforeDeal(Player target, List<Card> list, MovementHelper helper = null)
    {
        var move = new CardsMovement
        {
            Cards = new List<Card>(list)
        };
        move.Cards.Reverse();
        move.To = new DeckPlace(null, DeckType.Dealing);
        if (helper != null)
        {
            move.Helper = helper;
        }
        MoveCards(move, true, GameDelays.None);
        if (target != null)
        {
            PlayerLostCard(target, list);
        }
    }

    public void InsertAfterDeal(Player target, List<Card> list, MovementHelper helper = null)
    {
        var move = new CardsMovement
        {
            Cards = new List<Card>(list),
            To = new DeckPlace(null, DeckType.Dealing)
        };
        move.Helper.IsFakedMove = true;
        if (helper != null)
        {
            move.Helper = helper;
        }
        MoveCards(move, false, GameDelays.None);
        if (target != null)
        {
            PlayerLostCard(target, list);
        }
    }

    public void PlaceIntoDiscard(Player target, List<Card> list)
    {
        var move = new CardsMovement
        {
            Cards = new List<Card>(list),
            To = new DeckPlace(null, DeckType.Discard),
            Helper = new MovementHelper()
        };
        MoveCards(move);
        if (target != null)
        {
            PlayerLostCard(target, list);
        }
    }

    public bool PlayerCanDiscardCards(Player p, List<Card> cards)
    {
        foreach (var c in cards)
        {
            if (!PlayerCanDiscardCard(p, c))
            {
                return false;
            }
        }
        return true;
    }

    public bool PlayerCanBeTargeted(Player source, List<Player> targets, ICard card)
    {
        var arg = new GameEventArgs
        {
            Source = source,
            Targets = targets,
            Card = card
        };
        try
        {
            Emit(GameEvent.PlayerCanBeTargeted, arg);
            return true;
        }
        catch (TriggerResultException e)
        {
            if (e.Status == TriggerResult.Fail)
            {
                Trace.TraceInformation("Players cannot be targeted by {0}", card.Type.Name);
                return false;
            }
            else
            {
                Trace.Assert(false);
            }
        }
        return true;
    }

    public bool? PinDianReturnCards(Player from, Player to, out Card c1, out Card c2, ISkill skill, out bool c1Taken, out bool c2Taken)
    {
        NotificationProxy.NotifyLogEvent(new LogEvent("PinDianStart", from, to), new List<Player>() { from, to }, false);
        NotificationProxy.NotifyPinDianStart(from, to, skill);

        GlobalProxy.AskForMultipleCardUsage(new CardUsagePrompt("PinDian"), new PinDianVerifier(), [from, to], out var aSkill, out var aCards, out var aPlayers);
        Card card1, card2;
        if (!aCards.ContainsKey(from) || aCards[from].Count == 0)
        {
            card1 = Decks[from, DeckType.Hand][0];
            SyncImmutableCardAll(card1);
        }
        else
        {
            card1 = aCards[from][0];
        }
        if (!aCards.ContainsKey(to) || aCards[to].Count == 0)
        {
            card2 = Decks[to, DeckType.Hand][0];
            SyncImmutableCardAll(card2);
        }
        else
        {
            card2 = aCards[to][0];
        }
        c1 = card1;
        c2 = card2;
        NotificationProxy.NotifyPinDianEnd(c1, c2);
        NotificationProxy.NotifyLogEvent(new LogEvent("PinDianCard", from, c1), [from, to], false, false);
        NotificationProxy.NotifyLogEvent(new LogEvent("PinDianCard", to, c2), [from, to], false, false);
        NotificationProxy.NotifyLogEvent(new LogEvent("PinDianResult", from, to, new LogEventArg(c1.Rank > c2.Rank ? "Win" : "notWin")), [from, to], false);
        bool? ret = null;
        if (card1.Rank > card2.Rank) ret = true;
        if (card1.Rank < card2.Rank) ret = false;
        var arg = new PinDianCompleteEventArgs
        {
            Source = from,
            Targets = [to],
            Cards = [c1, c2],
            CardsResult = [false, false],
            PinDianResult = ret
        };
        Emit(GameEvent.PinDianComplete, arg);
        c1Taken = arg.CardsResult[0];
        c2Taken = arg.CardsResult[1];
        return ret;
    }

    public bool? PinDian(Player from, Player to, ISkill skill)
    {
        var ret = PinDianReturnCards(from, to, out var card1, out var card2, skill, out var c1, out var c2);
        EnterAtomicContext();
        card1.Log.Source = from;
        card2.Log.Source = to;
        if (!c1) PlaceIntoDiscard(from, [card1]);
        if (!c2) PlaceIntoDiscard(to, [card2]);
        ExitAtomicContext();
        return ret;
    }

    public Card SelectACardFrom(Player from, Player ask, Prompt prompt, string resultdeckname, bool equipExcluded = false, bool delayedToolsExcluded = true, bool noReveal = false)
    {
        var deck = from.HandCards();
        if (!equipExcluded) deck = new List<Card>(deck.Concat(from.Equipments()));
        if (!delayedToolsExcluded) deck = new List<Card>(deck.Concat(from.DelayedTools()));
        if (deck.Count == 0) return null;
        List<DeckPlace> places = [new DeckPlace(from, DeckType.Hand)];
        if (!equipExcluded) places.Add(new DeckPlace(from, DeckType.Equipment));
        if (!delayedToolsExcluded) places.Add(new DeckPlace(from, DeckType.DelayedTools));

        if (!ask.AskForCardChoice(prompt, places, [resultdeckname], [1], new RequireOneCardChoiceVerifier(noReveal), out var answer))
        {
            Trace.TraceInformation("Player {0} Invalid answer", ask);
            answer = [[deck.First()]];
        }
        Card theCard = answer[0][0];
        if (noReveal)
        {
            SyncCard(from, ref theCard);
        }
        else
        {
            SyncCardAll(ref theCard);
        }
        Trace.Assert(answer.Count == 1 && answer[0].Count == 1);
        return theCard;
    }

    public void HideHandCard(Card c)
    {
        if (IsClient && GameClient.SelfId != c.Place.Player.Id && c.Place.DeckType == DeckType.Hand)
        {
            c.Id = -1;
        }
    }

    public void ShowHandCards(Player p, List<Card> cards)
    {
        if (cards.Count == 0) return;
        NotificationProxy.NotifyShowCardsStart(p, cards);
        GlobalProxy.AskForMultipleChoice(new MultipleChoicePrompt("ShowCards", p), [Prompt.YesChoice], AlivePlayers, out var answers);
        NotificationProxy.NotifyShowCardsEnd();
        foreach (var c in cards) CurrentGame.HideHandCard(c);
    }

    public List<Card> PickDefaultCardsFrom(List<DeckPlace> places, int n = 1)
    {
        var cards = new List<Card>();
        foreach (var pl in places)
        {
            cards.AddRange(Decks[pl]);
        }
        var result = new List<Card>();
        while (n-- > 0)
        {
            if (cards.Count == 0) return result;
            var theCard = cards.First();
            cards.Remove(theCard);
            if (theCard.Place.DeckType == DeckType.Hand)
            {
                SyncCard(theCard.Place.Player, ref theCard);
            }
            result.Add(theCard);
        }
        return result;
    }

    public void RegisterSkillCleanup(ISkill skill, DeckType deck)
    {
        cleanupSquad.CalldownCleanupCrew(skill, deck);
    }

    public void RegisterMarkCleanup(ISkill skill, PlayerAttribute attr)
    {
        cleanupSquad.CalldownCleanupCrew(skill, attr);
    }

    public bool IsMainHero(Hero h, Player p) => h == p.Hero;

    public ISkill LastAction { get; set; }

    public List<TurnPhase> PhasesSkipped { get; set; }
}
