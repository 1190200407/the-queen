using System;
using System.Collections.Generic;
using BaseLib.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace ComicChess.TheQueen;

/// <summary>
/// 卡面百科：增加「敌怪牌池」筛选按钮，可单独查看 <see cref="EnemyCardPool"/> 中的牌。
/// </summary>
[HarmonyPatch(typeof(NCardLibrary), nameof(NCardLibrary._Ready))]
[HarmonyPriority(500)]
internal static class CardLibraryEnemyPoolFilterPatch
{
    private const string EnemyFilterIconPath = "res://TheQueen/images/charui/monster_card.png";
    private const string FallbackIconPath = "res://TheQueen/images/powers/power.png";

    private static void Postfix(NCardLibrary __instance)
    {
        var poolFilters = (Dictionary<NCardPoolFilter, Func<CardModel, bool>>)AccessTools
            .Field(typeof(NCardLibrary), "_poolFilters")
            .GetValue(__instance)!;

        NCardPoolFilter reference = __instance.GetNode<NCardPoolFilter>("%ColorlessPool");
        NCardPoolFilter enemyFilter = CreatePoolFilterButton(reference);
        reference.AddSibling(enemyFilter, forceReadableName: true);

        poolFilters.Add(enemyFilter, static c => c.Pool is EnemyCardPool);

        var update = AccessTools.MethodDelegate<Action<NCardPoolFilter>>(
            AccessTools.DeclaredMethod(typeof(NCardLibrary), "UpdateCardPoolFilter"),
            __instance);
        enemyFilter.Connect(NCardPoolFilter.SignalName.Toggled, Callable.From(update));

        var lastHoveredField = AccessTools.Field(typeof(NCardLibrary), "_lastHoveredControl");
        enemyFilter.Connect(
            Control.SignalName.FocusEntered,
            Callable.From(() => lastHoveredField.SetValue(__instance, enemyFilter)));

        Callable.From(() => MatchFilterSizeToReference(enemyFilter, reference)).CallDeferred();
    }

    private static void MatchFilterSizeToReference(NCardPoolFilter target, NCardPoolFilter reference)
    {
        target.CustomMinimumSize = reference.CustomMinimumSize;
        target.Size = reference.Size;
        target.PivotOffset = reference.PivotOffset;
        if (target.GetNodeOrNull<TextureRect>("Image") is { } img
            && reference.GetNodeOrNull<TextureRect>("Image") is { } refImg)
        {
            img.CustomMinimumSize = refImg.CustomMinimumSize;
            img.Size = refImg.Size;
            img.Position = refImg.Position;
            img.Scale = refImg.Scale;
            img.PivotOffset = refImg.PivotOffset;
        }

        if (target.GetNodeOrNull<NSelectionReticle>("SelectionReticle") is { } ret
            && reference.GetNodeOrNull<NSelectionReticle>("SelectionReticle") is { } refRet)
        {
            ret.CustomMinimumSize = refRet.CustomMinimumSize;
            ret.Size = refRet.Size;
            ret.PivotOffset = refRet.PivotOffset;
            ret.Position = refRet.Position;
        }
    }

    private static NCardPoolFilter CreatePoolFilterButton(NCardPoolFilter reference)
    {
        Texture2D? tex = ResourceLoader.Exists(EnemyFilterIconPath)
            ? ResourceLoader.Load<Texture2D>(EnemyFilterIconPath)
            : ResourceLoader.Load<Texture2D>(FallbackIconPath);

        NCardPoolFilter filter = reference.Duplicate() as NCardPoolFilter ?? new NCardPoolFilter();
        filter.Name = "FILTER-EnemyCardPool";
        filter.Size = new Vector2(64, 64);
        filter.CustomMinimumSize = new Vector2(64, 64);
        filter.TooltipText = string.Empty;
        filter.Loc = new LocString("card_library", "POOL_MONSTER_TIP");

        TextureRect? image = filter.GetNodeOrNull<TextureRect>("Image");
        if (image == null)
        {
            image = new TextureRect
            {
                Name = "Image",
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                Material = ShaderUtils.GenerateHsv(1f, 1f, 1f),
            };
            filter.AddChild(image);
            image.Owner = filter;
        }

        image.Texture = tex;
        image.Size = new Vector2(56, 56);
        image.Position = new Vector2(4, 4);
        image.Scale = new Vector2(0.9f, 0.9f);
        image.PivotOffset = new Vector2(28, 28);

        TextureRect? shadow = image.GetNodeOrNull<TextureRect>("Shadow");
        if (shadow != null)
        {
            shadow.Texture = tex;
            shadow.Size = new Vector2(56, 56);
            shadow.Position = new Vector2(4, 3);
            shadow.PivotOffset = new Vector2(28, 28);
            shadow.ShowBehindParent = true;
            shadow.Modulate = Colors.Black with { A = 0.25f };
        }

        if (filter.GetNodeOrNull<NSelectionReticle>("SelectionReticle") == null)
        {
            NSelectionReticle reticle = PreloadManager.Cache
                .GetScene(SceneHelper.GetScenePath("ui/selection_reticle"))
                .Instantiate<NSelectionReticle>();
            reticle.Name = "SelectionReticle";
            reticle.UniqueNameInOwner = true;
            filter.AddChild(reticle);
            reticle.Owner = filter;
        }

        return filter;
    }
}
