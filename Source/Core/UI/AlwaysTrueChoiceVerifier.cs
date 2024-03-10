using Sanguosha.Core.Cards;

namespace Sanguosha.Core.UI;

public class AlwaysTrueChoiceVerifier : ICardChoiceVerifier
{
    public VerifierResult Verify(List<List<Card>> answer)
    {
        return VerifierResult.Success;
    }

    public UiHelper Helper
    {
        get { return null; }
    }
}
