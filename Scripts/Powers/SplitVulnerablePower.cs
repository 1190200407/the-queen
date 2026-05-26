using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Scaffolding.Content;

namespace ComicChess.TheQueen;

/// <summary>类易伤：机制同原版 <see cref="MegaCrit.Sts2.Core.Models.Powers.VulnerablePower"/>，独立 Power 类型以便与易伤并存。</summary>
public sealed class SplitVulnerablePower : ModPowerTemplate
{
	private const string VanillaLocEntry = "VULNERABLE_POWER";
	private const string DamageIncreaseKey = "DamageIncrease";

	private static readonly PowerAssetProfile VanillaAssets = ContentAssetProfiles.Power(VanillaLocEntry);

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override PowerAssetProfile AssetProfile => VanillaAssets;

	public override string? CustomIconPath => VanillaAssets.IconPath;

	public override string? CustomBigIconPath => VanillaAssets.BigIconPath;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(DamageIncreaseKey, 1.5m)];

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target != base.Owner)
		{
			return 1m;
		}

		if (!props.IsPoweredAttack())
		{
			return 1m;
		}

		decimal multiplier = base.DynamicVars[DamageIncreaseKey].BaseValue;
		if (dealer != null)
		{
			PaperPhrog? paperPhrog = dealer.Player?.GetRelic<PaperPhrog>();
			if (paperPhrog != null)
			{
				multiplier = paperPhrog.ModifyVulnerableMultiplier(target, multiplier, props, dealer, cardSource);
			}

			CrueltyPower? cruelty = dealer.GetPower<CrueltyPower>();
			if (cruelty != null)
			{
				multiplier = cruelty.ModifyVulnerableMultiplier(target, multiplier, props, dealer, cardSource);
			}
		}

		DebilitatePower? debilitate = target.GetPower<DebilitatePower>();
		if (debilitate != null)
		{
			multiplier = debilitate.ModifyVulnerableMultiplier(target, multiplier, props, dealer, cardSource);
		}

		return multiplier;
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side == CombatSide.Enemy)
		{
			await PowerCmd.TickDownDuration(this);
		}
	}
}
