using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace ComicChess.TheQueen;

/// <summary>小夜灯：每累计 {SoulLampPerTrigger} 层魂灯，获得 {Block} 点格挡（经 <see cref="QueenCardCmd.AddSoulLamp"/>；余数跨获得保留）。</summary>
[Pool(typeof(QueenRelicPool))]
public sealed class NightLightRelic : QueenRelicModel
{
	public override RelicRarity Rarity => RelicRarity.Common;

	public override bool ShowCounter => true;

	public override int DisplayAmount => SoulLampTowardNightLight;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new IntVar("SoulLampPerTrigger", 2m),
		new BlockVar(3m, ValueProp.Unpowered),
	];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SoulLampPower>()];

	/// <summary>已计入、尚未凑满一次格挡奖励的魂灯层数（0 或 1，当 <see cref="SoulLampPerTrigger"/> 为 2 时）。</summary>
	[SavedProperty]
	public int SoulLampTowardNightLight { get; private set; }

	internal static async Task NotifySoulLampGained(Player owner, int lampAmount)
	{
		if (lampAmount <= 0 || owner.PlayerCombatState == null || !owner.Creature.IsAlive)
		{
			return;
		}

		NightLightRelic? relic = owner.Relics.OfType<NightLightRelic>().FirstOrDefault();
		if (relic == null)
		{
			return;
		}

		await relic.ApplySoulLampBlock(owner, lampAmount);
	}

	private async Task ApplySoulLampBlock(Player owner, int lampAmount)
	{
		int perTrigger = base.DynamicVars["SoulLampPerTrigger"].IntValue;
		if (perTrigger <= 0)
		{
			return;
		}

		AssertMutable();
		SoulLampTowardNightLight += lampAmount;
		int procs = SoulLampTowardNightLight / perTrigger;
		SoulLampTowardNightLight %= perTrigger;
		InvokeDisplayAmountChanged();
		if (procs <= 0)
		{
			return;
		}

		decimal blockEach = base.DynamicVars.Block.BaseValue;
		Flash();
		await CreatureCmd.GainBlock(owner.Creature, blockEach * procs, ValueProp.Unpowered, null);
	}
}
