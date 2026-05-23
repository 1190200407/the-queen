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
public sealed class DiffuseCharge : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Power;
	private const CardRarity rarity = CardRarity.Token;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = false;

	internal override bool HasSelfBound => true;
	public override IEnumerable<CardKeyword> CanonicalKeywords => [ModKeywordRegistry.GetCardKeyword(QueenKeyword.Fade)];
	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DexterityPower>(1m)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
		HoverTipFactory.FromPower<DexterityPower>(),
		ModKeywordRegistry.CreateHoverTip(QueenKeyword.Fade)
	];

	public DiffuseCharge()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<DexterityPower>(choiceContext, base.Owner.Creature, base.DynamicVars.Dexterity.BaseValue, base.Owner.Creature, this);
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Dexterity.UpgradeValueBy(1m);
	}
}
