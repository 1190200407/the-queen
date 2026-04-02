using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class FireWall : QueenCardModel
{
	private const int energyCost = 3;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(16m, ValueProp.Move)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		QueenHoverTips.SoulLamp,
		.. HoverTipFactory.FromAffliction<Bound>()
	];

	internal override bool UseBoundAfflictionOverlayForPreview => true;

	public FireWall()
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

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		await QueenCardCmd.AddSoulLamp(base.Owner);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(6m);
	}
}
