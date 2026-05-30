using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>活力火花：聚合体获得活力火花，你获得传染。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class VitalSpark : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    private const decimal vitalSparkStacks = 2m;
    private const decimal galvanizedStacks = 2m;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VitalSparkPower>(vitalSparkStacks),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<VitalSparkPower>(),
        HoverTipFactory.FromPower<TaintedPower>(),
    ];

    public override int MaxUpgradeLevel => 0;

    public VitalSpark()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (base.Owner.Creature == null)
        {
            return;
        }

        if (base.Owner.PlayerCombatState is { } playerCombatState)
        {
            foreach (CardModel card in playerCombatState.AllCards.ToList())
            {
                if (card.Affliction is null || card.Affliction is Tainted)
                    await CardCmd.Afflict<Tainted>(card, galvanizedStacks);
            }
        }

        await PowerCmd.Apply<VitalSparkPower>(base.Owner.Creature, vitalSparkStacks, base.Owner.Creature, this);
        QueenContagionPower? contagionPower = base.Owner.Creature.GetPower<QueenContagionPower>();
        if (contagionPower is not null)
        {
            await PowerCmd.ModifyAmount(contagionPower, contagionPower.Amount, base.Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<QueenContagionPower>(base.Owner.Creature, 2m, base.Owner.Creature, this);
        }
    }
}
