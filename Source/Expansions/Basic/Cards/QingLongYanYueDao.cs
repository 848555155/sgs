﻿using Sanguosha.Core.Cards;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Games;
using Sanguosha.Core.Players;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.UI;
using System.Diagnostics;

namespace Sanguosha.Expansions.Basic.Cards;


public class QingLongYanYueDao : Weapon
{
    public QingLongYanYueDao()
    {
        EquipmentSkill = new QingLongYanYueSkill() { ParentEquipment = this };
    }


    public class QingLongYanYueSkill : TriggerSkill, IEquipmentSkill
    {
        public Equipment ParentEquipment { get; set; }
        protected void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            while (ParentEquipment.ParentCard.Place.DeckType == DeckType.Equipment)
            {
                if (!Game.CurrentGame.PlayerCanBeTargeted(Owner, new List<Player>() { eventArgs.Targets[0] }, new CompositeCard() { Type = new Sha() }))
                {
                    return;
                }
                var v = new SingleCardUsageVerifier((c) => { return c.Type is Sha; }, true, new Sha());
                Game.CurrentGame.Emit(GameEvent.PlayerIsAboutToUseCard, new PlayerIsAboutToUseOrPlayCardEventArgs() { Source = Owner, Verifier = v });
                if (Game.CurrentGame.UiProxies[Owner].AskForCardUsage(new CardUsagePrompt("QingLongYanYueDao"),
                new SingleCardUsageVerifier((c) => { return c.Type is Sha; }, true),
                out skill, out cards, out players))
                {
                    try
                    {
                        Owner[Sha.NumberOfShaUsed]--;
                        var args = new GameEventArgs
                        {
                            Source = eventArgs.Source,
                            Targets = eventArgs.Targets,
                            Skill = skill,
                            Cards = cards,
                            ReadonlyCard = new ReadOnlyCard(new Card() { Place = new DeckPlace(null, null) })
                        };
                        args.ReadonlyCard[QingLongSha] = 1;
                        Game.CurrentGame.Emit(GameEvent.CommitActionToTargets, args);
                    }
                    catch (TriggerResultException e)
                    {
                        Trace.Assert(e.Status == TriggerResult.Retry);
                        continue;
                    }
                }
                break;
            }
        }
        public QingLongYanYueSkill()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                Run,
                TriggerCondition.OwnerIsSource
            )
            { IsAutoNotify = false, AskForConfirmation = false };
            Triggers.Add(ShaCancelling.PlayerShaTargetDodged, trigger);

            var notify = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => a.ReadonlyCard[QingLongSha] == 1,
                (p, e, a) => { },
                TriggerCondition.OwnerIsSource
            )
            { AskForConfirmation = false };
            Triggers.Add(GameEvent.PlayerUsedCard, notify);
        }

        private static readonly CardAttribute QingLongSha = CardAttribute.Register("QingLongSha");
    }

    public override int AttackRange => 3;

    protected override void RegisterWeaponTriggers(Player p)
    {
        return;
    }

    protected override void UnregisterWeaponTriggers(Player p)
    {
        return;
    }

}
