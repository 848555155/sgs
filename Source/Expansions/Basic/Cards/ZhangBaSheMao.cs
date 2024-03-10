﻿using System.Collections.Generic;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards;


public class ZhangBaSheMao : Weapon
{
    public ZhangBaSheMao()
    {
        EquipmentSkill = new ZhangBaSheMaoTransform() { ParentEquipment = this };
    }

    
    public class ZhangBaSheMaoTransform : CardTransformSkill, IEquipmentSkill
    {
        public Equipment ParentEquipment { get; set; }
        public override VerifierResult TryTransform(List<Card> cards, List<Player> arg, out CompositeCard card, bool isPlay)
        {
            card = new CompositeCard();
            card.Type = new RegularSha();
            if (Owner.HandCards().Count < 2) return VerifierResult.Fail;
            if (cards == null || cards.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (cards.Count > 2)
            {
                return VerifierResult.Fail;
            }
            if (cards[0].Owner != Owner || cards[0].Place.DeckType != DeckType.Hand)
            {
                return VerifierResult.Fail;
            }
            if (cards.Count == 2 && (cards[1].Owner != Owner || cards[1].Place.DeckType != DeckType.Hand))
            {
                return VerifierResult.Fail;
            }
            if (cards.Count == 1)
            {
                return VerifierResult.Partial;
            }
            card.Subcards = new List<Card>(cards);
            return VerifierResult.Success;
        }

        public override List<CardHandler> PossibleResults
        {
            get { return new List<CardHandler>() {new Sha()}; }
        }
    }

    public override int AttackRange
    {
        get { return 3; }
    }

    protected override void RegisterWeaponTriggers(Player p)
    {
        return;
    }

    protected override void UnregisterWeaponTriggers(Player p)
    {
        return;
    }

}
