using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>知识的诅咒：召唤并获得 1 层 <see cref="KnowledgeDemonPower"/>（可叠加，每层战斗开局多选 1 次诅咒牌）。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class CurseOfKnowledge : QueenCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    private const decimal summon = 20m;

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new SummonVar(summon).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<KnowledgeDemonPower>(),
        HoverTipFactory.FromCard<Rejuvenate>(),
        HoverTipFactory.FromCard<MindClarity>(),
        HoverTipFactory.FromCard<Disintegration>(),
    ];

    public CurseOfKnowledge()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (base.Owner.Creature.CombatState is not { })
        {
            return;
        }

        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);
        await PowerCmd.Apply<KnowledgeDemonPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
    }
}
