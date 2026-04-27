using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenCardPool))]
public sealed class LampDrive : QueenCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public LampDrive()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        int consumedSoulLamp = 0;
        SoulLampPower? lamp = base.Owner.Creature.GetPower<SoulLampPower>();
        if (lamp != null && lamp.Amount > 0)
        {
            consumedSoulLamp = (int)lamp.Amount;
            await PowerCmd.SetAmount<SoulLampPower>(base.Owner.Creature, -1m, base.Owner.Creature, this);
        }

        if (consumedSoulLamp <= 0)
        {
            return;
        }

        CombatState? combatState = base.Owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        Creature? amalgamCreature = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgamCreature?.Monster is not FriendlyAmalgam amalgam || !amalgamCreature.IsAlive)
        {
            return;
        }

        for (int i = 0; i < consumedSoulLamp; i++)
        {
            if (!amalgamCreature.IsAlive)
            {
                break;
            }

            await amalgam.ActCurrentIntentImmediatelyAsync(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
