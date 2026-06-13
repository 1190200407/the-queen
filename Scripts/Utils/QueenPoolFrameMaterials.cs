using Godot;
using STS2RitsuLib.Utils;

namespace ComicChess.TheQueen;

internal static class QueenPoolFrameMaterials
{
    internal static ShaderMaterial FromRgb(byte r, byte g, byte b)
    {
        float rf = r / 255f;
        float gf = g / 255f;
        float bf = b / 255f;
        float max = Math.Max(rf, Math.Max(gf, bf));
        float min = Math.Min(rf, Math.Min(gf, bf));
        float delta = max - min;

        float h = 0f;
        if (delta != 0f)
        {
            if (Mathf.IsEqualApprox(max, rf))
            {
                h = (gf - bf) / delta + (gf < bf ? 6f : 0f);
            }
            else if (Mathf.IsEqualApprox(max, gf))
            {
                h = (bf - rf) / delta + 2f;
            }
            else
            {
                h = (rf - gf) / delta + 4f;
            }

            h /= 6f;
        }

        float s = max == 0f ? 0f : delta / max;
        return MaterialUtils.CreateHsvShaderMaterial(h, s, max);
    }
}
