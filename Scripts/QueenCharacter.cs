
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Scaffolding.Visuals.StateMachine;

namespace ComicChess.TheQueen;

[RegisterCharacter]
public class QueenCharacter : ModCharacterTemplate<QueenCardPool, QueenRelicPool, QueenPotionPool>
{
    // 角色名称颜色rgb(161, 67, 144)
    public override Color NameColor => new(161f/255f, 67f/255f, 144f/255f);
    // 能量图标轮廓颜色rgb(44, 97, 24)
    public override Color EnergyLabelOutlineColor => new(44f/255f, 97f/255f, 24f/255f);
    // 地图绘画颜色rgb(161, 67, 144)
    public override Color MapDrawingColor => new(161f/255f, 67f/255f, 144f/255f);

    // 人物性别（男女中立）
    public override CharacterGender Gender => CharacterGender.Feminine;

    // 初始血量
    public override int StartingHp => 66;
    public override int StartingGold => 99;

    // 人物模型tscn路径。要自定义见下。
    public override string CustomVisualsPath => "res://TheQueen/scenes/creature_visuals/queen_character.tscn";
    // 卡牌拖尾场景。
    public override string CustomTrailPath => "res://scenes/vfx/card_trail_silent.tscn";
    // 人物头像路径。
    public override string CustomIconTexturePath => "res://TheQueen/images/charui/queen_boss.png";
    // 人物头像2号。
    public override string CustomIconPath => "res://TheQueen/scenes/ui/queen_icon.tscn";
    // 能量表盘tscn路径。要自定义见下。
    public override string CustomEnergyCounterPath => "res://TheQueen/scenes/ui/queen_energy_counter.tscn";
    // 篝火休息场景。
    public override string CustomRestSiteAnimPath => "res://TheQueen/scenes/creature_visuals/queen_rest_site.tscn";
    // 商店人物场景。
    public override string CustomMerchantAnimPath => "res://TheQueen/scenes/creature_visuals/queen_character_merchant.tscn";
    // 多人模式-手指。
    public override string CustomArmPointingTexturePath => "res://TheQueen/images/hands/multiplayer_hand_queen_point.png";
    // 多人模式剪刀石头布-石头。
    public override string CustomArmRockTexturePath => "res://TheQueen/images/hands/multiplayer_hand_queen_rock.png";
    // 多人模式剪刀石头布-布。
    public override string CustomArmPaperTexturePath => "res://TheQueen/images/hands/multiplayer_hand_queen_paper.png";
    // 多人模式剪刀石头布-剪刀。
    public override string CustomArmScissorsTexturePath => "res://TheQueen/images/hands/multiplayer_hand_queen_scissors.png";

    // 人物选择背景。
    public override string? CustomCharacterSelectBgPath => "res://TheQueen/scenes/screens/char_select_bg_queen.tscn";
    // 人物选择图标。
    public override string CustomCharacterSelectIconPath => "res://TheQueen/images/charui/char_select_queen.png";
    // 人物选择图标-锁定状态。
    public override string CustomCharacterSelectLockedIconPath => "res://TheQueen/images/charui/char_select_queen_locked.png";
    // 人物选择过渡动画。
    // public override string CustomCharacterSelectTransitionPath => "res://materials/transitions/ironclad_transition_mat.tres";
    // 地图上的角色标记图标、表情轮盘上的角色头像
    public override string CustomMapMarkerPath => "res://TheQueen/images/charui/queen_map_marker.png";
    // 攻击音效
    // public override string CustomAttackSfx => null;
    // 施法音效
    public override string CustomCastSfx => "event:/sfx/enemy/enemy_attacks/queen/queen_cast";
    // 死亡音效
    public override string CustomDeathSfx => "event:/sfx/enemy/enemy_attacks/queen/queen_die";
    // 角色选择音效
    public override string CharacterSelectSfx => "event:/sfx/enemy/enemy_attacks/queen/queen_cast";
    // 过渡音效。这个不能删。
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

    public override float AttackAnimDelay => 0.25f;

    public override float CastAnimDelay => 0.2f;

    protected override NCreatureVisuals? TryCreateCreatureVisuals() => RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(CustomVisualsPath);

    protected override Type? UnlocksAfterRunAsType => typeof(Defect);

    /// <summary>叠加轨循环动画（不占 track 0 的 attack/hurt 等）。原版女王 Boss 同用 <c>trackId = 1</c>。</summary>
    private const string WritheOverlayAnim = "tracks/writhe";

    /// <summary>Spine 叠加轨索引：0 = 战斗主动画，1 = writhe 层（对应常说的「第二条 track」）。</summary>
    private const int WritheOverlayTrackId = 1;

    /// <summary>
    /// track 0：标准战斗 trigger（Idle / Attack / Hit / Cast / Dead）。
    /// track 1：常驻 <see cref="WritheOverlayAnim"/>，与主动画并行，类似 Unity Animator 多 Layer。
    /// </summary>
    protected override ModAnimStateMachine? SetupCustomCombatAnimationStateMachine(Node visualsRoot, CharacterModel character)
    {
        var machine = base.SetupCustomCombatAnimationStateMachine(visualsRoot, character);
        
        if (visualsRoot is NCreatureVisuals { HasSpineAnimation: true, SpineBody: { } ready })
        {
            ready.GetAnimationState().SetAnimation(WritheOverlayAnim, loop: true, WritheOverlayTrackId);
        }
        return machine;
    }

    public override bool RequiresEpochAndTimeline => false;

    // 攻击建筑师的攻击特效列表
    public override List<string> GetArchitectAttackVfx() => [
        "vfx/vfx_attack_blunt",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_attack_slash",
        "vfx/vfx_bloody_impact",
        "vfx/vfx_rock_shatter"
    ];
}