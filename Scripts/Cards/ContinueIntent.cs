using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

/// <summary>续念：聚合体立刻行动 1 次。消逝衍生牌。</summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class ContinueIntent : QueenCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [ModKeywordRegistry.GetCardKeyword(QueenKeyword.Fade)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [ModKeywordRegistry.CreateHoverTip(QueenKeyword.Fade)];

    public ContinueIntent()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        ICombatState? combatState = base.Owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        Creature? amalgamCreature = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgamCreature?.Monster is not FriendlyAmalgam amalgam || !amalgamCreature.IsAlive)
        {
            return;
        }

        await amalgam.ActCurrentIntentImmediatelyAsync(choiceContext);
    }
}
