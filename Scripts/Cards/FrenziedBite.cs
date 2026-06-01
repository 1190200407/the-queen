using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>疯狂撕咬：聚合体连续执行若干次行动，随后遗忘所有意图。消耗。</summary>
[RegisterCard(typeof(QueenCardPool))]
public sealed class FrenziedBite : QueenCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Actions", 2m)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [QueenHoverTips.ForgetIntent];

    public FrenziedBite()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (base.Owner.Creature.CombatState is not CombatState combatState)
        {
            return;
        }

        Creature? amalgamCreature = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgamCreature?.Monster is not FriendlyAmalgam amalgam || !amalgamCreature.IsAlive)
        {
            return;
        }

        int actionCount = base.DynamicVars["Actions"].IntValue;
        for (int i = 0; i < actionCount; i++)
        {
            if (!amalgamCreature.IsAlive)
            {
                break;
            }

            await amalgam.ActCurrentIntentImmediatelyAsync(choiceContext);
        }

        if (!amalgamCreature.IsAlive)
        {
            return;
        }

        await ForgetAllIntentsAsync(amalgam);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Actions"].UpgradeValueBy(1m);
    }

    private static async Task ForgetAllIntentsAsync(FriendlyAmalgam amalgam)
    {
        int intentCount = 0;
        for (int slot = 0; slot < 3; slot++)
        {
            if (amalgam.HasIntentInTorchSlot(slot))
            {
                intentCount++;
            }
        }

        for (int i = 0; i < intentCount; i++)
        {
            await amalgam.ForgetCurrentTorchSlotIntentAsync();
        }
    }
}
