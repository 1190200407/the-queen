using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>瘴气打击：伤�?+ 随机施加中毒、灾厄或消亡其中一�?3 层�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class MiasmaStrike : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Common;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;
	private const decimal debuffStacks = 3m;

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromPower<PoisonPower>(),
		HoverTipFactory.FromPower<DoomPower>(),
		HoverTipFactory.FromPower<DemisePower>(),
	];

	public MiasmaStrike()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Target is not { IsAlive: true } target || base.Owner.Creature is not { IsAlive: true } applier)
		{
			return;
		}

		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);

		await QueenCardCmd.ApplyRandomTriadDebuff(base.Owner, target, applier, this, debuffStacks);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(3m);
	}
}
