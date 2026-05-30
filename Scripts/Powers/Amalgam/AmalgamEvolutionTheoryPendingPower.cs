using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>进化论：下回合开始时将记录的意图以 <see cref="AmalgamCompositeKey.Evolution"/> 重新聚合学习。</summary>
public sealed class AmalgamEvolutionTheoryPendingPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public readonly List<AmalgamActionModel> PendingIntents = [];

    public void AddPendingIntents(IEnumerable<AmalgamActionModel> intents)
    {
        foreach (AmalgamActionModel intent in intents)
        {
            PendingIntents.Add(intent.Clone());
        }
    }

    public override async Task AfterSideTurnStartLate(CombatSide side, IReadOnlyList<Creature> participants, CombatState combatState)
    {
        _ = participants;
        _ = combatState;

        if (!base.Owner.IsAlive || side != base.Owner.Side || PendingIntents.Count == 0)
        {
            return;
        }

        if (base.Owner.PetOwner is not Player queen)
        {
            await PowerCmd.Remove(this);
            return;
        }

        if (base.Owner.Monster is not FriendlyAmalgam amalgam || !amalgam.Creature.IsAlive)
        {
            await PowerCmd.Remove(this);
            return;
        }

        CardModel source = ModelDb.Card<EvolutionTheory>();
        var choiceContext = new ThrowingPlayerChoiceContext();
        IReadOnlyList<AmalgamActionModel> toRelearn = PendingIntents.ToArray();
        PendingIntents.Clear();

        Flash();

        foreach (AmalgamActionModel intent in toRelearn)
        {
            await FriendlyAmalgamCmd.CombineIntent(
                choiceContext,
                queen,
                intent,
                source,
                AmalgamCompositeKey.Evolution);
        }

        await PowerCmd.Remove(this);
    }
}
