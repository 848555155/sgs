using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards;

public class NanManRuQin : Aoe
{
    protected override string UsagePromptString => nameof(NanManRuQin);

    public override CardHandler RequiredCard { get; protected set; } = new Sha();
}
