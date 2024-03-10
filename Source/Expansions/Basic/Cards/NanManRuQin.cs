using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards;


public class NanManRuQin : Aoe
{
    public NanManRuQin()
    {
        RequiredCard = new Sha();
    }

    protected override string UsagePromptString
    {
        get { return "NanManRuQin"; }
    }

    public override CardHandler RequiredCard
    {
        get;
        protected set;
    }
}
