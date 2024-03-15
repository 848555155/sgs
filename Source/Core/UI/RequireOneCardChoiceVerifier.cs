using Sanguosha.Core.Cards;

namespace Sanguosha.Core.UI;

public class RequireOneCardChoiceVerifier(bool noreveal = false, bool showToAll = false) : ICardChoiceVerifier
{
    private readonly bool noCardReveal = noreveal;
    private readonly bool showToall = showToAll;

    public VerifierResult Verify(List<List<Card>> answer)
    {
        if ((answer.Count > 1) || (answer.Count > 0 && answer[0].Count > 1))
        {
            return VerifierResult.Fail;
        }
        if (answer.Count == 0 || answer[0].Count == 0)
        {
            return VerifierResult.Partial;
        }
        return VerifierResult.Success;
    }
    public UiHelper Helper => new() { RevealCards = !noCardReveal, ShowToAll = showToall };
}
