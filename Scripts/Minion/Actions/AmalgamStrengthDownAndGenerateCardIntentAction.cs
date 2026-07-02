using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>????????????????+ ?????????????????ActionModel???/summary>
public sealed class AmalgamStrengthDownAndGenerateCardIntentAction<T> : AmalgamActionModel
    where T : QueenCardModel
{
    public override string Key => GenericPoolKey("strength_down_and_generate_card", typeof(T));

    private const string StrengthLossParam = "strength_loss";
    private const string CardCountParam = "card_count";

    private readonly Creature? _forcedTarget;
    private readonly decimal _strengthLoss;
    private readonly int _cardCount;
    private readonly string _cardNameForIntent;

    public AmalgamStrengthDownAndGenerateCardIntentAction(decimal strengthLoss, decimal cardCount, Creature? forcedTarget = null)
    {
        _forcedTarget = forcedTarget;
        _strengthLoss = strengthLoss;
        _cardCount = Math.Max(0, (int)cardCount);
        _cardNameForIntent = ResolveCardName();
    }

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_STRENGTH_DOWN_AND_GENERATE_CARD",
            _ => Task.CompletedTask,
            new AmalgamStrengthDownIntent(_strengthLoss),
            new AmalgamGenerateCardIntent(_cardCount, _cardNameForIntent));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        CombatState? combatState = amalgam.CombatState;
        if (combatState == null || amalgam.PetOwner is not Player queen || !queen.Creature.IsAlive)
        {
            return;
        }

        if (_strengthLoss > 0m)
        {
            AmalgamActionModel? strDown = _forcedTarget is { } forcedTarget
                ? AmalgamActionRegistry.Rent<AmalgamStrengthDownIntentAction>(_strengthLoss, forcedTarget)
                : AmalgamActionRegistry.Rent<AmalgamStrengthDownIntentAction>(_strengthLoss);
            await AmalgamActionRegistry.ExecuteTemporaryAsync(choiceContext, amalgam, strDown);
        }

        if (_cardCount <= 0)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, "Cast", AmalgamGenerateCardIntentAction<T>.CastAnimDelay);
        for (int i = 0; i < _cardCount; i++)
        {
            await CreateInHandByType(queen, combatState);
        }
    }

    private async Task CreateInHandByType(Player owner, CombatState combatState)
    {
        await QueenCardCmd.CreateInHand<T>(owner, combatState);
    }

    private static string ResolveCardName()
    {
        CardModel card = ModelDb.Card<T>();
        return card.Title;
    }

    public override AmalgamActionModel Clone() =>
        new AmalgamStrengthDownAndGenerateCardIntentAction<T>(_strengthLoss, _cardCount, _forcedTarget);
}

