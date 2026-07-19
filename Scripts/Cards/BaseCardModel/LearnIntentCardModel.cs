using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;

namespace ComicChess.TheQueen;

public abstract class LearnIntentCardModel : QueenCardModel
{
    private const float LearnCombineSpacingSeconds = 0.5f;

    private AmalgamCompositeKey _compositeKey = AmalgamCompositeKey.None;

    private PlayerChoiceContext? _learnCombineSpacingContext;

    private int _learnCombineSpacingCount;

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
        if (_learnCombineSpacingContext != choiceContext)
        {
            _learnCombineSpacingContext = choiceContext;
            _learnCombineSpacingCount = 0;
        }

        if (_learnCombineSpacingCount > 0)
        {
            await Cmd.Wait(LearnCombineSpacingSeconds);
        }

        _learnCombineSpacingCount++;

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

    private static CardKeyword GetCompositeKeyword(AmalgamCompositeKey key) =>
        ModKeywordRegistry.GetCardKeyword(QueenKeyword.GetAmalgamCompositeKeywordId(key));

    private void SyncCompositeKeyword(AmalgamCompositeKey oldKey, AmalgamCompositeKey newKey)
    {
        if (!IsMutable)
        {
            return;
        }

        if (oldKey != AmalgamCompositeKey.None)
        {
            RemoveKeyword(GetCompositeKeyword(oldKey));
        }

        if (newKey != AmalgamCompositeKey.None)
        {
            CardKeyword newKeyword = GetCompositeKeyword(newKey);
            if (!Keywords.Contains(newKeyword))
            {
                AddKeyword(newKeyword);
            }
        }
    }
}
