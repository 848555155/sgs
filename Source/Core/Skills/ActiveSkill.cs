using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;

namespace Sanguosha.Core.Skills;

public abstract class ActiveSkill : ISkill
{
    public UiHelper Helper { get; protected set; } = new();

    /// <summary>
    /// 检查主动技的合法性。
    /// </summary>
    /// <param name="arg">参数</param>
    /// <param name="card">输出卡牌</param>
    /// <returns></returns>
    public abstract VerifierResult Validate(GameEventArgs arg);

    /// <summary>
    /// 提交主动技的发动请求。
    /// </summary>
    /// <param name="arg">参数</param>
    /// <returns>true if 可以打出, false if 不可打出</returns>
    public abstract bool Commit(GameEventArgs arg);

    public virtual bool NotifyAndCommit(GameEventArgs arg)
    {
        NotifyAction(Owner, arg.Targets, arg.Cards);
        if (IsAwakening || IsSingleUse) Utils.GameDelays.Delay(Utils.GameDelays.Awaken);
        return Commit(arg);
    }

    public PassiveSkill LinkedPassiveSkill { get; protected set; } = null;

    private Player owner;
    public virtual Player Owner
    {
        get { return owner; }
        set
        {
            if (owner == value) return;
            owner = value;
            if (LinkedPassiveSkill != null)
            {
                LinkedPassiveSkill.HeroTag = HeroTag;
                LinkedPassiveSkill.Owner = value;
            }
            if (owner != null)
            {
                foreach (var dk in DeckCleanup) Game.CurrentGame.RegisterSkillCleanup(this, dk);
                foreach (var att in AttributeCleanup) Game.CurrentGame.RegisterMarkCleanup(this, att);
            }
        }
    }

    public Hero HeroTag { get; set; }

    public virtual void NotifyAction(Player source, List<Player> targets, List<Card> cards)
    {
        var log = new ActionLog
        {
            GameAction = GameAction.None,
            CardAction = null,
            SkillAction = this,
            Source = source
        };
        TargetsSplit(targets, out var ft, out var st);
        log.Targets = ft;
        log.SecondaryTargets = st;
        foreach (var c in cards)
        {
            c.Log ??= new ActionLog();
            c.Log.SkillAction = this;
        }
        log.SpecialEffectHint = GenerateSpecialEffectHintIndex(source, targets, cards);
        Game.CurrentGame.NotificationProxy.NotifySkillUse(log);
    }

    protected virtual int GenerateSpecialEffectHintIndex(Player source, List<Player> targets, List<Card> cards)
    {
        return 0;
    }

    protected virtual void TargetsSplit(List<Player> targets, out List<Player> firstTargets, out List<Player> secondaryTargets)
    {
        if (targets == null)
        {
            firstTargets = [];
        }
        else
        {
            firstTargets = new List<Player>(targets);
        }
        secondaryTargets = null;
    }

    public object Clone()
    {
        var skill = Activator.CreateInstance(GetType()) as ActiveSkill;
        skill.Owner = Owner;
        skill.Helper = Helper;
        return skill;
    }

    protected List<DeckType> DeckCleanup { get; private set; } = [];
    protected List<PlayerAttribute> AttributeCleanup { get; private set; } = [];

    public bool IsRulerOnly { get; protected set; }
    public bool IsSingleUse { get; protected set; }
    public bool IsAwakening { get; protected set; }
    public bool IsEnforced { get; protected set; }
}
