using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace ComicChess.TheQueen;

/// <summary>火炬意图槽容器：挂在 <see cref="NCreature"/> 最后一个子节点，槽叠在 <c>%Hitbox</c> 之上。</summary>
[GlobalClass]
public partial class IntentSlotManager : Control
{
	public const string MountedNodeName = "TheQueenTorchIntentSlots";

	private const string IntentSlotScenePath = "res://TheQueen/scenes/ui/intent_slot.tscn";
	private const string ManagerScenePath = "res://TheQueen/scenes/ui/intent_slot_manager.tscn";

	private static readonly string[] TorchSlotPaths = { "Visuals/torch1Slot", "Visuals/torch2Slot", "Visuals/torch3Slot" };

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Ignore;
	}

	public void SetupTorchSlotsFromAmalgamVisualRoot(Node2D amalgamVisualRoot)
	{
		PackedScene? packed = GD.Load<PackedScene>(IntentSlotScenePath);
		if (packed == null)
		{
			Log.Warn($"[IntentSlotManager] missing PackedScene: {IntentSlotScenePath}");
			return;
		}

		for (int i = 0; i < 3; i++)
		{
			Node? torchNode = amalgamVisualRoot.GetNodeOrNull(TorchSlotPaths[i]);
			if (torchNode is not Node2D follow)
			{
				Log.Warn($"[IntentSlotManager] missing torch path: {TorchSlotPaths[i]}");
				continue;
			}

			Node inst = packed.Instantiate();
			if (inst is not NIntentSlot slot)
			{
				inst.QueueFree();
				Log.Warn("[IntentSlotManager] intent_slot root is not NIntentSlot.");
				continue;
			}

			slot.SlotIndex = i;
			slot.MountFollowingTorchAsManagerChild(this, follow);
		}
	}

	public static void MountOnCreature(NCreature creature, Node2D amalgamVisualRoot)
	{
		if (creature.GetNodeOrNull(MountedNodeName) != null)
		{
			return;
		}

		IntentSlotManager manager;
		if (ResourceLoader.Exists(ManagerScenePath))
		{
			manager = ResourceLoader.Load<PackedScene>(ManagerScenePath).Instantiate<IntentSlotManager>();
		}
		else
		{
			manager = new IntentSlotManager();
			Log.Warn($"[IntentSlotManager] scene missing, using new(): {ManagerScenePath}");
		}

		manager.Name = MountedNodeName;
		creature.AddChild(manager);
		creature.MoveChild(manager, -1);
		manager.SetupTorchSlotsFromAmalgamVisualRoot(amalgamVisualRoot);
	}
}
