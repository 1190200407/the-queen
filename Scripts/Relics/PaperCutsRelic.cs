using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;

using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace ComicChess.TheQueen;

/// <summary>纸伤难愈：战斗开始时，所有敌人的最大生命值减少 X（X 为遗物计数）。</summary>
[RegisterRelic(typeof(EventRelicPool))]
public sealed class PaperCutsRelic : ModRelicTemplate
{
	public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => true;

	public override string? CustomIconPath => "res://TheQueen/images/relics/paper_cuts.png";
    public override string? CustomIconOutlinePath => "res://TheQueen/images/relics/paper_cuts_outline.png";
    public override string? CustomBigIconPath => "res://TheQueen/images/relics/big/paper_cuts.png";

	[SavedProperty]
	public int Cuts {get; set;} = 0;
    public override int DisplayAmount => Cuts;

	public void AddCuts(int amount)
	{
		if (amount <= 0)
		{
			return;
		}

		Cuts += amount;
		InvokeDisplayAmountChanged();
	}

	internal static async Task AddCutsOnPickup(Player player, int amount)
	{
		if (amount <= 0)
		{
			return;
		}

		PaperCutsRelic? existing = player.Relics.OfType<PaperCutsRelic>().FirstOrDefault();
		if (existing != null)
		{
			existing.AddCuts(amount);
			return;
		}

		PaperCutsRelic obtained = await RelicCmd.Obtain<PaperCutsRelic>(player);
		obtained.AddCuts(amount);
	}

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, CombatState combatState)
    {
		if (side == base.Owner.Creature.Side && combatState.RoundNumber <= 1)
		{
			Flash();
			foreach (Creature enemy in combatState.Enemies)
			{
				decimal newMaxHp = enemy.MaxHp - Cuts;
				if (newMaxHp < 1m)
				{
					newMaxHp = 1m;
				}

				await CreatureCmd.SetMaxHp(enemy, newMaxHp);
				if (enemy.CurrentHp > newMaxHp)
				{
					await CreatureCmd.SetCurrentHp(enemy, newMaxHp);
				}
			}
		}
	}
}

