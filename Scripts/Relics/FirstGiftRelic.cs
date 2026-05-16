using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

[RegisterCharacterStarterRelic(typeof(QueenCharacter))]
[RegisterTouchOfOrobasRefinement(typeof(QueensGraceRelic))]
public class FirstGiftRelic : QueenRelicModel
{
	// 稀有度
	public override RelicRarity Rarity => RelicRarity.Starter;

	// 遗物的数值。替换本地化中的{Cards}。
	protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<SoulLampPower>()];

    public override async Task BeforeCombatStart()
	{
		// 战斗开始时获得魂灯（SoulLampPower）层数。
		int amount = base.DynamicVars["Cards"].IntValue;
		await QueenCardCmd.AddSoulLamp(new ThrowingPlayerChoiceContext(), base.Owner, amount);
	}
}

