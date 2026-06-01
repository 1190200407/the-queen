using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace ComicChess.TheQueen;

/// <summary>
/// 按怪物 <see cref="AbstractId.Entry"/> 决定「捕获」成功时的额外卡牌奖励类型。
/// 未配置的怪物返回 <c>null</c>：不展示意图位奖励预览，也不在捕获成功时给予该奖励。
/// </summary>
public static class MonsterCaptureRewardCatalog
{
    /// <summary>当前战斗遭遇的房间类型（普通 / 精英 / 首领等）。</summary>
    public static RoomType? GetEncounterRoomType(CombatState? combatState) =>
        combatState?.Encounter?.RoomType;

    /// <summary>原版 <see cref="MegaCrit.Sts2.Core.Models.Monsters.Flyconid"/> 的 Id。</summary>
    public const string Flyconid = "FLYCONID";
    public const string BruteRubyRaider = "BRUTE_RUBY_RAIDER";
    public const string AssassinRubyRaider = "ASSASSIN_RUBY_RAIDER";
    public const string AxeRubyRaider = "AXE_RUBY_RAIDER";
    public const string Axebot = "AXEBOT";
    public const string BowlbugRock = "BOWLBUG_ROCK";
    public const string BowlbugEgg = "BOWLBUG_EGG";
    public const string BowlbugNectar = "BOWLBUG_NECTAR";
    public const string BowlbugSilk = "BOWLBUG_SILK";
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
    public const string Ovicopter = "OVICOPTER";
    public const string LouseProgenitor = "LOUSE_PROGENITOR";
    public const string DecimillipedeSegment = "DECIMILLIPEDE_SEGMENT";
    public const string DecimillipedeSegmentBack = "DECIMILLIPEDE_SEGMENT_BACK";
    public const string DecimillipedeSegmentFront = "DECIMILLIPEDE_SEGMENT_FRONT";
    public const string DecimillipedeSegmentMiddle = "DECIMILLIPEDE_SEGMENT_MIDDLE";
    public const string DevotedSculptor = "DEVOTED_SCULPTOR";
    public const string Entomancer = "ENTOMANCER";
    public const string InfestedPrism = "INFESTED_PRISM";
    public const string Doormaker = "DOORMAKER";
    public const string TestSubject = "TEST_SUBJECT";
    public const string Crusher = "CRUSHER";
    public const string Rocket = "ROCKET";
    public const string TheInsatiable = "THE_INSATIABLE";
    public const string Fabricator = "FABRICATOR";
    public const string FlailKnight = "FLAIL_KNIGHT";
    public const string FrogKnight = "FROG_KNIGHT";
    public const string GlobeHead = "GLOBE_HEAD";
    public const string LivingShield = "LIVING_SHIELD";
    public const string MagiKnight = "MAGI_KNIGHT";
    public const string MechaKnight = "MECHA_KNIGHT";
    public const string OwlMagistrate = "OWL_MAGISTRATE";
    public const string ScrollOfBiting = "SCROLL_OF_BITING";
    public const string SlimedBerserker = "SLIMED_BERSERKER";
    public const string SoulNexus = "SOUL_NEXUS";
    public const string SlumberingBeetle = "SLUMBERING_BEETLE";
    public const string SpectralKnight = "SPECTRAL_KNIGHT";
    public const string TheForgotten = "THE_FORGOTTEN";
    public const string TheLost = "THE_LOST";
    public const string TurretOperator = "TURRET_OPERATOR";
    public const string KnowledgeDemon = "KNOWLEDGE_DEMON";
    public const string Aeonglass = "AEONGLASS";

    private static readonly Dictionary<string, Func<Player, CardModel>> RewardCreators =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { Flyconid, static owner => owner.RunState!.CreateCard<FrailSpores>(owner) },
            { BruteRubyRaider, static owner => owner.RunState!.CreateCard<Roar>(owner) },
            { AssassinRubyRaider, static owner => owner.RunState!.CreateCard<Killshot>(owner) },
            { AxeRubyRaider, static owner => owner.RunState!.CreateCard<Swing>(owner) },
            { Axebot, static owner => owner.RunState!.CreateCard<Stock>(owner) },
            { BowlbugRock, static owner => owner.RunState!.CreateCard<Headbutt>(owner) },
            { BowlbugEgg, static owner => owner.RunState!.CreateCard<Bite>(owner) },
            { BowlbugNectar, static owner => owner.RunState!.CreateCard<Buff>(owner) },
            { BowlbugSilk, static owner => owner.RunState!.CreateCard<ToxicSpit>(owner) },
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
            { LivingFog, static owner => owner.RunState!.CreateCard<AdvancedGas>(owner) },
            { TerrorEel, static owner => owner.RunState!.CreateCard<Shriek>(owner) },
            { Tunneler, static owner => owner.RunState!.CreateCard<Burrow>(owner) },
            { Chomper, static owner => owner.RunState!.CreateCard<Clamp>(owner) },
            { HunterKiller, static owner => owner.RunState!.CreateCard<Tender>(owner) },
            { ThievingHopper, static owner => owner.RunState!.CreateCard<Swipe>(owner) },
            { TheObscura, static owner => owner.RunState!.CreateCard<Illusion2>(owner) },
            { SpinyToad, static owner => owner.RunState!.CreateCard<ProtrudingSpikes>(owner) },
            { Myte, static owner => owner.RunState!.CreateCard<ToxicCard>(owner) },
            { Ovicopter, static owner => owner.RunState!.CreateCard<LayEggs>(owner) },
            { LouseProgenitor, static owner => owner.RunState!.CreateCard<CurlUp>(owner) },
            { DecimillipedeSegment, static owner => owner.RunState!.CreateCard<Reattach>(owner) },
            { DecimillipedeSegmentBack, static owner => owner.RunState!.CreateCard<Reattach>(owner) },
            { DecimillipedeSegmentFront, static owner => owner.RunState!.CreateCard<Reattach>(owner) },
            { DecimillipedeSegmentMiddle, static owner => owner.RunState!.CreateCard<Reattach>(owner) },
            { DevotedSculptor, static owner => owner.RunState!.CreateCard<ForbiddenIncantation>(owner) },
            { Entomancer, static owner => owner.RunState!.CreateCard<PheromoneSpit>(owner) },
            { InfestedPrism, static owner => owner.RunState!.CreateCard<VitalSpark>(owner) },
            { Doormaker, static owner => owner.RunState!.CreateCard<CloseDoor>(owner) },
            { TestSubject, static owner => owner.RunState!.CreateCard<TestSubject>(owner) },
            { Crusher, static owner => owner.RunState!.CreateCard<CrabRage>(owner) },
            { Rocket, static owner => owner.RunState!.CreateCard<BackAttack>(owner) },
            { TheInsatiable, static owner => owner.RunState!.CreateCard<LiquifyGround>(owner) },
            { Fabricator, static owner => owner.RunState!.CreateCard<Fabricate>(owner) },
            { FlailKnight, static owner => owner.RunState!.CreateCard<Flail>(owner) },
            { FrogKnight, static owner => owner.RunState!.CreateCard<BeetleCharge>(owner) },
            { GlobeHead, static owner => owner.RunState!.CreateCard<Galvanic>(owner) },
            { LivingShield, static owner => owner.RunState!.CreateCard<Rampart>(owner) },
            { MagiKnight, static owner => owner.RunState!.CreateCard<Dampen>(owner) },
            { MechaKnight, static owner => owner.RunState!.CreateCard<Flamethrower>(owner) },
            { OwlMagistrate, static owner => owner.RunState!.CreateCard<Soar>(owner) },
            { ScrollOfBiting, static owner => owner.RunState!.CreateCard<PaperCuts>(owner) },
            { SlimedBerserker, static owner => owner.RunState!.CreateCard<VomitIcho>(owner) },
            { SlumberingBeetle, static owner => owner.RunState!.CreateCard<RollOut>(owner) },
            { SoulNexus, static owner => owner.RunState!.CreateCard<DrainLife>(owner) },
            { SpectralKnight, static owner => owner.RunState!.CreateCard<Hex>(owner) },
            { TheForgotten, static owner => owner.RunState!.CreateCard<Miasma>(owner) },
            { TheLost, static owner => owner.RunState!.CreateCard<DebilitatingSmog>(owner) },
            { TurretOperator, static owner => owner.RunState!.CreateCard<Unload>(owner) },
            { KnowledgeDemon, static owner => owner.RunState!.CreateCard<CurseOfKnowledge>(owner) },
            { Aeonglass, static owner => owner.RunState!.CreateCard<WitheringPresence>(owner) },
        };

    /// <summary>为捕获预览或 <see cref="CaptureSuccessPower"/> 创建奖励牌实例；无配置时返回 <c>null</c>。</summary>
    public static CardModel? TryCreateCaptureRewardCard(Player owner, Creature enemy)
    {
        if (enemy.Monster?.Id.Entry is not { } monsterId)
        {
            return null;
        }
        if (owner.Creature.CombatState is not { } combatState)
        {
            return null;
        }

        if (CaptureOnceRegistry.HasPlayerCapturedMonster(owner, monsterId, combatState))
        {
            return null;
        }

        // 预览（敌人仍存活）只查配额，不写入；真实捕获时再占用该玩家对该怪物的名额。
        if (!enemy.IsAlive && !CaptureOnceRegistry.TryMarkPlayerCapturedMonster(owner, monsterId, combatState))
        {
            return null;
        }

        return CreateCaptureRewardCard(owner, monsterId);
    }

    /// <summary>按怪物 Id 直接创建对应敌怪卡并占用该玩家对该怪物的捕获名额；无配置时返回 <c>null</c>。</summary>
    public static CardModel? TryCreateCaptureRewardCard(Player owner, string monsterId)
    {
        if (owner.Creature.CombatState is not { } combatState)
        {
            return null;
        }

        if (CaptureOnceRegistry.HasPlayerCapturedMonster(owner, monsterId, combatState)
            || !CaptureOnceRegistry.TryMarkPlayerCapturedMonster(owner, monsterId, combatState))
        {
            return null;
        }

        return CreateCaptureRewardCard(owner, monsterId);
    }

    private static CardModel? CreateCaptureRewardCard(Player owner, string monsterId)
    {
        if (owner.RunState is null
            || !RewardCreators.TryGetValue(monsterId, out Func<Player, CardModel>? create))
        {
            return null;
        }

        return create(owner);
    }
}
