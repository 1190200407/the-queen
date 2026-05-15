using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ComicChess.TheQueen;

/// <summary>逃跑：聚合体脱离战斗，并结算偷到的奖励。（不登记为原版 CreatureEscaped）</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class Flee : QueenCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override int MaxUpgradeLevel => 0;

    public Flee()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        CombatState? combatState = base.Owner?.Creature?.CombatState;
        if (combatState is null || base.Owner is null)
        {
            return;
        }

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgam is null)
        {
            return;
        }

        await FriendlyAmalgamHook.OnEscape(combatState, amalgam);
        RemoveFromCombatWithoutEscapeFlag(combatState, amalgam);
    }

    internal static void RemoveFromCombatWithoutEscapeFlag(CombatState combatState, Creature creature)
    {
        if (creature.IsDead)
        {
            return;
        }

        if (creature.Monster is FriendlyAmalgam && creature.PetOwner is Player amalgamOwner)
        {
            AmalgamFledSummonBlock.MarkAmalgamFled(combatState, amalgamOwner);
        }

        creature.RemoveAllPowersInternalExcept();

        NCreature? nCreature = NCombatRoom.Instance?.GetCreatureNode(creature);
        if (nCreature != null)
        {
            NCombatRoom.Instance?.RemoveCreatureNode(nCreature);
            nCreature.ToggleIsInteractable(on: false);
            nCreature.Visible = false;
        }

        CombatManager.Instance.RemoveCreature(creature);
        combatState.RemoveCreature(creature);
    }
}

