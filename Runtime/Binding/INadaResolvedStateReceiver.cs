using NADA.VFX.Weapon.Core.State;

namespace NADA.VFX.Weapon.Runtime.Binding
{
    // Implemented by components that need VFX state without having local ItemData.
    // Used for remote multiplayer weapons where the state comes from the network.
    internal interface INadaResolvedStateReceiver
    {
        void SetResolvedState(VfxState state);
    }
}