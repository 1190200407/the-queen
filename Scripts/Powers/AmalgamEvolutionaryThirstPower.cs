using MegaCrit.Sts2.Core.Entities.Powers;

namespace ComicChess.TheQueen;

public sealed class AmalgamEvolutionaryThirstPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;

	public override bool ShouldPlayVfx => false;
}
