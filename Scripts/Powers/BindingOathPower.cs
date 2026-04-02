using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;

namespace ComicChess.TheQueen;

/// <summary>
/// 魂缚誓约：只限制你每回合最多打出 1 张带 [gold]魂缚(Bound)[/gold] 的卡牌。
/// </summary>
public sealed class BindingOathPower : QueenPowerModel
{
	private sealed class Data
	{
		public bool boundCardPlayed;
	}

	public override PowerType Type => PowerType.None;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		CardModel card = cardPlay.Card;
		if (card.IsDupe)
		{
			return Task.CompletedTask;
		}

		if (card.Owner.Creature != base.Owner)
		{
			return Task.CompletedTask;
		}

		if (!(card.Affliction is Bound))
		{
			return Task.CompletedTask;
		}

		GetInternalData<Data>().boundCardPlayed = true;
		return Task.CompletedTask;
	}

	public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
	{
		// 只限制本 Power 拥有者的一侧
		if (card.Owner.Creature != base.Owner)
		{
			return true;
		}
		if (!(card.Affliction is Bound))
		{
			return true;
		}

		// 只要你仍有魂灯层数，则所有魂缚牌都可以无视“每回合仅 1 张”限制。
		SoulLampPower? lamp = base.Owner.GetPower<SoulLampPower>();
		if (lamp != null && lamp.Amount > 0)
		{
			return true;
		}

		return !GetInternalData<Data>().boundCardPlayed;
	}

	public override Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		// 每回合开始前允许再打出 1 张 Bound 卡；但不移除 Bound 本身
		GetInternalData<Data>().boundCardPlayed = false;
		return Task.CompletedTask;
	}
}

