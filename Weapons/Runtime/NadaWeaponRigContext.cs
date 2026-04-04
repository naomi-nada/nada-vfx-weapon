using NADA.VFX.Runtime.Binding;
using UnityEngine;

namespace NADA.VFX.Weapons.Runtime
{
    internal sealed class NadaWeaponRigContext
    {
        public GameObject Root { get; }
        public Transform RootTransform { get; }
        public global::ItemDrop.ItemData ItemData { get; }
        public Transform WeaponVisualRoot { get; }

        public NadaWeaponRigContext(
            GameObject root,
            global::ItemDrop.ItemData itemData,
            Transform weaponVisualRoot)
        {
            Root = root;
            RootTransform = root != null ? root.transform : null;
            ItemData = itemData;
            WeaponVisualRoot = weaponVisualRoot;
        }

        public bool IsValid =>
            Root != null &&
            RootTransform != null &&
            WeaponVisualRoot != null;   
    }
}