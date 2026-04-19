using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace ComicChess.TheQueen;

/// <summary>
/// 固定三格意图槽：悬停/聚焦对齐 <see cref="MegaCrit.Sts2.Core.Nodes.Orbs.NOrb"/>（<see cref="NHoverTipSet"/> + <see cref="NSelectionReticle"/>）。
/// </summary>
[GlobalClass]
public partial class NIntentSlot : NClickableControl
{
	[Export]
	public int SlotIndex { get; set; }

	private Control _bounds = null!;
	private Control _labelContainer = null!;
	private NSelectionReticle _selectionReticle = null!;

	private IntentSlotManager? _intentLayerManager;
	private Node2D? _torchSlotFollowSource;
	private bool _isFocusing = false;

	private static HoverTip EmptyIntentSlotHoverTip => new(
		new LocString("monsters", "FRIENDLY_AMALGAM.intent_slot_empty.title"),
		new LocString("monsters", "FRIENDLY_AMALGAM.intent_slot_empty.description"));

	public override void _Ready()
	{
		ConnectSignals();
		_bounds = GetNode<Control>("Bounds");
		_labelContainer = GetNode<Control>("%LabelContainer");
		_selectionReticle = GetNode<NSelectionReticle>("%SelectionReticle");
		_labelContainer.Visible = false;
		_isFocusing = false;
	}

	/// <summary>挂到 <paramref name="manager"/> 下，每帧对齐 <paramref name="torchSlot"/> 世界原点；世界旋转与缩放保持单位。</summary>
	public void MountFollowingTorchAsManagerChild(IntentSlotManager manager, Node2D torchSlot)
	{
		_intentLayerManager = manager;
		_torchSlotFollowSource = torchSlot;
		manager.AddChild(this);
		SetAnchorsPreset(Control.LayoutPreset.TopLeft);
		SetProcess(true);
	}

	public override void _Process(double delta)
	{
		if (!_isFocusing &&
			_intentLayerManager != null
			&& GodotObject.IsInstanceValid(_intentLayerManager)
			&& _torchSlotFollowSource != null
			&& GodotObject.IsInstanceValid(_torchSlotFollowSource))
		{
			SyncLocalUnderManagerForTorchPositionUpright();
		}

		base._Process(delta);
	}

	private void SyncLocalUnderManagerForTorchPositionUpright()
	{
		Transform2D torchG = _torchSlotFollowSource!.GlobalTransform;
		Transform2D wantWorld = new(0f, torchG.Origin);
		Transform2D layerG = _intentLayerManager!.GetGlobalTransform();
		Transform2D local = layerG.AffineInverse() * wantWorld;
		Position = local.Origin;
		Rotation = local.Rotation;
		Scale = local.Scale;
	}

	private NCreature? FindCreatureNode()
	{
		for (Node? n = GetParent(); n != null; n = n.GetParent())
		{
			if (n is NCreature creatureNode)
			{
				return creatureNode;
			}
		}

		return null;
	}

	protected override void OnFocus()
	{
		_isFocusing = true;
		NCreature? creatureNode = FindCreatureNode();
		IEnumerable<IHoverTip> hoverTips;
		if (creatureNode?.Entity?.Monster is FriendlyAmalgam amalgam
			&& amalgam.TryGetTorchSlotHoverTip(creatureNode.Entity, SlotIndex, out HoverTip tip))
		{
			hoverTips = new List<IHoverTip> { tip };
		}
		else
		{
			hoverTips = new List<IHoverTip> { EmptyIntentSlotHoverTip };
		}

		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(_bounds, hoverTips, HoverTip.GetHoverTipAlignment(_bounds));
		nHoverTipSet.SetFollowOwner();
		base.Modulate = Colors.White;
		_selectionReticle.OnSelect();
	}

	protected override void OnUnfocus()
	{
		_isFocusing = false;
		_labelContainer.Visible = false;
		NHoverTipSet.Remove(_bounds);
		_selectionReticle.OnDeselect();
	}
}
