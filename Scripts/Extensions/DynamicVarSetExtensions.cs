using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// <see cref="PowerVar{T}"/> 默认以 <c>typeof(T).Name</c> 为键；与旧 BaseLib <c>DynamicVarSetExtensions.Power&lt;T&gt;</c> 一致。
/// RitsuLib 侧请用 <see cref="STS2RitsuLib.Cards.DynamicVars.DynamicVarExtensions.GetValueOrDefault"/> 读标量，或本扩展取 <see cref="DynamicVar"/> 实例。
/// </summary>
public static class DynamicVarSetExtensions
{
    public static DynamicVar Power<T>(this DynamicVarSet vars) where T : PowerModel
        => vars[typeof(T).Name];
}
