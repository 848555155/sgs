namespace Sanguosha.Core.Games;

/// <summary>
/// 每个回合各个阶段
/// </summary>
public enum TurnPhase
{
    Inactive = -1,
    BeforeStart = 0,
    Start = 1,
    Judge = 2,
    Draw = 3,
    Play = 4,
    Discard = 5,
    End = 6,
    PostEnd = 7,
}
