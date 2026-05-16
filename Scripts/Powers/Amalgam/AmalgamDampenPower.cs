using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>施加抑制：回合开始时，降级 1 张手牌，并升级其余所有手牌。类名不可为 <c>DampenPower</c>（与原版 ModelId 冲突）。</summary>
public sealed class AmalgamDampenPower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override string? CustomIconPath => "res://images/atlases/power_atlas.sprites/dampen_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/dampen_power.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != base.Owner.Player || player.PlayerCombatState?.Hand is not { } hand)
        {
            return;
        }

        CardModel? toDowngrade = (await CardSelectCmd.FromHand(
                choiceContext,
                player,
                new CardSelectorPrefs(new LocString("powers", "COMICCHESS-AMALGAM_DAMPEN_POWER.selectionPrompt"), 1),
                c => c.IsUpgraded,
                source: this))
            .FirstOrDefault();

        if (toDowngrade == null)
        {
            return;
        }

        Flash();

        foreach (CardModel c in hand.Cards)
        {
            CardCmd.Upgrade(c);
        }
        CardCmd.Downgrade(toDowngrade);
    }
}
