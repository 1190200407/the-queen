using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

[RegisterCard(typeof(QueenCardPool))]
public sealed class MyLegion : QueenCardModel
{
	private const int energyCost = 2;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Rare;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("SoulLamp", 2)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromPower<SoulLampPower>(),
		HoverTipFactory.FromCard<BindCard>(base.IsUpgraded),
	];

	public MyLegion()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = cardPlay;
		if (base.CombatState is not ICombatState combatState)
		{
			return;
		}

		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

		int soulLamp = (int)base.DynamicVars["SoulLamp"].BaseValue;
		foreach (Player player in combatState.Players)
		{
			if (!player.Creature.IsAlive)
			{
				continue;
			}

			await QueenCardCmd.AddSoulLamp(choiceContext, player, soulLamp);
			await QueenCardCmd.CreateInHand<BindCard>(player, combatState, base.IsUpgraded);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["SoulLamp"].UpgradeValueBy(1m);
	}
}
