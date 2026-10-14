namespace NADA.VFX.Weapon.Core.State.Blocks.Effects
{
    /// <summary>
    /// Weapon mesh appearance modifiers for one authored effect block.
    /// This is pure state; material ownership belongs to the runtime.
    /// </summary>
    internal sealed class WeaponAppearanceVfxSettings : VfxEffectSettings
    {
        // 1 = original weapon mesh; 0 = fully transparent weapon mesh.
        // Future appearance modifiers can be added without changing block identity.
        internal float Visibility { get; set; } = 1f;
    }
}