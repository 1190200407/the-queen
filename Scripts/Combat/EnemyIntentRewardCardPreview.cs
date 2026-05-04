using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ComicChess.TheQueen;

/// <summary>
/// 在敌人 <see cref="NCreatureVisuals.IntentPosition"/> 上挂奖励/预览用 <see cref="NCard"/>。
/// 四函数：显示全部、隐藏全部、放大某一怪、缩小某一怪；供捕获及后续其它预览复用。
/// 放大时对该预览 <see cref="NCard"/> 的模型显示 <see cref="NHoverTipSet"/>（随卡移动），缩小时移除。
/// Hover tip 位置：<see cref="NHoverTipSet.CreateAndShow"/> 内用私有的 SetAlignment(owner, alignment)；
/// alignment 来自 <see cref="HoverTip.GetHoverTipAlignment(Control, float)"/>（手牌用 <see cref="NCardHolder"/> 的 SetAlignmentForCardHolder，与裸 <see cref="NCard"/> 不同）。
/// 在本类用 <see cref="EnlargedHoverTipAlignmentThreshold"/> / <see cref="EnlargedHoverTipExtraFollowOffset"/> 微调即可，一般不必改 <see cref="NCard"/>。
/// 叠放：放大时 <see cref="IntentRewardPreviewRoot"/> 挂到 <see cref="NRun.GlobalUi"/> 顶层；hover tip 提高 <see cref="Control.ZIndex"/> 并移到 <see cref="NGame.HoverTipsContainer"/> 子节点末尾。
/// </summary>
internal static partial class EnemyIntentRewardCardPreview
{
    /// <summary>
    /// <see cref="HoverTip.GetHoverTipAlignment(Control, float)"/> 的屏幕分界线（原版默认 0.75；<see cref="NCreature"/> 自身提示用 0.5）。
    /// </summary>
    private const float EnlargedHoverTipAlignmentThreshold = 0.5f;

    /// <summary>
    /// <see cref="NHoverTipSet.SetExtraFollowOffset"/>：在 <see cref="NHoverTipSet.SetFollowOwner"/> 之后每帧叠加的全局偏移（正右、正下）。
    /// </summary>
    private static readonly Vector2 EnlargedHoverTipExtraFollowOffset = new Vector2(-350f, -120f);

    /// <summary>放大预览根挂在 <see cref="NRun.GlobalUi"/> 上时的 <see cref="Control.ZIndex"/>（需高于手牌 holder）。</summary>
    private const int EnlargedPreviewZOnGlobalUi = 160;

    /// <summary>捕获预览 hover tip 在 <see cref="NGame.HoverTipsContainer"/> 内相对其它 tip 的叠放。</summary>
    private const int EnlargedPreviewHoverTipZIndex = 320;

    private const string CardScenePath = "res://scenes/cards/card.tscn";

    private static readonly Vector2 BaseScale = Vector2.One * 0.42f;

    /// <summary>与战斗中常规手牌相同的显示尺寸。</summary>
    private static readonly Vector2 LargeScale = Vector2.One * 0.84f;

    private const float ScaleTweenDuration = 0.22f;

    /// <summary>相对 <see cref="Marker2D"/> 意图点，整卡预览根节点略下移（屏幕 Y+）。</summary>
    private static readonly Vector2 SceneRootLocalOffset = new(0f, 50f);

    private static readonly Vector2 CardPositionInAnchor =
        new(0, -NCard.defaultSize.Y * 0.5f);

    private static readonly Dictionary<Creature, Slot> Slots = new(CreatureRefComparer.Instance);

    private sealed class CreatureRefComparer : IEqualityComparer<Creature>
    {
        internal static readonly CreatureRefComparer Instance = new();

        public bool Equals(Creature? x, Creature? y) => ReferenceEquals(x, y);

        public int GetHashCode(Creature obj) => RuntimeHelpers.GetHashCode(obj);
    }

    private static bool _sessionActive;

    private static Creature? _lastEnlargedCreature;

    /// <summary>在若干敌人的意图位创建或复用 <see cref="NCard"/>，仅当 <paramref name="getPreviewModel"/> 对该怪返回非 null 时显示。</summary>
    public static void ShowAllRewardCards(
        Player owner,
        IEnumerable<Creature> creatures,
        Func<Player, Creature, CardModel?> getPreviewModel)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(creatures);
        ArgumentNullException.ThrowIfNull(getPreviewModel);

        HideAllRewardCards();

        bool anyShown = false;
        foreach (Creature creature in creatures)
        {
            if (!creature.IsHittable)
            {
                continue;
            }

            CardModel? model = getPreviewModel(owner, creature);
            if (model is null)
            {
                continue;
            }

            if (!Slots.TryGetValue(creature, out Slot? slot))
            {
                slot = CreateSlot(creature);
                if (slot is null)
                {
                    continue;
                }

                Slots[creature] = slot;
            }

            slot.Card.Model = model;
            slot.HoverTipsShownForEnlarged = false;
            slot.Card.Visibility = ModelVisibility.Visible;
            slot.Card.MouseFilter = Control.MouseFilterEnum.Ignore;
            slot.Card.ZIndex = 80;
            KillScaleTween(slot);
            slot.Card.Position = CardPositionInAnchor;
            slot.Card.Scale = Vector2.One;
            ApplyScaleInstant(slot.ScaleRoot, BaseScale);
            slot.Card.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
            slot.ScaleRoot.Visible = true;
            slot.Card.Visible = true;
            anyShown = true;
        }

        _sessionActive = anyShown;
    }

    public static void HideAllRewardCards()
    {
        _sessionActive = false;
        _lastEnlargedCreature = null;
        foreach (Slot slot in Slots.Values)
        {
            RemoveEnlargedPreviewHoverTip(slot);
            slot.ScaleRoot.SetDrawAboveCombatHand(false);
            KillScaleTween(slot);
            if (GodotObject.IsInstanceValid(slot.ScaleRoot))
            {
                slot.ScaleRoot.Visible = false;
            }

            if (GodotObject.IsInstanceValid(slot.Card))
            {
                slot.Card.Visible = false;
            }
        }
    }

    public static void EnlargeRewardCardForCreature(Creature creature)
    {
        if (!_sessionActive || !Slots.TryGetValue(creature, out Slot? slot)
            || !GodotObject.IsInstanceValid(slot.Card) || !GodotObject.IsInstanceValid(slot.ScaleRoot))
        {
            return;
        }

        if (!slot.Card.Visible)
        {
            return;
        }

        slot.ScaleRoot.SetDrawAboveCombatHand(true);
        AnimateSlotTo(slot, LargeScale);
        TryShowEnlargedPreviewHoverTip(slot);
    }

    public static void ShrinkRewardCardForCreature(Creature creature)
    {
        if (!Slots.TryGetValue(creature, out Slot? slot) || !GodotObject.IsInstanceValid(slot.Card)
            || !GodotObject.IsInstanceValid(slot.ScaleRoot))
        {
            return;
        }

        RemoveEnlargedPreviewHoverTip(slot);
        slot.ScaleRoot.SetDrawAboveCombatHand(false);
        AnimateSlotTo(slot, BaseScale);
    }

    /// <summary>与 <see cref="NCard.SetPreviewTarget"/> 同步：当前指向的怪放大，离开或换目标时缩小。</summary>
    internal static void SyncEnlargeWithPreviewTarget(Creature? previewTarget)
    {
        if (!_sessionActive)
        {
            return;
        }

        if (previewTarget is not null)
        {
            if (_lastEnlargedCreature is not null && !ReferenceEquals(_lastEnlargedCreature, previewTarget))
            {
                ShrinkRewardCardForCreature(_lastEnlargedCreature);
            }

            if (Slots.ContainsKey(previewTarget))
            {
                EnlargeRewardCardForCreature(previewTarget);
                _lastEnlargedCreature = previewTarget;
            }
            else
            {
                _lastEnlargedCreature = null;
            }
        }
        else
        {
            if (_lastEnlargedCreature is not null)
            {
                ShrinkRewardCardForCreature(_lastEnlargedCreature);
                _lastEnlargedCreature = null;
            }
        }
    }

    internal static void ResetAfterCombat()
    {
        HideAllRewardCards();
        foreach (Slot slot in Slots.Values)
        {
            KillScaleTween(slot);
            if (GodotObject.IsInstanceValid(slot.ScaleRoot))
            {
                slot.ScaleRoot.QueueFreeSafely();
            }
        }

        Slots.Clear();
    }

    private static void ApplyScaleInstant(Control scaleRoot, Vector2 scale)
    {
        scaleRoot.Scale = scale;
    }

    private static void TryShowEnlargedPreviewHoverTip(Slot slot)
    {
        if (slot.HoverTipsShownForEnlarged || !GodotObject.IsInstanceValid(slot.Card))
        {
            return;
        }

        CardModel? model = slot.Card.Model;
        if (model is null || !model.HoverTips.Any())
        {
            return;
        }

        if (!slot.Card.IsInsideTree())
        {
            return;
        }

        // 单体选目标时 NTargetManager 会置 shouldBlockHoverTips，CreateAndShow 会跳过 Init 导致 NRE。
        // 仅在为捕获预览卡展示 tip 的极短窗口内临时解除屏蔽，与 FinishTargeting 的清屏语义一致。
        bool hoverTipsWereBlocked = NHoverTipSet.shouldBlockHoverTips;
        if (hoverTipsWereBlocked)
        {
            NHoverTipSet.shouldBlockHoverTips = false;
        }

        try
        {
            NHoverTipSet.Remove(slot.Card);
            HoverTipAlignment alignment = HoverTip.GetHoverTipAlignment(slot.Card, EnlargedHoverTipAlignmentThreshold);
            NHoverTipSet tipSet = NHoverTipSet.CreateAndShow(slot.Card, model.HoverTips, alignment);
            tipSet.SetFollowOwner();
            tipSet.SetExtraFollowOffset(EnlargedHoverTipExtraFollowOffset);
            tipSet.ZAsRelative = false;
            tipSet.ZIndex = EnlargedPreviewHoverTipZIndex;
            if (NGame.Instance?.HoverTipsContainer is Node hoverTips)
            {
                NHoverTipSet tipRef = tipSet;
                Callable.From(() =>
                {
                    if (!GodotObject.IsInstanceValid(hoverTips) || !GodotObject.IsInstanceValid(tipRef)
                        || tipRef.GetParent() != hoverTips)
                    {
                        return;
                    }

                    hoverTips.MoveChild(tipRef, hoverTips.GetChildCount() - 1);
                }).CallDeferred();
            }

            slot.HoverTipsShownForEnlarged = true;
        }
        finally
        {
            if (hoverTipsWereBlocked)
            {
                NHoverTipSet.shouldBlockHoverTips = true;
            }
        }
    }

    private static void RemoveEnlargedPreviewHoverTip(Slot slot)
    {
        if (!slot.HoverTipsShownForEnlarged || !GodotObject.IsInstanceValid(slot.Card))
        {
            return;
        }

        NHoverTipSet.Remove(slot.Card);
        slot.HoverTipsShownForEnlarged = false;
    }

    private static void KillScaleTween(Slot slot)
    {
        if (GodotObject.IsInstanceValid(slot.ScaleTween))
        {
            slot.ScaleTween.Kill();
        }

        slot.ScaleTween = null;
    }

    private static void AnimateSlotTo(Slot slot, Vector2 targetScale)
    {
        Control root = slot.ScaleRoot;
        if (!GodotObject.IsInstanceValid(root))
        {
            return;
        }

        KillScaleTween(slot);
        Tween tween = root.CreateTween();
        slot.ScaleTween = tween;
        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(root, "scale", targetScale, ScaleTweenDuration);
        tween.Finished += () =>
        {
            if (slot.ScaleTween == tween)
            {
                slot.ScaleTween = null;
            }
        };
    }

    private static Slot? CreateSlot(Creature creature)
    {
        NCreature? node = NCombatRoom.Instance?.GetCreatureNode(creature);
        Marker2D? marker = node?.Visuals.IntentPosition;
        if (marker is null || !GodotObject.IsInstanceValid(marker))
        {
            return null;
        }

        IntentRewardPreviewRoot scaleRoot = new IntentRewardPreviewRoot
        {
            Name = "IntentRewardCardPreviewRoot",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Position = SceneRootLocalOffset,
        };
        scaleRoot.AttachIntentMarker(marker, SceneRootLocalOffset);
        marker.AddChildSafely(scaleRoot);

        NCard card = PreloadManager.Cache.GetScene(CardScenePath).Instantiate<NCard>(PackedScene.GenEditState.Disabled);
        card.Visible = false;
        scaleRoot.AddChildSafely(card);
        card.Position = CardPositionInAnchor;
        card.Scale = Vector2.One;
        scaleRoot.Visible = false;
        return new Slot(scaleRoot, card);
    }

    /// <summary>
    /// 预览卡根：默认在意图 <see cref="Marker2D"/> 下；放大时挂 <see cref="NRun.GlobalUi"/> 顶层（<see cref="EnlargedPreviewZOnGlobalUi"/>），避免被手牌挡住。
    /// </summary>
    private sealed partial class IntentRewardPreviewRoot : Control
    {
        private Marker2D? _intentMarker;

        private Vector2 _offsetInMarkerSpace;

        private bool _drawAboveCombatHand;

        public void AttachIntentMarker(Marker2D marker, Vector2 offsetInMarkerSpace)
        {
            _intentMarker = marker;
            _offsetInMarkerSpace = offsetInMarkerSpace;
        }

        public void SetDrawAboveCombatHand(bool above)
        {
            if (above == _drawAboveCombatHand)
            {
                return;
            }

            if (_intentMarker is null || !GodotObject.IsInstanceValid(_intentMarker))
            {
                return;
            }

            NRun? run = NRun.Instance;
            Control? globalUi = run?.GlobalUi;
            if (above && globalUi is null)
            {
                return;
            }

            _drawAboveCombatHand = above;
            if (above)
            {
                if (GetParent() == globalUi)
                {
                    ApplyGlobalUiDrawOrder();
                    return;
                }

                Reparent(globalUi!, keepGlobalTransform: true);
                ApplyGlobalUiDrawOrder();
                Callable.From(() =>
                {
                    if (!GodotObject.IsInstanceValid(globalUi) || !GodotObject.IsInstanceValid(this)
                        || GetParent() != globalUi)
                    {
                        return;
                    }

                    globalUi.MoveChild(this, globalUi.GetChildCount() - 1);
                }).CallDeferred();
            }
            else
            {
                if (GetParent() == _intentMarker)
                {
                    ResetLocalDrawOrderUnderMarker();
                    return;
                }

                Reparent(_intentMarker, keepGlobalTransform: true);
                ResetLocalDrawOrderUnderMarker();
            }
        }

        public override void _Process(double delta)
        {
            if (!_drawAboveCombatHand || _intentMarker is null || !GodotObject.IsInstanceValid(_intentMarker))
            {
                return;
            }

            GlobalPosition = _intentMarker.ToGlobal(_offsetInMarkerSpace);
        }

        private void ApplyGlobalUiDrawOrder()
        {
            ZAsRelative = false;
            ZIndex = EnlargedPreviewZOnGlobalUi;
        }

        private void ResetLocalDrawOrderUnderMarker()
        {
            ZAsRelative = true;
            ZIndex = 0;
            Position = _offsetInMarkerSpace;
        }
    }

    private sealed class Slot(IntentRewardPreviewRoot scaleRoot, NCard card)
    {
        public readonly IntentRewardPreviewRoot ScaleRoot = scaleRoot;

        public readonly NCard Card = card;

        public Tween? ScaleTween;

        /// <summary>避免 <see cref="EnlargeRewardCardForCreature"/> 每帧重复调用时反复创建 hovertip。</summary>
        public bool HoverTipsShownForEnlarged;
    }
}
