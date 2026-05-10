using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenPotionPool))]
public sealed class DeathElixir : QueenPotionModel
{
	//#6b4a8a
	private static readonly Color SplashTint = new("6b4a8a");

	public override TargetType TargetType => TargetType.AnyEnemy;

	public override PotionRarity Rarity => PotionRarity.Uncommon;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new PowerVar<PoisonPower>(3m),
		new PowerVar<DoomPower>(3m),
		new PowerVar<DemisePower>(3m),
	];

	public override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<PoisonPower>(),
		HoverTipFactory.FromPower<DoomPower>(),
		HoverTipFactory.FromPower<DemisePower>(),
	];

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		PotionModel.AssertValidForTargetedPotion(target);
		Creature enemy = target!;
		NCombatRoom.Instance?.PlaySplashVfx(enemy, SplashTint);
		Creature applier = base.Owner.Creature;
		await PowerCmd.Apply<PoisonPower>(enemy, 3m, applier, null);
		await PowerCmd.Apply<DoomPower>(enemy, 3m, applier, null);
		await PowerCmd.Apply<DemisePower>(enemy, 3m, applier, null);
	}
}
