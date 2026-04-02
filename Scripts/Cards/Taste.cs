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
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class Taste : QueenCardModel
{
	private const int energyCost = 2;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.AllEnemies;
	private const bool shouldShowInCardLibrary = true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<WeakPower>(2m)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<WeakPower>(),
		QueenHoverTips.SoulLamp
	];

	public Taste()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.CombatState == null)
		{
			return;
		}

		decimal weakStacks = base.DynamicVars.Weak.BaseValue;
		List<Creature> enemies = base.CombatState.HittableEnemies.ToList();
		foreach (Creature enemy in enemies)
		{
			await PowerCmd.Apply<WeakPower>(enemy, weakStacks, base.Owner.Creature, this);
		}

		await QueenCardCmd.AddSoulLamp(base.Owner, enemies.Count);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Weak.UpgradeValueBy(1m);
	}
}
