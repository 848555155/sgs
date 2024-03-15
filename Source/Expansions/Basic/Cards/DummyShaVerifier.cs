using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.UI;

namespace Sanguosha.Expansions.Basic.Cards;

public class DummyShaVerifier : CardUsageVerifier
{
    public override VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
    {
        return FastVerify(source, skill, cards, players);
    }

    public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
    {
        if (players != null && players.Any(p => p.IsDead))
        {
            return VerifierResult.Fail;
        }
        players ??= [];
        var sha = new CompositeCard() { Type = type };
        if (!Game.CurrentGame.PlayerCanBeTargeted(source, players, sha))
        {
            return VerifierResult.Fail;
        }
        List<Player> newList = new List<Player>(players);
        if (target != null)
        {
            if (!newList.Contains(target))
            {
                newList.Insert(0, target);
            }
            else
            {
                return VerifierResult.Fail;
            }
        }
        if (cards != null && cards.Count > 0)
        {
            return VerifierResult.Fail;
        }
        if (skill is CardTransformSkill sk)
        {
            if (sk.TryTransform(dummyCards, null, out sha) != VerifierResult.Success)
            {
                return VerifierResult.Fail;
            }
            if (helper != null) sha[helper] = 1;
            return new Sha().VerifyCore(source, sha, newList);
        }
        else if (skill != null)
        {
            return VerifierResult.Fail;
        }
        if (helper != null) sha[helper] = 1;
        return new Sha().VerifyCore(source, sha, newList);
    }

    public override IList<CardHandler> AcceptableCardTypes
    {
        get { return null; }
    }

    private readonly Player target;
    private readonly CardHandler type;
    private readonly List<Card> dummyCards;
    private readonly CardAttribute helper;

    public DummyShaVerifier(Player t, CardHandler shaType, CardAttribute helper = null)
    {
        target = t;
        type = shaType;
        this.helper = helper;
        dummyCards = [new Card() { Type = shaType, Place = new DeckPlace(null, DeckType.None) }];
    }
}
