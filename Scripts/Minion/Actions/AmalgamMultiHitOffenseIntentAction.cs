using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图：每击 <see cref="AmalgamActionModel.Amount"/> 点伤害，连续若干次�?/summary>
public sealed class AmalgamMultiHitOffenseIntentAction : AmalgamActionModel
{
    public override string Key => "offense_multi";

    private int _hitCount;

    public AmalgamMultiHitOffenseIntentAction()
    {
    }

	public AmalgamMultiHitOffenseIntentAction(decimal damagePerHit, int hitCount)
        : this()
	{
        Amount = damagePerHit;
		_hitCount = hitCount;
	}

    protected override void ResetForInit()
    {
        base.ResetForInit();
        _hitCount = 0;
    }

    public override bool Init(decimal amount) => false;

    public override bool Init(object[] args)
    {
        if (!AmalgamActionArgs.TryGetDecimal(args, 0, out decimal damagePerHit)
            || !AmalgamActionArgs.TryGetInt(args, 1, out int hitCount)
            || !AmalgamActionArgs.IsPositive(damagePerHit)
            || hitCount <= 0)
        {
            return false;
        }

        ResetForInit();
        Amount = damagePerHit;
        _hitCount = hitCount;
        return true;
    }

	protected override MoveState CreateMoveState()
	{
		return new MoveState(
			"AMALGAM_INTENT_OFFENSE_MULTI",
			_ => Task.CompletedTask,
			new AmalgamMultiHitAttackIntent(Amount, _hitCount));
	}

	protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
	{
		await FriendlyAmalgamCmd.ExecuteMultiHitOffense(
			choiceContext,
			amalgam,
			target: null,
			Amount,
			_hitCount,
			"PowerAttack",
			0.7f,
			"vfx/vfx_attack_blunt",
			"event:/sfx/enemy/enemy_attacks/torch_head_amalgam/torch_head_amalgam_beam");
	}
}
