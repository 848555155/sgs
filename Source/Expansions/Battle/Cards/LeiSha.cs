using Sanguosha.Core.Games;
using Sanguosha.Expansions.Basic.Cards;

namespace Sanguosha.Expansions.Battle.Cards;


public class LeiSha : Sha
{
    public override DamageElement ShaDamageElement
    {
        get
        {
            return DamageElement.Lightning;
        }
    }
}
