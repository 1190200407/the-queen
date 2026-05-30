using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace ComicChess.TheQueen;

/// <summary>银制手链：拾起时从牌组选 1 张牌附魔灯火（<see cref="SoulLight"/> 兼管魂缚预览与战斗内侵蚀）。</summary>
public sealed class SilverBraceletRelic : QueenRelicModel
{
	public override RelicRarity Rarity => RelicRarity.Shop;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new IntVar("SoulLightAmount", 1m),
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [.. HoverTipFactory.FromEnchantment<SoulLight>()];

	public override async Task AfterObtained()
	{
		SoulLight canonicalSoulLight = ModelDb.Enchantment<SoulLight>();
		decimal soulLightAmount = base.DynamicVars["SoulLightAmount"].BaseValue;
		CardSelectorPrefs prefs = new(CardSelectorPrefs.EnchantSelectionPrompt, 1);
		List<CardModel> candidates = PileType.Deck.GetPile(base.Owner).Cards
			.Where(c => c is { Affliction: null, Enchantment: null } && canonicalSoulLight.CanEnchant(c))
			.ToList();

		IEnumerable<CardModel> chosen;
		try
		{
			chosen = await CardSelectCmd.FromDeckForEnchantment(candidates, canonicalSoulLight, 1, prefs);
		}
		catch (OperationCanceledException)
		{
			return;
		}

		foreach (CardModel card in chosen)
		{
			if (!canonicalSoulLight.CanEnchant(card))
			{
				continue;
			}

			CardCmd.Enchant(canonicalSoulLight.ToMutable(), card, soulLightAmount);

			NCardEnchantVfx? vfx = NCardEnchantVfx.Create(card);
			if (vfx != null && NRun.Instance?.GlobalUi.CardPreviewContainer is Node previewRoot)
			{
				previewRoot.AddChildSafely(vfx);
			}

			break;
		}
	}
}
