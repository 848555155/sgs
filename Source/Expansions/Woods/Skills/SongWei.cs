﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Expansions.Woods.Skills;

/// <summary>
/// 颂威―主公技，其他魏势力角色的判定结果为黑色且生效后，可以令你摸一张牌。
/// </summary>
public class SongWeiGivenSkill : TriggerSkill, IRulerGivenSkill
{
    public SongWeiGivenSkill()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return a.Cards[0].SuitColor == SuitColorType.Black; },
            (p, e, a) => { Game.CurrentGame.DrawCards(Master, 1); },
            TriggerCondition.OwnerIsSource
        );
        Triggers.Add(GameEvent.PlayerJudgeDone, trigger);
        IsAutoInvoked = false;
    }

    public Player Master { get; set; }
}

public class SongWei : RulerGivenSkillContainerSkill
{
    public SongWei()
        : base(new SongWeiGivenSkill(), Allegiance.Wei)
    {
    }
}
