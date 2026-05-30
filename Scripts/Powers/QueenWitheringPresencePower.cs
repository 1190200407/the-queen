using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 凋萎存在：每打出 4 张牌抽 1 张，并为抽到的牌附魔 <see cref="Withering"/>。
/// 图标复用原版 <c>WITHERING_PRESENCE_POWER</c>。
/// </summary>
public sealed class QueenWitheringPresencePower : QueenPowerModel
{
	private const int CardsPerTrigger = 4;

	private const string CardsLeftKey = "CardsLeft";

	private const string WitheringEnchantAmountKey = "WitheringEnchantAmount";

	private sealed class Data
	{
		public decimal WitheringEnchantAmount = 2m;
	}

	protected override object? InitInternalData() => new Data();

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	public override int DisplayAmount => DynamicVars[CardsLeftKey].IntValue;

	public override string? CustomIconPath =>
		"res://images/atlases/power_atlas.sprites/withering_presence_power.tres";

	public override string? CustomBigIconPath => "res://images/powers/withering_presence_power.png";

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DynamicVar(CardsLeftKey, CardsPerTrigger),
		new DynamicVar(WitheringEnchantAmountKey, 2m),
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		[..HoverTipFactory.FromEnchantment<Withering>(DynamicVars[WitheringEnchantAmountKey].IntValue)];

	internal void Configure(decimal witheringEnchantAmount)
	{
		Data data = GetInternalData<Data>();
		data.WitheringEnchantAmount = witheringEnchantAmount;
		DynamicVars[WitheringEnchantAmountKey].BaseValue = witheringEnchantAmount;
		DynamicVars[CardsLeftKey].BaseValue = CardsPerTrigger;
		InvokeDisplayAmountChanged();
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardModel played = cardPlay.Card;
		if (played.Owner?.Creature != base.Owner)
		{
			return;
		}

		DynamicVar cardsLeft = DynamicVars[CardsLeftKey];
		cardsLeft.BaseValue--;
		InvokeDisplayAmountChanged();
		if (cardsLeft.IntValue > 0)
		{
			return;
		}

		Player? player = base.Owner.Player;
		if (player == null)
		{
			return;
		}

		IEnumerable<CardModel> drawn = await CardPileCmd.Draw(choiceContext, 1, player);
		Data data = GetInternalData<Data>();
		EnchantmentModel withering = ModelDb.Enchantment<Withering>().ToMutable();
		decimal amount = data.WitheringEnchantAmount;
		foreach (CardModel card in drawn)
		{
			if (!withering.CanEnchant(card))
			{
				continue;
			}

			CardCmd.Enchant<Withering>(card, amount);
		}

		Flash();
		cardsLeft.BaseValue = CardsPerTrigger;
		InvokeDisplayAmountChanged();
	}
}
