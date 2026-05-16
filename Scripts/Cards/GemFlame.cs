using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>宝石火焰：单体伤害，打出后将此牌侵蚀为魂缚�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class GemFlame : QueenCardModel
{
	private const int energyCost = 2;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Common;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(13m, ValueProp.Move)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [.. HoverTipFactory.FromAffliction<Bound>()];

	internal override bool HasSelfBound => false;

	public GemFlame()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Target is not { IsAlive: true } target)
		{
			return;
		}

		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
			.FromCard(this)
			.Targeting(target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);

		if (Affliction is not Bound)
		{
			await CardCmd.Afflict<Bound>(this, 1m);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(5m);
	}
}
