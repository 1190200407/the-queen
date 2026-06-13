using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

[RegisterSingleton]
public sealed class AmalgamActionPoolModel : SingletonModel
{
    public override bool ShouldReceiveCombatHooks => false;

    private readonly Dictionary<string, Stack<AmalgamActionModel>> _pools = new();

    public T Rent<T>()
        where T : AmalgamActionModel, new()
    {
        string key = new T().Key;
        if (_pools.TryGetValue(key, out Stack<AmalgamActionModel>? stack) && stack.Count > 0)
        {
            return (T)stack.Pop();
        }

        return new T();
    }

    public void Return(AmalgamActionModel instance)
    {
        instance.PrepareForPoolReturn();
        if (!instance.PoolWhenReturned)
        {
            return;
        }

        string key = instance.Key;
        if (!_pools.TryGetValue(key, out Stack<AmalgamActionModel>? stack))
        {
            stack = new Stack<AmalgamActionModel>();
            _pools[key] = stack;
        }

        stack.Push(instance);
    }

    public void Clear() => _pools.Clear();
}
