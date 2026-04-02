using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace ComicChess.TheQueen;

[Pool(typeof(QueenRelicPool))]
public class FirstGiftRelic : QueenRelicModel
{
	// 稀有度
	public override RelicRarity Rarity => RelicRarity.Starter;

	// 遗物的数值。替换本地化中的{Cards}。
	protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [QueenHoverTips.SoulLamp];

    public override async Task BeforeCombatStart()
	{
		// 战斗开始时获得魂灯（SoulLampPower）层数。
		int amount = base.DynamicVars["Cards"].IntValue;
		await QueenCardCmd.AddSoulLamp(base.Owner, amount);
	}
}

