using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class Salvage : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [QueenHoverTips.SoulLamp];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Draw", 2m)];

	// 有魂灯时不可打出。
	protected override bool IsPlayable => (base.Owner?.Creature?.GetPower<SoulLampPower>()?.Amount ?? 0) <= 0;

	public Salvage()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CardPileCmd.Draw(choiceContext, base.DynamicVars["Draw"].BaseValue, base.Owner);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["Draw"].UpgradeValueBy(1m);
	}
}
