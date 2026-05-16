using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

using STS2RitsuLib.Keywords;
namespace ComicChess.TheQueen;

[RegisterCard(typeof(TokenCardPool))]
public sealed class GuiYuZhiYe : QueenCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    protected override IEnumerable<string> RegisteredKeywordIds => [QueenKeyword.Fade];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        ModKeywordRegistry.CreateHoverTip(QueenKeyword.Fade),
        .. HoverTipFactory.FromAffliction<Bound>(),
        HoverTipFactory.FromPower<SoulLampPower>()
    ];

    internal override bool HasSelfBound => true;

    public GuiYuZhiYe()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        ICombatState? combatState = base.Owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }

        Creature? amalgamCreature = FriendlyAmalgamCmd.GetExisting(combatState, base.Owner);
        if (amalgamCreature?.Monster is not FriendlyAmalgam amalgam || !amalgamCreature.IsAlive)
        {
            return;
        }

        int forgotten = 0;
        for (int slot = 0; slot < 3; slot++)
        {
            if (amalgam.HasIntentInTorchSlot(slot))
            {
                forgotten++;
            }
        }

        for (int i = 0; i < forgotten; i++)
        {
            await amalgam.ForgetCurrentTorchSlotIntentAsync();
        }

        await QueenCardCmd.AddSoulLamp(base.Owner, forgotten + 1);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
