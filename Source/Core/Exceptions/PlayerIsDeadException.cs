using Sanguosha.Core.Players;

namespace Sanguosha.Core.Exceptions;


public class PlayerIsDeadException(Player p) : SgsException
{
    public Player Player { get; set; } = p;
}
