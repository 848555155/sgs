using Sanguosha.Core.Cards;

namespace Sanguosha.Core.UI;

public class RequireCardsChoiceVerifier(int count, bool noreveal = false, bool showToAll = false) : ICardChoiceVerifier
{
    private readonly bool noCardReveal = noreveal;
    private readonly int count = count;
    private readonly bool showToall = showToAll;

    public VerifierResult Verify(List<List<Card>> answer)
    {
        if ((answer.Count > 1) || (answer.Count > 0 && answer[0].Count > count))
        {
            return VerifierResult.Fail;
        }
        if (answer == null || answer[0] == null || answer[0].Count < count)
        {
            return VerifierResult.Partial;
        }
        return VerifierResult.Success;
    }
    public UiHelper Helper => new() { RevealCards = !noCardReveal, ShowToAll = showToall };
}
