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
        public RigGroups Groups { get; }
        public RigGroups EffectsGroups { get; private set; }

        public NadaWeaponRigContext(
            GameObject root,
            global::ItemDrop.ItemData itemData,
            Transform weaponVisualRoot,
            RigGroups groups)
        {
            Root = root;
            RootTransform = root != null ? root.transform : null;
            ItemData = itemData;
            WeaponVisualRoot = weaponVisualRoot;
            Groups = groups;
        }

        public bool IsValid =>
            Root != null &&
            RootTransform != null &&
            WeaponVisualRoot != null;   

        public NadaOrbsTargets OrbTargets { get; private set; }
        
        public void SetOrbTargets(NadaOrbsTargets orbTargets)
        {
            if (orbTargets != null && orbTargets.IsValid)
                OrbTargets = orbTargets;
        }
        
        public void SetEffectsGroups(RigGroups groups)
        {
            if (groups != null)
                EffectsGroups = groups;
        }
    }
}