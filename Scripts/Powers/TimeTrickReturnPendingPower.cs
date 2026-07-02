using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>时间把戏：记录待在下回合 energy 重置时移回手牌的标记牌（合并多次打出）。</summary>
public sealed class TimeTrickReturnPendingPower : QueenPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public readonly List<CardModel> CardsToReturn = [];

	public override LocString Description
	{
		get
		{
			LocString d = new("powers", base.Id.Entry + ".description");
			string names = FormatCardTitles(CardsToReturn);
			d.Add("CardNames", string.IsNullOrEmpty(names) ? "—" : names);
			return d;
		}
	}

	private static string FormatCardTitles(IEnumerable<CardModel> cards)
	{
		List<CardModel> distinct = cards.Distinct().ToList();
		if (distinct.Count == 0)
		{
			return "";
		}

		string sep = ListSeparatorForLocale();
		return string.Join(sep, distinct.Select(static c => $"[gold]{c.Title}[/gold]"));
	}

	private static string ListSeparatorForLocale()
	{
		string? lang = LocManager.Instance?.Language;
		if (lang != null && lang.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
		{
			return "、";
		}

		return ", ";
	}

	public override async Task AfterEnergyReset(Player player)
	{
		if (player != base.Owner.Player || base.CombatState == null)
		{
			return;
		}

		ICombatState combatState = base.CombatState;
		foreach (CardModel card in CardsToReturn.Distinct().ToList())
		{
			if (card.HasBeenRemovedFromState || !combatState.ContainsCard(card) || card.Owner != player)
			{
				continue;
			}

			await CardPileCmd.Add(card, PileType.Hand);
		}

		CardsToReturn.Clear();
		await PowerCmd.Remove(this);
	}
}
