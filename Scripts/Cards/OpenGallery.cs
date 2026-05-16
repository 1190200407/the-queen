using System.Threading.Tasks;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.HoverTips;

using STS2RitsuLib.Interop.AutoRegistration;

namespace ComicChess.TheQueen;

/// <summary>开馆：每位联机队友获得一张「入场券」（升级后的开馆生成升级后的入场券）�?/summary>

[RegisterCard(typeof(QueenCardPool))]
public sealed class OpenGallery : QueenCardModel
{
	private const int energyCost = 1;
	private const CardType type = CardType.Skill;
	private const CardRarity rarity = CardRarity.Uncommon;
	private const TargetType targetType = TargetType.Self;
	private const bool shouldShowInCardLibrary = true;

	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromCard<AdmissionTicket>(upgrade: base.IsUpgraded)];
    public OpenGallery()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		_ = cardPlay;
		if (base.Owner.Creature.CombatState is not { } combatState)
		{
			return;
		}

		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

		foreach (Player teammate in base.Owner.RunState.Players)
		{
			if (teammate.NetId == base.Owner.NetId)
			{
				continue;
			}

			AdmissionTicket ticket = combatState.CreateCard<AdmissionTicket>(teammate);
			ticket.SetGiftFromQueen(base.Owner);
			if (base.IsUpgraded)
			{
				CardCmd.Upgrade(ticket);
			}

			await CardPileCmd.AddGeneratedCardToCombat(ticket, PileType.Hand, addedByPlayer: true);
		}
	}
}
