using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace ComicChess.TheQueen;

/// <summary>桃心木剑：每场战斗开始时，为你牌堆中所有敌怪池卡牌添加 <see cref="CardKeyword.Ethereal"/>。</summary>
[Pool(typeof(QueenRelicPool))]
public sealed class PeachHeartWoodenSwordRelic : QueenRelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];

	public override Task BeforeCombatStart()
	{
		if (base.Owner.PlayerCombatState == null)
		{
			return Task.CompletedTask;
		}

		foreach (CardModel card in base.Owner.PlayerCombatState.AllCards.ToList())
		{
			if (card.Pool is not EnemyCardPool)
			{
				continue;
			}

			if (card.Keywords.Contains(CardKeyword.Ethereal))
			{
				continue;
			}

			card.AddKeyword(CardKeyword.Ethereal);
		}

		return Task.CompletedTask;
	}
}
