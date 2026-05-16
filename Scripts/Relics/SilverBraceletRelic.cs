using System.Collections.Generic;
using System.Threading.Tasks;

using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Saves.Runs;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>
/// 银制手链：拾起时从牌组选 1 张牌，侵蚀为魂缚并附魔灯火 1；在遗物上保存 <see cref="MarkedCard"/> 快照，
/// 局外悬停用 <see cref="HoverTipFactory.FromCard"/> 展示带侵蚀/附魔的卡牌。
/// </summary>

public sealed class SilverBraceletRelic : QueenRelicModel
{
	private readonly List<IHoverTip> _AdditionalHoverTips = [];

	private SerializableCard? _markedCard;

	public override RelicRarity Rarity => RelicRarity.Shop;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new IntVar("SoulLightAmount", 1m),
	];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [..HoverTipFactory.FromEnchantment<SoulLight>()];

	[SavedProperty]
	public SerializableCard? MarkedCard
	{
		get => _markedCard;
		private set
		{
			AssertMutable();
			_markedCard = value;
		}
	}

	protected override void AfterCloned()
	{
		base.AfterCloned();
		_AdditionalHoverTips.Clear();
	}

	public override async Task AfterObtained()
	{
		CardSelectorPrefs prefs = new(CardSelectorPrefs.EnchantSelectionPrompt, 1);
		IEnumerable<CardModel> chosen = await CardSelectCmd.FromDeckForEnchantment(
			base.Owner,
			ModelDb.Enchantment<SoulLight>(),
			1,
			static c => c is { Affliction: null, Enchantment: null },
			prefs);

		foreach (CardModel card in chosen)
		{
			if (!await QueenCardCmd.TryAfflictBoundOnCard(card, 1m))
			{
				continue;
			}

			CardCmd.Enchant<SoulLight>(card, base.DynamicVars["SoulLightAmount"].BaseValue);
			MarkedCard = card.ToSerializable();

			NCardEnchantVfx? vfx = NCardEnchantVfx.Create(card);
			if (vfx != null && NRun.Instance?.GlobalUi.CardPreviewContainer is Node previewRoot)
			{
				previewRoot.AddChildSafely(vfx);
			}

			break;
		}
	}
}
