using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;


[RegisterCard(typeof(QueenCardPool))]
public sealed class LocalMaterials : QueenCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<SoulLampPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("GenerateThreshold", 5m)
    ];

    public LocalMaterials()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        int threshold = (int)base.DynamicVars["GenerateThreshold"].BaseValue;
        LocalMaterialsPower? power = await PowerCmd.Apply<LocalMaterialsPower>(base.Owner.Creature, threshold, base.Owner.Creature, this);
        if (power != null)
        {
            power.ConfigureThresholdAsync(threshold, base.Owner.Creature, null);
        }
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["GenerateThreshold"].UpgradeValueBy(-1m);
    }
}
