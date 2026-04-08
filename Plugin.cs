// Made by Naomi Nada B.F.
// ^-^
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using NADA.VFX.Core.Config;
using NADA.VFX.Runtime.Structure;

namespace NADA.VFX
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGuid = "naomi.nada.vfx";
        public const string ModName = "NADA VFX";
        public const string ModVersion = "0.5.9";

        internal static ManualLogSource Log;
        internal static Plugin Instance;

        private static readonly Harmony Harmony = new Harmony(ModGuid);

        // Runtime root names
        internal const string LocalWeaponRootName = "NADA Weapon";
        internal const string EffectsRootName = "Effects";

        // Target/reference prefabs
        internal const string TargetPrefabName = "BWA_FlametalGreatSword";
        internal const string ReferencePrefabName = "SwordDyrnwyn";
        internal const string ReferenceRigPath = "attach/Burny vfx";
        internal const string DemisterPrefabName = "demister_ball";

        // Core effect names
        internal const string InnerFlamesName = "Inner Flames";
        internal const string OuterFlamesName = "Outer Flames";
        internal const string FlareName = "Flare";
        internal const string SparksName = "Sparks";
        internal const string MirageName = "Mirage";
        
        // Orbitals names
        internal const string OrbitalsName = "Orbitals";
        internal const string OrbitalsRigRootName = "Orbitals Rig";
        internal const string OrbitalsMotionRootsName = "Orbitals Motion Roots";
        internal const string OrbitalsPoolsRootName = "Orbitals Pools";

        internal const string OrbitalsOrbsName = "Orbs";
        internal const string OrbitalsOrbsMotionRootName = "Orbs Motion Root";
        internal const string OrbitalsOrbsPoolName = "Orbs Pool";
        internal const int MaxOrbitalsOrbsVisuals = 30;

        internal const string OrbitalsFlamesName = "Flames";
        internal const string OrbitalsFlamesMotionRootName = "Flames Motion Root";
        internal const string OrbitalsFlamesPoolName = "Flames Pool";
        internal const int MaxOrbitalsFlameVisuals = 30;

        internal const string OrbitalsEmbersName = "Embers";
        internal const string OrbitalsEmbersMotionRootName = "Embers Motion Root";
        internal const string OrbitalsEmbersPoolName = "Embers Pool";
        internal const int MaxOrbitalsEmberVisuals = 30;

        // Reference rig transform
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
            Log.LogInfo($"{ModName}: Applied Harmony patches.");

            StartCoroutine(NadaRigCache.CacheReferenceAssetsWhenReady());
        }
    }
}