using BaseLib.Utils;
using ComicChess.TheQueen;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenPotionPool))]
public sealed class LampInABottle : QueenPotionModel
{
    public override TargetType TargetType => TargetType.AnyAlly;
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override IEnumerable<IHoverTip> ExtraHoverTips => [QueenHoverTips.SoulLamp];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (target is null || target.Player is null)
        {
            return;
        }
        await QueenCardCmd.AddSoulLamp(target.Player, 1);
    }
}