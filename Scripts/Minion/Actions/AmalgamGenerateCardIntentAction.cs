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

/// <summary>聚合体意图：在手牌中加入指定卡牌�?/summary>
public sealed class AmalgamGenerateCardIntentAction<T> : AmalgamSingleDecimalActionModel
    where T : QueenCardModel
{
    public override string Key => GenericPoolKey("generate_card", typeof(T));

    private static readonly MethodInfo CreateInHandGeneric = typeof(QueenCardCmd)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(m =>
            m.Name == nameof(QueenCardCmd.CreateInHand)
            && m.IsGenericMethodDefinition
            && m.GetParameters().Length == 3);

    private static readonly string CardNameForIntent = ResolveCardName();

    private bool _generateUpgradedCard;

    public AmalgamGenerateCardIntentAction()
    {
    }

    public AmalgamGenerateCardIntentAction(decimal count, bool generateUpgradedCard = false)
        : base(count)
    {
        _generateUpgradedCard = generateUpgradedCard;
    }

    public override bool Init(decimal amount)
    {
        if (!TryInitSingleDecimal(amount))
        {
            return false;
        }

        _generateUpgradedCard = false;
        return true;
    }

    public override bool Init(object[] args)
    {
        if (!AmalgamActionArgs.TryGetDecimal(args, 0, out decimal count) || !AmalgamActionArgs.IsPositive(count))
        {
            return false;
        }

        ResetForInit();
        Amount = count;
        _generateUpgradedCard = AmalgamActionArgs.TryGetBool(args, 1, out bool upgraded) && upgraded;
        return true;
    }

    public static readonly float CastAnimDelay = 1.5f;

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_GENERATE_CARD",
            _ => Task.CompletedTask,
            new AmalgamGenerateCardIntent(Amount, CardNameForIntent));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        _ = choiceContext;
        if (amalgam.CombatState is not { } combatState
            || amalgam.PetOwner is not Player queen
            || !queen.Creature.IsAlive)
        {
            return;
        }

        int count = (int)Amount;
        if (count <= 0)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(amalgam, "Cast", CastAnimDelay);
        for (int i = 0; i < count; i++)
        {
            await CreateInHandByType(queen, combatState);
        }
    }

    private async Task CreateInHandByType(Player owner, CombatState combatState)
    {
        await QueenCardCmd.CreateInHand<T>(owner, combatState, _generateUpgradedCard);
    }

    private static string ResolveCardName()
    {
        CardModel card = ModelDb.Card<T>();
        return card.Title;
    }
}
