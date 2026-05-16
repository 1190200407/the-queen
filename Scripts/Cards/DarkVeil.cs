using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;


public sealed class DarkVeil : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Common;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	public override bool GainsBlock => true;

	// BaseLib/引擎在「无魂灯、额外格挡生效」时仍给 ShouldGlowGoldInternal=true，与 OnPlay 判定相反；0 魂灯时强制不要金闪。
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

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<SoulLampPower>()];

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new BlockVar(5m, ValueProp.Move),
		new BlockVar("BonusBlock", 5m, ValueProp.Move)
	];

	public DarkVeil()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);

		SoulLampPower? lamp = base.Owner.Creature.GetPower<SoulLampPower>();
		if (lamp == null || lamp.Amount <= 0)
		{
			await CreatureCmd.GainBlock(base.Owner.Creature, (BlockVar)base.DynamicVars["BonusBlock"], cardPlay);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(2m);
	}
}
