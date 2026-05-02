using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>饥饿：每层使 <see cref="Devour"/> 效果再翻一倍（总倍率 2^层数）。</summary>
public sealed class HungerPower : QueenPowerModel
{
	public const string MultiplyVarName = "Multiply";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	/// <summary>状态栏数字：当前吞噬倍率（2、4、8…），与 <see cref="DevourEffectMultiplier"/> 一致。</summary>
	public override int DisplayAmount => MultiplierFromStacks(Amount);

	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar(MultiplyVarName, 1m)];

	/// <summary>层数 <paramref name="stacks"/> 时吞噬倍率：2^stacks；0 层为 1。</summary>
	internal static int MultiplierFromStacks(int stacks)
	{
		if (stacks <= 0)
		{
			return 1;
		}

		long result = 1L;
		for (int i = 0; i < stacks; i++)
		{
			result *= 2L;
			if (result > int.MaxValue)
			{
				return int.MaxValue;
			}
		}

		return (int)result;
	}

	internal static decimal DevourEffectMultiplier(Creature? creature)
	{
		if (creature?.GetPower<HungerPower>() is not { } h || h.Amount <= 0)
		{
			return 1m;
		}

		return MultiplierFromStacks(h.Amount);
	}

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		SyncMultiplyVar();
		InvokeDisplayAmountChanged();
		return base.AfterApplied(applier, cardSource);
	}

	public override Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (power == this)
		{
			SyncMultiplyVar();
			InvokeDisplayAmountChanged();
		}

		return Task.CompletedTask;
	}

	private void SyncMultiplyVar()
	{
		base.DynamicVars[MultiplyVarName].BaseValue = MultiplierFromStacks(Amount);
		Player? player = base.Owner.Player;
		if (player?.PlayerCombatState is null)
		{
			return;
		}

		foreach (CardModel card in player.PlayerCombatState.AllCards)
		{
			if (card is Devour devour && devour.Owner == player)
			{
				devour.SyncMultiplyVar();
			}
		}
	}

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card is Devour devour)
        {
            devour.SyncMultiplyVar();
        }
        return Task.CompletedTask;
    }
}
