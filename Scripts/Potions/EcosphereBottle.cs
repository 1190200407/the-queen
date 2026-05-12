using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;

namespace ComicChess.TheQueen;

/// <summary>生态箱：对敌人施加捕获标记；目标在战斗中死亡且可映射捕获牌时，与宣告相同地追加额外卡牌奖励。</summary>
[Pool(typeof(QueenPotionPool))]
public sealed class EcosphereBottle : QueenPotionModel
{
	public override TargetType TargetType => TargetType.AnyEnemy;

	public override PotionRarity Rarity => PotionRarity.Rare;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override IEnumerable<IHoverTip> ExtraHoverTips => [QueenHoverTips.Capture];

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		_ = choiceContext;
		PotionModel.AssertValidForTargetedPotion(target);
		Creature enemy = target!;
		Player owner = base.Owner;
		if (owner.Creature.CombatState == null)
		{
			return;
		}

		await PowerCmd.Apply<EcosphereCaptureMarkPower>(enemy, 1m, owner.Creature, null);
	}
}
