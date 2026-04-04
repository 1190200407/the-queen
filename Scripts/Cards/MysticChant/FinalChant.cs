using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

[Pool(typeof(TokenCardPool))]
public sealed class FinalChant : QueenCardModel
{
	private const int energyCost = 13;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Token;
	private const TargetType targetType = TargetType.AllEnemies;
	private const bool shouldShowInCardLibrary = false;

	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [QueenKeyword.fade];

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(13m, ValueProp.Move),
		new IntVar("Repeat", 1m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [.. HoverTipFactory.FromAffliction<Bound>()];

	internal override bool UseBoundAfflictionOverlayForPreview => true;

	public FinalChant()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.CombatState == null)
		{
			return;
		}

		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.WithHitCount(base.DynamicVars["Repeat"].IntValue)
			.FromCard(this)
			.TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);
	}

	protected override void AddExtraArgsToDescription(LocString description)
	{
		string repeatLine = base.DynamicVars["Repeat"].IntValue > 1
			? $"\n[blue]重复{base.DynamicVars["Repeat"].IntValue}次[/blue]"
			: string.Empty;
		description.Add("RepeatLine", repeatLine);
	}
}
