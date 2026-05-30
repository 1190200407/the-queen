using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Random;

namespace ComicChess.TheQueen;

/// <summary>
/// 女王篝火角色：底图 + 遮罩光效按当前幕染色，眼睛火焰同步背景火堆色调。
/// </summary>
[GlobalClass]
public partial class NQueenRestSiteCharacter : NRestSiteCharacter
{
	private const string LightMaskPath = "res://TheQueen/images/charui/queen_rest_site_light_mask.png";
	private const float BodyLightAlpha = 0.5f;

	private static readonly StringName TimeOffsetParam = "TimeOffset";
	private static readonly StringName NoiseScroll1Param = "Noise_Scroll_1";
	private static readonly StringName NoiseScroll2Param = "Noise_Scroll_2";
	private static readonly StringName ColorParameter = "ColorParameter";

	private Sprite2D? _bodyFireLight;
	private readonly List<CanvasItem> _flameGlowNodes = [];
	private readonly List<ColorRect> _eyeFireNodes = [];
	private readonly List<ShaderMaterial> _eyeFireMaterials = [];

	public override void _Ready()
	{
		base._Ready();

		_bodyFireLight = GetNode<Sprite2D>("%BodyFireLight");
		CollectFlameGlowNodes(GetNode("%CharacterVisual"));
		DuplicateEyeFireMaterials();
		RefreshBodyFireLightMask();
		ApplyActLighting();
	}

	public void HideQueenFlameGlow()
	{
		foreach (CanvasItem node in _flameGlowNodes)
		{
			node.Visible = false;
		}
	}

	private void ApplyActLighting()
	{
		if (Player?.RunState == null)
		{
			return;
		}

		RestSiteFirePalette palette = GetPalette(Player.RunState.CurrentActIndex);
		ApplyBodyFireLight(palette.BodyTint);
		ApplyEyeFireColors(palette);
		RandomizeEyeFireMaterials();
	}

	private void RefreshBodyFireLightMask()
	{
		if (_bodyFireLight == null)
		{
			return;
		}

		// 强制重载 PNG，避免 Godot / mod 复制后仍显示旧 ctex。
		Texture2D? mask = ResourceLoader.Load<Texture2D>(LightMaskPath, null, ResourceLoader.CacheMode.Ignore);
		if (mask != null)
		{
			_bodyFireLight.Texture = mask;
		}
	}

	private void CollectFlameGlowNodes(Node root)
	{
		foreach (Node node in root.GetChildren())
		{
			if (node is Sprite2D bodyLight && node.Name == "BodyFireLight")
			{
				_flameGlowNodes.Add(bodyLight);
				continue;
			}

			if (node is ColorRect colorRect && colorRect.Material is ShaderMaterial)
			{
				_flameGlowNodes.Add(colorRect);
				_eyeFireNodes.Add(colorRect);
				continue;
			}

			if (node.GetChildCount() > 0)
			{
				CollectFlameGlowNodes(node);
			}
		}
	}

	private void DuplicateEyeFireMaterials()
	{
		_eyeFireMaterials.Clear();
		foreach (ColorRect colorRect in _eyeFireNodes)
		{
			if (colorRect.Material is not ShaderMaterial material)
			{
				continue;
			}

			var duplicate = (ShaderMaterial)material.Duplicate(true);
			colorRect.Material = duplicate;
			_eyeFireMaterials.Add(duplicate);
		}
	}

	private void ApplyBodyFireLight(Color bodyTint)
	{
		if (_bodyFireLight == null)
		{
			return;
		}

		_bodyFireLight.SelfModulate = new Color(bodyTint.R, bodyTint.G, bodyTint.B, BodyLightAlpha);
	}

	private void ApplyEyeFireColors(RestSiteFirePalette palette)
	{
		if (_eyeFireMaterials.Count == 0)
		{
			return;
		}

		ApplyEyeMaterialColor(_eyeFireNodes[0], _eyeFireMaterials[0], palette.Eye1Outer);
		if (_eyeFireMaterials.Count > 1)
		{
			ApplyEyeMaterialColor(_eyeFireNodes[1], _eyeFireMaterials[1], palette.Eye1Inner);
		}

		if (_eyeFireMaterials.Count > 2)
		{
			ApplyEyeMaterialColor(_eyeFireNodes[2], _eyeFireMaterials[2], palette.Eye2Outer);
		}

		if (_eyeFireMaterials.Count > 3)
		{
			ApplyEyeMaterialColor(_eyeFireNodes[3], _eyeFireMaterials[3], palette.Eye2Inner);
		}
	}

	private static void ApplyEyeMaterialColor(ColorRect colorRect, ShaderMaterial material, Color color)
	{
		material.SetShaderParameter(ColorParameter, color);
		colorRect.Color = color;
	}

	private void RandomizeEyeFireMaterials()
	{
		foreach (ShaderMaterial material in _eyeFireMaterials)
		{
			material.SetShaderParameter(TimeOffsetParam, Rng.Chaotic.NextFloat());
			Vector2 scroll1 = material.GetShaderParameter(NoiseScroll1Param).AsVector2();
			Vector2 scroll2 = material.GetShaderParameter(NoiseScroll2Param).AsVector2();
			material.SetShaderParameter(
				NoiseScroll1Param,
				scroll1 + new Vector2(Rng.Chaotic.NextFloat(-0.1f, 0.1f), Rng.Chaotic.NextFloat(-0.1f, 0.1f)));
			material.SetShaderParameter(
				NoiseScroll2Param,
				scroll2 + new Vector2(Rng.Chaotic.NextFloat(-0.1f, 0.1f), Rng.Chaotic.NextFloat(-0.1f, 0.1f)));
		}
	}

	private static RestSiteFirePalette GetPalette(int actIndex) =>
		actIndex switch
		{
			1 => HivePalette,
			2 => GloryPalette,
			_ => OvergrowthPalette,
		};

	private static readonly RestSiteFirePalette OvergrowthPalette = new(
		BodyTint: new Color(0.19215687f, 1f, 0.24313726f),
		Eye1Outer: new Color(0.6479158f, 1f, 0.57077026f),
		Eye1Inner: new Color(0.7921399f, 0.9768372f, 0.83003885f),
		Eye2Outer: new Color(0.72156864f, 1f, 0f),
		Eye2Inner: new Color(0.7921569f, 0.9764706f, 0.83137256f));

	private static readonly RestSiteFirePalette HivePalette = new(
		BodyTint: new Color(0.8901961f, 0.8627451f, 0f),
		Eye1Outer: new Color(1f, 0.536f, 0.04f),
		Eye1Inner: new Color(1f, 0.72f, 0.2f),
		Eye2Outer: new Color(1f, 0.45f, 0.02f),
		Eye2Inner: new Color(1f, 0.65f, 0.15f));

	private static readonly RestSiteFirePalette GloryPalette = new(
		BodyTint: new Color(0.19215687f, 1f, 0.93333334f),
		Eye1Outer: new Color(0f, 0.666667f, 1f),
		Eye1Inner: new Color(0.45f, 0.85f, 1f),
		Eye2Outer: new Color(0.1f, 0.55f, 0.95f),
		Eye2Inner: new Color(0.35f, 0.78f, 1f));

	private readonly record struct RestSiteFirePalette(
		Color BodyTint,
		Color Eye1Outer,
		Color Eye1Inner,
		Color Eye2Outer,
		Color Eye2Inner);
}
