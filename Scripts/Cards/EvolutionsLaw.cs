// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;

// using MegaCrit.Sts2.Core.Commands;
// using MegaCrit.Sts2.Core.Entities.Cards;
// using MegaCrit.Sts2.Core.GameActions.Multiplayer;
// using MegaCrit.Sts2.Core.HoverTips;
// using MegaCrit.Sts2.Core.Localization.DynamicVars;
// using MegaCrit.Sts2.Core.Models.Afflictions;
// using MegaCrit.Sts2.Core.Models.CardPools;
// using MegaCrit.Sts2.Core.ValueProps;

// using STS2RitsuLib.Interop.AutoRegistration;

// namespace ComicChess.TheQueen;

// /// <summary>进化论：造成伤害；下一张学习意图牌耗能�?0；将此牌侵蚀为魂缚�?/summary>

// [RegisterCard(typeof(QueenCardPool))]
// public sealed class EvolutionsLaw : QueenCardModel
// {
// 	private const int energyCost = 2;
// 	private const CardType type = CardType.Attack;
// 	private const CardRarity rarity = CardRarity.Uncommon;
// 	private const TargetType targetType = TargetType.AnyEnemy;
// 	private const bool shouldShowInCardLibrary = true;
//     internal override bool HasSelfBound => true;

//     protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move)];

// 	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
// 	[
// 		HoverTipFactory.FromPower<EvolutionsLawFreeLearnIntentPower>(),
// 		QueenHoverTips.LearnIntent,
// 		.. HoverTipFactory.FromAffliction<Bound>()
// 	];

// 	public EvolutionsLaw()
// 		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
// 	{
// 	}

// 	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
// 	{
// 		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

// 		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
// 			.FromCard(this)
// 			.Targeting(cardPlay.Target)
// 			.WithHitFx("vfx/vfx_attack_blunt")
// 			.Execute(choiceContext);

// 		await PowerCmd.Apply<EvolutionsLawFreeLearnIntentPower>(choiceContext, 
// 			base.Owner.Creature,
// 			1m,
// 			base.Owner.Creature,
// 			this);
// 	}

// 	protected override void OnUpgrade()
// 	{
// 		base.DynamicVars.Damage.UpgradeValueBy(3m);
// 	}
// }
