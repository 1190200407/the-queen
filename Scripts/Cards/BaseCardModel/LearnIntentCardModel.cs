using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

public abstract class LearnIntentCardModel : QueenCardModel
{
    private AmalgamCompositeKey _compositeKey = AmalgamCompositeKey.None;

    protected LearnIntentCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public virtual AmalgamCompositeKey CompositeKey
    {
        get => _compositeKey;
        set
        {
            if (_compositeKey == value)
            {
                return;
            }

            AmalgamCompositeKey old = _compositeKey;
            _compositeKey = value;
            SyncCompositeKeyword(old, value);
        }
    }

    protected override bool ShouldGlowGoldInternal =>
        (base.Owner?.Creature?.CombatState is { } combatState
            && FriendlyAmalgamCmd.GetExisting(combatState, base.Owner) is { Monster: FriendlyAmalgam amalgam }
            && amalgam.Creature.IsAlive
            && amalgam.HasAllTorchSlotsFilled
            && !amalgam.BlockActionFromSleep);

    protected override IEnumerable<string> RegisteredKeywordIds
    {
        get
        {
            if (CompositeKey == AmalgamCompositeKey.None)
            {
                yield break;
            }

            yield return QueenKeyword.GetAmalgamCompositeKeywordId(CompositeKey);
        }
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return QueenHoverTips.LearnIntent;

            if (CompositeKey != AmalgamCompositeKey.None)
            {
                yield return ModKeywordRegistry.CreateHoverTip(QueenKeyword.GetAmalgamCompositeKeywordId(CompositeKey));
            }
        }
    }

    protected virtual bool ShouldSummonBeforeLearnIntent => false;

    protected virtual decimal GetSummonAmount(PlayerChoiceContext choiceContext, CardPlay cardPlay) => base.DynamicVars.Summon.BaseValue;

    protected virtual Task AfterSummonBeforeLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        Task.CompletedTask;

    protected virtual Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([]);

    protected async Task ApplyLearnOrCombineIntentAsync(PlayerChoiceContext choiceContext, AmalgamActionModel intent)
    {
        if (CompositeKey == AmalgamCompositeKey.None)
        {
            await FriendlyAmalgamCmd.LearnIntent(choiceContext, base.Owner, intent, this);
            return;
        }

        await FriendlyAmalgamCmd.CombineIntent(choiceContext, base.Owner, intent, this, CompositeKey);
    }

    protected async Task PlayLearnIntentsFromCreateAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IReadOnlyList<AmalgamActionModel?> intents = await CreateLearnIntentsAsync(choiceContext, cardPlay);
        foreach (AmalgamActionModel? intent in intents)
        {
            if (intent == null)
            {
                continue;
            }

            await ApplyLearnOrCombineIntentAsync(choiceContext, intent);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (ShouldSummonBeforeLearnIntent)
        {
            await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, GetSummonAmount(choiceContext, cardPlay), this);
        }

        await AfterSummonBeforeLearnIntentsAsync(choiceContext, cardPlay);
        await PlayLearnIntentsFromCreateAsync(choiceContext, cardPlay);
    }

    private void SyncCompositeKeyword(AmalgamCompositeKey oldKey, AmalgamCompositeKey newKey)
    {
        if (!IsMutable)
        {
            return;
        }

        if (oldKey != AmalgamCompositeKey.None)
        {
            this.RemoveModKeyword(QueenKeyword.GetAmalgamCompositeKeywordId(oldKey));
        }

        if (newKey != AmalgamCompositeKey.None)
        {
            this.AddModKeyword(QueenKeyword.GetAmalgamCompositeKeywordId(newKey));
        }
    }
}
