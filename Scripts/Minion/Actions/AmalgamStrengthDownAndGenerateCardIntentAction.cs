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

/// <summary>聚合体意图：目标本回合失去力量 + 在手牌中加入指定卡牌（合并为一个 ActionModel）。</summary>
public sealed class AmalgamStrengthDownAndGenerateCardIntentAction<T> : AmalgamActionModel
    where T : QueenCardModel
{
    private const string StrengthLossParam = "strength_loss";
    private const string CardCountParam = "card_count";

    private readonly Creature? _forcedTarget;
    private readonly decimal _strengthLoss;
    private readonly int _cardCount;
    private readonly string _cardNameForIntent;

    public AmalgamStrengthDownAndGenerateCardIntentAction(decimal strengthLoss, decimal cardCount, Creature? forcedTarget = null)
        : base(new System.Collections.Generic.Dictionary<string, decimal>
        {
            [StrengthLossParam] = strengthLoss,
            [CardCountParam] = cardCount,
        })
    {
        _forcedTarget = forcedTarget;
        _strengthLoss = GetParameterOrDefault(StrengthLossParam, 0m);
        _cardCount = Math.Max(0, (int)GetParameterOrDefault(CardCountParam, 0m));
        _cardNameForIntent = ResolveCardName();
    }

    private AmalgamStrengthDownIntentAction _strengthDownIntentAction;

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
        ICombatState? combatState = amalgam.CombatState;
        if (combatState == null || amalgam.PetOwner is not Player queen || !queen.Creature.IsAlive)
        {
            return;
        }

        if (_strengthLoss > 0m)
        {
            _strengthDownIntentAction = _strengthDownIntentAction ?? new AmalgamStrengthDownIntentAction(_strengthLoss, _forcedTarget);
            await _strengthDownIntentAction.ExecuteAsync(choiceContext, amalgam);
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

    private async Task CreateInHandByType(Player owner, ICombatState combatState)
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

