using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class OppressivePresence : QueenCardModel
{
	private const int energyCost = 2;
	private const CardType type = CardType.Attack;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(3m, ValueProp.Move),
		new IntVar("Hits", 4m)
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromCard<Devour>(),
		.. HoverTipFactory.FromAffliction<Bound>()
	];

	internal override bool HasSelfBound => true;

	public OppressivePresence()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.CombatState == null || base.Owner.PlayerCombatState == null)
		{
			return;
		}

		// 嵌套 AutoPlay 时若对后续牌 skipCardPileVisuals=true，牌堆与节点同步可能异常，导致
		// CardModel.Pile（Hand 优先于 Play 解析）仍落在手牌，OnPlayWrapper 末尾不会走 Exhaust，
		// 出现「效果触发但牌未进消耗堆」。因此每张吞噬都完整走打出区流程。
		List<CardModel> devoursInHand = base.Owner.PlayerCombatState.Hand.Cards.Where(static c => c is Devour).ToList();
		foreach (CardModel card in devoursInHand)
		{
			if (card.Pile?.Type != PileType.Hand)
			{
				continue;
			}

			Creature? devourTarget = null;
			if (card is Devour devour)
			{
				if (devour.IsUpgraded)
				{
					List<Creature> enemies = base.CombatState.HittableEnemies.ToList();
					if (enemies.Count == 0)
					{
						continue;
					}

					devourTarget = base.Owner.RunState.Rng.CombatCardSelection.NextItem(enemies);
				}
				else
				{
					devourTarget = base.Owner.Creature;
				}
			}
			else
			{
				devourTarget = base.Owner.Creature;
			}

			await CardCmd.AutoPlay(
				choiceContext,
				card,
				target: devourTarget,
				AutoPlayType.Default,
				skipXCapture: false,
				skipCardPileVisuals: false);
		}

		decimal hitDamage = base.DynamicVars.Damage.BaseValue;
		int hits = base.DynamicVars["Hits"].IntValue;
		for (int i = 0; i < hits; i++)
		{
			List<Creature> enemies = base.CombatState.HittableEnemies.ToList();
			if (enemies.Count == 0)
			{
				break;
			}

			if (base.Owner.RunState.Rng.CombatCardSelection.NextItem(enemies) is not { } target)
			{
				break;
			}

			await DamageCmd.Attack(hitDamage)
				.FromCard(this)
				.Targeting(target)
				.WithHitFx("vfx/vfx_attack_blunt")
				.Execute(choiceContext);
		}
	}

    protected override void OnUpgrade()
    {
		base.DynamicVars["Hits"].UpgradeValueBy(1m);
    }
}
