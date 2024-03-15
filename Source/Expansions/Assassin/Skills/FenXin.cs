using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Utils;

namespace Sanguosha.Expansions.Assassin.Skills;

/// <summary>
/// 焚心-限定技，若你的身份不是主公，当你杀死一名非主公角色时，在其亮出身份牌前，你可以与其交换身份牌。
/// </summary>
public class FenXin : TriggerSkill
{
    private void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
    {
        Owner[FenXinUsed] = 1;
        Player target = eventArgs.Targets[0];

        Card role1 = Game.CurrentGame.Decks[Owner, RoleGame.RoleDeckType][0];
        Card role2 = Game.CurrentGame.Decks[target, RoleGame.RoleDeckType][0];
        Game.CurrentGame.SyncCard(target, ref role1);
        Game.CurrentGame.SyncCard(Owner, ref role2);

        List<CardsMovement> moves = [];
        var move1 = new CardsMovement
        {
            Cards = [role1],
            To = new DeckPlace(target, RoleGame.RoleDeckType)
        };
        moves.Add(move1);

        var move2 = new CardsMovement
        {
            Cards = [role2],
            To = new DeckPlace(Owner, RoleGame.RoleDeckType)
        };
        moves.Add(move2);

        Game.CurrentGame.MoveCards(moves);

        if (role2.Type is RoleCardHandler role)
        {
            Owner.Role = role.Role;
        }
        role = role1.Type as RoleCardHandler;
        if (role != null)
        {
            target.Role = role.Role;
        }
        GameDelays.Delay(GameDelays.RoleDistribute);
    }

    public FenXin()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return p[FenXinUsed] == 0 && p.Role != Role.Ruler && a.Targets[0].Role != Role.Ruler; },
            Run,
            TriggerCondition.OwnerIsSource
        );
        Triggers.Add(GameEvent.BeforeRevealRole, trigger);
        IsSingleUse = true;
    }

    private static readonly PlayerAttribute FenXinUsed = PlayerAttribute.Register("FenXinUsed");
}
