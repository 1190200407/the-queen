using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace ComicChess.TheQueen;

/// <summary>聚合体意图：先攻击，再使敌人本回合失去力量（回合结束恢复）。</summary>
public sealed class AmalgamAttackAndStrengthDownIntentAction : AmalgamActionModel
{
    private const string StrengthLossParam = "strengthLoss";

    private readonly decimal _strengthLoss;

    public AmalgamAttackAndStrengthDownIntentAction(decimal damage, decimal strengthLoss)
        : base(new Dictionary<string, decimal>
        {
            [AmountParam] = damage,
            [StrengthLossParam] = strengthLoss,
        })
    {
        _strengthLoss = GetParameterOrDefault(StrengthLossParam, 0m);
    }

    public override LocString IntentTitle => new("monsters", "FRIENDLY_AMALGAM.intent_offense.title");

    public override LocString GetIntentDescription()
    {
        LocString desc = new("monsters", "FRIENDLY_AMALGAM.intent_offense.description");
        desc.Add("Amount", Amount);
        return desc;
    }

    protected override MoveState CreateMoveState()
    {
        return new MoveState(
            "AMALGAM_INTENT_ATTACK_AND_STRENGTH_DOWN",
            _ => Task.CompletedTask,
            new AmalgamSingleAttackIntent(Amount),
            new AmalgamStrengthDownIntent(_strengthLoss));
    }

    protected override async Task OnExecute(PlayerChoiceContext choiceContext, Creature amalgam)
    {
        if (Amount > 0m)
        {
            AmalgamActionModel? offense = AmalgamActionRegistry.CreateOffense(Amount);
            if (offense != null)
            {
                await offense.ExecuteAsync(choiceContext, amalgam);
            }
        }

        if (_strengthLoss > 0m)
        {
            AmalgamActionModel? strDown = AmalgamActionRegistry.Create(AmalgamActionRegistry.StrengthDown, _strengthLoss);
            if (strDown != null)
            {
                await strDown.ExecuteAsync(choiceContext, amalgam);
            }
        }
    }
}

