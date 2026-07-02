using System.Threading.Tasks;

using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.TestSupport;

namespace ComicChess.TheQueen;

/// <summary>
/// 咏唱链强化：储君精炼音效 → 与 <see cref="CardCmd.Preview"/> 同款的弹出预览 →
/// 在<strong>预览用</strong>的 <see cref="NCard"/> 上挂 <see cref="NCardSmithVfx"/>（火花）→ 停留后启动飞回；方法在飞行动画<strong>开始后</strong>即返回，不等待飞完。
/// </summary>
internal static class MysticChantStrengthenVfx
{
	private const string RegentRefineSfx = "event:/sfx/characters/regent/regent_refine";

	/// <summary>与 <see cref="CardCmd.Preview"/> 默认一致：弹出后展示时长再飞回。</summary>
	private const float PreviewHoldSeconds = 0.6f;

	public static async Task PlayAfterStrengthen(CardModel strengthenedCard, bool fullPresentation = true)
	{
		NCombatRoom? room = NCombatRoom.Instance;
		if (TestMode.IsOn
			|| CombatManager.Instance.IsEnding
			|| !LocalContext.IsMine(strengthenedCard)
			|| strengthenedCard.Pile == null
			|| room == null)
		{
			return;
		}

		PileType pileType = strengthenedCard.Pile.Type;
		NCard.FindOnTable(strengthenedCard)?.UpdateVisuals(pileType, CardPreviewMode.Normal);

		Control? previewContainer = ResolvePreviewContainer(pileType, room);
		if (previewContainer == null)
		{
			return;
		}

		NCard? previewNode = NCard.Create(strengthenedCard);
		if (previewNode == null)
		{
			return;
		}

		previewContainer.AddChildSafely(previewNode);
		previewNode.UpdateVisuals(pileType, CardPreviewMode.Normal);
		previewNode.Scale = Vector2.Zero;

		SfxCmd.Play(RegentRefineSfx);

		Tween? popTween = previewNode.CreateTween();
		if (popTween == null)
		{
			return;
		}
		popTween.TweenProperty(previewNode, "scale", Vector2.One, 0.25f)
			.From(Vector2.Zero)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Cubic);
			
		if (fullPresentation)
			await room.ToSignal(popTween, Tween.SignalName.Finished);

		NCardSmithVfx? smith = NCardSmithVfx.Create(previewNode, playSfx: false);
		NRun.Instance?.GlobalUi.AboveTopBarVfxContainer.AddChildSafely(smith);

		if (fullPresentation)
			await Cmd.CustomScaledWait(PreviewHoldSeconds * 0.5f, PreviewHoldSeconds);

		Node? flyParent = pileType != PileType.Deck
			? room.CombatVfxContainer
			: NRun.Instance?.GlobalUi.TopBar.TrailContainer;
		PileType targetPileType = strengthenedCard.Pile?.Type ?? pileType;
		Vector2 targetPosition = targetPileType.GetTargetPosition(previewNode);
		string trailPath = strengthenedCard.Owner.Character.TrailPath ?? string.Empty;
		NCardFlyVfx? fly = NCardFlyVfx.Create(previewNode, targetPosition, isAddingToPile: false, trailPath);
		if (fly != null && flyParent != null)
		{
			// 与 CardCmd.Preview 的飞回演出一致，但不阻塞出牌逻辑：飞行动画在后台播完即可。
			flyParent.AddChildSafely(fly);
		}
		else
		{
			previewNode.QueueFreeSafely();
		}
	}

	private static Control? ResolvePreviewContainer(PileType pileType, NCombatRoom room)
	{
		if (pileType.IsCombatPile())
		{
			return room.Ui.CardPreviewContainer;
		}

		return NRun.Instance?.GlobalUi.CardPreviewContainer;
	}
}
