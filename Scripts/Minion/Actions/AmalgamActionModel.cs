using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>? <c>params object[]</c> ???????? <see cref="AmalgamActionModel"/> ? <see cref="AmalgamActionModel.Init(object[])"/> ???</summary>
internal static class AmalgamActionArgs
{
    public static bool IsPositive(decimal value) => value > 0m;

    public static bool TryGetDecimal(object[] args, int index, out decimal value)
    {
        if ((uint)index >= (uint)args.Length)
        {
            value = 0m;
            return false;
        }

        return TryCoerceDecimal(args[index], out value);
    }

    public static decimal RequireDecimal(object[] args, int index)
    {
        if (!TryGetDecimal(args, index, out decimal value))
        {
            throw new ArgumentException($"Expected decimal at args[{index}].", nameof(args));
        }

        return value;
    }

    public static bool TryGetInt(object[] args, int index, out int value)
    {
        if ((uint)index >= (uint)args.Length)
        {
            value = 0;
            return false;
        }

        object raw = args[index];
        switch (raw)
        {
            case int i:
                value = i;
                return true;
            case decimal d when d >= int.MinValue && d <= int.MaxValue && d == Math.Truncate(d):
                value = (int)d;
                return true;
            default:
                value = 0;
                return false;
        }
    }

    public static int RequireInt(object[] args, int index)
    {
        if (!TryGetInt(args, index, out int value))
        {
            throw new ArgumentException($"Expected int at args[{index}].", nameof(args));
        }

        return value;
    }

    public static Creature? TryGetCreature(object[] args, int index)
    {
        if ((uint)index >= (uint)args.Length)
        {
            return null;
        }

        return args[index] as Creature;
    }

    public static bool TryGetBool(object[] args, int index, out bool value)
    {
        if ((uint)index >= (uint)args.Length)
        {
            value = false;
            return false;
        }

        if (args[index] is bool b)
        {
            value = b;
            return true;
        }

        value = false;
        return false;
    }

    public static string? TryGetString(object[] args, int index)
    {
        if ((uint)index >= (uint)args.Length)
        {
            return null;
        }

        return args[index] as string;
    }

    public static string RequireString(object[] args, int index)
    {
        string? value = TryGetString(args, index);
        if (value == null)
        {
            throw new ArgumentException($"Expected string at args[{index}].", nameof(args));
        }

        return value;
    }

    private static bool TryCoerceDecimal(object raw, out decimal value)
    {
        switch (raw)
        {
            case decimal d:
                value = d;
                return true;
            case int i:
                value = i;
                return true;
            case float f:
                value = (decimal)f;
                return true;
            case double dbl:
                value = (decimal)dbl;
                return true;
            default:
                value = 0m;
                return false;
        }
    }
}

/// <summary>
/// ????????????????? Power/Action ????
/// ?? <see cref="Init(decimal)"/> / <see cref="Init(object[])"/> ????????????
/// </summary>
public abstract class AmalgamActionModel
{
    protected MoveState? _moveState;

    protected AmalgamActionModel()
    {
    }

    protected AmalgamActionModel(decimal amount)
        : this()
    {
        Amount = amount;
    }

    /// <summary>??? lookup ???????????</summary>
    public abstract string Key { get; }

    /// <summary>? <see cref="decimal"/> ???????? <c>params object[]</c> ???</summary>
    public virtual bool Init(decimal amount) => false;

    /// <summary>??? / ??????????</summary>
    public virtual bool Init(object[] args) => false;

    /// <summary>???????????????????????????</summary>
    public decimal Amount { get; protected set; }

    public MoveState MoveState => _moveState ??= CreateMoveState();

    /// <summary>??????????????????? override ????????</summary>
    protected virtual void ResetForInit()
    {
        Amount = 0m;
        _moveState = null;
    }

    /// <summary>???????????????? / ????????????????????????? <see langword="false"/>?</summary>
    internal virtual bool PoolWhenReturned => true;

    /// <summary>??????????????? <see cref="AmalgamCompositeIntentAction"/> ? <c>_parts</c>??</summary>
    protected virtual void ReturnChildrenToPool()
    {
    }

    internal void PrepareForPoolReturn()
    {
        ReturnChildrenToPool();
        ResetForInit();
    }

    /// <summary>?? Action ???? key?????????????????????</summary>
    protected static string GenericPoolKey(string prefix, Type typeArgument) =>
        $"{prefix}:{typeArgument.Name}";

    /// <summary>
    /// ?????????????????????????????/???????????
    /// ??? <see cref="MemberwiseClone"/> ??? <c>_moveState</c>?????????????? <see cref="AmalgamCompositeIntentAction"/>?????
    /// </summary>
    public virtual AmalgamActionModel Clone()
    {
        AmalgamActionModel copy = (AmalgamActionModel)MemberwiseClone();
        copy._moveState = null;
        return copy;
    }

    /// <summary>??????????????? <see cref="Hook.ModifyDamage"/>????????????????????? <see cref="MoveState"/>?</summary>
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

/// <summary>???? <see cref="decimal"/> ???????????</summary>
public abstract class AmalgamSingleDecimalActionModel : AmalgamActionModel
{
    protected AmalgamSingleDecimalActionModel()
    {
    }

    protected AmalgamSingleDecimalActionModel(decimal amount)
        : this()
    {
        TryInitSingleDecimal(amount);
    }

    protected bool TryInitSingleDecimal(decimal amount)
    {
        if (!AmalgamActionArgs.IsPositive(amount))
        {
            return false;
        }

        ResetForInit();
        Amount = amount;
        return true;
    }

    protected bool TryInitSingleDecimal(object[] args)
    {
        if (!AmalgamActionArgs.TryGetDecimal(args, 0, out decimal amount))
        {
            return false;
        }

        return TryInitSingleDecimal(amount);
    }
}
