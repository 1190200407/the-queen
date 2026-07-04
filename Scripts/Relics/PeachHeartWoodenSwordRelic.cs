using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>
/// 桃心木剑：与原版 <c>LastingCandy</c> 相同，在遭遇战 <see cref="CardCreationSource.Encounter"/> 的卡牌奖励选项中追加一张牌；
/// 追加牌来自 <see cref="EnemyCardPool"/>。非首领战每 2 场触发一次（首领战不计数）；首领战卡牌奖励必定追加一张。
/// </summary>

public sealed class PeachHeartWoodenSwordRelic : QueenRelicModel
{
	private int _combatsSeen;
	private bool _isActivating;

	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool ShowCounter => true;

	public override int DisplayAmount
	{
		get
		{
			if (_isActivating)
			{
				return 2;
			}

			return CombatsSeen % 2;
		}
	}

	[SavedProperty]
	public int CombatsSeen
	{
		get => _combatsSeen;
		set
		{
			AssertMutable();
			_combatsSeen = value;
		}
	}

	/// <summary>与 <see cref="MegaCrit.Sts2.Core.Models.Relics.LastingCandy"/> 相同：用于非首领「每两场」奖励。</summary>
	private bool IsInTriggeringCombat => CombatsSeen > 0 && CombatsSeen % 2 == 0;

	public override bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> options, CardCreationOptions creationOptions)
	{
		if (base.Owner != player)
		{
			return false;
		}

		if (creationOptions.Source != CardCreationSource.Encounter)
		{
			return false;
		}

		bool isBossCardReward = creationOptions.RarityOdds == CardRarityOddsType.BossEncounter;
		if (!isBossCardReward && !IsInTriggeringCombat)
		{
			return false;
		}

		IEnumerable<CardModel> pool = ModelDb.CardPool<EnemyCardPool>().GetUnlockedCards(
			player.UnlockState,
			player.RunState.CardMultiplayerConstraint);

		IEnumerable<CardModel> pickFrom = pool.Where(static c => c.CanBeGeneratedInCombat)
			.Where(c => options.TrueForAll(o => o.originalCard.Id != c.Id));
		if (!pickFrom.Any())
		{
			pickFrom = pool.Where(static c => c.CanBeGeneratedInCombat);
		}

		if (!pickFrom.Any())
		{
			return false;
		}

		CardCreationOptions rollOptions = new CardCreationOptions(
				[ModelDb.CardPool<EnemyCardPool>()],
				CardCreationSource.Other,
				creationOptions.RarityOdds,
				c => pickFrom.Any(p => p.Id == c.Id))
			.WithFlags(CardCreationFlags.NoModifyHooks | CardCreationFlags.NoCardPoolModifications);
		CardModel? cardModel = CardFactory.CreateForReward(base.Owner, 1, rollOptions).FirstOrDefault()?.Card;
		if (cardModel != null)
		{
			CardCreationResult result = new CardCreationResult(cardModel);
			result.ModifyCard(cardModel, this);
			options.Add(result);
		}

		return cardModel != null;
	}

	public override Task AfterCombatEnd(CombatRoom room)
	{
		if (room.RoomType == RoomType.Boss)
		{
			InvokeDisplayAmountChanged();
			return Task.CompletedTask;
		}

		CombatsSeen++;
		if (IsInTriggeringCombat)
		{
			TaskHelper.RunSafely(DoActivateVisuals());
		}

		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}

	private async Task DoActivateVisuals()
	{
		_isActivating = true;
		InvokeDisplayAmountChanged();
		Flash();
		await Cmd.Wait(1f);
		_isActivating = false;
		InvokeDisplayAmountChanged();
	}
}
