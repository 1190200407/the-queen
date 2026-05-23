using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

using STS2RitsuLib.Interop.AutoRegistration;

using STS2RitsuLib.Keywords;
namespace ComicChess.TheQueen;

[RegisterCard(typeof(TokenCardPool))]
public sealed class AwakenPain : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Token;
	private const TargetType targetType = TargetType.AnyEnemy;
	private const bool shouldShowInCardLibrary = false;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [ModKeywordRegistry.GetCardKeyword(QueenKeyword.Fade)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
		HoverTipFactory.FromPower<VulnerablePower>(),
		ModKeywordRegistry.CreateHoverTip(QueenKeyword.Fade)
	];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VulnerablePower>(2m)];

	internal override bool HasSelfBound => true;

	public AwakenPain()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Target == null)
		{
			return;
		}

		await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Vulnerable.UpgradeValueBy(1m);
	}
}
