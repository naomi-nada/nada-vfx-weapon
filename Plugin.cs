// Made by Naomi Nada B.F.
// ^-^

// File: Plugin.cs
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using NADA.VFX.Core.Config;
using NADA.VFX.Runtime.Binding;

namespace NADA.VFX
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGuid = "naomi.nada.vfx";
        public const string ModName = "NADA VFX";
        public const string ModVersion = "0.5.5";

        internal static ManualLogSource Log;
        internal static Plugin Instance;

        private static readonly Harmony Harmony = new Harmony(ModGuid);

        internal const string LocalRootName = "NADA VFX Local";
        internal const string WorldRootName = "NADA VFX World";

        internal const string LocalWeaponRootName = "Nada Weapon (L)";
        internal const string WorldWeaponRootName = "Nada Weapon (W)";

        internal const string EffectsRootName = "Effects";
        internal const string ObjectsRootName = "Objects";

        internal const string TargetPrefabName = "BWA_FlametalGreatSword";
        internal const string ReferencePrefabName = "SwordDyrnwyn";
        internal const string ReferenceRigPath = "attach/Burny vfx";

        internal const string FlareName = "Flare";
        internal const string MirageName = "Mirage";
        internal const string SparksName = "Sparks";

        internal const string InnerFlamesName = "Inner Flames";
        internal const string OuterFlamesName = "Outer Flames";

        internal const string DemisterPrefabName = "demister_ball";
        internal const string OrbitalsOrbsName = "Orbs";

        internal const string OrbitalsName = "Orbitals";

        internal const string OrbitalsRigRootName = "Orbitals Rig";
        internal const string OrbitalsAnchorsRootName = "Orbitals Anchors";
        internal const string OrbitalsPoolsRootName = "Orbitals Pools";

        internal const string OrbitalsFlamesName = "Flames";
        internal const string OrbitalsFlamesLeadAnchorName = "Flames Lead Anchor";
        internal const string OrbitalsFlamesPoolName = "Flames Pool";
        internal const int MaxOrbitalsFlameVisuals = 20;

        internal const string OrbitalsEmbersName = "Embers";
        internal const string OrbitalsEmbersLeadAnchorName = "Embers Lead Anchor";
        internal const string OrbitalsEmbersPoolName = "Embers Pool";
        internal const int MaxOrbitalsEmberVisuals = 20;

        internal static readonly Vector3 RigLocalPosition = new Vector3(0.0f, 1.1f, 0.0f);
        internal static readonly Vector3 RigLocalEulerAngles = new Vector3(90f, 0f, 0f);
        internal static readonly Vector3 RigLocalScale = new Vector3(1.25f, 1.25f, 1.25f);

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            PluginConfig.Bind(Config);

            Log.LogInfo($"{ModName} loaded! Version {ModVersion}");

            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo($"{ModName}: Patched ZNetScene.Awake + VisEquipment.UpdateEquipmentVisuals + ItemDrop.Awake.");

            StartCoroutine(NadaRigCache.CacheReferenceAssetsWhenReady());
        }
    }
}