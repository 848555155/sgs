using Sanguosha.Core.Games;
using Sanguosha.Core.Heroes;
using Sanguosha.Expansions.Assassin.Skills;

namespace Sanguosha.Expansions;

public class AssassinExpansion : Expansion
{
    public AssassinExpansion()
    {
        CardSet.AddRange(
            [
                CreateHeroCard<MouKui>("FuWan", true, Allegiance.Qun, 4),
                CreateHeroCard<TianMing, MiZhao>("LiuXie", true, Allegiance.Qun, 3),
                CreateHeroCard<JieYuan, FenXin>("LingJu", true, Allegiance.Qun, 3)
            ]
        );
    }
}
