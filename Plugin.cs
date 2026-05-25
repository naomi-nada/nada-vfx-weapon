// Made by Naomi Nada B.F.
// ^-^
using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Core.State;
using NADA.VFX.Weapon.Core.Visuals;
using NADA.VFX.Weapon.Modules.Motion;
using NADA.VFX.Weapon.Runtime.Structure;
using NADA.VFX.Weapon.Weapons.Runtime;
using UnityEngine;

namespace NADA.VFX.Weapon
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGuid = "naomi.nada.vfx.weapon";
        public const string ModName = "NADA VFX: Weapon";
        public const string ModVersion = "0.8.2";

        internal static ManualLogSource Log;
        internal static Plugin Instance;

        private static readonly Harmony Harmony = new Harmony(ModGuid);

        internal static ConfigEntry<bool> DebugLoggingEnabled;
        internal static ConfigEntry<bool> EquipLoggingEnabled;

        // Runtime root names
        internal const string LocalWeaponRootName = "NADA Weapon";
        internal const string EffectsRootName = "Effects";

        // Target/reference prefabs
        internal const string TargetPrefabName = "BWA_FlametalGreatSword";
        internal const string ReferencePrefabName = "SwordDyrnwyn";
        internal const string ReferenceRigPath = "attach/Burny vfx";
        internal const string DemisterPrefabName = "demister_ball";
        internal const string SparksReferencePrefabName = "AtgeirHimminAfl";
        internal const string SparksReferencePath = "attach/equiped/Sparcs";
        internal const string AuraReferenceMaterialName = "glow_pulse_purple";
        internal const string StrandsReferencePrefabName = "vfx_Potion_health_medium";
        internal const string StrandsReferencePath = "trails";
        internal const string CoresReferencePrefabName = "SurtlingCore";
        internal const string CoresReferencePath = "attach/core";

        // Core effect names
        internal const string InnerFlamesName = "Inner Flames";
        internal const string OuterFlamesName = "Outer Flames";
        internal const string StrandsName = "Strands";
        internal const string SparksName = "Sparks";
        internal const string FlareName = "Flare";
        internal const string AuraName = "Aura";

        // Orbitals names
        internal const string OrbitalsName = "Orbitals";
        internal const string OrbitalsRigRootName = "Orbitals Rig";
        internal const string OrbitalsMotionRootsName = "Orbitals Motion Roots";
        internal const string OrbitalsPoolsRootName = "Orbitals Pools";

        internal const string OrbitalsOrbsName = "Orbs";
        internal const string OrbitalsOrbsMotionRootName = "Orbs Motion Root";
        internal const string OrbitalsOrbsPoolName = "Orbs Pool";
        internal const int MaxOrbitalsOrbsVisuals = 40;
        
        internal const string OrbitalsCoresName = "Cores";
        internal const string OrbitalsCoresMotionRootName = "Cores Motion Root";
        internal const string OrbitalsCoresPoolName = "Cores Pool";
        internal const int MaxOrbitalsCoreVisuals = 40;

        internal const string OrbitalsFlamesName = "Flames";
        internal const string OrbitalsFlamesMotionRootName = "Flames Motion Root";
        internal const string OrbitalsFlamesPoolName = "Flames Pool";
        internal const int MaxOrbitalsFlameVisuals = 40;

        internal const string OrbitalsEmbersName = "Embers";
        internal const string OrbitalsEmbersMotionRootName = "Embers Motion Root";
        internal const string OrbitalsEmbersPoolName = "Embers Pool";
        internal const int MaxOrbitalsEmberVisuals = 40;

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

        private void Update()
        {
            if (PluginConfig.AttachHotkey.Value.IsDown())
                NadaEquippedRigActions.TryAttachToEquipped();

            if (PluginConfig.BindHotkey.Value.IsDown())
                NadaEquippedRigActions.TryBindEquipped();

            NadaRigVisibility.TickCharacterSelectionPreview();
        }

        internal void TryUnbindEquipped()
        {
            NadaEquippedRigActions.TryUnbindEquipped();
        }

        internal void RefreshExistingEquippedRigsOnly()
        {
            NadaEquippedRigActions.RefreshExistingEquippedRigsOnly();
        }

        internal void RefreshExistingUnboundEquippedRigsOnly()
        {
            NadaEquippedRigActions.RefreshExistingUnboundEquippedRigsOnly();
        }
        
        internal void ResetOrbitalsStartPoints()
        {
            foreach (NadaOrbitalsMotion motion in FindObjectsOfType<NadaOrbitalsMotion>(true))
            {
                if (motion == null)
                    continue;

                motion.ResetOrbitStartPoint();
            }
        }

        internal void RefreshDroppedItemVisibility()
        {
            NadaRigVisibility.RefreshDroppedItemVisibility();
        }

        internal void SaveCurrentStyleFromManager(string styleName)
        {
            if (string.IsNullOrWhiteSpace(styleName))
            {
                Log.LogInfo($"{ModName}: [Style] no style name entered.");
                return;
            }

            VfxState state = VfxStateIO.FromConfig();

            if (!VfxStyleStore.Save(styleName, state))
            {
                Log.LogInfo($"{ModName}: [Style] failed to save style '{styleName}'.");
                return;
            }

            PluginConfig.StyleName.Value = string.Empty;
            Config.Save();

            Log.LogInfo($"{ModName}: [Style] saved '{styleName.Trim()}'.");
        }

        internal void LoadStyleIntoManager(string styleName)
        {
            styleName = string.IsNullOrWhiteSpace(styleName)
                ? "Default"
                : styleName.Trim();

            if (!VfxStyleStore.TryGet(styleName, out VfxState state))
            {
                Log.LogInfo($"{ModName}: [Style] could not find style '{styleName}'.");
                return;
            }

            VfxStateIO.ApplyToConfig(state);

            if (PluginConfig.LoadStyle != null &&
                !string.Equals(
                    PluginConfig.LoadStyle.Value,
                    styleName,
                    StringComparison.OrdinalIgnoreCase))
            {
                PluginConfig.LoadStyle.Value = styleName;
            }

            Config.Save();

            RefreshExistingEquippedRigsOnly();

            Log.LogInfo($"{ModName}: [Style] loaded '{styleName}'.");
        }
    }
}