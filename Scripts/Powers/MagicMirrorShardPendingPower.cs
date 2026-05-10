using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace ComicChess.TheQueen;

/// <summary>魔镜碎片：记录待在本场战斗结束时加入主牌组的牌（快照）。</summary>
public sealed class MagicMirrorShardPendingPower : QueenPowerModel
{
	private sealed class Data
	{
		public readonly List<SerializableCard> Pending = [];
		public readonly List<string> DisplayTitles = [];
	}

	protected override object? InitInternalData() => new Data();

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override bool IsInstanced => true;

	public override LocString Description
	{
		get
		{
			LocString d = new("powers", base.Id.Entry + ".description");
			Data data = GetInternalData<Data>();
			string names = FormatCardTitles(data.DisplayTitles);
			d.Add("CardNames", string.IsNullOrEmpty(names) ? "—" : names);
			return d;
		}
	}

	private static string FormatCardTitles(IEnumerable<string> titles)
	{
		List<string> list = titles.Where(static s => !string.IsNullOrWhiteSpace(s)).ToList();
		if (list.Count == 0)
		{
			return "";
		}

		string sep = ListSeparatorForLocale();
		return string.Join(sep, list.Select(static t => $"[gold]{t}[/gold]"));
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

	internal void EnqueueSnapshot(SerializableCard snapshot, string formattedTitle)
	{
		Data data = GetInternalData<Data>();
		data.Pending.Add(snapshot);
		data.DisplayTitles.Add(formattedTitle);
	}

	public override async Task AfterCombatEnd(CombatRoom room)
	{
		Player? player = base.Owner.Player;
		if (player is not { RunState: { } runState })
		{
			return;
		}

		CombatRoom? combatRoom = room ?? runState.CurrentRoom as CombatRoom;
		if (combatRoom == null)
		{
			return;
		}

		Data data = GetInternalData<Data>();
		List<SerializableCard> pending = data.Pending.ToList();
		if (pending.Count == 0)
		{
			await PowerCmd.Remove(this);
			return;
		}

		foreach (SerializableCard save in pending)
		{
			CardModel rewardCard = runState.LoadCard(save, player);
			combatRoom.AddExtraReward(player, new SpecialCardReward(rewardCard, player));
		}

		data.Pending.Clear();
		data.DisplayTitles.Clear();
		await PowerCmd.Remove(this);
	}
}
