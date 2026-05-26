using System.Reflection;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace ComicChess.TheQueen;

/// <summary>
/// 女王专用能量指示器：在能量球旁显示 <see cref="SoulLampPower"/> 层数。
/// 场景根节点挂本脚本；Ritsu 走 <see cref="NEnergyCounter"/> 工厂时会因 <c>source is NEnergyCounter</c> 而保留子类实例。
/// </summary>
[GlobalClass]
public partial class NQueenEnergyCounter : NEnergyCounter
{
	private static readonly FieldInfo PlayerField =
		typeof(NEnergyCounter).GetField("_player", BindingFlags.Instance | BindingFlags.NonPublic)!;

	private static NQueenEnergyCounter? _activeInstance;

	private MegaLabel? _soulLampLabel;
	private Control? _soulLampLayer;
	private IHoverTip? _soulLampHoverTip;
	private int _displayedSoulLampAmount = int.MinValue;

	/// <summary>本地战斗 UI 中当前活跃的女王能量指示器（战斗结束时会清空）。</summary>
	internal static NQueenEnergyCounter? ActiveInstance => _activeInstance;

	public override void _Ready()
	{
		base._Ready();
		_soulLampLayer = GetNodeOrNull<Control>("%SoulLampLayer")
			?? GetNodeOrNull<Control>("SoulLampLayer");
		_soulLampLabel = GetNodeOrNull<MegaLabel>("%SoulLampLabel")
			?? GetNodeOrNull<MegaLabel>("SoulLampLayer/SoulLampLabel")
			?? GetNodeOrNull<MegaLabel>("SoulLampLabel");

		if (_soulLampLabel != null)
		{
			_soulLampLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
		}

		if (_soulLampLayer != null)
		{
			_soulLampLayer.MouseFilter = Control.MouseFilterEnum.Stop;
			_soulLampHoverTip = HoverTipFactory.FromPower<SoulLampPower>();
			_soulLampLayer.Connect(Control.SignalName.MouseEntered, Callable.From(OnSoulLampHovered));
			_soulLampLayer.Connect(Control.SignalName.MouseExited, Callable.From(OnSoulLampUnhovered));
		}

		RefreshSoulLampFromOwner();
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		_activeInstance = this;
		CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChangedForSoulLamp;
	}

	public override void _ExitTree()
	{
		CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChangedForSoulLamp;
		if (_activeInstance == this)
		{
			_activeInstance = null;
		}

		base._ExitTree();
	}

	/// <summary>由 <see cref="SoulLampPower"/> 等在魂灯层数变化时调用。</summary>
	internal static void TryRefresh(Player player)
	{
		if (player.Character is not QueenCharacter)
		{
			return;
		}

		if (_activeInstance?.OwnerPlayer != player)
		{
			return;
		}

		_activeInstance.RefreshSoulLampFromOwner();
	}

	private Player? OwnerPlayer => PlayerField.GetValue(this) as Player;

	private void OnCombatStateChangedForSoulLamp(CombatState _)
	{
		RefreshSoulLampFromOwner();
	}

	private void RefreshSoulLampFromOwner()
	{
		Player? player = OwnerPlayer;
		if (player == null)
		{
			return;
		}

		SoulLampPower? lamp = player.Creature?.GetPower<SoulLampPower>();
		int amount = lamp?.DisplayAmount ?? 0;
		ApplySoulLampDisplay(player, amount);
	}

	private void ApplySoulLampDisplay(Player player, int amount)
	{
		if (_soulLampLabel == null)
		{
			return;
		}

		if (_soulLampLayer != null)
		{
			_soulLampLayer.Visible = true;
		}

		_soulLampLabel.Visible = true;
		if (_displayedSoulLampAmount == amount)
		{
			return;
		}

		_displayedSoulLampAmount = amount;
		_soulLampLabel.Text = amount.ToString();
		_soulLampLabel.AddThemeColorOverride(
			ThemeConstants.Label.FontColor,
			amount == 0 ? StsColors.red : StsColors.cream);
		_soulLampLabel.AddThemeColorOverride(
			ThemeConstants.Label.FontOutlineColor,
			amount == 0 ? StsColors.unplayableEnergyCostOutline : player.Character.EnergyLabelOutlineColor);
	}

	private void OnSoulLampHovered()
	{
		if (_soulLampHoverTip == null || _soulLampLayer == null)
		{
			return;
		}

		NHoverTipSet.CreateAndShow(_soulLampLayer, _soulLampHoverTip)
			?.SetGlobalPosition(_soulLampLayer.GlobalPosition + new Vector2(-34f, -150f));
	}

	private void OnSoulLampUnhovered()
	{
		if (_soulLampLayer != null)
		{
			NHoverTipSet.Remove(_soulLampLayer);
		}
	}
}
