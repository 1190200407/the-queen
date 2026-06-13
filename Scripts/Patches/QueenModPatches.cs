using STS2RitsuLib.Patching.Core;
using STS2RitsuLib.Patching.Models;

namespace ComicChess.TheQueen;

/// <summary>集中注册本 mod 全部 Ritsu <see cref="IPatchMethod"/>。</summary>
internal sealed class QueenModPatches : IModPatches
{
	public static void AddTo(ModPatcher patcher)
	{
		// 核心战斗 / 卡牌逻辑
		patcher.RegisterPatch<FadeOnDiscardCardPileCmdAddPatch>();
		patcher.RegisterPatch<FadeOnDiscardCardPileAddInternalPatch>();
		patcher.RegisterPatch<FadeOnDiscardCardPileCmdAddFlushPatch>();
		patcher.RegisterPatch<FadeOnDiscardCombatHistoryCardDiscardedPatch>();
		patcher.RegisterPatch<FadeOnDiscardHookAfterCardDiscardedPatch>();
		patcher.RegisterPatch<BurningSticksAfterCardExhaustedPatch>();
		patcher.RegisterPatch<BindingOathHookShouldPlayPatch>();
		patcher.RegisterPatch<BindingOathHookBeforeCardPlayedPatch>();
		patcher.RegisterPatch<BindingOathHookBeforeTurnEndPatch>();
		patcher.RegisterPatch<BindingOathHookBeforeCombatStartPatch>();
		patcher.RegisterPatch<ChainsOfBindingHookBeforeCombatStartPatch>();
		patcher.RegisterPatch<ChainsOfBindingPowerShouldPlayPatch>();
		patcher.RegisterPatch<ChainsOfBindingPowerBeforeCardPlayedPatch>();
		patcher.RegisterPatch<ChainsOfBindingPowerAfterCardDrawnPatch>();
		patcher.RegisterPatch<ChainsOfBindingPowerBeforeTurnEndPatch>();
		patcher.RegisterPatch<MindControlHookBeforeAttackPatch>();
		patcher.RegisterPatch<MindControlHookAfterAttackPatch>();
		patcher.RegisterPatch<MindControlCreatureCmdDamagePatch>();
		patcher.RegisterPatch<MindControlAttackIntentGetSingleDamagePatch>();
		patcher.RegisterPatch<QueenSummonOnCombatStartPatch>();
		patcher.RegisterPatch<NCombatRoomAddCreatureFriendlyAmalgamPatch>();
		patcher.RegisterPatch<OstyAmalgamSummonCrossRedirectPatch>();
		patcher.RegisterPatch<CreatureCmdHealNullGuardPatch>();
		patcher.RegisterPatch<PersonalHivePowerAmalgamDealerTransferPatch>();
		patcher.RegisterPatch<CardCmdDiscardAndDrawPatch>();
		patcher.RegisterPatch<CardPileCmdAddCrossOwnerHandCleanupPatch>();
		patcher.RegisterPatch<ExcludeNonDeckableFromCardTransformPatch>();
		patcher.RegisterPatch<UnfinishedCalamityPowerPatch>();
		patcher.RegisterPatch<MagicTimeGainEnergyPatch>();
		patcher.RegisterPatch<BurnEnchantmentTurnEndInHandPatch>();
		patcher.RegisterPatch<ReleasePickupShouldAddToDeckPatch>();
		patcher.RegisterPatch<ReleaseMerchantPurchasePatch>();
		patcher.RegisterPatch<WrigglePickupShouldAddToDeckPatch>();
		patcher.RegisterPatch<WriggleMerchantPurchasePatch>();
		patcher.RegisterPatch<PaperCutsPickupShouldAddToDeckPatch>();
		patcher.RegisterPatch<PaperCutsMerchantPurchasePatch>();

		// UI / 预览 / 表现（可选失败不关停 mod）
		patcher.RegisterPatch<BoundDescriptionPreviewPatch>();
		patcher.RegisterPatch<BoundOverlayPreviewPatch>();
		patcher.RegisterPatch<SoulLightBuiltInOverlayPatch>();
		patcher.RegisterPatch<NCardSoulLightOverlayRefreshPatch>();
		patcher.RegisterPatch<EcosphereBottleTargetingPreview_NPotionHolder_TargetNode_Patch>();
		patcher.RegisterPatch<EcosphereBottleTargetingPreview_NTargetManager_FinishTargeting_Patch>();
		patcher.RegisterPatch<EnemyIntentRewardCardPreview_NMouseCardPlay_SingleCreatureTargeting_Patch>();
		patcher.RegisterPatch<EnemyIntentRewardCardPreview_NControllerCardPlay_SingleCreatureTargeting_Patch>();
		patcher.RegisterPatch<EnemyIntentRewardCardPreview_NCard_SetPreviewTarget_Patch>();
		patcher.RegisterPatch<EnemyIntentRewardCardPreview_NCardPlay_Cleanup_Patch>();
		patcher.RegisterPatch<EnemyIntentRewardCardPreview_Hook_AfterCombatEnd_Patch>();
		patcher.RegisterPatch<NCardInfectionCurseOverlayPatch>();
		patcher.RegisterPatch<NCardRewardSelectionScreenSelectCardGuardPatch>();
		patcher.RegisterPatch<NCreatureAnimDisableUiFriendlyAmalgamPatch>();
		patcher.RegisterPatch<NCreatureOnPowerIncreasedShouldPlayVfxPatch>();
		patcher.RegisterPatch<NCreatureStateDisplayTrackBlockStatusPatch>();
		patcher.RegisterPatch<NIntentAmalgamBlockValueLabelPatch>();
		patcher.RegisterPatch<BigMushroomGrowScalePatch>();
		patcher.RegisterPatch<SurroundedPowerQueenFacingPatch>();
		patcher.RegisterPatch<QueenRestSiteHideFlameGlowPatch>();
	}
}
