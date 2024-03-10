using Sanguosha.Core.Games;

namespace Sanguosha.Core.UI;

public class CardChoiceCallback
{
    public static void GenericCardChoiceCallback(CardRearrangement obj)
    {
        if (Game.CurrentGame.IsClient)
        {
            Game.CurrentGame.GameClient.CardChoiceCallBack(obj);
        }
    }
}
