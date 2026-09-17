using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using STS2RitsuLib.Settings;

namespace ComicChess.TheQueen;

/// <summary>魂灯能力的独立战斗计数器。</summary>
[GlobalClass]
public partial class NQueenSoulLampCounter : Control
{
	private const string SoulLampDarkenedMaterialPath = "res://materials/ui/energy_orb_dark.tres";

	private MegaLabel? _soulLampLabel;
	private Control? _soulLampLayer;
	private readonly List<TextureRect> _soulLampVisualLayers = [];
	private Node2D? _soulLampFire;
	private GpuParticles2D? _soulLampGainParticle;
	private GpuParticles2D? _soulLampConstantParticle;
	private IHoverTip? _soulLampHoverTip;
	private Player? _boundPlayer;
	private int _displayedSoulLampAmount = int.MinValue;
	private bool _soulLampFireDesiredActive;
	private bool _soulLampConstantDesiredActive;
	private bool _soulLampVisualsLit = true;

	public override void _Ready()
	{
		base._Ready();
		Visible = false;

		_soulLampLayer = GetNodeOrNull<Control>("%SoulLampLayer")
			?? GetNodeOrNull<Control>("SoulLampLayer");
		_soulLampLabel = GetNodeOrNull<MegaLabel>("%SoulLampLabel")
			?? GetNodeOrNull<MegaLabel>("SoulLampLayer/SoulLampLabel")
			?? GetNodeOrNull<MegaLabel>("SoulLampLabel");

		if (_soulLampLabel != null)
		{
			_soulLampLabel.MouseFilter = MouseFilterEnum.Ignore;
		}

		if (_soulLampLayer != null)
		{
			_soulLampLayer.MouseFilter = MouseFilterEnum.Stop;
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
			_soulLampLayer.Connect(SignalName.MouseEntered, Callable.From(OnSoulLampHovered));
			_soulLampLayer.Connect(SignalName.MouseExited, Callable.From(OnSoulLampUnhovered));
		}

		ModSettingsBindingWriteEvents.SubscribeValueWrittenWhileNodeAlive(
			this,
			_ => ApplySoulLampVfxPresentation());
		ApplySoulLampVfxPresentation();
		Refresh(_boundPlayer);
	}

	public void Refresh(Player? player)
	{
		_boundPlayer = player;
		if (player == null)
		{
			Visible = false;
			return;
		}

		SoulLampPower? lamp = player.Creature?.GetPower<SoulLampPower>();
		ApplySoulLampDisplay(player, lamp?.DisplayAmount ?? 0);
	}

	private void ApplySoulLampDisplay(Player player, int amount)
	{
		if (_soulLampLabel == null)
		{
			return;
		}

		Visible = true;
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
