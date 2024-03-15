﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using System.Diagnostics;

namespace Sanguosha.Core.UI;

public interface IGameLog
{
}

public class CardUseLog : IGameLog
{
    public ISkill Skill { get; set; }
    public List<Card> Cards { get; set; }
    public Player Source { get; set; }
    public List<Player> Targets { get; set; }
    public CardHandler Type { get; set; }
    public Player Target
    {
        get { Trace.Assert(Targets.Count == 1); return Targets[0]; }
        set { Trace.Assert(Targets == null); Targets = [value]; }
    }
}

public class DamageLog : IGameLog
{
    public Player Source { get; set; }
    public Player Target { get; set; }
    private int Delta { get; set; }
    private DamageElement Element { get; set; }
    private bool HealthLost { get; set; }
}

public class HpRecoveryLog : IGameLog
{
    public Player Source { get; set; }
    public Player Target { get; set; }
    private int Delta { get; set; }
}

public class HpLostLog : IGameLog
{
    public Player Player { get; set; }
    private int Delta { get; set; }
}

public enum GameAction
{
    None,
    Play,
    Use,
    Discard,
    PlaceIntoDiscard,
    Show,
    Judge,
    ReplaceJudge,
    Reforge
}

public class ActionLog : IGameLog
{
    public GameAction GameAction { get; set; }
    public ISkill SkillAction { get; set; }

    public ICard CardAction { get; set; }
    public Player Source { get; set; }
    public List<Player> Targets { get; set; } = [];
    public List<Player> SecondaryTargets { get; set; } = [];
    /// <summary>
    /// Gets/sets skill tag that is used for indexing different types of animation/audio
    /// associated with the same skill.
    /// </summary>
    public int SpecialEffectHint { get; set; }

    /// <summary>
    /// 一些特别的，要使用到指示线的CardTransformSkill，如激将，蛊惑
    /// </summary>
    public bool ShowCueLine { get; set; }

    /// <summary>
    /// 只播放技能音效，不显示log
    /// </summary>
    public bool SkillSoundOnly { get; set; }
}
