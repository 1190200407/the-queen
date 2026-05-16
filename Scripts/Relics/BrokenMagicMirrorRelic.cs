using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rewards;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>破碎魔镜：拾起时通过奖励界面领取 {Potions} 瓶 <see cref="MagicMirrorShard"/>（与原版 <c>LostCoffer</c> / <c>Orrery</c> 的 <see cref="RewardsCmd.OfferCustom"/> 一致）。</summary>
[RegisterRelic(typeof(QueenRelicPool))]
public sealed class BrokenMagicMirrorRelic : QueenRelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Potions", 1m)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPotion<MagicMirrorShard>()];

	public override async Task AfterObtained()
	{
		int n = base.DynamicVars["Potions"].IntValue;
		if (n <= 0)
		{
			return;
		}

		List<Reward> rewards = new(n);
		for (int i = 0; i < n; i++)
		{
			rewards.Add(new PotionReward(ModelDb.Potion<MagicMirrorShard>().ToMutable(), base.Owner));
		}

		await RewardsCmd.OfferCustom(base.Owner, rewards);
	}
}
