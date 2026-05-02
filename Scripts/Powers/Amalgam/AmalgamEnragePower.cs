using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ComicChess.TheQueen;

/// <summary>激怒（聚合体版）：每当你打出一张技能牌时，若聚合体存活，则其获得 <see cref="PowerModel.Amount"/> 点力量。</summary>
public sealed class AmalgamEnragePower : QueenPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string? CustomPackedIconPath => "res://images/atlases/power_atlas.sprites/enrage_power.tres";
    public override string? CustomBigIconPath => "res://images/powers/enrage_power.png";

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner.PetOwner || cardPlay.Card.Type != CardType.Skill || Amount <= 0m)
        {
            return;
        }

        Player? player = base.Owner.PetOwner;
        if (player == null || base.Owner.CombatState is not { } combatState)
        {
            return;
        }

        Creature? amalgam = FriendlyAmalgamCmd.GetExisting(combatState, player);
        if (amalgam is not { IsAlive: true } targetAmalgam)
        {
            return;
        }

        Flash();
        await Cmd.Wait(0.5f);
        await PowerCmd.Apply<StrengthPower>(targetAmalgam, Amount, base.Owner, null);
    }
}

