using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
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
    public const string Toadpole = "TOADPOLE";
    public const string CalcifiedCultist = "CALCIFIED_CULTIST";
    public const string FossilStalker = "FOSSIL_STALKER";
    public const string HauntedShip = "HAUNTED_SHIP";
    public const string PunchConstruct = "PUNCH_CONSTRUCT";
    public const string SkulkingColony = "SKULKING_COLONY";
    public const string WaterfallGiant = "WATERFALL_GIANT";
    public const string SoulFysh = "SOUL_FYSH";
    public const string CorpseSlug = "CORPSE_SLUG";
    public const string LagavulinMatriarch = "LAGAVULIN_MATRIARCH";
    public const string KinPriest = "KIN_PRIEST";

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
            { SewerClam, static owner => owner.RunState!.CreateCard<Screech>(owner) },
            { Toadpole, static owner => owner.RunState!.CreateCard<Spiken>(owner) },
            { CalcifiedCultist, static owner => owner.RunState!.CreateCard<Incantation>(owner) },
            { FossilStalker, static owner => owner.RunState!.CreateCard<Suck>(owner) },
            { HauntedShip, static owner => owner.RunState!.CreateCard<Haunt>(owner) },
            { PunchConstruct, static owner => owner.RunState!.CreateCard<PunchOff>(owner) },
            { SkulkingColony, static owner => owner.RunState!.CreateCard<HardenedShell>(owner) },
            { WaterfallGiant, static owner => owner.RunState!.CreateCard<SteamEruption>(owner) },
            { SoulFysh, static owner => owner.RunState!.CreateCard<Beckon>(owner) },
            { CorpseSlug, static owner => owner.RunState!.CreateCard<CorpseSlugHunger>(owner) },
            { LagavulinMatriarch, static owner => owner.RunState!.CreateCard<SoulSiphon>(owner) },
            { KinPriest, static owner => owner.RunState!.CreateCard<TheKins>(owner) },
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

        if (CaptureOnceRegistry.TryMarkCaptured(enemy, owner.Creature.CombatState))
        {
            return null;
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
