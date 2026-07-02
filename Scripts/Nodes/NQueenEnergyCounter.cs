using System.Collections.Generic;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using STS2RitsuLib.Settings;

namespace ComicChess.TheQueen;

/// <summary>
/// 女王专用能量指示器：在能量球旁显示 <see cref="SoulLampPower"/> 层数。
/// 场景根节点挂本脚本；Ritsu 走 <see cref="NEnergyCounter"/> 工厂时会因 <c>source is NEnergyCounter</c> 而保留子类实例。
/// </summary>
[GlobalClass]
public partial class NQueenEnergyCounter : NEnergyCounter
{
	private const string SoulLampDarkenedMaterialPath = "res://materials/ui/energy_orb_dark.tres";

	private static readonly FieldInfo PlayerField =
		typeof(NEnergyCounter).GetField("_player", BindingFlags.Instance | BindingFlags.NonPublic)!;

	private static NQueenEnergyCounter? _activeInstance;

	private MegaLabel? _soulLampLabel;
	private Control? _soulLampLayer;
	private readonly List<TextureRect> _soulLampVisualLayers = [];
	private Node2D? _soulLampFire;
	private GpuParticles2D? _soulLampGainParticle;
	private GpuParticles2D? _soulLampConstantParticle;
	private IHoverTip? _soulLampHoverTip;
	private int _displayedSoulLampAmount = int.MinValue;
	private bool _soulLampFireDesiredActive;
	private bool _soulLampConstantDesiredActive;
	private bool _soulLampVisualsLit = true;

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
			foreach (Node child in _soulLampLayer.GetChildren())
			{
				if (child is TextureRect textureRect)
				{
					_soulLampVisualLayers.Add(textureRect);
				}
			}

			_soulLampFire = _soulLampLayer.GetNodeOrNull<Node2D>("Fire");
			_soulLampGainParticle = _soulLampLayer.GetNodeOrNull<GpuParticles2D>("GainParticle");
			_soulLampConstantParticle = _soulLampLayer.GetNodeOrNull<GpuParticles2D>("ConstantParticle");
			if (_soulLampGainParticle != null)
			{
				_soulLampGainParticle.OneShot = true;
				_soulLampGainParticle.Emitting = false;
			}
			_soulLampHoverTip = HoverTipFactory.FromPower<SoulLampPower>();
			_soulLampLayer.Connect(Control.SignalName.MouseEntered, Callable.From(OnSoulLampHovered));
			_soulLampLayer.Connect(Control.SignalName.MouseExited, Callable.From(OnSoulLampUnhovered));
		}

		ModSettingsBindingWriteEvents.SubscribeValueWrittenWhileNodeAlive(
			this,
			_ => ApplySoulLampVfxPresentation());
		ApplySoulLampVfxPresentation();

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

		int previousAmount = _displayedSoulLampAmount;
		if (previousAmount != int.MinValue && amount > previousAmount)
		{
			PlaySoulLampGainParticle();
		}

		SetSoulLampFireActive(amount > 0);
		ApplySoulLampVisualDim(amount > 0);

		_displayedSoulLampAmount = amount;
		_soulLampLabel.Text = amount.ToString();
		_soulLampLabel.AddThemeColorOverride(
			ThemeConstants.Label.FontColor,
			amount == 0 ? StsColors.red : StsColors.cream);
		_soulLampLabel.AddThemeColorOverride(
			ThemeConstants.Label.FontOutlineColor,
			amount == 0 ? StsColors.unplayableEnergyCostOutline : player.Character.EnergyLabelOutlineColor);
	}

	private void PlaySoulLampGainParticle()
	{
		if (QueenSettingsStore.IsSoulLampIndicatorVfxSimplified() || _soulLampGainParticle == null)
		{
			return;
		}

		_soulLampGainParticle.OneShot = true;
		_soulLampGainParticle.Emitting = false;
		_soulLampGainParticle.Restart();
		_soulLampGainParticle.Emitting = true;
	}

	private void SetSoulLampFireActive(bool active)
	{
		_soulLampFireDesiredActive = active;
		ApplySoulLampFireVisualState();
		SetSoulLampConstantParticleActive(active);
	}

	private void SetSoulLampConstantParticleActive(bool active)
	{
		_soulLampConstantDesiredActive = active;
		ApplySoulLampConstantParticleVisualState();
	}

	private void ApplySoulLampVfxPresentation()
	{
		bool simplified = QueenSettingsStore.IsSoulLampIndicatorVfxSimplified();
		if (_soulLampGainParticle != null)
		{
			_soulLampGainParticle.Visible = !simplified;
			if (simplified)
			{
				_soulLampGainParticle.Emitting = false;
			}
		}

		ApplySoulLampFireVisualState();
		ApplySoulLampConstantParticleVisualState();
	}

	private void ApplySoulLampFireVisualState()
	{
		bool simplified = QueenSettingsStore.IsSoulLampIndicatorVfxSimplified();
		bool active = _soulLampFireDesiredActive && !simplified;
		if (_soulLampFire != null)
		{
			_soulLampFire.Visible = !simplified;
			foreach (Node child in _soulLampFire.GetChildren())
			{
				if (child is CpuParticles2D cpuParticle)
				{
					cpuParticle.Emitting = active;
					if (active)
					{
						cpuParticle.Restart();
					}
				}
			}
		}
	}

	private void ApplySoulLampConstantParticleVisualState()
	{
		if (_soulLampConstantParticle == null)
		{
			return;
		}

		bool simplified = QueenSettingsStore.IsSoulLampIndicatorVfxSimplified();
		bool active = _soulLampConstantDesiredActive && !simplified;
		_soulLampConstantParticle.Visible = !simplified;
		_soulLampConstantParticle.Emitting = active;
		if (active)
		{
			_soulLampConstantParticle.Restart();
		}
	}

	private void ApplySoulLampVisualDim(bool lit)
	{
		if (_soulLampVisualLayers.Count == 0 || _soulLampVisualsLit == lit)
		{
			return;
		}

		_soulLampVisualsLit = lit;
		Material? material = lit ? null : PreloadManager.Cache.GetMaterial(SoulLampDarkenedMaterialPath);
		Color modulate = lit ? Colors.White : Colors.DarkGray;
		foreach (TextureRect layer in _soulLampVisualLayers)
		{
			layer.Material = material;
			layer.Modulate = modulate;
		}
	}

	private void OnSoulLampHovered()
	{
		if (_soulLampHoverTip == null || _soulLampLayer == null)
		{
			return;
		}

		NHoverTipSet.CreateAndShow(_soulLampLayer, _soulLampHoverTip)
			?.SetGlobalPosition(_soulLampLayer.GlobalPosition + new Vector2(-14f, -190f));
	}

	private void OnSoulLampUnhovered()
	{
		if (_soulLampLayer != null)
		{
			NHoverTipSet.Remove(_soulLampLayer);
		}
	}
}
