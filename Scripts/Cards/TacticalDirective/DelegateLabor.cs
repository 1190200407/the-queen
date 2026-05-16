using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

using STS2RitsuLib.Interop.AutoRegistration;

using STS2RitsuLib.Keywords;
namespace ComicChess.TheQueen;

/// <summary>代劳：你失去所有力量，聚合体获得等量力量。</summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class DelegateLabor : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Token;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = false;

	public override int MaxUpgradeLevel => 0;

	internal override bool HasSelfBound => true;

	protected override IEnumerable<string> RegisteredKeywordIds => [QueenKeyword.Fade];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromPower<StrengthPower>(),
		ModKeywordRegistry.CreateHoverTip(QueenKeyword.Fade)
	];

	public DelegateLabor()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = choiceContext;
		_ = cardPlay;
		Creature self = base.Owner.Creature;
		CombatState? cs = self.CombatState;
		Creature? amalgam = cs != null ? FriendlyAmalgamCmd.GetExisting(cs, base.Owner) : null;
		if (amalgam is not { IsAlive: true })
		{
			return;
		}

		StrengthPower? playerStr = self.GetPower<StrengthPower>();
		decimal transfer = playerStr?.Amount ?? 0m;
		if (transfer > 0m)
		{
			await PowerCmd.SetAmount<StrengthPower>(self, 0m, self, this);
			await PowerCmd.Apply<StrengthPower>(amalgam, transfer, self, this);
		}

		await CreatureCmd.TriggerAnim(self, "Cast", base.Owner.Character.CastAnimDelay);
	}
}
