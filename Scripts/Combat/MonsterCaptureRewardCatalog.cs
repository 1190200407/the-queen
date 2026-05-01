using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace ComicChess.TheQueen;

/// <summary>
/// 按怪物 <see cref="AbstractId.Entry"/> 决定「捕获」成功时的额外卡牌奖励类型。
/// 未配置的怪物返回 <c>null</c>：不展示意图位奖励预览，也不在捕获成功时给予该奖励。
/// </summary>
public static class MonsterCaptureRewardCatalog
{
    /// <summary>原版 <see cref="MegaCrit.Sts2.Core.Models.Monsters.Flyconid"/> 的 Id。</summary>
    public const string Flyconid = "FLYCONID";
    public const string BruteRubyRaider = "BRUTE_RUBY_RAIDER";
    public const string AssassinRubyRaider = "ASSASSIN_RUBY_RAIDER";
    public const string AxeRubyRaider = "AXE_RUBY_RAIDER";
    public const string TrackerRubyRaider = "TRACKER_RUBY_RAIDER";
    public const string Byrdonis = "BYRDONIS";
    public const string BygoneEffigy = "BYGONE_EFFIGY";
    public const string Mawler = "MAWLER";
    public const string FuzzyWurmCrawler = "FUZZY_WURM_CRAWLER";
    public const string LeafSlimeS = "LEAF_SLIME_S";
    public const string TwigSlimeS = "TWIG_SLIME_S";
    public const string LeafSlimeM = "LEAF_SLIME_M";
    public const string TwigSlimeM = "TWIG_SLIME_M";
    public const string Inklet = "INKLET";
    public const string SnappingJaxfruit = "SNAPPING_JAXFRUIT";
    public const string CeremonialBeast = "CEREMONIAL_BEAST";
    public const string Vantom = "VANTOM";
    public const string Seapunk = "SEAPUNK";
    public const string Fogmog = "FOGMOG";
    public const string SludgeSpinner = "SLUDGE_SPINNER";
    public const string Nibbit = "NIBBIT";
    public const string CubexConstruct = "CUBEX_CONSTRUCT";
    public const string SlitheringStrangler = "SLITHERING_STRANGLER";
    public const string ShrinkerBeetle = "SHRINKER_BEETLE";
    public const string SewerClam = "SEWER_CLAM";
    public const string TwoTailedRat = "TWO_TAILED_RAT";
    public const string Toadpole = "TOADPOLE";
    public const string CalcifiedCultist = "CALCIFIED_CULTIST";
    public const string FossilStalker = "FOSSIL_STALKER";
    public const string HauntedShip = "HAUNTED_SHIP";
    public const string PunchConstruct = "PUNCH_CONSTRUCT";
    public const string SkulkingColony = "SKULKING_COLONY";
    public const string Exoskeleton = "EXOSKELETON";
    public const string WaterfallGiant = "WATERFALL_GIANT";
    public const string SoulFysh = "SOUL_FYSH";
    public const string CorpseSlug = "CORPSE_SLUG";
    public const string LagavulinMatriarch = "LAGAVULIN_MATRIARCH";
    public const string KinPriest = "KIN_PRIEST";
    public const string GremlinMerc = "gremlin_merc";
    public const string SneakyGremlin = "SNEAKY_GREMLIN";
    public const string Wriggler = "WRIGGLER";
    public const string PhrogParasite = "PHROG_PARASITE";
    public const string PhantasmalGardener = "PHANTASMAL_GARDENER";
    public const string CrossbowRubyRaider = "CROSSBOW_RUBY_RAIDER";
    public const string VineShambler = "VINE_SHAMBLER";
    public const string LivingFog = "LIVING_FOG";
    public const string FatGremlin = "FAT_GREMLIN";
    public const string TerrorEel = "TERROR_EEL";
    public const string Tunneler = "TUNNELER";
    public const string Chomper = "CHOMPER";
    public const string HunterKiller = "HUNTER_KILLER";
    public const string ThievingHopper = "THIEVING_HOPPER";
    public const string TheObscura = "THE_OBSCURA";
    public const string SpinyToad = "SPINY_TOAD";
    public const string Myte = "MYTE";
    public const string LouseProgenitor = "LOUSE_PROGENITOR";
    public const string DecimillipedeSegment = "DECIMILLIPEDE_SEGMENT";
    public const string Entomancer = "ENTOMANCER";
    public const string InfestedPrism = "INFESTED_PRISM";

    private static readonly Dictionary<string, Func<Player, CardModel>> RewardCreators =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { Flyconid, static owner => owner.RunState!.CreateCard<FrailSpores>(owner) },
            { BruteRubyRaider, static owner => owner.RunState!.CreateCard<Roar>(owner) },
            { AssassinRubyRaider, static owner => owner.RunState!.CreateCard<Killshot>(owner) },
            { AxeRubyRaider, static owner => owner.RunState!.CreateCard<Swing>(owner) },
            { TrackerRubyRaider, static owner => owner.RunState!.CreateCard<Hounds>(owner) },
            { Byrdonis, static owner => owner.RunState!.CreateCard<Territorial>(owner) },
            { BygoneEffigy, static owner => owner.RunState!.CreateCard<YourJoueneyEndsHere>(owner) },
            { Mawler, static owner => owner.RunState!.CreateCard<RoarClaw>(owner) },
            { FuzzyWurmCrawler, static owner => owner.RunState!.CreateCard<AcidGoop>(owner) },
            { LeafSlimeS, static owner => owner.RunState!.CreateCard<Goop>(owner) },
            { TwigSlimeS, static owner => owner.RunState!.CreateCard<Goop>(owner) },
            { LeafSlimeM, static owner => owner.RunState!.CreateCard<StickyShot>(owner) },
            { TwigSlimeM, static owner => owner.RunState!.CreateCard<StickyShot>(owner) },
            { Inklet, static owner => owner.RunState!.CreateCard<Slippery>(owner) },
            { SnappingJaxfruit, static owner => owner.RunState!.CreateCard<EnergyOrb>(owner) },
            { CeremonialBeast, static owner => owner.RunState!.CreateCard<BeastCry>(owner) },
            { Vantom, static owner => owner.RunState!.CreateCard<AllSlippery>(owner) },
            { Seapunk, static owner => owner.RunState!.CreateCard<SpinningKick>(owner) },
            { Fogmog, static owner => owner.RunState!.CreateCard<Illusion>(owner) },
            { SludgeSpinner, static owner => owner.RunState!.CreateCard<OilSpray>(owner) },
            { Nibbit, static owner => owner.RunState!.CreateCard<Butt>(owner) },
            { CubexConstruct, static owner => owner.RunState!.CreateCard<DoubleBlast>(owner) },
            { SlitheringStrangler, static owner => owner.RunState!.CreateCard<Constrict>(owner) },
            { ShrinkerBeetle, static owner => owner.RunState!.CreateCard<Shrinker>(owner) },
            { SewerClam, static owner => owner.RunState!.CreateCard<Plating>(owner) },
            { TwoTailedRat, static owner => owner.RunState!.CreateCard<Screech>(owner) },
            { Toadpole, static owner => owner.RunState!.CreateCard<Spiken>(owner) },
            { CalcifiedCultist, static owner => owner.RunState!.CreateCard<Incantation>(owner) },
            { FossilStalker, static owner => owner.RunState!.CreateCard<Suck>(owner) },
            { HauntedShip, static owner => owner.RunState!.CreateCard<Haunt>(owner) },
            { PunchConstruct, static owner => owner.RunState!.CreateCard<PunchOff>(owner) },
            { SkulkingColony, static owner => owner.RunState!.CreateCard<HardenedShell>(owner) },
            { Exoskeleton, static owner => owner.RunState!.CreateCard<HardToKill>(owner) },
            { WaterfallGiant, static owner => owner.RunState!.CreateCard<SteamEruption>(owner) },
            { SoulFysh, static owner => owner.RunState!.CreateCard<Beckon>(owner) },
            { CorpseSlug, static owner => owner.RunState!.CreateCard<CorpseSlugHunger>(owner) },
            { LagavulinMatriarch, static owner => owner.RunState!.CreateCard<SoulSiphon>(owner) },
            { KinPriest, static owner => owner.RunState!.CreateCard<TheKins>(owner) },
            { GremlinMerc, static owner => owner.RunState!.CreateCard<Gimme>(owner) },
            { SneakyGremlin, static owner => owner.RunState!.CreateCard<GremlinStab>(owner) },
            { FatGremlin, static owner => owner.RunState!.CreateCard<Flee>(owner) },
            { Wriggler, static owner => owner.RunState!.CreateCard<Wriggle>(owner) },
            { PhrogParasite, static owner => owner.RunState!.CreateCard<Lash>(owner) },
            { PhantasmalGardener, static owner => owner.RunState!.CreateCard<Skittish>(owner) },
            { CrossbowRubyRaider, static owner => owner.RunState!.CreateCard<ReloadFire>(owner) },
            { VineShambler, static owner => owner.RunState!.CreateCard<GraspingVines>(owner) },
            { LivingFog, static owner => owner.RunState!.CreateCard<Smoggy>(owner) },
            { TerrorEel, static owner => owner.RunState!.CreateCard<Shriek>(owner) },
            { Tunneler, static owner => owner.RunState!.CreateCard<Burrow>(owner) },
            { Chomper, static owner => owner.RunState!.CreateCard<Clamp>(owner) },
            { HunterKiller, static owner => owner.RunState!.CreateCard<Tender>(owner) },
            { ThievingHopper, static owner => owner.RunState!.CreateCard<Swipe>(owner) },
            { TheObscura, static owner => owner.RunState!.CreateCard<Illusion2>(owner) },
            { SpinyToad, static owner => owner.RunState!.CreateCard<ProtrudingSpikes>(owner) },
            { Myte, static owner => owner.RunState!.CreateCard<ToxicCard>(owner) },
            { LouseProgenitor, static owner => owner.RunState!.CreateCard<CurlUp>(owner) },
            { DecimillipedeSegment, static owner => owner.RunState!.CreateCard<Reattach>(owner) },
            { Entomancer, static owner => owner.RunState!.CreateCard<PheromoneSpit>(owner) },
            { InfestedPrism, static owner => owner.RunState!.CreateCard<VitalSpark>(owner) },
        };

    /// <summary>为捕获预览或 <see cref="CaptureSuccessPower"/> 创建奖励牌实例；无配置时返回 <c>null</c>。</summary>
    public static CardModel? TryCreateCaptureRewardCard(Player owner, Creature enemy)
    {
        if (enemy.Monster?.Id.Entry is not { } monsterId)
        {
            return null;
        }
        if (owner.Creature.CombatState is null)
        {
            return null;
        }

        // 预览模式（敌人仍存活）不可产生副作用：不写入「已捕获」。
        // 仅在真实捕获（敌人已死亡）时做“每战一次”去重。
        if (!enemy.IsAlive)
        {
            if (!CaptureOnceRegistry.TryMarkCaptured(enemy, owner.Creature.CombatState))
            {
                return null;
            }
        }

        return TryCreateCaptureRewardCard(owner, monsterId);
    }

    /// <summary>按怪物 Id 直接创建对应敌怪卡；无配置时返回 <c>null</c>。</summary>
    public static CardModel? TryCreateCaptureRewardCard(Player owner, string monsterId)
    {
        if (owner.RunState is null
            || !RewardCreators.TryGetValue(monsterId, out Func<Player, CardModel>? create))
        {
            return null;
        }

        return create(owner);
    }
}
