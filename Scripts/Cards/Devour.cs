using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

using STS2RitsuLib.Interop.AutoRegistration;

using STS2RitsuLib.Keywords;
namespace ComicChess.TheQueen;

[RegisterCard(typeof(TokenCardPool))]
public sealed class Devour : QueenCardModel
{
	private const int energyCost = 0;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Token;
	private const TargetType constructorTargetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	/// <summary>消逝 + 魂缚；文案由补丁/关键词展示，勿在 <c>cards.json</c> 重复写。</summary>
	internal override bool HasSelfBound => true;

	protected override IEnumerable<string> RegisteredKeywordIds => [QueenKeyword.Fade];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new PowerVar<StrengthPower>(1m),
		new CalculationBaseVar(0m),
		new CalculationExtraVar(1m),
		new CalculatedVar("HungerPower").WithMultiplier(static (CardModel card, Creature? _) =>
			card.Owner?.Creature?.GetPower<QueenHungerPower>() is { Amount: > 0 } h ? h.Amount : 0m),
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		ModKeywordRegistry.CreateHoverTip(QueenKeyword.Fade),
		HoverTipFactory.FromPower<StrengthPower>(),
		.. HoverTipFactory.FromAffliction<Bound>(),
	];

	public Devour()
		: base(energyCost, type, rarity, constructorTargetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.Owner?.Creature is not { } creature)
		{
			return;
		}

		decimal strAmount = base.DynamicVars.Strength.BaseValue;
		await PowerCmd.Apply<DevourStrengthPower>(choiceContext, creature, strAmount, creature, this);

		if (creature.GetPower<QueenHungerPower>() is { Amount: > 0 } hunger)
		{
			await QueenCardCmd.AddSoulLamp(base.Owner, hunger.Amount);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Strength.UpgradeValueBy(1m);
	}
}
