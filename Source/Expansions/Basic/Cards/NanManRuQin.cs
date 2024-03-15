using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards;


public class NanManRuQin : Aoe
{
    public NanManRuQin() => RequiredCard = new Sha();

    protected override string UsagePromptString => "NanManRuQin";

    public override CardHandler RequiredCard
    {
        get;
        protected set;
    }
}
