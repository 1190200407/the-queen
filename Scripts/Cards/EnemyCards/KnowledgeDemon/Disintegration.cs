using System.Collections.Generic;
using System.Threading.Tasks;


using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Entities.Creatures;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

[RegisterCard(typeof(TokenCardPool))]
public sealed class Disintegration : QueenCardModel, KnowledgeDemon.IChoosable
{
    private const int energyCost = -1;
    private const CardType type = CardType.Status;
    private const CardRarity rarity = CardRarity.Status;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = false;

	public override int MaxUpgradeLevel => 0;

	public override bool CanBeGeneratedInCombat => false;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DisintegrationPower>(6m)];

    public Disintegration()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

	public async Task OnChosen()
	{
        CombatState? combatState = base.Owner.Creature.CombatState;
        if (combatState is null)
        {
            return;
        }
        foreach (Creature enemy in combatState.Enemies)
        {
            await PowerCmd.Apply<DisintegrationPower>(enemy, base.DynamicVars["DisintegrationPower"].BaseValue, base.Owner.Creature, this);
        }
	}
}
