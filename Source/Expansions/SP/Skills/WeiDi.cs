﻿using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;

namespace Sanguosha.Expansions.SP.Skills;

/// <summary>
/// 伪帝-锁定技，你视为拥有当前主公的主公技。
/// </summary>
public class WeiDi : TriggerSkill
{
    private readonly Dictionary<ISkill, ISkill> theSkills;
    private Player ruler;
    public WeiDi()
    {
        var trigger = new AutoNotifyPassiveSkillTrigger(
            this,
            (p, e, a) => { return a.Source == ruler && a.Source != Owner; },
            (p, e, a) =>
            {
                bool canInvoke = true;
                if (e == GameEvent.PlayerSkillSetChanged)
                {
                    SkillSetChangedEventArgs arg = a as SkillSetChangedEventArgs;
                    canInvoke = arg.Skills.Any(sk => sk.IsRulerOnly);
                }
                if (canInvoke)
                {
                    UninstallSkills();
                    InstallSkills(p, ruler);
                }
            },
            TriggerCondition.Global
        )
        { AskForConfirmation = false, IsAutoNotify = false };
        Triggers.Add(GameEvent.PlayerSkillSetChanged, trigger);
        Triggers.Add(GameEvent.PlayerChangedHero, trigger);

        IsEnforced = true;
        ruler = null;
        theSkills = new Dictionary<ISkill, ISkill>();
    }
    protected override void InstallTriggers(Player owner)
    {
        InstallSkills(owner);
        base.InstallTriggers(owner);
    }

    private void InstallSkills(Player owner)
    {
        foreach (var p in Game.CurrentGame.AlivePlayers)
        {
            if (p.Role == Role.Ruler && p != owner)
            {
                ruler = p;
                foreach (var sk in p.ActionableSkills)
                {
                    if (sk.IsRulerOnly)
                    {
                        var toAdd = Activator.CreateInstance(sk.GetType()) as ISkill;
                        Game.CurrentGame.PlayerAcquireAdditionalSkill(owner, toAdd, HeroTag, true);
                        theSkills.Add(sk, toAdd);
                    }
                }
                break;
            }
        }
    }

    private void InstallSkills(Player owner, Player ruler)
    {
        foreach (var sk in ruler.ActionableSkills)
        {
            if (sk.IsRulerOnly)
            {
                var toAdd = Activator.CreateInstance(sk.GetType()) as ISkill;
                Game.CurrentGame.PlayerAcquireAdditionalSkill(owner, toAdd, HeroTag, true);
                theSkills.Add(sk, toAdd);
            }
        }
    }

    private void UninstallSkills()
    {
        foreach (var skill in theSkills)
        {
            Game.CurrentGame.PlayerLoseAdditionalSkill(Owner, skill.Value, true);
        }
        theSkills.Clear();
    }

    protected override void UninstallTriggers(Player owner)
    {
        UninstallSkills();
        base.UninstallTriggers(owner);
    }
}