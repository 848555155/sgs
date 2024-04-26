using CommunityToolkit.Mvvm.ComponentModel;
using Sanguosha.Core.Cards;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Network;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;
using Sanguosha.Core.Utils;
using Sanguosha.Lobby.Core;
using System.Diagnostics;

namespace Sanguosha.Core.Games;

public class GameOverException : SgsException
{
    public bool IsDraw { get; set; }
    public List<Player> Winners { get; private set; }
    public bool OutOfCards { get; set; }
    public bool EveryoneQuits { get; set; }
    public GameOverException()
        : this(true)
    {
    }
    public GameOverException(bool isDraw)
    {
        IsDraw = isDraw;
        Winners = [];
    }
    public GameOverException(bool isDraw, IEnumerable<Player> winners)
    {
        IsDraw = isDraw;
        Winners = new List<Player>(winners);
    }
}

public class CardsMovement
{
    public List<Card> Cards { get; set; } = [];

    public DeckPlace To { get; set; }

    public MovementHelper Helper { get; set; } = new MovementHelper();

}

public enum DamageElement
{
    None,
    Fire,
    Lightning,
}

public enum DiscardReason
{
    Discard,
    Play,
    Use,
    Judge,
}

public abstract partial class Game : ObservableObject
{
    private class GameAlreadyStartedException : SgsException { }

    public GameSettings Settings { get; set; }

    public List<CardHandler> AvailableCards { get; private set; }

    private readonly List<DelayedTriggerRegistration> triggersToRegister = [];

    public Dictionary<Player, List<Player>> HandCardVisibility { get; set; }= [];

    public Game()
    {
        DyingPlayers = new Stack<Player>();
        Settings = new GameSettings();
        cleanupSquad = new CleanupSquad
        {
            Priority = -1
        };
        PhasesSkipped = [];
        AvailableRoles = [];
        HandCardSwitcher = new HandCardSwitcher();
    }

    internal HandCardSwitcher HandCardSwitcher { get; private set; }

    public void LoadExpansion(Expansion expansion)
    {
        if (Settings.IsGodEnabled)
        {
            OriginalCardSet.AddRange(expansion.CardSet);
        }
        else
        {
            foreach (var card in expansion.CardSet)
            {
                if (card.Type is HeroCardHandler hero)
                {
                    if (hero.Hero.Allegiance == Allegiance.God)
                        continue;
                }
                OriginalCardSet.Add(card);
            }
        }


        if (expansion.TriggerRegistration != null)
        {
            triggersToRegister.AddRange(expansion.TriggerRegistration);
        }
    }

    protected CleanupSquad cleanupSquad;

    public Network.Server GameServer { get; set; }
    public Network.Client GameClient { get; set; }

    public ReplayController ReplayController => GameClient?.ReplayController;

    public void SyncUnknownLocationCard(Player player, Card card)
    {
        if (GameClient != null)
        {
            bool confirmed = true;
            SyncConfirmationStatus(ref confirmed);
            if (confirmed)
            {
                int id = player != null ? player.Id : Players.Count;
                if (id != GameClient.SelfId)
                {
                    return;
                }
                GameClient.Receive();
            }
        }
        else if (GameServer != null)
        {
            bool status = true;
            if (card.Place.DeckType == DeckType.Equipment || card.Place.DeckType == DeckType.DelayedTools)
            {
                status = false;
            }
            SyncConfirmationStatus(ref status);
            if (status)
            {
                card.RevealOnce = true;
                if (player == null) GameServer.SendPacket(GameServer.MaxClients - 1, new CardSync() { Item = CardItem.Parse(card, -1) });
                else GameServer.SendPacket(player.Id, new CardSync() { Item = CardItem.Parse(card, -1) });
            }
        }
    }

    public void SyncUnknownLocationCardAll(Card card)
    {
        foreach (Player p in Players)
        {
            SyncUnknownLocationCard(p, card);
        }
        SyncUnknownLocationCard(null, card);
    }

    public void SyncCard(Player player, ref Card card)
    {
        var cards = new List<Card>() { card };
        SyncCards(player, cards);
        card = cards[0];
    }

    private void SyncCards(Player player, List<Card> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            if (card.Place.DeckType == DeckType.Equipment || card.Place.DeckType == DeckType.DelayedTools)
            {
                continue;
            }
            if (GameClient != null)
            {
                int id = player != null ? player.Id : Players.Count;
                if (id != GameClient.SelfId)
                {
                    return;
                }
                var recv = GameClient.Receive();
                Trace.Assert(recv is Card);
                card = recv as Card;
            }
            else if (GameServer != null)
            {
                card.RevealOnce = true;
                if (player == null) GameServer.SendPacket(GameServer.MaxClients - 1, new CardSync() { Item = CardItem.Parse(card, -1) });
                else GameServer.SendPacket(player.Id, new CardSync() { Item = CardItem.Parse(card, player.Id) });
            }
            cards[i] = card;
        }
    }

    public void SyncCardAll(ref Card card)
    {
        foreach (var p in Players)
        {
            SyncCard(p, ref card);
        }
        SyncCard(null, ref card);
    }

    private void SyncCardsAll(List<Card> cards)
    {
        foreach (var p in Players)
        {
            SyncCards(p, cards);
        }
        SyncCards(null, cards);
    }

    public void SyncImmutableCard(Player player, Card card)
    {
        SyncImmutableCards(player, [card]);
    }

    public void SyncImmutableCards(Player player, List<Card> cards)
    {
        foreach (var card in cards)
        {
            if (card.Place.DeckType == DeckType.Equipment || card.Place.DeckType == DeckType.DelayedTools)
            {
                return;
            }
            if (GameClient != null)
            {
                int id = player != null ? player.Id : Players.Count;
                if (id != GameClient.SelfId)
                {
                    return;
                }
                GameClient.Receive();
            }
            else if (GameServer != null)
            {
                card.RevealOnce = true;
                if (player == null) GameServer.SendPacket(GameServer.MaxClients - 1, new CardSync() { Item = CardItem.Parse(card, -1) });
                else GameServer.SendPacket(player.Id, new CardSync() { Item = CardItem.Parse(card, player.Id) });
            }
        }
    }

    public void SyncImmutableCardAll(Card card)
    {
        foreach (var p in Players)
        {
            SyncImmutableCard(p, card);
        }
        SyncImmutableCard(null, card);
    }

    public void SyncImmutableCardsAll(List<Card> cards)
    {
        foreach (var p in Players)
        {
            SyncImmutableCards(p, cards);
        }
        SyncImmutableCards(null, cards);
    }

    public void SyncConfirmationStatus(ref bool confirmed)
    {
        if (GameServer != null)
        {
            for (int i = 0; i < GameServer.MaxClients; i++)
            {
                GameServer.SendPacket(i, new StatusSync() { Status = confirmed ? 1 : 0 });
            }
        }
        else if (GameClient != null)
        {
            object o = GameClient.Receive();
            Trace.Assert(o is int);
            if ((int)o == 1)
            {
                confirmed = true;
            }
            else
            {
                confirmed = false;
            }
        }
    }

    public void SyncSeed(ref int value)
    {
        if (GameServer != null)
        {
            for (int i = 0; i < GameServer.MaxClients; i++)
            {
                try
                {
                    GameServer.SendPacket(i, new SeedSync(value));
                }
                catch (Exception)
                {
                }
            }
        }
        else
        {
            GameClient?.Receive();
        }
    }

    public bool IsClient => GameClient != null;

    public void Abort()
    {
        if (MainThread == null) return;
        Trace.Assert(MainThread != Thread.CurrentThread);
        GlobalProxy?.Abort();
#pragma warning disable SYSLIB0006 // 类型或成员已过时
        MainThread.Abort();
#pragma warning restore SYSLIB0006 // 类型或成员已过时
        MainThread = null;
    }

    public Thread MainThread { get; private set; }

    public virtual void Run()
    {
        MainThread = Thread.CurrentThread;
        if (!games.ContainsKey(Thread.CurrentThread))
        {
            /*throw new GameAlreadyStartedException();
        }
        else
        {*/
            if (!IsClient)
            {
                RegisterCurrentThread();
            }
            // games.Add(Thread.CurrentThread, this);
        }
        if (GameServer != null)
        {
            try
            {
                GameServer.Start();
            }
            catch (Exception)
            {
                Trace.Assert(false);
                return;
            }
        }

        foreach (var expansionName in Settings.PackagesEnabled)
        {
            LoadExpansion(GameEngine.Expansions[expansionName]);
        }

        AvailableCards = [];
        foreach (Card c in OriginalCardSet)
        {
            bool typeCheck = false;
            foreach (var type in AvailableCards)
            {
                if (type.GetType().Name.Equals(c.Type.GetType().Name))
                {
                    typeCheck = true;
                    break;
                }
            }
            if (!typeCheck)
            {
                AvailableCards.Add(c.Type);
            }
        }

        foreach (var card in OriginalCardSet)
        {
            //you are client. everything is unknown
            if (IsClient && !IsPanorama)
            {
                unknownCard = new Card
                {
                    Id = Card.UnknownCardId,
                    Rank = 0,
                    Suit = SuitType.None
                };
                if (card.Type is HeroCardHandler heroCardHandler)
                {
                    unknownCard.Type = new UnknownHeroCardHandler();
                    unknownCard.Id = heroCardHandler.Hero.IsSpecialHero ? Card.UnknownSPHeroId : Card.UnknownHeroId;
                }
                else if (card.Type is RoleCardHandler)
                {
                    unknownCard.Id = Card.UnknownRoleId;
                    unknownCard.Type = card.Type;
                }
                else
                {
                    unknownCard.Type = new UnknownCardHandler();
                }
            }
            //you are server.
            else
            {
                unknownCard = new Card();
                unknownCard.CopyFrom(card);
                if (unknownCard.Type is CardHandler)
                {
                    unknownCard.Type = (CardHandler)unknownCard.Type.Clone();
                }
            }
            CardSet.Add(unknownCard);
        }

        foreach (var trig in triggersToRegister)
        {
            RegisterTrigger(trig.key, trig.trigger);
        }

        InitTriggers();
        try
        {
            Emit(GameEvent.GameStart, new GameEventArgs());
        }
        catch (GameOverException e)
        {
            HandleGameOver(e.IsDraw, e.Winners);
            lock (games)
            {
                var keys = new List<Thread>(from t in games.Keys where games[t] == this select t);
                foreach (var t in keys)
                {
                    games.Remove(t);
                }
            }

            NotificationProxy = null;
            UiProxies = null;
        }
#if !DEBUG
        catch (Exception e)
        {
            lock (games)
            {
                var keys = new List<Thread>(from t in games.Keys where games[t] == this select t);
                foreach (var t in keys)
                {
                    games.Remove(t);
                }
            }
            this.NotificationProxy = null;

            while (e.InnerException != null)
            {
                e = e.InnerException;
            }

            if (!(e is ThreadAbortException))
            {
                Trace.TraceError(e.StackTrace);
                Trace.Assert(false, e.StackTrace);

                var crashReport = new StreamWriter(FileRotator.CreateFile("./Crash", "crash", ".dmp", 1000));
                crashReport.WriteLine(e);
                crashReport.Close();
            }
        }
#endif
        MainThread = null;

        if (GameServer != null)
        {
            GameServer.Stop();
        }
        else
        {
            GameClient?.Stop();
        }
        Trace.TraceInformation("Game exited normally");
    }

    private void TallyGameResult(List<Player> winners)
    {
        if (GameServer == null) return;
        foreach (Player p in Players)
        {
            int idx = Players.IndexOf(p);
            var account = Settings.Accounts[idx];
            account.TotalGames++;
            if (GameServer.IsDisconnected(idx))
            {
                if (!account.IsDead)
                {
                    account.Quits++;
                    continue;
                }
            }
            if (winners.Contains(p))
            {
                account.Wins++;
                account.Experience += 5;
                if (p.Role == Role.Defector && Players.Count > 3) Settings.Accounts[idx].Experience += 50;
            }
            else
            {
                account.Losses++;
                account.Experience -= 1;
            }
        }
    }

    protected virtual void HandleGameOver(bool isDraw, List<Player> winners)
    {
        int seed = Seed;
        TallyGameResult(new List<Player>(winners));
        SyncSeed(ref seed);
        NotificationProxy.NotifyGameOver(false, winners.ToList());
    }

    /// <summary>
    /// Initialize triggers at game start time.
    /// </summary>
    protected abstract void InitTriggers();

    /// <summary>
    /// Speed up current game access for client process
    /// </summary>
    public static Game CurrentGameOverride { get; set; }

    public static Game CurrentGame
    {
        get
        {
            lock (games)
            {
                return CurrentGameOverride ?? (games.ContainsKey(Thread.CurrentThread) ? games[Thread.CurrentThread] : null);
            }
        }
    }

    /// <summary>
    /// Mapping from a thread to the game it hosts.
    /// </summary>
    private static readonly Dictionary<Thread, Game> games = [];

    public void RegisterCurrentThread()
    {
        lock (games)
        {
            games.Remove(Thread.CurrentThread);
            games.Add(Thread.CurrentThread, this);
        }
    }

    public void UnregisterCurrentThread()
    {
        lock (games)
        {
            games.Remove(Thread.CurrentThread);
        }
    }

    /// <summary>
    /// All eligible card copied verbatim from the game engine. All cards in this set are known cards.
    /// </summary>
    public List<Card> OriginalCardSet { get; } = [];

    /// <summary>
    /// Current state of all cards used in the game. Some of the cards can be unknown in the client side.
    /// The collection is empty before Run() is called.
    /// </summary>
    public List<Card> CardSet { get; } = [];

    private Card unknownCard;
    private readonly Dictionary<GameEvent, List<Trigger>> triggers = [];

    public void RegisterTrigger(GameEvent gameEvent, Trigger trigger)
    {
        if (trigger == null)
        {
            return;
        }
        if (!triggers.TryGetValue(gameEvent, out List<Trigger> value))
        {
            triggers[gameEvent] = value = [];
        }

        value.Add(trigger);
    }

    public void UnregisterTrigger(GameEvent gameEvent, Trigger trigger)
    {
        if (trigger == null)
        {
            return;
        }
        if (triggers.TryGetValue(gameEvent, out List<Trigger> value))
        {
            Trace.Assert(value.Contains(trigger));
            value.Remove(trigger);
        }
    }

    private class TriggerComparer(Game game) : IComparer<TriggerWithParam>
    {
        private readonly Game game = game;
        public int Compare(TriggerWithParam a, TriggerWithParam b)
        {
            int result2 = a.trigger.Type.CompareTo(b.trigger.Type);
            if (result2 != 0)
            {
                return -result2;
            }
            int result = a.trigger.Priority.CompareTo(b.trigger.Priority);
            if (result != 0)
            {
                return -result;
            }
            Player p = game.CurrentPlayer;
            int result3 = 0;
            if (a.trigger.Owner != b.trigger.Owner)
            {
                do
                {
                    if (p == a.trigger.Owner)
                    {
                        result3 = -1;
                        break;
                    }
                    if (p == b.trigger.Owner)
                    {
                        result3 = 1;
                        break;
                    }
                    p = game.NextPlayer(p);
                } while (p != game.CurrentPlayer);

            }
            return result3;
        }
    }

    private void EmitTriggers(List<TriggerWithParam> triggers)
    {
        var result = triggers.OrderBy((a) => { return a; }, new TriggerComparer(this));
        foreach (var t in result)
        {
            if (this.triggers[t.gameEvent].Contains(t.trigger) && (t.trigger.Owner == null || !t.trigger.Owner.IsDead))
            {
                t.trigger.Run(t.gameEvent, t.args);
            }
        }
    }


    /// <summary>
    /// Emit a game event to invoke associated triggers.
    /// </summary>
    /// <param name="gameEvent">Game event to be emitted.</param>
    /// <param name="eventParam">Additional helper for triggers listening on this game event.</param>
    public void Emit(GameEvent gameEvent, GameEventArgs eventParam, bool beforeMove = false)
    {
        if (!triggers.ContainsKey(gameEvent)) return;
        var additionalTriggers = new List<Trigger>(triggers[gameEvent]);
        var oldTriggers = new List<Trigger>(triggers[gameEvent]);
        while (true)
        {
            if (additionalTriggers == null || additionalTriggers.Count == 0) return;
            var triggersToRun = new List<TriggerWithParam>();
            foreach (var t in additionalTriggers)
            {
                if (t.Enabled)
                {
                    triggersToRun.Add(new TriggerWithParam() { gameEvent = gameEvent, trigger = t, args = eventParam });
                }
            }
            if (!atomic)
            {
                EmitTriggers(triggersToRun);
                additionalTriggers = new List<Trigger>(triggers[gameEvent].Except(oldTriggers));
                oldTriggers = new List<Trigger>(triggers[gameEvent]);
                continue;
            }
            else
            {
                var triggerPlace = atomicTriggers;
                if (beforeMove)
                {
                    triggerPlace = atomicTriggersBeforeMove;
                }
                triggerPlace.AddRange(triggersToRun);
            }
            break;
        }
    }

    public Dictionary<Player, IPlayerProxy> UiProxies { get; set; } = [];

    public IGlobalUiProxy GlobalProxy { get; set; }

    private bool isUiDetached;
    public bool IsUiDetached
    {
        get { return isUiDetached; }
        set
        {
            if (isUiDetached == value) return;
            isUiDetached = value;
            UpdateUiAttachStatus();
        }
    }
    public void UpdateUiAttachStatus()
    {
        if (ReplayController != null) return;
        if (NotificationProxy == null) return;
        if (!isUiDetached)
        {
            foreach (var pair in UiProxies)
            {
                if (pair.Value is ClientNetworkProxy proxy)
                    proxy.IsUiDetached = false;
            }
            NotificationProxy.NotifyUiAttached();
        }
        else
        {
            foreach (var pair in UiProxies)
            {
                if (pair.Value is ClientNetworkProxy proxy)
                    proxy.IsUiDetached = true;
            }
            NotificationProxy.NotifyUiDetached();
        }
    }

    public INotificationProxy NotificationProxy { get; set; }


    /// <summary>
    /// Card usage handler for a given card's type name.
    /// </summary>
    public Dictionary<string, CardHandler> CardHandlers { get; set; } = [];

    public DeckContainer Decks { get; set; } = new();

    public List<Player> Players { get; set; } = [];

    public List<Player> AlivePlayers
    {
        get
        {
            var list = new List<Player>();
            foreach (Player p in Players)
            {
                if (!p.IsDead)
                {
                    list.Add(p);
                }
            }
            SortByOrderOfComputation(currentPlayer, list);
            return list;
        }
    }

    public int NumberOfAliveAllegiances
    {
        get
        {
            var ret =
            (from p in AlivePlayers select p.Allegiance).Distinct().Count();
            return ret;
        }
    }

    private bool atomic = false;
    private int atomicLevel = 0;

    private struct TriggerWithParam
    {
        public GameEvent gameEvent;
        public Trigger trigger;
        public GameEventArgs args;
    }

    private List<CardsMovement> atomicMoves;
    private List<TriggerWithParam> atomicTriggers;
    private List<TriggerWithParam> atomicTriggersBeforeMove;

    public void EnterAtomicContext()
    {
        atomic = true;
        if (atomicLevel == 0)
        {
            atomicMoves = [];
            atomicTriggers = [];
            atomicTriggersBeforeMove = [];
        }
        atomicLevel++;
    }

    public void ExitAtomicContext()
    {
        atomicLevel--;
        if (atomicLevel > 0)
        {
            return;
        }
        var moves = atomicMoves;
        var triggers = atomicTriggers;
        var btriggers = atomicTriggersBeforeMove;
        atomic = false;
        EmitTriggers(btriggers);
        MoveCards(moves);
        EmitTriggers(triggers);
        GameDelays.Delay(GameDelays.CardTransfer);
    }

    private void AddAtomicMoves(List<CardsMovement> moves)
    {
        int i = 0;
        foreach (var m in moves)
        {
            var newM = new CardsMovement
            {
                Cards = m.Cards,
                To = new DeckPlace(m.To.Player, m.To.DeckType),
                Helper = new MovementHelper(m.Helper)
            };
            atomicMoves.Add(newM);
            i++;
        }
    }

    ///<remarks>
    ///YOU ARE NOT ALLOWED TO TRIGGER ANY EVENT ANYWHERE INSIDE THIS FUNCTION!!!!!
    ///你不可以在这个函数中触发任何事件!!!!!
    ///</remarks>
    public void MoveCards(List<CardsMovement> moves, List<bool> insertBefore = null, int delay = GameDelays.CardTransfer)
    {
        if (atomic)
        {
            AddAtomicMoves(moves);
            return;
        }
        foreach (CardsMovement move in moves)
        {
            var cards = new List<Card>(move.Cards);
            foreach (var card in cards)
            {
                if (move.To.Player == null && move.To.DeckType == DeckType.Discard)
                {
                    SyncImmutableCardAll(card);
                }
                if (card.Place.Player != null && move.To.Player != null && move.To.DeckType == DeckType.Hand)
                {
                    SyncImmutableCard(move.To.Player, card);
                }
            }
        }

        NotificationProxy.NotifyCardMovement(moves);

        int i = 0;
        foreach (CardsMovement move in moves)
        {
            var cards = new List<Card>(move.Cards);
            // Update card's deck mapping
            foreach (Card card in cards)
            {
                Trace.TraceInformation("Card {0}{1}{2} from {3}{4} to {5}{6}.", card.Suit, card.Rank, card.Type.Name.ToString(),
                    card.Place.Player == null ? "G" : card.Place.Player.Id.ToString(), card.Place.DeckType.Name, move.To.Player == null ? "G" : move.To.Player.Id.ToString(), move.To.DeckType.Name);
                card.Log = new ActionLog();
                // unregister triggers for equipment 例如武圣将红色的雌雄双绝（假设有这么一个雌雄双绝）打出杀女性角色，不能发动雌雄
                if (card.Place.Player != null && card.Place.DeckType == DeckType.Equipment && CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Equipment))
                {
                    Equipment e = card.Type as Equipment;
                    e.UnregisterTriggers(card.Place.Player);
                }
                if (move.To.Player != null && move.To.DeckType == DeckType.Equipment && CardCategoryManager.IsCardCategory(card.Type.Category, CardCategory.Equipment))
                {
                    Equipment e = card.Type as Equipment;
                    e.RegisterTriggers(move.To.Player);
                }
                Decks[card.Place].Remove(card);
                int isLastHandCard = (card.Place.DeckType == DeckType.Hand && Decks[card.Place].Count == 0) ? 1 : 0;
                if (insertBefore != null && insertBefore[i])
                {
                    Decks[move.To].Insert(0, card);
                }
                else
                {
                    Decks[move.To].Add(card);
                }
                card.HistoryPlace2 = card.HistoryPlace1;
                card.HistoryPlace1 = card.Place;
                card.Place = move.To;
                //reset card type if entering hand or discard
                if (!IsClient && (move.To.DeckType == DeckType.Dealing || move.To.DeckType == DeckType.Discard || move.To.DeckType == DeckType.Hand))
                {
                    _ResetCard(card);
                    if (card.Attributes != null) card.Attributes.Clear();
                }

                //reset color if entering delayedtools
                if (move.To.DeckType == DeckType.DelayedTools)
                {
                    card.Suit = GameEngine.CardSet[card.Id].Suit;
                }

                //reset card type if entering hand or discard
                if (IsClient && (move.To.DeckType == DeckType.Dealing || move.To.DeckType == DeckType.Discard || move.To.DeckType == DeckType.Hand))
                {
                    card.Log = new ActionLog();
                    _ResetCard(card);
                    card.Attributes?.Clear();
                }
                card[Card.IsLastHandCard] = isLastHandCard;

                if (IsClient && !IsPanorama && move.To.DeckType == DeckType.Hand && GameClient.SelfId != move.To.Player.Id)
                {
                    card.Id = -1;
                }
                if (move.To.Player != null)
                {
                    _FilterCard(move.To.Player, card);
                }
            }
            i++;
        }
        if (!atomic) GameDelays.Delay(delay);
    }

    public bool IsPanorama { get; set; }

    public void MoveCards(CardsMovement move, bool insertBefore = false, int delay = GameDelays.CardTransfer)
    {
        if (move.Cards.Count == 0) return;
        List<CardsMovement> moves = [move];
        MoveCards(moves, [insertBefore], delay);
    }

    public Card PeekCard(int i)
    {
        var drawDeck = Decks[DeckType.Dealing];
        if (i >= drawDeck.Count)
        {
            Emit(GameEvent.Shuffle, new GameEventArgs());
        }
        if (drawDeck.Count == 0)
        {
            throw new GameOverException() { OutOfCards = true };
        }
        return drawDeck[i];
    }

    public Card DrawCard()
    {
        var drawDeck = Decks[DeckType.Dealing];
        if (drawDeck.Count == 0)
        {
            Emit(GameEvent.Shuffle, new GameEventArgs());
        }
        if (drawDeck.Count == 0)
        {
            throw new GameOverException() { OutOfCards = true };
        }
        Card card = drawDeck.First();
        drawDeck.RemoveAt(0);
        return card;
    }

    public void DrawCards(Player player, int num)
    {
        if (player.IsDead || num <= 0) return;
        List<Card> cardsDrawn = [];

        for (int i = 0; i < num; i++)
        {
            SyncImmutableCard(player, PeekCard(0));
            cardsDrawn.Add(DrawCard());
        }
        var move = new CardsMovement
        {
            Cards = cardsDrawn,
            To = new DeckPlace(player, DeckType.Hand)
        };
        MoveCards(move, false, GameDelays.Draw);
        PlayerAcquiredCard(player, cardsDrawn);
    }

    private Player currentPlayer;

    public Player CurrentPlayer
    {
        get { return currentPlayer; }
        set
        {
            Trace.Assert(value != null);
            if (currentPlayer != null)
            {
                var temp = new Dictionary<PlayerAttribute, int>(currentPlayer.Attributes);
                foreach (var pair in temp)
                {
                    if (pair.Key.AutoReset)
                    {
                        currentPlayer[pair.Key] = 0;
                    }
                }
            }
            SetProperty(ref currentPlayer, value);
        }
    }

    //回合结束后，直到下个角色回合开始时这段时间里，不属于任何角色的回合

    public Player PhasesOwner
    {
        get 
        { 
            if (CurrentPhase == TurnPhase.Inactive) 
                return null; 
            return currentPlayer.IsDead ? null : currentPlayer; 
        }
    }

    [ObservableProperty]
    private TurnPhase currentPhase;

    public int CurrentPhaseEventIndex { get; set; }

    public static readonly Dictionary<TurnPhase, GameEvent>[] PhaseEvents =
                     [ GameEvent.PhaseBeginEvents, GameEvent.PhaseProceedEvents,
                       GameEvent.PhaseEndEvents, GameEvent.PhaseOutEvents ];

    /// <summary>
    /// Get player next to the a player in counter-clock seat map. (must be alive)
    /// </summary>
    /// <param name="p">Player</param>
    /// <returns></returns>
    public virtual Player NextAlivePlayer(Player p)
    {
        p = NextPlayer(p);
        while (p.IsDead)
        {
            p = NextPlayer(p);
        }
        return p;
    }

    /// <summary>
    /// Get player next to the a player in counter-clock seat map. (must be alive)
    /// </summary>
    /// <param name="p">Player</param>
    /// <returns></returns>
    public virtual Player NextPlayer(Player p)
    {
        int numPlayers = Players.Count;
        int i;
        for (i = 0; i < numPlayers; i++)
        {
            if (Players[i] == p)
            {
                break;
            }
        }

        // The next player to the last player is the first player.
        if (i == numPlayers - 1)
        {
            return Players[0];
        }
        else if (i >= numPlayers)
        {
            Trace.Assert(false);
            return null;
        }
        else
        {
            return Players[i + 1];
        }
    }

    public virtual int OrderOf(Player withRespectTo, Player target)
    {
        int numPlayers = Players.Count;
        int i;
        for (i = 0; i < numPlayers; i++)
        {
            if (Players[i] == withRespectTo)
            {
                break;
            }
        }

        // The next player to the last player is the first player.
        int order = 0;
        while (Players[i] != target)
        {
            if (i == numPlayers - 1)
            {
                i = 0;
            }
            else
            {
                i++;
            }
            order++;
        }
        Trace.Assert(order < numPlayers);
        return order;
    }

    public virtual void SortByOrderOfComputation(Player withRespectTo, List<Player> players)
    {
        if (withRespectTo == null) return;
        players.Sort((a, b) => OrderOf(withRespectTo, a).CompareTo(OrderOf(withRespectTo, b)));
    }


    /// <summary>
    /// Get player previous to the a player in counter-clock seat map. (must be alive)
    /// </summary>
    /// <param name="p">Player</param>
    /// <returns></returns>
    public virtual Player PreviousAlivePlayer(Player p)
    {
        p = PreviousPlayer(p);
        while (p.IsDead)
        {
            p = PreviousPlayer(p);
        }
        return p;
    }

    /// <summary>
    /// Get player previous to a player in counter-clock seat map
    /// </summary>
    /// <param name="p">Player</param>
    /// <returns></returns>
    public virtual Player PreviousPlayer(Player p)
    {
        int numPlayers = Players.Count;
        int i;
        for (i = 0; i < numPlayers; i++)
        {
            if (Players[i] == p)
            {
                break;
            }
        }

        // The previous player to the first player is the last player
        if (i == 0)
        {
            return Players[numPlayers - 1];
        }
        else if (i >= numPlayers)
        {
            return null;
        }
        else
        {
            return Players[i - 1];
        }
    }

    public virtual int DistanceTo(Player from, Player to)
    {
        int distRight = from[Player.RangeMinus], distLeft = from[Player.RangeMinus];
        Player p = from;
        while (p != to)
        {
            p = NextAlivePlayer(p);
            distRight++;
        }
        distRight += to[Player.RangePlus];
        p = from;
        while (p != to)
        {
            p = PreviousAlivePlayer(p);
            distLeft++;
        }
        distLeft += to[Player.RangePlus];

        var args = new AdjustmentEventArgs
        {
            Source = from,
            Targets = [to],
            AdjustmentAmount = 0
        };
        Emit(GameEvent.PlayerDistanceAdjustment, args);
        distLeft += args.AdjustmentAmount;
        distRight += args.AdjustmentAmount;

        var ret = distRight > distLeft ? distLeft : distRight;
        if (ret < 1)
        {
            ret = from == to ? 0 : 1;
        }

        // the minimum distance is 1 between any two. if distance is 0, it means this is the distance to heself or herself.
        // some skills took ignore distance, it means the distance between them is aways 1.
        args = new AdjustmentEventArgs
        {
            Source = from,
            Targets = [to],
            AdjustmentAmount = ret
        };
        Emit(GameEvent.PlayerDistanceOverride, args);
        ret = args.AdjustmentAmount;

        return ret;
    }

    public void _FilterCard(Player p, Card card)
    {
        Emit(GameEvent.EnforcedCardTransform, new GameEventArgs
        {
            Source = p,
            Card = card
        });
    }

    private void _ResetCard(Card card)
    {
        if (card.Id > 0 && !IsPanorama)
        {
            card.Type = (CardHandler)GameEngine.CardSet[card.Id].Type.Clone();
            card.Suit = GameEngine.CardSet[card.Id].Suit;
            card.Rank = GameEngine.CardSet[card.Id].Rank;
        }
    }

    private void _ResetCards(Player p)
    {
        foreach (var card in Decks[p, DeckType.Hand])
        {
            if (card.Id > 0)
            {
                _ResetCard(card);
                _FilterCard(p, card);
            }
        }
    }

    public void NotifyIntermediateJudgeResults(Player player, ActionLog log, JudgementResultSucceed intermDel)
    {
        Trace.Assert(Decks[player, DeckType.JudgeResult].Count > 0);
        Card c = Decks[player, DeckType.JudgeResult][0];
        bool? succeed = null;
        if (intermDel != null)
        {
            succeed = intermDel(c);
        }
        NotificationProxy.NotifyJudge(player, c, log, succeed, false);
    }

    public bool CommitCardTransform(Player p, ISkill skill, List<Card> cards, out ICard result, List<Player> targets, bool isPlay)
    {
        if (skill != null)
        {
            CardTransformSkill s = (CardTransformSkill)skill;
            if (!s.Transform(cards, null, out var card, targets))
            {
                result = null;
                return false;
            }
            result = card;
        }
        else
        {
            result = cards[0];
        }
        return true;
    }

    public bool PlayerCanDiscardCard(Player p, Card c)
    {
        if (c.Type is Equipment equipment && equipment.InUse) return false;
        var arg = new GameEventArgs
        {
            Source = p,
            Card = c
        };
        try
        {
            Emit(GameEvent.PlayerCanDiscardCard, arg);
        }
        catch (TriggerResultException e)
        {
            if (e.Status == TriggerResult.Fail)
            {
                Trace.TraceInformation("Player {0} cannot discard {1}", p.Id, c.Type.Name);
                return false;
            }
            else
            {
                Trace.Assert(false);
            }
        }
        return true;
    }

    public bool PlayerCanUseCard(Player p, ICard c)
    {
        GameEventArgs arg = new GameEventArgs();
        arg.Source = p;
        arg.Card = c;
        try
        {
            Emit(GameEvent.PlayerCanUseCard, arg);
        }
        catch (TriggerResultException e)
        {
            if (e.Status == TriggerResult.Fail)
            {
                Trace.TraceInformation("Player {0} cannot use {1}", p.Id, c.Type.Name);
                return false;
            }
            else
            {
                Trace.Assert(false);
            }
        }
        return true;
    }

    public bool PlayerCanPlayCard(Player p, ICard c)
    {
        var arg = new GameEventArgs
        {
            Source = p,
            Card = c
        };
        try
        {
            Emit(GameEvent.PlayerCanPlayCard, arg);
        }
        catch (TriggerResultException e)
        {
            if (e.Status == TriggerResult.Fail)
            {
                Trace.TraceInformation("Player {0} cannot play {1}", p.Id, c.Type.Name);
                return false;
            }
            else
            {
                Trace.Assert(false);
            }
        }
        return true;
    }

    public Stack<Player> DyingPlayers { get; set; }

    public class PlayerHpChanged : Trigger
    {
        public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
        {
            Trace.Assert(eventArgs.Targets.Count == 1);
            Player target = eventArgs.Targets[0];
            if (target.Health > 0 || (eventArgs as HealthChangedEventArgs).Delta > 0) return;
            if (target[Player.SkipDeathComputation] != 0) { target[Player.SkipDeathComputation] = 0; return; }

            CurrentGame.DyingPlayers.Push(target);
            target[Player.IsDying] = 1;
            Trace.TraceInformation("Player {0} dying", target.Id);
            GameEventArgs args = new GameEventArgs
            {
                Source = eventArgs.Source,
                Targets = [target]
            };
            try
            {
                CurrentGame.Emit(GameEvent.PlayerIsAboutToDie, args);
            }
            catch (TriggerResultException)
            {
            }
            if (target.Health <= 0)
            {
                try
                {
                    CurrentGame.Emit(GameEvent.PlayerDying, args);
                }
                catch (TriggerResultException)
                {
                }
            }
            Player temp = CurrentGame.DyingPlayers.Pop();
            Trace.Assert(temp == target);
            target[Player.IsDying] = 0;
            if (target.IsDead || target.Health > 0) return;
            if (target[Player.SkipDeathComputation] != 0) { target[Player.SkipDeathComputation] = 0; return; }
            Trace.TraceInformation("Player {0} dead", target.Id);
            CurrentGame.Emit(GameEvent.GameProcessPlayerIsDead, eventArgs);
        }
    }

    public delegate int NumberOfCardsToForcePlayerDiscard(Player p, int discarded);

    private class PlayerForceDiscardVerifier(int n, bool equip, int min) : ICardUsageVerifier
    {
        public UiHelper Helper => new();

        public VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
        {
            if (skill != null)
            {
                return VerifierResult.Fail;
            }
            if (players != null && players.Count > 0)
            {
                return VerifierResult.Fail;
            }
            if (cards == null || cards.Count == 0)
            {
                return VerifierResult.Partial;
            }
            foreach (Card c in cards)
            {
                if (!CurrentGame.PlayerCanDiscardCard(source, c))
                {
                    return VerifierResult.Fail;
                }
                if (!canDiscardEquip && c.Place.DeckType != DeckType.Hand)
                {
                    return VerifierResult.Fail;
                }
            }
            if (cards.Count < minimun)
            {
                return VerifierResult.Partial;
            }
            if (cards.Count > toDiscard)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public IList<CardHandler> AcceptableCardTypes => null;

        public VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
        {
            return FastVerify(source, skill, cards, players);
        }

        private readonly int toDiscard = n;
        private readonly bool canDiscardEquip = equip;
        private readonly int minimun = min;
    }


    public class PinDianVerifier : ICardUsageVerifier
    {
        public VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
        {
            if (skill != null || (players != null && players.Count > 0))
            {
                return VerifierResult.Fail;
            }
            if (cards == null || cards.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (cards.Count > 1)
            {
                return VerifierResult.Fail;
            }
            if (cards[0].Place.DeckType != DeckType.Hand)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public IList<CardHandler> AcceptableCardTypes => null;

        public VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
        {
            return FastVerify(source, skill, cards, players);
        }

        public UiHelper Helper => new();
    }

    public void MoveHandCard(Player player, int from, int to)
    {
        if (IsClient && player.Id == GameClient.SelfId)
        {
            GameClient.MoveHandCard(from, to);
        }
    }

    public void DoPlayer(Player p)
    {
        var phase = CurrentPhase;
        var index = CurrentPhaseEventIndex;
        var player = CurrentPlayer;
        CurrentPhaseEventIndex = 0;
        CurrentPhase = TurnPhase.BeforeStart;
        Emit(GameEvent.DoPlayer, new GameEventArgs() { Source = p });
        CurrentPhase = phase;
        CurrentPhaseEventIndex = index;
        CurrentPlayer = player;
    }

    public List<Role> AvailableRoles { get; set; }
    public int Seed { get; set; }
    public Random RandomGenerator { get; set; }

    public void Shuffle(IList<Card> list)
    {
        Random rng = RandomGenerator;
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public ClientNetworkProxy ActiveClientProxy { get; set; }
}

