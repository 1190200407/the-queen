using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

[RegisterCard(typeof(QueenCardPool))]
public sealed class EvolutionTheory : QueenCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            if (!base.IsUpgraded)
            {
                yield return CardKeyword.Exhaust;
            }

            yield return ModKeywordRegistry.GetCardKeyword(QueenKeyword.AmalgamComposite);
        }
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        QueenHoverTips.ForgetIntent,
        ModKeywordRegistry.CreateHoverTip(QueenKeyword.AmalgamComposite),
    ];

    public EvolutionTheory()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        CombatState? combatState = base.Owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        Creature? amalgamCreature = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgamCreature?.Monster is not FriendlyAmalgam amalgam || !amalgamCreature.IsAlive)
        {
            return;
        }

        IReadOnlyList<AmalgamActionModel> forgotten = await amalgam.ForgetAllNonCompositeTorchIntentsAsync();
        if (forgotten.Count == 0)
        {
            return;
        }

        AmalgamEvolutionTheoryPendingPower? pending = amalgamCreature.GetPower<AmalgamEvolutionTheoryPendingPower>();
        if (pending == null)
        {
            pending = await PowerCmd.Apply<AmalgamEvolutionTheoryPendingPower>(amalgamCreature,
                1m,
                base.Owner.Creature,
                this);
        }

        pending?.AddPendingIntents(forgotten);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
