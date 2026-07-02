using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>凋萎存在（永世沙漏）：召唤；获得「每 4 张牌抽 1 并为抽到的牌附魔凋萎」的能力。</summary>
[RegisterCard(typeof(EnemyCardPool))]
public sealed class WitheringPresence : QueenCardModel
{
	private const int energyCost = 3;
	private const CardType type = CardType.Power;
	private const CardRarity rarity = CardRarity.Rare;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	private const decimal summonHp = 25m;
	private const int defaultWitheringEnchantAmount = 3;

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new SummonVar(summonHp).WithSharedTooltip("QUEEN_SUMMON_DYNAMIC"),
		new IntVar("WitheringEnchantAmount", defaultWitheringEnchantAmount),
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		[..HoverTipFactory.FromEnchantment<Withering>(DynamicVars["WitheringEnchantAmount"].IntValue)];

	public override int MaxUpgradeLevel => 0;

	public WitheringPresence()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = cardPlay;
		await FriendlyAmalgamCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);

		QueenWitheringPresencePower? power = await PowerCmd.Apply<QueenWitheringPresencePower>(
			choiceContext,
			base.Owner.Creature,
			1m,
			base.Owner.Creature,
			this);
		power?.Configure(DynamicVars["WitheringEnchantAmount"].BaseValue);
	}
}
