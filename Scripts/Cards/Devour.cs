using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>饥饿倍率写入 Preview，与 Base 对比出 <c>{StrengthPower:diff()}</c> 绿字；避免默认 PowerVar 预览冲掉倍率。</summary>
internal sealed class DevourStrengthPreviewVar : PowerVar<StrengthPower>
{
	public DevourStrengthPreviewVar()
		: base(1m)
	{
	}

	public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
	{
		decimal amount = BaseValue;
		if (runGlobalHooks && card.CombatState is not null && card.Owner?.Creature is { } dealer)
		{
			amount = Hook.ModifyPowerAmountGiven(
				card.CombatState,
				ModelDb.Power<StrengthPower>(),
				dealer,
				BaseValue,
				target,
				card,
				out IEnumerable<AbstractModel> _);
		}

		PreviewValue = card.Owner?.Creature is { } c
			? amount * HungerPower.DevourEffectMultiplier(c)
			: amount;
	}
}

[Pool(typeof(TokenCardPool))]
public sealed class Devour : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Token;
	private const TargetType constructorTargetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [QueenKeyword.fade];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DevourStrengthPreviewVar()];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<StrengthPower>(),
	];

	public override TargetType TargetType => base.IsUpgraded ? TargetType.AnyEnemy : TargetType.Self;

	public Devour()
		: base(energyCost, type, rarity, constructorTargetType, shouldShowInCardLibrary)
	{
	}

	private const decimal BaseStrengthGain = 1m;

	public void SyncMultiplyVar()
	{
		if (base.Owner?.Creature is null || base.CombatState is null)
		{
			return;
		}

		base.UpdateDynamicVarPreview(CardPreviewMode.Normal, null, base.DynamicVars);
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		decimal strGain = BaseStrengthGain * HungerPower.DevourEffectMultiplier(base.Owner.Creature);
		await PowerCmd.Apply<DevourStrengthPower>(base.Owner.Creature, strGain, base.Owner.Creature, this);

		if (base.IsUpgraded)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target);
			await PowerCmd.Apply<DevourEnemyStrengthDownPower>(cardPlay.Target, strGain, base.Owner.Creature, this);
		}
	}
}
