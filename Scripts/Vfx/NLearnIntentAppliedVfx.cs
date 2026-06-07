using System.Threading.Tasks;

using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace ComicChess.TheQueen;

[GlobalClass]
public partial class NLearnIntentAppliedVfx : Control
{
	private const string ScenePath = "res://TheQueen/scenes/vfx/learn_intent_applied_vfx.tscn";

	private MegaLabel _label = null!;

	private string _text = string.Empty;

	private Vector2 _spawnPosition;

	public override void _Ready()
	{
		_label = GetNode<MegaLabel>("Label");
		_label.SetTextAutoSize(_text);
		_label.Modulate = StsColors.green;
		GlobalPosition = _spawnPosition;
		_label.Position = new Vector2(_label.Position.X, _label.Position.Y + NCreature.PowerAppliedVfxPositionOffset.Y);
		TaskHelper.RunSafely(AnimateAndFreeAsync());
	}

	public static NLearnIntentAppliedVfx? Create(Creature amalgamCreature, string text)
	{
		if (TestMode.IsOn || NCombatUi.IsDebugHideTextVfx)
		{
			return null;
		}

		NCreature? creatureNode = NCombatRoom.Instance?.GetCreatureNode(amalgamCreature);
		if (creatureNode == null)
		{
			return null;
		}

		NLearnIntentAppliedVfx vfx = PreloadManager.Cache.GetScene(ScenePath)
			.Instantiate<NLearnIntentAppliedVfx>(PackedScene.GenEditState.Disabled);
		vfx._text = text;
		vfx._spawnPosition = creatureNode.VfxSpawnPosition;
		return vfx;
	}

	private async Task AnimateAndFreeAsync()
	{
		float targetY = _label.Position.Y - 100f;
		Tween textTween = CreateTween().SetParallel();
		textTween.TweenProperty(_label, "position:y", targetY, 1.25)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Cubic);
		textTween.TweenProperty(_label, "modulate:a", 1f, 0.25)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
		textTween.TweenProperty(_label, "modulate:a", 0f, 0.75)
			.SetEase(Tween.EaseType.In)
			.SetTrans(Tween.TransitionType.Expo)
			.From(1f)
			.SetDelay(0.25);
		await textTween.AwaitFinished(this);
		this.QueueFreeSafely();
	}
}
