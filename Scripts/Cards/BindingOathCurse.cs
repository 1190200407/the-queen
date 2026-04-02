using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Afflictions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ComicChess.TheQueen;

[Pool(typeof(CurseCardPool))]
public sealed class BindingOathCurse : QueenCardModel
{
	private const int energyCost = -1;
	private const CardType type = CardType.Curse;
	private const CardRarity rarity = CardRarity.Curse;
	private const TargetType targetType = TargetType.None;
	private const bool shouldShowInCardLibrary = true;

	public override int MaxUpgradeLevel => 0;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, QueenKeyword.fade, CardKeyword.Eternal];

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [QueenHoverTips.BindingOath];

	public BindingOathCurse()
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	public override async Task BeforeCombatStart()
	{
		// 仅当拥有者处于战斗中且尚未获得该 Power 时才施加。
		if (base.Owner?.Creature?.CombatState == null)
		{
			return;
		}

		if (!base.Owner.Creature.HasPower<BindingOathPower>())
		{
			await PowerCmd.Apply<BindingOathPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
		}
	}
}

