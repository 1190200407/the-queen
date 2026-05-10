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
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ComicChess.TheQueen;

/// <summary>魔镜碎片：从手牌选1张非衍生牌，战斗结束时加入牌组。</summary>
[Pool(typeof(TokenPotionPool))]
public sealed class MagicMirrorShard : QueenPotionModel
{
	private static readonly Color SplashTint = new("c8b8e8");

	public override TargetType TargetType => TargetType.Self;

	public override PotionRarity Rarity => PotionRarity.Token;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		_ = target;
		Player owner = base.Owner;
		if (owner.PlayerCombatState?.Hand.Cards.Any(c => Hook.ShouldAddToDeck(owner.RunState, c, out _)) != true)
		{
			return;
		}

		NCombatRoom.Instance?.PlaySplashVfx(owner.Creature, SplashTint);
		CardSelectorPrefs prefs = new(base.SelectionScreenPrompt, 1, 1);
		CardModel? picked = (await CardSelectCmd.FromHand(
			choiceContext,
			owner,
			prefs,
			c => Hook.ShouldAddToDeck(owner.RunState, c, out _),
			this)).FirstOrDefault();

		if (picked == null)
		{
			return;
		}

		picked.AssertMutable();
		SerializableCard snapshot = picked.ToSerializable();

		MagicMirrorShardPendingPower? pending = owner.Creature.GetPower<MagicMirrorShardPendingPower>();
		if (pending == null)
		{
			pending = await PowerCmd.Apply<MagicMirrorShardPendingPower>(owner.Creature, 1m, owner.Creature, null);
		}

		pending?.EnqueueSnapshot(snapshot, picked.Title);
	}
}
