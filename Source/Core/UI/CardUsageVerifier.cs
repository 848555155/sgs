using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;

namespace Sanguosha.Core.UI;

public class UiHelper
{
    /// <summary>
    /// Whether a player can be targeted more than once (e.g. 业炎).
    /// </summary>
    public bool IsPlayerRepeatable { get; set; }

    /// <summary>
    /// Whether it is related to the action stage.
    /// </summary>
    /// <remarks>
    /// 出牌阶段和求闪/桃阶段，取消和结束按钮的作用不同，故设置此参数。
    /// </remarks>
    public bool IsActionStage { get; set; }

    /// <summary>
    /// Whether "Confirm" button needs to be clicked to invoke the skill (e.g. 苦肉，乱舞).
    /// </summary>        
    public bool HasNoConfirmation { get; set; }

    /// <summary>
    /// 不展示卡牌使用 (e.g. 遗计, card usage only）
    /// </summary>
    public bool NoCardReveal { get; set; }

    public bool RevealCards { get; set; }

    /// <summary></summary>
    /// <seealso cref="Sanguosha.Expansions.OverKnightFame11.Skills.XinZhan"/>
    public List<bool> AdditionalFineGrainedCardChoiceRevealPolicy { get; set; } = [];

    public List<DeckType> OtherDecksUsed { get; set; } = [];

    public Dictionary<DeckPlace, int?> OtherGlobalCardDeckUsed { get; set; } = [];

    public int ExtraTimeOutSeconds { get; set; }

    public bool ShowToAll { get; set; }

}

public interface ICardUsageVerifier
{
    VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players);

    /// <summary>
    /// Gets/sets card types that can possibly be accepted by the verifier. 
    /// If value is null, any card type may be accepted. If value is empty,
    /// no card is accepted by the verifier.
    /// </summary>
    IList<CardHandler> AcceptableCardTypes { get; }
    VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players);
    UiHelper Helper { get; }
}

public abstract class CardUsageVerifier : ICardUsageVerifier
{
    public virtual VerifierResult Verify(Player source, ISkill skill, List<Card> cards, List<Player> players)
    {
        if (skill is PassiveSkill)
        {
            return VerifierResult.Fail;
        }

        if (AcceptableCardTypes == null)
        {
            return SlowVerify(source, skill, cards, players);
        }

        if (skill is CardTransformSkill transformSkill)
        {
            if (transformSkill is IAdditionalTypedSkill
                || transformSkill.PossibleResults == null)
            {
                return SlowVerify(source, skill, cards, players);
            }
            else
            {
                var commonResult = from type1 in AcceptableCardTypes
                                   where transformSkill.PossibleResults.Any(ci => type1.GetType().IsAssignableFrom(ci.GetType()))
                                   select type1;
                if (commonResult.Any())
                {
                    return SlowVerify(source, skill, cards, players);
                }
            }
            return VerifierResult.Fail;
        }

        if (skill is ActiveSkill)
        {
            if (SlowVerify(source, skill, null, null) == VerifierResult.Fail)
            {
                return VerifierResult.Fail;
            }
        }

        return SlowVerify(source, skill, cards, players);
    }

    private VerifierResult SlowVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
    {
        VerifierResult initialResult = FastVerify(source, skill, cards, players);

        if (Properties.Settings.Default.IsUsing386) return initialResult;

        if (skill == null)
        {
            return initialResult;
        }
        if (initialResult == VerifierResult.Success)
        {
            return VerifierResult.Success;
        }
        bool NothingWorks = true;
        List<Card> tryList = new List<Card>();
        if (cards != null)
        {
            tryList.AddRange(cards);
        }
        var cardsToTry = new List<Card>(Game.CurrentGame.Decks[source, DeckType.Hand].Concat(Game.CurrentGame.Decks[source, DeckType.Equipment]));

        foreach (var dk in skill.Helper.OtherDecksUsed)
        {
            cardsToTry.AddRange(Game.CurrentGame.Decks[source, dk]);
        }

        foreach (Card c in cardsToTry)
        {
            tryList.Add(c);
            if (FastVerify(source, skill, tryList, players) != VerifierResult.Fail)
            {
                NothingWorks = false;
                break;
            }
            tryList.Remove(c);
        }
        List<Player> tryList2 = [];
        if (players != null)
        {
            tryList2.AddRange(players);
        }
        foreach (Player p in Game.CurrentGame.Players)
        {
            tryList2.Add(p);
            if (FastVerify(source, skill, cards, tryList2) != VerifierResult.Fail)
            {
                NothingWorks = false;
                break;
            }
            tryList2.Remove(p);
        }
        if (NothingWorks)
        {
            return VerifierResult.Fail;
        }
        return initialResult;
    }

    public abstract VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players);

    public abstract IList<CardHandler> AcceptableCardTypes { get; }


    public UiHelper Helper { get; set; } = new();

}
