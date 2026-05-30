using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>死亡洪流：对所有敌人随机施加毒/灾厄/消亡；仅 1 名敌人时效果再执行 1 次。</summary>
[RegisterCard(typeof(QueenCardPool))]
public sealed class DeathTorrent : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("TriadStacks", 6m)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<PoisonPower>(),
        HoverTipFactory.FromPower<DoomPower>(),
        HoverTipFactory.FromPower<DemisePower>()
    ];

    public DeathTorrent()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (base.CombatState is not CombatState combatState || base.Owner.Creature is not { IsAlive: true } applier)
        {
            return;
        }

        Player owner = base.Owner;
        decimal stacks = base.DynamicVars["TriadStacks"].BaseValue;
        List<Creature> enemies = combatState.HittableEnemies.ToList();
        int executions = enemies.Count == 1 ? 2 : 1;

        for (int i = 0; i < executions; i++)
        {
            foreach (Creature enemy in enemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                await QueenCardCmd.ApplyRandomTriadDebuff(choiceContext, owner, enemy, applier, this, stacks);
            }
        }

        await CreatureCmd.TriggerAnim(applier, "Cast", owner.Character.CastAnimDelay);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["TriadStacks"].UpgradeValueBy(2m);
    }
}
