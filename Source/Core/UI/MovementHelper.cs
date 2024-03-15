using Sanguosha.Core.Heroes;

namespace Sanguosha.Core.UI;

public class MovementHelper
{
    /// <summary>
    /// the movement is not really a game move but a logical move designed to facilitate user choices, e.g. 遗计
    /// </summary>
    public bool IsFakedMove { get; set; }

    public int WindowId { get; set; }

    public bool IsWuGu { get; set; }

    public Hero PrivateDeckHeroTag { get; set; }

    //show card move log even faked move.  e.g.落英
    public bool AlwaysShowLog { get; set; }

    public MovementHelper()
    {
    }

    public MovementHelper(MovementHelper helper)
    {
        IsFakedMove = helper.IsFakedMove;
        WindowId = helper.WindowId;
        IsWuGu = helper.IsWuGu;
        PrivateDeckHeroTag = helper.PrivateDeckHeroTag;
        AlwaysShowLog = helper.AlwaysShowLog;
    }
}
