using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>「首份礼物」经 <see cref="MegaCrit.Sts2.Core.Models.Relics.TouchOfOrobas"/> 升级后的形态：战斗开始时获得更多魂灯。</summary>

public sealed class QueensGraceRelic : QueenRelicModel
{
	public override RelicRarity Rarity => RelicRarity.Starter;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<SoulLampPower>()];

	public override async Task BeforeCombatStart()
	{
		int amount = base.DynamicVars["Cards"].IntValue;
		await QueenCardCmd.AddSoulLamp(new ThrowingPlayerChoiceContext(), base.Owner, amount);
	}
}
