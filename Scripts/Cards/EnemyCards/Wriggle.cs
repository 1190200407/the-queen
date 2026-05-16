using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>寄生：拾起时为一张学习意图（意图伤害）牌添加附魔：感染。本卡不加入牌组。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class Wriggle : QueenCardModel
{
    private const int infestedStacks = 3;
    private const int energyCost = -1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("InfestedStacks", infestedStacks)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [..HoverTipFactory.FromEnchantment<Infested>(infestedStacks)];

    public override int MaxUpgradeLevel => 0;

    public Wriggle()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override bool ShouldAddToDeck(CardModel card) => card is not Wriggle;

    public override async Task AfterAddToDeckPrevented(CardModel card)
    {
        await InfestOneLearnIntentDamageCardOnPickup(card.Owner, infestedStacks);
    }

    private static async Task InfestOneLearnIntentDamageCardOnPickup(Player player, decimal stacks)
    {
        CardModel cardModel = (await CardSelectCmd.FromDeckForEnchantment(player, ModelDb.Enchantment<Infested>(), infestedStacks, (CardModel? c) => ModelDb.Enchantment<Infested>().CanEnchant(c), new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 1))).FirstOrDefault();
		if (cardModel != null)
		{
			CardCmd.Enchant<Infested>(cardModel, infestedStacks);
			NCardEnchantVfx nCardEnchantVfx = NCardEnchantVfx.Create(cardModel);
			if (nCardEnchantVfx != null)
			{
				NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
			}
		}
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        Task.CompletedTask;
}

