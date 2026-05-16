
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
    // 角色名称颜色 #814390
    public override Color NameColor => new(161f/255f, 67f/255f, 144f/255f);
    // 能量图标轮廓颜色rgb(161, 67, 144)
    public override Color EnergyLabelOutlineColor => new(161f/255f, 67f/255f, 144f/255f);
    // 能量图标轮廓颜色rgb(161, 67, 144)
    public override Color MapDrawingColor => new(161f/255f, 67f/255f, 144f/255f);


    // 人物性别（男女中立）
    public override CharacterGender Gender => CharacterGender.Feminine;

    // 初始血量
    public override int StartingHp => 66;
    public override int StartingGold => 99;

    // 人物模型tscn路径。要自定义见下。
    public override string CustomVisualsPath => "res://TheQueen/scenes/creature_visuals/queen_character.tscn";
    // 卡牌拖尾场景。
    // public override string CustomTrailPath => "res://scenes/vfx/card_trail_ironclad.tscn";
    // 人物头像路径。
    public override string CustomIconTexturePath => "res://TheQueen/images/charui/queen_boss.png";
    // 人物头像2号。
    public override string CustomIconPath => "res://TheQueen/scenes/ui/queen_icon.tscn";
    // 能量表盘tscn路径。要自定义见下。
    //public override string CustomEnergyCounterPath => "res://test/scenes/test_energy_counter.tscn";
    // 篝火休息场景。
    // public override string CustomRestSiteAnimPath => "res://scenes/rest_site/characters/ironclad_rest_site.tscn";
    // 商店人物场景。
    // public override string CustomMerchantAnimPath => "res://scenes/merchant/characters/ironclad_merchant.tscn";
    // 多人模式-手指。
    public override string CustomArmPointingTexturePath => "res://TheQueen/images/hands/multiplayer_hand_ironclad_point.png";
    // 多人模式剪刀石头布-石头。
    public override string CustomArmRockTexturePath => "res://TheQueen/images/hands/multiplayer_hand_ironclad_rock.png";
    // 多人模式剪刀石头布-布。
    public override string CustomArmPaperTexturePath => "res://TheQueen/images/hands/multiplayer_hand_ironclad_paper.png";
    // 多人模式剪刀石头布-剪刀。
    public override string CustomArmScissorsTexturePath => "res://TheQueen/images/hands/multiplayer_hand_ironclad_scissors.png";

    // 人物选择背景。
    //public override string CustomCharacterSelectBg => "res://test/scenes/test_bg.tscn";
    // 人物选择图标。
    public override string CustomCharacterSelectIconPath => "res://TheQueen/images/charui/char_select_queen.png";
    // 人物选择图标-锁定状态。
    public override string CustomCharacterSelectLockedIconPath => "res://TheQueen/images/charui/char_select_queen_locked.png";
    // 人物选择过渡动画。
    // public override string CustomCharacterSelectTransitionPath => "res://materials/transitions/ironclad_transition_mat.tres";
    // 地图上的角色标记图标、表情轮盘上的角色头像
    //public override string CustomMapMarkerPath => "res://TheQueen/images/charui/queen_boss.png";
    // 攻击音效
    // public override string CustomAttackSfx => null;
    // 施法音效
    // public override string CustomCastSfx => null;
    // 死亡音效
    // public override string CustomDeathSfx => null;
    // 角色选择音效
    // public override string CharacterSelectSfx => null;
    // 过渡音效。这个不能删。
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

    public override float AttackAnimDelay => 0.25f;

    public override float CastAnimDelay => 0.2f;

    protected override NCreatureVisuals? TryCreateCreatureVisuals() => RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(CustomVisualsPath);

    protected override Type? UnlocksAfterRunAsType => typeof(Defect);
    protected override ModAnimStateMachine? SetupCustomCombatAnimationStateMachine(Node visualsRoot, CharacterModel character)
    {
        ModAnimStateMachine? animator = base.SetupCustomCombatAnimationStateMachine(visualsRoot, character);
        //animator?.GetAnimationState().SetAnimation("tracks/writhe", loop: true, 1);
        return animator;
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