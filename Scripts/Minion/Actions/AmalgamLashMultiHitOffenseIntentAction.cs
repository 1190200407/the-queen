using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace ComicChess.TheQueen;

/// <summary>
/// 甩动式多段进攻：对齐原版 Phrog Parasite Lash（Attack 动画×1、虫噬命中 VFX、寄生蛙音效）。
/// </summary>
public sealed class AmalgamLashMultiHitOffenseIntentAction : AmalgamActionModel
{
	private const string RepeatParam = "repeat";

	private readonly int _hitCount;

	public AmalgamLashMultiHitOffenseIntentAction(decimal damagePerHit, int hitCount)
		: base(new Dictionary<string, decimal>
		{
			[AmountParam] = damagePerHit,
			[RepeatParam] = hitCount,
		})
	{
		_hitCount = (int)GetParameterOrDefault(RepeatParam, 0m);
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
