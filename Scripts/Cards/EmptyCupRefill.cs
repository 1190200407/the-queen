// using System.Collections.Generic;
// using System.Threading.Tasks;
// 
// using MegaCrit.Sts2.Core.Entities.Cards;
// using MegaCrit.Sts2.Core.GameActions.Multiplayer;
// using MegaCrit.Sts2.Core.HoverTips;
// using MegaCrit.Sts2.Core.Models.CardPools;

// namespace ComicChess.TheQueen;

// 
// public sealed class EmptyCupRefill : LearnIntentCardModel
// {
//     private const int energyCost = 0;
//     private const CardType type = CardType.Skill;
//     private const CardRarity rarity = CardRarity.Rare;
//     private const TargetType targetType = TargetType.Self;
//     private const bool shouldShowInCardLibrary = true;

//     public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

//     protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
//     [
//         ..base.AdditionalHoverTips,
//         base.EnergyHoverTip
//     ];

//     public EmptyCupRefill()
//         : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
//     {
//     }

//     protected override Task<IReadOnlyList<AmalgamActionModel?>> CreateLearnIntentsAsync(PlayerChoiceContext choiceContext, CardPlay cardPlay)
//     {
//         _ = choiceContext;
//         _ = cardPlay;
//         return Task.FromResult<IReadOnlyList<AmalgamActionModel?>>([AmalgamActionRegistry.CreateEmptyCup()]);
//     }
// }
