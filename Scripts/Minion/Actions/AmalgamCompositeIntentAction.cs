using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>
/// 将多�?<see cref="AmalgamActionModel"/> 合并为单条已学意图：
/// 意图条上按顺序展示各子模型在 <see cref="AmalgamActionModel.GetMoveStateForDisplay"/> 中的全部 <see cref="AbstractIntent"/>�?/// 执行时按同一顺序对子模型调用 <see cref="AmalgamActionModel.ExecuteAsync"/>（每个子行动各自走完�?<see cref="FriendlyAmalgamHook.AfterAct"/> 的完整流程）�?/// </summary>
public sealed class AmalgamCompositeIntentAction : AmalgamActionModel
{
    public override string Key => _compositeKey.ToString();

    private readonly AmalgamCompositeKey _compositeKey;
    private readonly List<AmalgamActionModel> _parts;

    public AmalgamCompositeIntentAction(AmalgamCompositeKey compositeKey, params AmalgamActionModel[] parts)
    {
        ArgumentNullException.ThrowIfNull(parts);
        if (parts.Length == 0)
        {
            throw new ArgumentException("Composite intent requires at least one sub-action.", nameof(parts));
        }

        if (compositeKey == AmalgamCompositeKey.None)
        {
            throw new ArgumentException("Composite intent requires a composite key.", nameof(compositeKey));
        }

        _compositeKey = compositeKey;
        _parts = new List<AmalgamActionModel>(parts);
    }

    public AmalgamCompositeKey CompositeKey => _compositeKey;

    public IReadOnlyList<AmalgamActionModel> Parts => _parts;

    internal override bool PoolWhenReturned => false;

    protected override void ReturnChildrenToPool()
    {
        foreach (AmalgamActionModel part in _parts)
        {
            AmalgamActionRegistry.Return(part);
        }
    }

    public override AmalgamActionModel Clone()
    {
        AmalgamActionModel[] forked = _parts.Select(static p => p.Clone()).ToArray();
        return new AmalgamCompositeIntentAction(_compositeKey, forked);
    }

    public void AddPart(AmalgamActionModel part)
    {
        ArgumentNullException.ThrowIfNull(part);
        _parts.Add(part);
        _moveState = CreateMoveState();
    }

    protected override MoveState CreateMoveState() => BuildCombinedMoveState(static p => p.MoveState);

    public override MoveState GetMoveStateForDisplay(Creature amalgam) =>
        BuildCombinedMoveState(p => p.GetMoveStateForDisplay(amalgam));

    private MoveState BuildCombinedMoveState(Func<AmalgamActionModel, MoveState> getState)
    {
        var intents = new List<AbstractIntent>();
        foreach (AmalgamActionModel part in _parts)
        {
            foreach (AbstractIntent intent in getState(part).Intents)
            {
                intents.Add(intent);
            }
        }

        return new MoveState(
            $"AMALGAM_COMPOSITE_{_compositeKey}",
            _ => Task.CompletedTask,
            intents.ToArray());
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        foreach (AmalgamActionModel part in _parts)
        {
            await part.ExecuteAsync(choiceContext, amalgam);
        }
    }
}
