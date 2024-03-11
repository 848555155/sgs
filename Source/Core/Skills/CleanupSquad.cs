using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Players;
using Sanguosha.Core.Triggers;
using System.Diagnostics;

namespace Sanguosha.Core.Skills;

public class CleanupSquad : Trigger
{
    private readonly Dictionary<ISkill, List<DeckType>> deckCleanup = [];
    private readonly Dictionary<ISkill, List<PlayerAttribute>> markCleanup = [];

    public void CalldownCleanupCrew(ISkill skill, DeckType deck)
    {
        if (!deckCleanup.ContainsKey(skill)) deckCleanup.Add(skill, new List<DeckType>());
        deckCleanup[skill].Add(deck);
    }

    public void CalldownCleanupCrew(ISkill skill, PlayerAttribute attr)
    {
        if (!markCleanup.ContainsKey(skill)) markCleanup.Add(skill, new List<PlayerAttribute>());
        markCleanup[skill].Add(attr);
    }

    public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
    {
        if (gameEvent == GameEvent.PlayerSkillSetChanged)
        {
            SkillSetChangedEventArgs args = eventArgs as SkillSetChangedEventArgs;
            Trace.Assert(args != null);
            if (!args.IsLosingSkill) return;
            foreach (var sk in args.Skills)
            {
                if (deckCleanup.TryGetValue(sk, out List<DeckType> decks))
                {
                    foreach (var deck in decks)
                    {
                        if (Game.CurrentGame.Decks[args.Source, deck].Count > 0)
                        {
                            var toDiscard = new List<Card>(Game.CurrentGame.Decks[args.Source, deck]);
                            if (toDiscard.Any(c => c.Type.IsCardCategory(CardCategory.Hero)))
                            {
                                //HuaShenDeck
                                if (Game.CurrentGame.IsClient)
                                {
                                    foreach (var hc in toDiscard)
                                    {
                                        hc.Id = Card.UnknownHeroId;
                                        hc.Type = new UnknownHeroCardHandler();
                                    }
                                }
                                var move = new CardsMovement
                                {
                                    Cards = toDiscard,
                                    To = new DeckPlace(null, DeckType.Heroes)
                                };
                                move.Helper.IsFakedMove = true;
                                Game.CurrentGame.MoveCards(move);
                            }
                            else
                            {
                                Game.CurrentGame.HandleCardDiscard(args.Source, toDiscard);
                            }
                        }
                    }
                }
                if (markCleanup.TryGetValue(sk, out List<PlayerAttribute> marks))
                {
                    foreach (var player in Game.CurrentGame.Players)
                    {
                        foreach (var mark in marks)
                        {
                            player[mark] = 0;
                        }
                    }
                }
            }
        }
    }
}
