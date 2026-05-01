using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

/// <summary>施加抑制：回合开始时，降级 1 张牌，升级所有其他手牌。</summary>
[Pool(typeof(EnemyCardPool))]
public sealed class Dampen : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private const decimal stacks = 1m;

    public override int MaxUpgradeLevel => 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<DampenPower>(stacks),
    ];

    public Dampen()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        await PowerCmd.Apply<DampenPower>(base.Owner.Creature, stacks, base.Owner.Creature, this);
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
    }
}

