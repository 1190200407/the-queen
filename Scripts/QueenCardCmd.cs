using System.Threading.Tasks;

using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Helpers;
using STS2RitsuLib.Audio;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;

namespace ComicChess.TheQueen;

public static class QueenCardCmd
{
	private const string SoulLampGainSfx = "event:/sfx/characters/regent/regent_forge";
	private const float SoulLampGainSfxVolume = 1.2f;
	private const float SoulLampGainSfxPitch = 2f;

	/// <summary>
	/// 施加魂缚：战斗内走 <see cref="CardCmd.Afflict"/>；战斗外仅允许已在主牌组中的牌，直接 <see cref="CardModel.AfflictInternal"/>（商店改牌等）。
	/// </summary>
	public static async Task<bool> TryAfflictBoundOnCard(CardModel card, decimal amount)
	{
		if (card.Affliction != null)
		{
			return false;
		}

		ICombatState? cs = card.CombatState ?? card.Owner?.Creature?.CombatState;
		if (cs != null && card.Owner?.RunState?.CurrentRoom is CombatRoom)
		{
			AfflictionModel? applied = await CardCmd.Afflict<Bound>(card, amount);
			return applied != null;
		}

		if (card.Pile?.Type != PileType.Deck || card.Owner?.RunState == null)
		{
			return false;
		}

		AfflictionModel affliction = ModelDb.Affliction<Bound>().ToMutable();
		if (!affliction.CanAfflict(card))
		{
			return false;
		}

		card.AfflictInternal(affliction, amount);
		affliction.AfterApplied();
		return true;
	}

	public static async Task CreateInHand<T>(Player owner, ICombatState combatState, bool isUpgraded = false) where T : CardModel
	{
		CardModel card = combatState.CreateCard<T>(owner);
		await CreateInHandInternal(card, isUpgraded, owner);
	}

	private static async Task CreateInHandInternal(CardModel card, bool isUpgraded = false, Player? creator = null)
	{
		if (isUpgraded)
		{
			CardCmd.Upgrade(card);
		}

		await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, creator);
	}

	public static async Task AddSoulLamp(PlayerChoiceContext choiceContext, Player owner, int amount = 1)
	{
		if (amount <= 0)
		{
			return;
		}

		bool silent = owner.Character is QueenCharacter && LocalContext.IsMe(owner);

		SoulLampPower? existing = owner.Creature.GetPower<SoulLampPower>();
		if (existing == null)
		{
			await PowerCmd.Apply<SoulLampPower>(choiceContext, owner.Creature, amount, owner.Creature, null, silent);
		}
		else if (existing.Amount <= 0)
		{
			// SoulLampPower uses -1 as the hidden "display 0" sentinel.
			// When gaining Soul Lamp from this state, jump directly to gained amount.
			await PowerCmd.ModifyAmount(choiceContext, existing, amount - existing.Amount, owner.Creature, null, silent);
		}
		else
		{
			await PowerCmd.ModifyAmount(choiceContext, existing, amount, owner.Creature, null, silent);
		}

		if (LocalContext.IsMe(owner))
		{
			PlaySoulLampGainSfx();
		}
	}

	private static void PlaySoulLampGainSfx()
	{
		if (NonInteractiveMode.IsActive || CombatManager.Instance.IsEnding)
		{
			return;
		}

		GodotObject? instance = FmodStudioEventInstances.TryCreate(SoulLampGainSfx);
		if (instance is null)
		{
			SfxCmd.Play(SoulLampGainSfx, SoulLampGainSfxVolume);
			return;
		}

		instance.Call("set_volume", SoulLampGainSfxVolume);
		instance.Call("set_pitch", SoulLampGainSfxPitch);
		instance.Call("start");
		instance.Call("release");
	}

	private static readonly QueenTriadDebuffKind[] TriadDebuffKinds =
	[
		QueenTriadDebuffKind.Poison,
		QueenTriadDebuffKind.Doom,
		QueenTriadDebuffKind.Demise,
	];

	public static async Task ApplyRandomTriadDebuff(
		PlayerChoiceContext choiceContext,
		Player owner,
		Creature target,
		Creature applier,
		CardModel? cardSource,
		decimal amount)
	{
		QueenTriadDebuffKind kind = PickTriadDebuff(owner, target);
		await ApplyTriadDebuff(choiceContext, kind, target, applier, cardSource, amount);
	}

	/// <summary>
	/// 按目标当前层数加权：尚未拥有的类型权重大，已有层数越高权重略降，便于尽快「三种都挂上」再偏向往层数低的一侧叠。
	/// </summary>
	public static QueenTriadDebuffKind PickTriadDebuff(Player owner, Creature target)
	{
		Rng rng = owner.RunState.Rng.CombatCardSelection;
		QueenTriadDebuffKind? picked = rng.WeightedNextItem(TriadDebuffKinds, k => WeightForTriadKind(target, k));
		return picked ?? QueenTriadDebuffKind.Poison;
	}

	public static async Task ApplyTriadDebuff(
		PlayerChoiceContext choiceContext,
		QueenTriadDebuffKind kind,
		Creature target,
		Creature applier,
		CardModel? cardSource,
		decimal amount)
	{
		switch (kind)
		{
			case QueenTriadDebuffKind.Poison:
				await PowerCmd.Apply<PoisonPower>(choiceContext, target, amount, applier, cardSource);
				break;
			case QueenTriadDebuffKind.Doom:
				await PowerCmd.Apply<DoomPower>(choiceContext, target, amount, applier, cardSource);
				break;
			default:
				await PowerCmd.Apply<DemisePower>(choiceContext, target, amount, applier, cardSource);
				break;
		}
	}

	private static float WeightForTriadKind(Creature target, QueenTriadDebuffKind? kind)
	{
		if (kind is not { } k)
		{
			return 0.01f;
		}

		decimal stacks = k switch
		{
			QueenTriadDebuffKind.Poison => target.GetPower<PoisonPower>()?.Amount ?? 0m,
			QueenTriadDebuffKind.Doom => target.GetPower<DoomPower>()?.Amount ?? 0m,
			QueenTriadDebuffKind.Demise => target.GetPower<DemisePower>()?.Amount ?? 0m,
			_ => 0m,
		};

		double s = (double)Math.Max(0m, stacks);

		const float Base = 3f;
		const float FreshBonus = 12f;
		const double StackDampen = 0.45;

		if (s <= 0.0)
		{
			return Base + FreshBonus;
		}

		return Base + (float)(1.0 / (1.0 + s * StackDampen));
	}
}

/// <summary>毒 / 灾厄(Doom) / 消亡(Demise) 三选一加权随机的标签。</summary>
public enum QueenTriadDebuffKind
{
	Poison,
	Doom,
	Demise,
}
