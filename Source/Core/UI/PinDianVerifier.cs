using Sanguosha.Core.Games;
using Sanguosha.Core.Players;

namespace Sanguosha.Core.UI;

public class PinDianVerifier : CardsAndTargetsVerifier
{
    public PinDianVerifier()
    {
        MaxCards = 0;
        MinPlayers = 1;
        MaxPlayers = 1;
    }

    protected override bool VerifyPlayer(Player source, Player player)
    {
        return source != player && player.HandCards().Count > 0;
    }
}
