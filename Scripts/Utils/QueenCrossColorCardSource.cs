using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace ComicChess.TheQueen;

/// <summary>
/// 对齐原版 <see cref="MegaCrit.Sts2.Core.Models.Relics.Kaleidoscope"/>：仅使用「非当前角色」的卡池，不含女王等己方角色牌。
/// </summary>
internal static class QueenCrossColorCardSource
{
	public static IEnumerable<CardPoolModel> OtherCharacterPools(Player player) =>
		player.UnlockState.CharacterCardPools.Where(p => p != player.Character.CardPool);

	public static CardCreationOptions WithOtherCharacterPoolsOnly(Player player, CardCreationOptions options)
	{
		if (options.Flags.HasFlag(CardCreationFlags.NoCardPoolModifications))
		{
			return options;
		}

		if (options.CardPools.All(static p => p.IsColorless))
		{
			return options;
		}

		return options.WithCardPools(OtherCharacterPools(player)).WithFilter(options.CardPoolFilter);
	}

	public static List<CardModel> GetOtherCharacterUnlockedCards(Player player) =>
		OtherCharacterPools(player)
			.SelectMany(p => p.GetUnlockedCards(player.UnlockState, player.RunState!.CardMultiplayerConstraint))
			.Where(static c => c.Rarity is not (CardRarity.Basic or CardRarity.Ancient))
			.ToList();
}
