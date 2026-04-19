using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ComicChess.TheQueen;

/// <summary>
/// 在敌人 <see cref="NCreatureVisuals.IntentPosition"/> 上挂奖励/预览用 <see cref="NCard"/>。
/// 四函数：显示全部、隐藏全部、放大某一怪、缩小某一怪；供捕获及后续其它预览复用。
/// </summary>
internal static class EnemyIntentRewardCardPreview
{
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

        AnimateSlotTo(slot, LargeScale);
    }

    public static void ShrinkRewardCardForCreature(Creature creature)
    {
        if (!Slots.TryGetValue(creature, out Slot? slot) || !GodotObject.IsInstanceValid(slot.Card)
            || !GodotObject.IsInstanceValid(slot.ScaleRoot))
        {
            return;
        }

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

        Control scaleRoot = new Control
        {
            Name = "IntentRewardCardPreviewRoot",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Position = SceneRootLocalOffset,
        };
        marker.AddChildSafely(scaleRoot);

        NCard card = PreloadManager.Cache.GetScene(CardScenePath).Instantiate<NCard>(PackedScene.GenEditState.Disabled);
        card.Visible = false;
        scaleRoot.AddChildSafely(card);
        card.Position = CardPositionInAnchor;
        card.Scale = Vector2.One;
        scaleRoot.Visible = false;
        return new Slot(scaleRoot, card);
    }

    private sealed class Slot(Control scaleRoot, NCard card)
    {
        public readonly Control ScaleRoot = scaleRoot;

        public readonly NCard Card = card;

        public Tween? ScaleTween;
    }
}
