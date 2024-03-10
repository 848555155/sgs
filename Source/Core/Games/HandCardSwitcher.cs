using Sanguosha.Core.Cards;
using Sanguosha.Core.Network;

namespace Sanguosha.Core.Games;

public class HandCardSwitcher
{
    private readonly List<HandCardMovementNotification> handCardMovements = [];

    public void QueueHandCardMovement(HandCardMovementNotification notif)
    {
        lock (handCardMovements)
        {
            handCardMovements.Add(notif);
        }
    }

    public void HandleHandCardMovements()
    {
        lock (handCardMovements)
        {
            foreach (var notif in handCardMovements)
            {
                HandleHandCardMovement(notif);
            }
        }
    }

    private void HandleHandCardMovement(HandCardMovementNotification notif)
    {
        var player = notif.PlayerItem?.ToPlayer();
        if (player == null) return;
        var deck = Game.CurrentGame.Decks[player, DeckType.Hand];
        if (!(notif.To < 0 || notif.From < 0 || notif.From >= deck.Count || notif.To >= deck.Count))
        {
            var card1 = deck[notif.From];
            deck.Remove(card1);
            deck.Insert(notif.To, card1);
        }
    }

}
