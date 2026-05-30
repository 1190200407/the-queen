using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>
/// 启示之杯：每场战斗中，由你打出的牌触发的<strong>第一次</strong>意图学习（<see cref="FriendlyAmalgamCmd.LearnIntent"/> 或
/// <see cref="FriendlyAmalgamCmd.CombineIntent"/>）会再写入一次相同意图（克隆）。
/// 通过 <see cref="IAmalgamEventListener"/> 接入，不污染 <see cref="FriendlyAmalgamCmd"/>。
/// 不作用于 <see cref="SoulResonancePower"/> 等镜像学习（来源牌的 <see cref="CardModel.Owner"/> 非 relic 持有者）。
/// </summary>

public sealed class RevelationCupRelic : QueenRelicModel, IAmalgamEventListener
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [QueenHoverTips.LearnIntent];

	private bool _wasUsedThisCombat;
	public bool WasUsedThisCombat
	{
		get
		{
			return _wasUsedThisCombat;
		}
		set
		{
			AssertMutable();
			_wasUsedThisCombat = value;
		}
	}

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		if (!(room is CombatRoom))
		{
			return Task.CompletedTask;
		}
		WasUsedThisCombat = false;
		base.Status = RelicStatus.Active;
		return Task.CompletedTask;
	}

	public async Task AfterLearnIntent(
		ICombatState combatState,
		PlayerChoiceContext choiceContext,
		Player amalgamOwner,
		Creature amalgam,
		AmalgamActionModel intent,
		AbstractModel? source)
	{
		if (!TryClaimFirstIntentLearn(amalgamOwner, source))
		{
			return;
		}

		await FriendlyAmalgamCmd.LearnIntent(choiceContext, amalgamOwner, intent.Clone(), source);
	}

	public async Task AfterCombineIntent(
		ICombatState combatState,
		PlayerChoiceContext choiceContext,
		Player amalgamOwner,
		Creature amalgam,
		AmalgamActionModel intent,
		AbstractModel? source,
		AmalgamCompositeKey compositeKey)
	{
		if (!TryClaimFirstIntentLearn(amalgamOwner, source))
		{
			return;
		}

		await FriendlyAmalgamCmd.CombineIntent(choiceContext, amalgamOwner, intent.Clone(), source, compositeKey);
	}

	private bool TryClaimFirstIntentLearn(Player amalgamOwner, AbstractModel? source)
	{
		if (amalgamOwner != base.Owner)
		{
			return false;
		}

		if (source is not CardModel card || card.Owner != base.Owner)
		{
			return false;
		}

		if (WasUsedThisCombat)
		{
			return false;
		}

		WasUsedThisCombat = true;
		base.Status = RelicStatus.Normal;
		Flash();
		return true;
	}
}
