using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ComicChess.TheQueen;

[Pool(typeof(SharedPotionPool))]
public sealed class SourceOfClarity : QueenPotionModel
{
	private static readonly Color SplashTint = new("a8e6cf");

	public override TargetType TargetType => TargetType.AnyAlly;

	public override PotionRarity Rarity => PotionRarity.Uncommon;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		if (target is null || target.Player is null)
		{
			return;
		}

		Player recipient = target.Player;
		if (recipient.PlayerCombatState?.Hand.Cards.Any(static c => c.Affliction != null) != true)
		{
			return;
		}

		NCombatRoom.Instance?.PlaySplashVfx(target, SplashTint);
		CardSelectorPrefs prefs = new(base.SelectionScreenPrompt, 1, 1);
		CardModel? picked = (await CardSelectCmd.FromHand(
			choiceContext,
			recipient,
			prefs,
			static c => c.Affliction != null,
			this)).FirstOrDefault();
		if (picked is not null)
		{
			CardCmd.ClearAffliction(picked);
			if (picked.Pile?.Type is PileType pt)
			{
				NCard.FindOnTable(picked)?.UpdateVisuals(pt, CardPreviewMode.Normal);
			}
		}
	}
}
