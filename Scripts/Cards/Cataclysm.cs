using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>灾变：目标下 1 次（升级：2 次）受到未被格挡的攻击伤害时，随机获得等同于该伤害的毒/灾厄/消亡之一。可叠加。魂缚。</summary>
[RegisterCard(typeof(QueenCardPool))]
public sealed class Cataclysm : QueenCardModel
{
	private const int energyCost = 2;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Rare;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = true;

	internal override bool HasSelfBound => true;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromPower<CataclysmPower>(),
		HoverTipFactory.FromPower<PoisonPower>(),
		HoverTipFactory.FromPower<DoomPower>(),
		HoverTipFactory.FromPower<DemisePower>(),
		.. HoverTipFactory.FromAffliction<Bound>(),
	];

	public Cataclysm()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		Creature target = cardPlay.Target;
		if (!target.IsAlive || base.Owner.Creature is not { IsAlive: true } applier)
		{
			return;
		}

		decimal triggerCount = base.IsUpgraded ? 2m : 1m;
		await PowerCmd.Apply<CataclysmPower>(choiceContext, target, triggerCount, applier, this);
		await CreatureCmd.TriggerAnim(applier, "Cast", base.Owner.Character.CastAnimDelay);
	}
}
