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
/// 将多个 <see cref="AmalgamActionModel"/> 合并为单条已学意图：
/// 意图条上按顺序展示各子模型在 <see cref="AmalgamActionModel.GetMoveStateForDisplay"/> 中的全部 <see cref="AbstractIntent"/>；
/// 执行时按同一顺序对子模型调用 <see cref="AmalgamActionModel.ExecuteAsync"/>（每个子行动各自走完含 <see cref="FriendlyAmalgamHook.AfterAct"/> 的完整流程）。
/// </summary>
public sealed class AmalgamCompositeIntentAction : AmalgamActionModel
{
    private readonly string _indexKey;
    private readonly List<AmalgamActionModel> _parts;

    /// <param name="compositeIndexKey">整合索引，用于本地化与 <see cref="MoveState"/> 的 id 后缀（如 <c>BOWL_BUG</c>）。</param>
    /// <param name="parts">按顺序合并展示与执行；至少一项。</param>
    public AmalgamCompositeIntentAction(string compositeIndexKey, params AmalgamActionModel[] parts)
        : base(0m)
    {
        ArgumentNullException.ThrowIfNull(parts);
        if (parts.Length == 0)
        {
            throw new ArgumentException("Composite intent requires at least one sub-action.", nameof(parts));
        }

        if (string.IsNullOrWhiteSpace(compositeIndexKey))
        {
            throw new ArgumentException("Composite index key is required.", nameof(compositeIndexKey));
        }

        _indexKey = compositeIndexKey.Trim();
        _parts = new List<AmalgamActionModel>(parts);
    }

    /// <summary>与本地化键 <c>AMALGAM_COMPOSITE.{整合索引}.*</c> 对应的整合索引。</summary>
    public string CompositeIndexKey => _indexKey;

    /// <summary>子行动顺序（只读视图）。</summary>
    public IReadOnlyList<AmalgamActionModel> Parts => _parts;

    public override AmalgamActionModel Clone()
    {
        AmalgamActionModel[] forked = _parts.Select(static p => p.Clone()).ToArray();
        return new AmalgamCompositeIntentAction(_indexKey, forked);
    }

    /// <summary>在原组合末尾追加子行动，并失效展示用 <see cref="AmalgamActionModel.MoveState"/> 缓存。</summary>
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
            $"AMALGAM_COMPOSITE_{_indexKey}",
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
