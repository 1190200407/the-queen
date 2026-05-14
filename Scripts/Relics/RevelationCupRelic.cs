using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace ComicChess.TheQueen;

/// <summary>
/// 启示之杯：每场战斗中，由你打出的牌触发的<strong>第一次</strong> <see cref="FriendlyAmalgamCmd.LearnIntent"/> 会再写入一次相同意图（克隆）。
/// 通过 <see cref="IAmalgamEventListener.AfterLearnIntent"/> 接入，不污染 <see cref="FriendlyAmalgamCmd"/>。
/// 不作用于 <see cref="SoulResonancePower"/> 等镜像学习（来源牌的 <see cref="CardModel.Owner"/> 非 relic 持有者）。
/// </summary>
[Pool(typeof(QueenRelicPool))]
public sealed class RevelationCupRelic : QueenRelicModel, IAmalgamEventListener
{
	private bool _firstLearnDuplicateConsumed;

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [QueenHoverTips.LearnIntent];

	public override Task BeforeCombatStart()
	{
		_firstLearnDuplicateConsumed = false;
		return Task.CompletedTask;
	}

	public async Task AfterLearnIntent(
		CombatState combatState,
		PlayerChoiceContext choiceContext,
		Player amalgamOwner,
		Creature amalgam,
		AmalgamActionModel intent,
		AbstractModel? source)
	{
		if (amalgamOwner != base.Owner)
		{
			return;
		}

		if (source is not CardModel card || card.Owner != base.Owner)
		{
			return;
		}

		if (_firstLearnDuplicateConsumed)
		{
			return;
		}

		_firstLearnDuplicateConsumed = true;
		Flash();
		await FriendlyAmalgamCmd.LearnIntent(choiceContext, amalgamOwner, intent.Clone(), source);
	}
}
