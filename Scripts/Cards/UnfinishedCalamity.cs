using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>未了之祸：能力牌；对所有可攻击敌人各施加一条可叠加的 <see cref="UnfinishedCalamityPower"/>（<see cref="UnfinishedCalamityPower.IsInstanced"/>）；带有该标记的敌人身上其它负面结束时，每条实例对所有可攻击敌人各随机施加 5 层毒/灾厄/消亡之一。</summary>
[Pool(typeof(QueenCardPool))]
public sealed class UnfinishedCalamity : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Power;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.AllEnemies;
	private const bool shouldShowInCardLibrary = true;

	public override int MaxUpgradeLevel => 0;

	protected override IEnumerable<IHoverTip> ExtraHoverTips =>
	[
		HoverTipFactory.FromPower<PoisonPower>(),
		HoverTipFactory.FromPower<DoomPower>(),
		HoverTipFactory.FromPower<DemisePower>(),
	];

	public UnfinishedCalamity()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = choiceContext;
		_ = cardPlay;
		if (base.CombatState is not CombatState combatState)
		{
			return;
		}

		Player player = base.Owner;
		Creature applier = player.Creature;
		foreach (Creature enemy in combatState.HittableEnemies.ToList())
		{
			if (!enemy.IsAlive)
			{
				continue;
			}

			await PowerCmd.Apply<UnfinishedCalamityPower>(enemy, 1m, applier, this);
		}

		await CreatureCmd.TriggerAnim(applier, "Cast", player.Character.CastAnimDelay);
	}

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
