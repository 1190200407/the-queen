using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图：每击 <see cref="AmalgamActionModel.Amount"/> 点伤害，连续若干次。</summary>
public sealed class AmalgamMultiHitOffenseIntentAction : AmalgamActionModel
{
    private const string RepeatParam = "repeat";
	private readonly int _hitCount;

	public AmalgamMultiHitOffenseIntentAction(decimal damagePerHit, int hitCount)
		: base(new Dictionary<string, decimal>
		{
			[AmountParam] = damagePerHit,
			[RepeatParam] = hitCount
		})
	{
		_hitCount = (int)GetParameterOrDefault(RepeatParam, 0m);
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
