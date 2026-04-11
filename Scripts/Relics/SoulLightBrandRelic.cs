// using System.Collections.Generic;
// using System.Threading.Tasks;
// using BaseLib.Utils;
// using Godot;
// using MegaCrit.Sts2.Core.CardSelection;
// using MegaCrit.Sts2.Core.Commands;
// using MegaCrit.Sts2.Core.Entities.Cards;
// using MegaCrit.Sts2.Core.Entities.Relics;
// using MegaCrit.Sts2.Core.Helpers;
// using MegaCrit.Sts2.Core.HoverTips;
// using MegaCrit.Sts2.Core.Models;
// using MegaCrit.Sts2.Core.Nodes;
// using MegaCrit.Sts2.Core.Nodes.Vfx;

// namespace ComicChess.TheQueen;

// /// <summary>
// /// 拾起时：从牌组选择 1 张牌，为其附上附魔「灯火」1 层。
// /// </summary>
// [Pool(typeof(QueenRelicPool))]
// public sealed class SoulLightBrandRelic : QueenRelicModel
// {
// 	public override RelicRarity Rarity => RelicRarity.Shop;

// 	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<SoulLight>();

// 	public override async Task AfterObtained()
// 	{
// 		IEnumerable<CardModel> chosen = await CardSelectCmd.FromDeckForEnchantment(
// 			player: base.Owner,
// 			enchantment: ModelDb.Enchantment<SoulLight>(),
// 			amount: 1,
// 			prefs: new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1));

// 		foreach (CardModel card in chosen)
// 		{
// 			CardCmd.Enchant<SoulLight>(card, 1m);
// 			NCardEnchantVfx? vfx = NCardEnchantVfx.Create(card);
// 			if (vfx != null && NRun.Instance?.GlobalUi.CardPreviewContainer is Node previewRoot)
// 			{
// 				previewRoot.AddChildSafely(vfx);
// 			}
// 		}
// 	}
// }
