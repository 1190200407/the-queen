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

/// <summary>类虚弱：机制同原版 <see cref="MegaCrit.Sts2.Core.Models.Powers.WeakPower"/>，独立 Power 类型以便与虚弱并存。</summary>
public sealed class SplitWeakPower : ModPowerTemplate
{
	private const string VanillaLocEntry = "WEAK_POWER";
	private const string DamageDecreaseKey = "DamageDecrease";

	private static readonly PowerAssetProfile VanillaAssets = ContentAssetProfiles.Power(VanillaLocEntry);

	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override PowerAssetProfile AssetProfile => VanillaAssets;

	public override string? CustomIconPath => VanillaAssets.IconPath;

	public override string? CustomBigIconPath => VanillaAssets.BigIconPath;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(DamageDecreaseKey, 0.75m)];

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, MegaCrit.Sts2.Core.Entities.Cards.CardPlay? cardPlay)
	{
		if (dealer != base.Owner)
		{
			return 1m;
		}

		if (!props.IsPoweredAttack())
		{
			return 1m;
		}

		decimal multiplier = base.DynamicVars[DamageDecreaseKey].BaseValue;
		PaperKrane? paperKrane = target?.Player?.GetRelic<PaperKrane>();
		if (paperKrane != null)
		{
			multiplier = paperKrane.ModifyWeakMultiplier(target, multiplier, props, dealer, cardSource);
		}

		DebilitatePower? debilitate = dealer.GetPower<DebilitatePower>();
		if (debilitate != null)
		{
			multiplier = debilitate.ModifyWeakMultiplier(dealer, multiplier, props, dealer, cardSource);
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
