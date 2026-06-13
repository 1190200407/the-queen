using System;
using System.Reflection;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Vfx;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>提线木偶：获得目标对应敌怪卡�? 回合后将其永久加入牌组�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class Marionette : QueenCardModel, ICanMonsterCapture
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public bool CanCapture(MonsterModel monster, ICombatState combatState) =>
        monster is not null && combatState is not null;

    public Marionette()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    /// <summary>
    /// 对齐原版 <c>MegaCrit.Sts2.Core.Models.Monsters.Queen</c> �?<c>AmalgamDeathResponse</c> 的激怒分支（不反射调用该方法本体）�?    /// </summary>
    private static bool TryEnrageQueenFromMarionette(Creature queenCreature)
    {
        if (queenCreature.Monster is not Queen queen)
        {
            return false;
        }

        try
        {
            NRunMusicController.Instance?.UpdateMusicParameter("queen_progress", 2f);
            if (!queenCreature.IsDead)
            {
                FieldInfo? hasDiedField = typeof(Queen).GetField("_hasAmalgamDied", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo? amalgamField = typeof(Queen).GetField("_amalgam", BindingFlags.Instance | BindingFlags.NonPublic);
                hasDiedField?.SetValue(queen, true);
                amalgamField?.SetValue(queen, null);

                LocString line = MonsterModel.L10NMonsterLookup("QUEEN.marionetteLine");
                TalkCmd.Play(line, queenCreature, VfxColor.Purple, VfxDuration.Custom);

                FieldInfo? burnField = typeof(Queen).GetField("_burnBrightForMeState", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo? enragedField = typeof(Queen).GetField("_enragedState", BindingFlags.Instance | BindingFlags.NonPublic);
                if (burnField?.GetValue(queen) is MoveState burn
                    && enragedField?.GetValue(queen) is MoveState enraged
                    && ReferenceEquals(queen.NextMove, burn))
                {
                    queen.SetMoveImmediate(enraged, false);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Marionette: TryEnrageQueenFromMarionette failed: {ex}");
            return true;
        }
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

        if (TryEnrageQueenFromMarionette(target))
        {
            return;
        }

        CardModel? enemyCard = MonsterCaptureRewardCatalog.CreateCaptureRewardCard(base.Owner, monsterId, target);
        if (enemyCard is null)
        {
            return;
        }
        enemyCard = base.CombatState?.CloneCard(enemyCard);
        if (enemyCard is null)
        {
            return;
        }

        await CardPileCmd.AddGeneratedCardToCombat(enemyCard, PileType.Hand, base.Owner);

        MarionettePendingPower? pending = await PowerCmd.Apply<MarionettePendingPower>(choiceContext, 
            base.Owner.Creature,
            3m,
            base.Owner.Creature,
            this);
        pending?.ConfigureMonsterId(
            monsterId,
            MonsterCaptureRewardCatalog.CanReceiveUnknownSoulFallback(target));
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
