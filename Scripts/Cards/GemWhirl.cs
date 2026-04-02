using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class GemWhirl : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Rare;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [QueenKeyword.fade];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Draw", 1m)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromKeyword(QueenKeyword.fade),
		.. HoverTipFactory.FromAffliction<Bound>()
	];

	internal override bool UseBoundAfflictionOverlayForPreview => true;

	public GemWhirl()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	public override async Task BeforeCombatStart()
	{
		if (base.Owner?.Creature?.CombatState == null)
		{
			return;
		}
		if (Affliction is not Bound)
		{
			await CardCmd.Afflict<Bound>(this, 1m);
		}
	}

	// 与原版 Bombardment / HowlFromBeyond 一样：回合开始摸牌前，若在消耗堆则自动打出。
	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		if (player != base.Owner)
		{
			return;
		}
		if (base.Pile?.Type == PileType.Exhaust)
		{
			await CardCmd.AutoPlay(choiceContext, this, null);
		}
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CardPileCmd.Draw(choiceContext, base.DynamicVars["Draw"].BaseValue, base.Owner);
		// 每次打出后下次多抽 1 张（线性增长）；不用重放，避免与回合初自动打出叠加指数增长。
		base.DynamicVars["Draw"].BaseValue += 1m;
		CardCmd.Preview(this);
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
