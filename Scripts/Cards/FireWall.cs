using System.Collections.Generic;
using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;


[RegisterCard(typeof(QueenCardPool))]
public sealed class FireWall : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<SoulLampPower>()];

	public FireWall()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.IsUpgraded)
		{
			await QueenCardCmd.AddSoulLamp(base.Owner, 1);
		}

		SoulLampPower? lamp = base.Owner.Creature.GetPower<SoulLampPower>();
		decimal stacks = lamp?.DisplayAmount ?? 0m;
		if (stacks == 0m)
		{
			return;
		}
		decimal total = base.DynamicVars.Block.BaseValue * stacks;
		await CreatureCmd.GainBlock(base.Owner.Creature, total, ValueProp.Move, cardPlay);
	}
}
