using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards;


public class WanJianQiFa : Aoe
{
    public WanJianQiFa()
    {
        RequiredCard = new Shan();
    }

    protected override string UsagePromptString
    {
        get { return "WanJianQiFa"; }
    }

    public override CardHandler RequiredCard
    {
        get;
        protected set;
    }
}
