using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
public partial class NewNQueenVFx : NQueenVfx
{
    public override void _Ready()
    {
        var sprayField = typeof(NQueenVfx).GetField("_sprayParticles", BindingFlags.Instance | BindingFlags.NonPublic);
        if (sprayField != null && sprayField.GetValue(this) == null)
        {
            var particles = GetNodeOrNull<GpuParticles2D>("GPUParticles2D");
            if (particles != null && sprayField.FieldType.IsAssignableFrom(particles.GetType()))
                sprayField.SetValue(this, particles);
        }

        var spineField = typeof(NQueenVfx).GetField("_spineNode", BindingFlags.Instance | BindingFlags.NonPublic);
        var visuals = GetParent()?.GetParent();
        if (spineField != null && spineField.GetValue(this) == null && visuals != null)
        {
            var ft = spineField.FieldType;
            if (ft.IsAssignableFrom(visuals.GetType()))
                spineField.SetValue(this, visuals);
            else
            {
                var ctor = ft.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    binder: null,
                    types: new[] { typeof(Variant) },
                    modifiers: null);
                if (ctor != null)
                {
                    Variant v = visuals;
                    spineField.SetValue(this, ctor.Invoke(new object[] { v }));
                }
            }
        }

        base._Ready();
    }
}
