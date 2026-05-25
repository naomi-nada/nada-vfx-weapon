namespace NADA.VFX.Weapon.Runtime.Binding
{
    // Implemented by components that need per item VFX state.
    // The rig system injects ItemData after instantiation.
    internal interface INadaItemDataReceiver
    {
        void SetItemData(global::ItemDrop.ItemData itemData);
    }
}