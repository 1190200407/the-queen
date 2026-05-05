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
using MegaCrit.Sts2.Core.Logging;
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
/// 叠放：放大时 <see cref="IntentRewardPreviewRoot"/> 挂到 <see cref="NRun.GlobalUi"/> 顶层；hover tip 提高 <see cref="Control.ZIndex"/> 并移到 <see cref="NGame.HoverTipsContainer"/> 子节点末尾。夹紧在放大时显式调用，不按帧跟随意图点。
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

    /// <summary>放大夹紧时，卡面包围盒相对<strong>本视口可见矩形</strong>边缘的最小留白（像素）。</summary>
    private const float PreviewClampViewportMargin = 12f;

    /// <summary>
    /// 在 <see cref="GetPreviewCardClampBounds"/> 得到的矩形外再扩张一圈（屏幕像素），用于卡面/描边/粒子略超出 <see cref="NCard"/> 逻辑 <see cref="Control.GetGlobalRect"/> 的情况。
    /// </summary>
    private const float PreviewClampCardBoundsOutset = 12f;

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

    /// <summary>进程内只打一次诊断（改日志字段时递增后缀，便于再打一版）。</summary>
    private static bool _loggedIntentPreviewClampDiagV5Once;

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
            EnsureIntentPreviewCardHasLayoutSize(slot.Card);
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

        EnsureIntentPreviewCardHasLayoutSize(slot.Card);
        slot.ScaleRoot.SetDrawAboveCombatHand(true);
        // 放大改为瞬时缩放（便于排除 Tween 期间 scale 逐帧变化对夹紧/跟随的影响）；缩小仍用 AnimateSlotTo。
        KillScaleTween(slot);
        ApplyScaleInstant(slot.ScaleRoot, LargeScale);
        slot.ScaleRoot.ApplyFollowAndClampFromIntentNow();
        IntentRewardPreviewRoot previewRootForDeferredClamp = slot.ScaleRoot;
        Callable.From(() =>
        {
            if (!GodotObject.IsInstanceValid(previewRootForDeferredClamp))
            {
                return;
            }

            previewRootForDeferredClamp.ApplyFollowAndClampFromIntentNow();
        }).CallDeferred();
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

    /// <summary>
    /// 意图预览不经过手牌 <see cref="NCardHolder"/>，父 <see cref="IntentRewardPreviewRoot"/> 也无布局尺寸，
    /// 导致 <see cref="NCard"/> 的 <see cref="Control.Size"/> 保持 (0,0)、<see cref="Control.GetGlobalRect"/> 面积为零（日志已证实）。
    /// 显式最小/尺寸与 <see cref="NCard.defaultSize"/> 一致，使夹紧与 hover 对齐按真实卡面矩形计算。
    /// </summary>
    private static void EnsureIntentPreviewCardHasLayoutSize(NCard card)
    {
        if (!GodotObject.IsInstanceValid(card))
        {
            return;
        }

        card.CustomMinimumSize = NCard.defaultSize;
        if (card.Size.X < 2f || card.Size.Y < 2f)
        {
            card.Size = NCard.defaultSize;
        }
    }

    /// <summary>
    /// 将预览根的全局位置限制在视口内，使 <paramref name="card"/> 的包围盒落在安全矩形内。
    /// 多轮迭代：同一帧内父节点 <see cref="Control.Scale"/> 由 Tween 更新后，<see cref="Control.GetGlobalRect"/> 才会稳定，单轮夹紧容易仍越界。
    /// 视口矩形用 <paramref name="root"/> 所在 <see cref="Viewport.GetVisibleRect"/>（与预览根、<see cref="NCard"/> 的 <see cref="Control.GetGlobalRect"/> 同一画布坐标），
    /// 不用 <see cref="NGame.GetViewportRect"/>，避免与 <see cref="NRun.GlobalUi"/> 下控件的全局坐标不一致导致要把 margin 调到接近卡宽才“看起来对”。
    /// 包围盒优先 <see cref="Control.GetGlobalRect"/>；若尺寸过小（布局未提交等），用 <see cref="NCard.defaultSize"/> 经 <see cref="CanvasItem.GetGlobalTransform"/> 变换后的轴对齐外包矩形。
    /// </summary>
    private static Vector2 ClampPreviewRootGlobalPositionForCardOnScreen(
        IntentRewardPreviewRoot root,
        NCard card,
        Vector2 desiredRootGlobalPos)
    {
        if (!GodotObject.IsInstanceValid(root) || !GodotObject.IsInstanceValid(card))
        {
            return desiredRootGlobalPos;
        }

        Viewport? viewport = root.GetViewport();
        if (viewport is null)
        {
            return desiredRootGlobalPos;
        }

        Rect2 visible = viewport.GetVisibleRect();
        float m = PreviewClampViewportMargin;
        Rect2 safe = new(visible.Position + new Vector2(m, m), visible.Size - new Vector2(2f * m, 2f * m));

        Vector2 pos = desiredRootGlobalPos;
        Rect2 boundsRef = default;
        Vector2 boundsRefRootPos = default;
        const int maxIterations = 8;
        for (int i = 0; i < maxIterations; i++)
        {
            root.GlobalPosition = pos;
            Rect2 bounds;
            if (i == 0)
            {
                boundsRef = GetPreviewCardClampBounds(card);
                boundsRefRootPos = pos;
                bounds = boundsRef;
            }
            else
            {
                bounds = new Rect2(boundsRef.Position + (pos - boundsRefRootPos), boundsRef.Size);
            }

            Vector2 correction = Vector2.Zero;

            if (bounds.Position.X < safe.Position.X)
            {
                correction.X += safe.Position.X - bounds.Position.X;
            }

            float boundsRight = bounds.Position.X + bounds.Size.X;
            float safeRight = safe.Position.X + safe.Size.X;
            if (boundsRight > safeRight)
            {
                correction.X -= boundsRight - safeRight;
            }

            if (bounds.Position.Y < safe.Position.Y)
            {
                correction.Y += safe.Position.Y - bounds.Position.Y;
            }

            float boundsBottom = bounds.Position.Y + bounds.Size.Y;
            float safeBottom = safe.Position.Y + safe.Size.Y;
            if (boundsBottom > safeBottom)
            {
                correction.Y -= boundsBottom - safeBottom;
            }

            if (correction == Vector2.Zero)
            {
                break;
            }

            pos += correction;
        }

        if (!_loggedIntentPreviewClampDiagV5Once)
        {
            _loggedIntentPreviewClampDiagV5Once = true;
            root.GlobalPosition = pos;
            Rect2 boundsAfter = GetPreviewCardClampBounds(card);
            Log.Info(BuildIntentPreviewClampDiagnostics(
                root,
                card,
                visible,
                safe,
                viewport,
                desiredRootGlobalPos,
                pos,
                boundsAfter));
        }

        return pos;
    }

    private static Rect2 MergeGlobalRects(Rect2 a, Rect2 b)
    {
        float x1 = Mathf.Min(a.Position.X, b.Position.X);
        float y1 = Mathf.Min(a.Position.Y, b.Position.Y);
        float x2 = Mathf.Max(a.Position.X + a.Size.X, b.Position.X + b.Size.X);
        float y2 = Mathf.Max(a.Position.Y + a.Size.Y, b.Position.Y + b.Size.Y);
        return new Rect2(new Vector2(x1, y1), new Vector2(x2 - x1, y2 - y1));
    }

    /// <summary>
    /// <see cref="NCard"/> 根上用于夹紧的轴对齐包围盒（无 outset）：逻辑 rect 或 defaultSize 回退，再与所有可见子孙 <see cref="Control"/> 的 <see cref="Control.GetGlobalRect"/> 合并
    /// （立绘/框常超出根 300×422，仅靠根 rect 会误判「在屏内」）。
    /// </summary>
    private static Rect2 GetPreviewCardClampBoundsCoreMerged(NCard card)
    {
        Rect2 core = GetPreviewCardClampBoundsCore(card);
        return MergeVisibleDescendantControlGlobalRects(card, core);
    }

    private static Rect2 GetPreviewCardClampBoundsCore(NCard card)
    {
        const float minDim = 2f;
        Rect2 gr = card.GetGlobalRect();
        if (gr.Size.X >= minDim && gr.Size.Y >= minDim && gr.Size.X * gr.Size.Y > 0f)
        {
            return gr;
        }

        Vector2 localSize = card.Size;
        if (localSize.X < minDim || localSize.Y < minDim)
        {
            localSize = new Vector2(
                Mathf.Max(card.CustomMinimumSize.X, NCard.defaultSize.X),
                Mathf.Max(card.CustomMinimumSize.Y, NCard.defaultSize.Y));
        }

        return GlobalAxisAlignedRectFromLocalRect(card, localSize);
    }

    /// <summary>
    /// <see cref="NCardHighlight"/> 的 <see cref="Control.GetGlobalRect"/> 为「整手可玩/高亮」级大矩形，与单卡视觉无关；
    /// 并入夹紧会误判需整块屏幕留白（日志里 area≈50 万即此类）。
    /// </summary>
    private static bool ShouldMergeControlIntoPreviewClampBounds(Control c)
    {
        return c is not NCardHighlight;
    }

    private static Rect2 MergeVisibleDescendantControlGlobalRects(Node root, Rect2 seed)
    {
        Rect2 m = seed;
        int visited = 0;
        const int maxVisit = 200;

        void Walk(Node n)
        {
            if (visited++ > maxVisit)
            {
                return;
            }

            foreach (Node ch in n.GetChildren())
            {
                if (ch is Control c && c.Visible && GodotObject.IsInstanceValid(c) && ShouldMergeControlIntoPreviewClampBounds(c))
                {
                    Rect2 r = c.GetGlobalRect();
                    if (r.Size.X > 2f && r.Size.Y > 2f && r.Size.X < 8000f && r.Size.Y < 8000f)
                    {
                        m = MergeGlobalRects(m, r);
                    }
                }

                Walk(ch);
            }
        }

        Walk(root);
        return m;
    }

    /// <summary>
    /// 夹紧用卡牌屏幕包围盒：根 + 子孙可见控件并包后再 <see cref="OutsetRect2"/>。
    /// </summary>
    private static Rect2 OutsetRect2(Rect2 r, float outset)
    {
        if (outset <= 0f)
        {
            return r;
        }

        return new Rect2(r.Position - new Vector2(outset, outset), r.Size + new Vector2(2f * outset, 2f * outset));
    }

    private static Rect2 GetPreviewCardClampBounds(NCard card)
    {
        Rect2 merged = GetPreviewCardClampBoundsCoreMerged(card);
        return OutsetRect2(merged, PreviewClampCardBoundsOutset);
    }

    /// <summary>与 <see cref="OutsetRect2"/> 相反：从已扩张的矩形收回一圈（用于日志还原逻辑卡面包围盒）。</summary>
    private static Rect2 InsetRect2(Rect2 r, float inset)
    {
        if (inset <= 0f)
        {
            return r;
        }

        return new Rect2(r.Position + new Vector2(inset, inset), r.Size - new Vector2(2f * inset, 2f * inset));
    }

    /// <summary>将控件局部 <c>(0,0)—size</c> 矩形经全局变换得到轴对齐外包（含旋转时的 AABB）。</summary>
    private static Rect2 GlobalAxisAlignedRectFromLocalRect(Control c, Vector2 localSize)
    {
        Transform2D xf = c.GetGlobalTransform();
        Vector2 p00 = xf * Vector2.Zero;
        Vector2 p10 = xf * new Vector2(localSize.X, 0f);
        Vector2 p01 = xf * new Vector2(0f, localSize.Y);
        Vector2 p11 = xf * localSize;
        float minX = Mathf.Min(Mathf.Min(p00.X, p10.X), Mathf.Min(p01.X, p11.X));
        float maxX = Mathf.Max(Mathf.Max(p00.X, p10.X), Mathf.Max(p01.X, p11.X));
        float minY = Mathf.Min(Mathf.Min(p00.Y, p10.Y), Mathf.Min(p01.Y, p11.Y));
        float maxY = Mathf.Max(Mathf.Max(p00.Y, p10.Y), Mathf.Max(p01.Y, p11.Y));
        return new Rect2(new Vector2(minX, minY), new Vector2(maxX - minX, maxY - minY));
    }

    private static string FormatBoundsVsSafe(Rect2 bounds, Rect2 safe)
    {
        float safeR = safe.Position.X + safe.Size.X;
        float safeB = safe.Position.Y + safe.Size.Y;
        float bR = bounds.Position.X + bounds.Size.X;
        float bB = bounds.Position.Y + bounds.Size.Y;
        bool l = bounds.Position.X >= safe.Position.X - 0.01f;
        bool t = bounds.Position.Y >= safe.Position.Y - 0.01f;
        bool r = bR <= safeR + 0.01f;
        bool bottom = bB <= safeB + 0.01f;
        return $"insideSafe=L{l} T{t} R{r} B{bottom} (all={l && t && r && bottom})";
    }

    /// <summary>卡面包围盒相对 <paramref name="safe"/> 四边剩余像素；负值表示穿出 safe（夹紧应已避免）。</summary>
    private static string FormatMarginsToSafe(Rect2 bounds, Rect2 safe, string label)
    {
        float left = bounds.Position.X - safe.Position.X;
        float top = bounds.Position.Y - safe.Position.Y;
        float right = (safe.Position.X + safe.Size.X) - (bounds.Position.X + bounds.Size.X);
        float bottom = (safe.Position.Y + safe.Size.Y) - (bounds.Position.Y + bounds.Size.Y);
        return $"  [留白→{label}] L={left:F1} T={top:F1} R={right:F1} B={bottom:F1} px（相对 safe；负=越界）";
    }

    /// <summary>列出 <see cref="NCard"/> 根及所有可见子孙 <see cref="Control"/> 的 <see cref="Control.GetGlobalRect"/>（按面积降序），用于定位谁把合并包围盒撑大。</summary>
    private static string FormatNCardDescendantControlRectsForDiagnostics(NCard card)
    {
        List<(string path, string typeName, Rect2 rect, float area)> rows = new();
        int visited = 0;
        const int maxVisit = 220;
        const int maxLines = 100;

        void Walk(Node n)
        {
            if (visited++ > maxVisit)
            {
                return;
            }

            foreach (Node ch in n.GetChildren())
            {
                if (ch is Control c && c.Visible && GodotObject.IsInstanceValid(c))
                {
                    Rect2 r = c.GetGlobalRect();
                    if (r.Size.X > 0.5f && r.Size.Y > 0.5f)
                    {
                        string pathNote = c is NCardHighlight ? " [合并夹紧时忽略]" : string.Empty;
                        rows.Add((c.GetPath().ToString() + pathNote, c.GetType().Name, r, r.Size.X * r.Size.Y));
                    }
                }

                Walk(ch);
            }
        }

        Rect2 rootGr = card.GetGlobalRect();
        rows.Add((card.GetPath().ToString() + " [NCard根]", card.GetType().Name, rootGr, rootGr.Size.X * rootGr.Size.Y));
        Walk(card);

        System.Text.StringBuilder sb = new();
        sb.AppendLine(
            "  [NCard 下各 Control 的 GetGlobalRect]（含根；按面积降序；合并夹紧时并包除 NCardHighlight 外的矩形——Highlight 为整手级大框）");
        IOrderedEnumerable<(string path, string typeName, Rect2 rect, float area)> ordered =
            rows.OrderByDescending(static x => x.area).ThenBy(static x => x.path);
        int n = 0;
        foreach ((string path, string typeName, Rect2 rect, float area) in ordered)
        {
            if (n++ >= maxLines)
            {
                sb.AppendLine($"  … 共 {rows.Count} 条，此处仅列出前 {maxLines} 条");
                break;
            }

            sb.AppendLine(
                $"    area={area:F0} rect={rect} [{typeName}] path={path}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// 首次夹紧<strong>计算完成后</strong>打一次：说明各字段含义、desired→clamped、卡面包围盒与 safe 关系、坐标空间对照。
    /// 不表示“每帧只执行一次夹紧”；仅避免刷屏。
    /// </summary>
    private static string BuildIntentPreviewClampDiagnostics(
        IntentRewardPreviewRoot root,
        NCard card,
        Rect2 visibleGame,
        Rect2 safe,
        Viewport viewport,
        Vector2 desiredRootGlobal,
        Vector2 clampedRootGlobal,
        Rect2 cardBoundsAfterClamp)
    {
        Rect2 vvr = viewport.GetVisibleRect();
        Rect2 gr = card.GetGlobalRect();
        Rect2 fb = GlobalAxisAlignedRectFromLocalRect(
            card,
            card.Size.X >= 2f && card.Size.Y >= 2f ? card.Size : NCard.defaultSize);

        Control? game = NGame.Instance;
        Control? globalUi = NRun.Instance?.GlobalUi;
        Rect2 coreCardAfterClamp = InsetRect2(cardBoundsAfterClamp, PreviewClampCardBoundsOutset);

        static string ControlLine(Control c)
        {
            return
                $"  [{c.GetType().Name}] name={c.Name} path={c.GetPath()} visible={c.Visible} size={c.Size} globalPos={c.GlobalPosition} scale={c.Scale} globalScale={c.GetGlobalTransform().Scale} zRel={c.ZAsRelative} z={c.ZIndex} globalRect={c.GetGlobalRect()}";
        }

        static string HierarchyUp(Node? n, string label)
        {
            System.Text.StringBuilder sb = new();
            sb.AppendLine(label);
            int depth = 0;
            while (n is not null && GodotObject.IsInstanceValid(n))
            {
                string indent = new string(' ', depth * 2);
                if (n is Control cc)
                {
                    sb.AppendLine($"{indent}{ControlLine(cc)}");
                }
                else
                {
                    sb.AppendLine($"{indent}[{n.GetType().Name}] name={n.Name} path={n.GetPath()}");
                }

                n = n.GetParent();
                depth++;
                if (depth > 32)
                {
                    sb.AppendLine($"{indent}…(truncated)");
                    break;
                }
            }

            return sb.ToString();
        }

        return
            "[IntentRewardCardPreview] one-shot diagnostics (after first clamp pass; see legend):\n"
            + "  [含义] visibleGame=viewport 全屏可见矩形；safe=调整后允许区域（visible 四边各向内 margin，卡用于夹紧的包围盒须完全落在 safe 内）。\n"
            + $"  [全视口 visibleGame] Position={visibleGame.Position} Size={visibleGame.Size}\n"
            + $"  [调整后允许区域 safe] Position={safe.Position} Size={safe.Size} (margin={PreviewClampViewportMargin}, outset={PreviewClampCardBoundsOutset})\n"
            + $"  [卡包围盒·夹紧用·含outset] {cardBoundsAfterClamp}\n"
            + $"  [卡包围盒·根+子孙合并后去outset] {coreCardAfterClamp}\n"
            + FormatMarginsToSafe(cardBoundsAfterClamp, safe, "safe 到「含outset」卡包边")
            + "\n"
            + FormatMarginsToSafe(coreCardAfterClamp, safe, "safe 到「合并后无outset」边")
            + "\n"
            + "  [含义续] desiredRootGlobal=意图 ToGlobal(offset)；夹紧包围盒已含 NCard 下可见子 Control 的 GetGlobalRect 并包。\n"
            + "  [其它因素] NHoverTipSet 只跟说明框。下行对比 NGame.GetViewportRect 与 viewport 可见区。\n"
            + $"  desiredRootGlobal={desiredRootGlobal} clampedRootGlobal={clampedRootGlobal} delta={clampedRootGlobal - desiredRootGlobal}\n"
            + $"  {FormatBoundsVsSafe(cardBoundsAfterClamp, safe)} cardBoundsAfterClamp={cardBoundsAfterClamp} safe={safe}\n"
            + $"  root.GlobalPosition={root.GlobalPosition} root.Scale={root.Scale} root.GetGlobalRect()={root.GetGlobalRect()}\n"
            + $"  NGame.GetViewportRect()={game?.GetViewportRect()} NGame.GetGlobalRect()={(game is null ? "null" : game.GetGlobalRect().ToString())}\n"
            + $"  NRun.GlobalUi.GetGlobalRect()={(globalUi is null ? "null" : globalUi.GetGlobalRect().ToString())}\n"
            + $"  viewport.GetVisibleRect()={vvr} (应与 visibleGame 一致)\n"
            + $"  card.GetGlobalRect()={gr} fallbackAabbFromSize={fb} card.Size={card.Size} defaultSize={NCard.defaultSize}\n"
            + FormatNCardDescendantControlRectsForDiagnostics(card)
            + HierarchyUp(card, "Hierarchy from NCard up:")
            + HierarchyUp(root, "Hierarchy from IntentRewardPreviewRoot up:");
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
        scaleRoot.CustomMinimumSize = NCard.defaultSize;
        marker.AddChildSafely(scaleRoot);

        NCard card = PreloadManager.Cache.GetScene(CardScenePath).Instantiate<NCard>(PackedScene.GenEditState.Disabled);
        card.Visible = false;
        scaleRoot.AddChildSafely(card);
        card.Position = CardPositionInAnchor;
        card.Scale = Vector2.One;
        EnsureIntentPreviewCardHasLayoutSize(card);
        scaleRoot.BindPreviewCard(card);
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

        private NCard? _previewCard;

        public void AttachIntentMarker(Marker2D marker, Vector2 offsetInMarkerSpace)
        {
            _intentMarker = marker;
            _offsetInMarkerSpace = offsetInMarkerSpace;
        }

        public void BindPreviewCard(NCard card)
        {
            _previewCard = card;
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

        /// <summary>按意图点全局坐标更新根节点位置并夹紧；放大流程内显式/延迟调用，不跑每帧 <c>_Process</c>。</summary>
        public void ApplyFollowAndClampFromIntentNow()
        {
            if (!_drawAboveCombatHand || _intentMarker is null || !GodotObject.IsInstanceValid(_intentMarker))
            {
                return;
            }

            Vector2 desired = _intentMarker.ToGlobal(_offsetInMarkerSpace);
            GlobalPosition = _previewCard is not null && GodotObject.IsInstanceValid(_previewCard)
                ? ClampPreviewRootGlobalPositionForCardOnScreen(this, _previewCard, desired)
                : desired;
        }

        private void ApplyGlobalUiDrawOrder()
        {
            ZAsRelative = false;
            ZIndex = EnlargedPreviewZOnGlobalUi;
            ProcessMode = ProcessModeEnum.Always;
        }

        private void ResetLocalDrawOrderUnderMarker()
        {
            ZAsRelative = true;
            ZIndex = 0;
            Position = _offsetInMarkerSpace;
            ProcessMode = ProcessModeEnum.Inherit;
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
