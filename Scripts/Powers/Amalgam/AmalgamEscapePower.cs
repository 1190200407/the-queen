using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace ComicChess.TheQueen;
public sealed class AmalgamEscapePower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (base.Owner.PetOwner is not Player amalgamOwner)
            return;

        await PowerCmd.Decrement(this);
        if (Amount > 0)
            return;

        // 聚合体逃跑（与打出 Flee 相同；显式登记，避免仅依赖 Remove 内部顺序或 PetOwner 时机）
        if (base.CombatState is { } combatState)
        {
            AmalgamFledSummonBlock.MarkAmalgamFled(combatState, amalgamOwner);
            await FriendlyAmalgamHook.OnEscape(combatState, base.Owner);
            Flee.RemoveFromCombatWithoutEscapeFlag(combatState, base.Owner);
        }
    }
}