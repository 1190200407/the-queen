using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.HoverTips;

namespace ComicChess.TheQueen;

/// <summary>缠绕藤蔓：学习特殊意图；下回合获得缠结，技能耗能 -1。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class GraspingVines : LearnIntentCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ..base.ExtraHoverTips,
        HoverTipFactory.FromPower<TangledPower>(),
    ];

    public GraspingVines()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        AmalgamActionModel intent = new AmalgamSpecialIntentAction(
            moveId: "AMALGAM_INTENT_SPECIAL_GRASPING_VINES",
            intentDescriptionKey: "AMALGAM_SPECIAL_GRASPING_VINES.description",
            execute: async (PlayerChoiceContext _, Creature amalgam, Creature owner) =>
            {
                await CreatureCmd.TriggerAnim(amalgam, "Cast", AmalgamSpecialIntentAction.CastAnimDelay);
                await PowerCmd.Apply<TangledPower>(owner, 1m, applier: amalgam, cardSource: null);
                await PowerCmd.Apply<SkillCostMinusOneThisTurnPower>(owner, 1m, applier: amalgam, cardSource: null, silent: true);
            });

        return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([intent]);
    }
}

