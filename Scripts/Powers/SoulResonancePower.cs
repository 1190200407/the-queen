using MegaCrit.Sts2.Core.Entities.Powers;

namespace ComicChess.TheQueen;

/// <summary>灵魂同调：你在打出 <see cref="LearnIntentCardModel"/> 时，<see cref="FriendlyAmalgamCmd.LearnIntent"/> 会为其他玩家镜像学习同一意图。</summary>
public sealed class SoulResonancePower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;
}
