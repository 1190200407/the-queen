using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 带「抓挠」标签、且本场战斗中可被 <see cref="Scratch"/> 强化伤害的攻击牌基类（机制同原版 <c>Claw</c>）。
/// </summary>
public abstract class ScratchTaggedCard : QueenCardModel
{
	private int _scratchExtraHitCountFromEndlessScratch;

	private decimal _extraDamageFromScratchPlays;

	private decimal ExtraDamageFromScratchPlays
	{
		get => _extraDamageFromScratchPlays;
		set
		{
			AssertMutable();
			_extraDamageFromScratchPlays = value;
		}
	}
	protected override IEnumerable<string> RegisteredCardTagIds => [QueenCardTags.Scratch];

	protected ScratchTaggedCard(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	/// <summary>
	/// 首次进入战斗牌堆时补全本场已累积的抓挠伤害与无尽抓挠段数（此前仅在手牌生成路径调用 <see cref="QueenScratchBonusTracker.ApplyToNewScratchTagged"/>，会漏掉抽堆/奖励等后入场的抓挠牌）。
	/// </summary>
	public override async Task AfterCardEnteredCombat(CardModel card)
	{
		await base.AfterCardEnteredCombat(card);
		if (card != this || base.IsClone)
		{
			return;
		}

		QueenScratchBonusTracker.ApplyToNewScratchTagged(base.Owner, this);
	}

	internal void BuffFromScratchPlay(decimal extraDamage)
	{
		base.DynamicVars.Damage.BaseValue += extraDamage;
		ExtraDamageFromScratchPlays += extraDamage;
	}

	/// <summary>自身攻击段数（不含无尽抓挠本场加成）。多段攻击的派生牌可重写。</summary>
	protected virtual int ScratchBaseHitCount => 1;

	/// <summary>与 <see cref="MegaCrit.Sts2.Core.Localization.DynamicVars.RepeatVar"/> 同步，供卡面显示攻击次数（同 Sovereign Blade）。</summary>
	private void SyncRepeatVarToCombatHits()
	{
		base.DynamicVars.Repeat.BaseValue = ScratchBaseHitCount + _scratchExtraHitCountFromEndlessScratch;
	}

	/// <summary>本场战斗中由 <see cref="EndlessScratch"/> 叠加的额外攻击段数（每张牌独立计数）。</summary>
	internal void BuffHitCountFromEndlessScratch(int delta = 1)
	{
		AssertMutable();
		_scratchExtraHitCountFromEndlessScratch += delta;
		SyncRepeatVarToCombatHits();
	}

	protected override void AfterDowngraded()
	{
		base.AfterDowngraded();
		base.DynamicVars.Damage.BaseValue += ExtraDamageFromScratchPlays;
		SyncRepeatVarToCombatHits();
	}

	protected override void AfterCloned()
	{
		base.AfterCloned();
		SyncRepeatVarToCombatHits();
	}
}
