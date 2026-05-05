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

	// 参考 <see cref="DarkVeil"/>：无魂灯时额外效果生效，卡面发金光。
	protected override bool ShouldGlowGoldInternal
	{
		get
		{
			SoulLampPower? lamp = base.Owner?.Creature?.GetPower<SoulLampPower>();
			if (lamp == null || lamp.Amount <= 0)
			{
				return true;
			}

			return base.ShouldGlowGoldInternal;
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [QueenHoverTips.SoulLamp];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new IntVar("Draw", 1m),
		new IntVar("ExtraDraw", 1m),
	];

	public Salvage()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CardPileCmd.Draw(choiceContext, base.DynamicVars["Draw"].BaseValue, base.Owner);

		SoulLampPower? lamp = base.Owner.Creature.GetPower<SoulLampPower>();
		if (lamp == null || lamp.Amount <= 0)
		{
			await CardPileCmd.Draw(choiceContext, base.DynamicVars["ExtraDraw"].BaseValue, base.Owner);
			await QueenCardCmd.AddSoulLamp(base.Owner, 1);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["ExtraDraw"].UpgradeValueBy(1m);
	}
}
