using System.Threading.Tasks;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>
/// 聚合体专用意图基类（独立模型，不走 Power/Action 系统）。
/// </summary>
public abstract class AmalgamActionModel
{
    public const string AmountParam = "amount";

    protected MoveState? _moveState;
    private readonly IReadOnlyDictionary<string, decimal> _parameters;

    protected AmalgamActionModel()
        : this(new Dictionary<string, decimal>())
    {
    }

    protected AmalgamActionModel(decimal amount)
        : this(new Dictionary<string, decimal> { [AmountParam] = amount })
    {
    }

    protected AmalgamActionModel(IReadOnlyDictionary<string, decimal> parameters)
    {
        _parameters = parameters;
        Amount = GetParameterOrDefault(AmountParam, 0m);
    }

    /// <summary>兼容旧逻辑的主数值（等同参数表中的 <c>amount</c>，无则为 0）。</summary>
    public decimal Amount { get; }

    /// <summary>可扩展参数集合（如 <c>amount</c>、<c>repeat</c> 等）。</summary>
    public IReadOnlyDictionary<string, decimal> Parameters => _parameters;

    public decimal GetParameterOrDefault(string key, decimal defaultValue = 0m) =>
        _parameters.TryGetValue(key, out decimal value) ? value : defaultValue;

    public MoveState MoveState => _moveState ??= CreateMoveState();

    /// <summary>
    /// 灵魂同调等：为其他玩家再学同一意图时复制一份，避免多盏灯槽/多名玩家共享同一引用。
    /// 默认同 <see cref="MemberwiseClone"/> 并清空 <c>_moveState</c>；含可变集合子状态的类型（如 <see cref="AmalgamCompositeIntentAction"/>）须重写。
    /// </summary>
    public virtual AmalgamActionModel Clone()
    {
        AmalgamActionModel copy = (AmalgamActionModel)MemberwiseClone();
        copy._moveState = null;
        return copy;
    }

    /// <summary>意图条等展示用；进攻类可在此把 <see cref="Hook.ModifyDamage"/>（出手方为聚合体）与卡面数字对齐。默认等同 <see cref="MoveState"/>。</summary>
    public virtual MoveState GetMoveStateForDisplay(Creature amalgam) => MoveState;

    public async Task ExecuteAsync(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        if (!amalgam.IsAlive || amalgam.CombatState == null)
        {
            return;
        }

        await OnExecute(choiceContext, amalgam);
        await FriendlyAmalgamHook.AfterAct(amalgam.CombatState, choiceContext, amalgam);
    }

    protected abstract MoveState CreateMoveState();

    protected abstract Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam);
}