using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>一个意图：获得格挡 + 下回合开始造成伤害（以能力实现）�?/summary>
public sealed class AmalgamReloadFireIntentAction : AmalgamActionModel
{
    public override string Key => "reload_fire";

    private readonly decimal _block;
    private readonly decimal _damage;

    public AmalgamReloadFireIntentAction(decimal block, decimal nextTurnDamage)
    {
        _block = block;
        _damage = nextTurnDamage;
    }

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_RELOAD_FIRE",
            _ => Task.CompletedTask,
            new AmalgamGainBlockIntent(_block),
            new AmalgamGainBuffIntent("AMALGAM_NEXT_ROUND_ATTACK_POWER", _damage));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        if (amalgam.PetOwner is not { Creature: { } queen } || !queen.IsAlive)
        {
            return;
        }

        if (_block > 0m)
        {
            await CreatureCmd.TriggerAnim(amalgam, "Cast", AmalgamGainBlockIntentAction.CastAnimDelay);
            await CreatureCmd.GainBlock(queen, _block, ValueProp.Move, null);
        }

        if (_damage > 0m)
        {
            await PowerCmd.Apply<AmalgamNextRoundAttackPower>(amalgam, _damage, applier: queen, cardSource: null, silent: true);
        }
    }
}

