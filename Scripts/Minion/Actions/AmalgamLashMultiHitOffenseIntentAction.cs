using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace ComicChess.TheQueen;

/// <summary>
/// 甩动式多段进攻：对齐原版 Phrog Parasite Lash（Attack 动画×1、虫噬命�?VFX、寄生蛙音效）�?/// </summary>
public sealed class AmalgamLashMultiHitOffenseIntentAction : AmalgamActionModel
{
    public override string Key => "lash_offense_multi";

	private int _hitCount;

    public AmalgamLashMultiHitOffenseIntentAction()
    {
    }

	public AmalgamLashMultiHitOffenseIntentAction(decimal damagePerHit, int hitCount)
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
			"AMALGAM_INTENT_LASH",
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
			"Attack",
			0.55f,
			hitVfxPath: "",
			attackerSfxPath: "event:/sfx/enemy/enemy_attacks/phrog_parasite/phrog_parasite_attack",
			PlayWormyImpactVfx);
	}

	private static Task PlayWormyImpactVfx(Creature target)
	{
		NWormyImpactVfx? vfx = NWormyImpactVfx.Create(target);
		if (vfx != null)
		{
			target.GetVfxContainer()?.AddChildSafely(vfx);
		}

		return Task.CompletedTask;
	}
}
