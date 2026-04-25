using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

/// <summary>提线木偶：获得目标对应敌怪卡，3 回合后将其永久加入牌组。</summary>
[Pool(typeof(QueenCardPool))]
public sealed class Marionette : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    public override bool IsCapture => true;

    public Marionette()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
        Creature target = cardPlay.Target;
        string? monsterId = target.Monster?.Id.Entry;
        if (string.IsNullOrWhiteSpace(monsterId))
        {
            return;
        }

        CardModel? enemyCard = MonsterCaptureRewardCatalog.TryCreateCaptureRewardCard(base.Owner, monsterId);
        if (enemyCard is null)
        {
            return;
        }
        enemyCard = base.CombatState?.CloneCard(enemyCard);
        if (enemyCard is null)
        {
            return;
        }

        await CardPileCmd.AddGeneratedCardToCombat(enemyCard, PileType.Hand, addedByPlayer: true);

        MarionettePendingPower? pending = await PowerCmd.Apply<MarionettePendingPower>(
            base.Owner.Creature,
            3m,
            base.Owner.Creature,
            this);
        pending?.ConfigureMonsterId(monsterId);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
