using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图：为你的角色获得 <see cref="AmalgamActionModel.Amount"/> 点格挡。</summary>
public sealed class AmalgamGainBlockIntentAction : AmalgamActionModel
{
	public AmalgamGainBlockIntentAction(decimal block) : base(block)
	{
	}

	public static readonly float CastAnimDelay = 1.5f;

	public override LocString IntentTitle => new("monsters", "FRIENDLY_AMALGAM.intent_block.title");

	public override LocString GetIntentDescription()
	{
		var desc = new LocString("monsters", "FRIENDLY_AMALGAM.intent_block.description");
		desc.Add("Amount", Amount);
		return desc;
	}

	protected override MoveState CreateMoveState()
	{
		return new MoveState(
			"AMALGAM_INTENT_BLOCK",
			_ => Task.CompletedTask,
			new AmalgamGainBlockIntent(Amount));
	}

	protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
	{
		_ = choiceContext;
		if (amalgam.PetOwner is not { Creature: { } owner } || !owner.IsAlive)
		{
			return;
		}

		await CreatureCmd.TriggerAnim(amalgam, "Cast", CastAnimDelay);
		await CreatureCmd.GainBlock(owner, Amount, ValueProp.Move, null);
	}
}
