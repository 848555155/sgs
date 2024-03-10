using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Core.Cards;

public interface IEquipmentSkill : ISkill
{
    Equipment ParentEquipment { get; }
}

public abstract class Equipment : CardHandler
{
    /// <summary>
    /// 注册装备应有的trigger到玩家
    /// </summary>
    /// <param name="p"></param>
    protected virtual void RegisterEquipmentTriggers(Player p)
    {
    }
    /// <summary>
    /// 从玩家注销装备应有的trigger
    /// </summary>
    /// <param name="p"></param>
    protected virtual void UnregisterEquipmentTriggers(Player p)
    {
    }

    public virtual void RegisterTriggers(Player p)
    {
        if (EquipmentSkill != null)
        {
            Trace.TraceInformation("registered {0} to {1}", EquipmentSkill.GetType().Name, p.Id);
            p.AcquireEquipmentSkill(EquipmentSkill);
        }
        RegisterEquipmentTriggers(p);
    }

    public virtual void UnregisterTriggers(Player p)
    {
        if (EquipmentSkill != null)
        {
            Trace.TraceInformation("unregistered {0} from {1}", EquipmentSkill.GetType().Name, p.Id);
            p.LoseEquipmentSkill(EquipmentSkill);
        }
        UnregisterEquipmentTriggers(p);
    }

    /// <summary>
    /// 给某个玩家穿装备
    /// </summary>
    /// <param name="p"></param>
    /// <param name="card"></param>
    public void Install(Player p, Card card, Player installedBy)
    {
        ParentCard = card;
        var attachMove = new CardsMovement
        {
            Cards = [card],
            To = new DeckPlace(p, DeckType.Equipment)
        };
        foreach (var c in Game.CurrentGame.Decks[p, DeckType.Equipment])
        {
            if (CardCategoryManager.IsCardCategory(c.Type.Category, Category))
            {
                Equipment e = (Equipment)c.Type;
                Trace.Assert(e != null);
                Game.CurrentGame.EnterAtomicContext();
                if (installedBy != null) Game.CurrentGame.PlayerLostCard(installedBy, [card]);
                if (installedBy != p) Game.CurrentGame.PlayerAcquiredCard(p, [card]);
                Game.CurrentGame.HandleCardDiscard(p, [c]);
                Game.CurrentGame.MoveCards(attachMove);
                Game.CurrentGame.ExitAtomicContext();
                return;
            }
        }

        Game.CurrentGame.MoveCards(attachMove);
        if (installedBy != null) Game.CurrentGame.PlayerLostCard(installedBy, [card]);
        if (installedBy != p) Game.CurrentGame.PlayerAcquiredCard(p, [card]);
        return;
    }

    public void Install(Player p, Card card)
    {
        Install(p, card, p);
    }

    public override void Process(GameEventArgs handlerArgs)
    {
        var source = handlerArgs.Source;
        var dests = handlerArgs.Targets;
        var readonlyCard = handlerArgs.ReadonlyCard;
        var inResponseTo = handlerArgs.InResponseTo;
        var card = handlerArgs.Card;
        Trace.Assert(dests == null || dests.Count == 0);
        Trace.Assert(card is Card);
        Card c = (Card)card;
        Install(source, c);
    }

    public override VerifierResult Verify(Player source, ICard card, List<Player> targets, bool isLooseVerify)
    {
        if (targets == null || targets.Count == 0)
        {
            return VerifierResult.Success;
        }
        return VerifierResult.Fail;
    }

    public IEquipmentSkill EquipmentSkill
    {
        get; protected set;
    }

    public bool InUse { get; set; }

    public Card ParentCard { get; set; }

    public override CardCategory Category
    {
#pragma warning disable CA1065 // 不要在意外的位置引发异常
        get { throw new NotImplementedException(); }
#pragma warning restore CA1065 // 不要在意外的位置引发异常
    }
}
