using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class YouAreMine : QueenCardModel
{
	private const int energyCost = 2;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Rare;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;
	private static readonly LocString YouAreMineDoneLine =
		new ("monsters", "QUEEN.YOU_ARE_MINE.doneLine");

	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Hits", 10m)];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [
		HoverTipFactory.FromPower<VulnerablePower>(),
		HoverTipFactory.FromPower<WeakPower>(),
		.. HoverTipFactory.FromAffliction<Bound>()
	];

	internal override bool HasSelfBound => true;

	public YouAreMine()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.CombatState == null)
		{
			return;
		}

		if (base.Owner.Character is QueenCharacter)
		{
			TalkCmd.Play(YouAreMineDoneLine, base.Owner.Creature, VfxColor.Purple, VfxDuration.Standard);
		}

		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await Cmd.CustomScaledWait(0.5f, 1f);

		int hits = base.DynamicVars["Hits"].IntValue;
		for (int i = 0; i < hits; i++)
		{
			List<Creature> enemies = base.CombatState.HittableEnemies.ToList();
			if (enemies.Count == 0)
			{
				break;
			}

			Creature target = base.Owner.RunState.Rng.CombatCardSelection.NextItem(enemies);
			if (base.Owner.RunState.Rng.CombatCardSelection.NextItem(new List<int> { 0, 1 }) == 0)
			{
				await PowerCmd.Apply<VulnerablePower>(target, 1m, base.Owner.Creature, this);
			}
			else
			{
				await PowerCmd.Apply<WeakPower>(target, 1m, base.Owner.Creature, this);
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["Hits"].UpgradeValueBy(5m);
	}
}
