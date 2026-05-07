// Made by Naomi Nada B.F.
// ^-^
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using NADA.VFX.Core.Config;
using NADA.VFX.Core.State;
using NADA.VFX.Core.Styles;
using NADA.VFX.Runtime.Structure;
using NADA.VFX.Weapons.Runtime;
using NADA.VFX.Weapons.Targets;
using UnityEngine.SceneManagement;

namespace NADA.VFX
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGuid = "naomi.nada.vfx";
        public const string ModName = "NADA VFX";
        public const string ModVersion = "0.7.1";

        internal static ManualLogSource Log;
        internal static Plugin Instance;

        private static readonly Harmony Harmony = new Harmony(ModGuid);
        
        internal static ConfigEntry<bool> DebugLoggingEnabled;
        internal static ConfigEntry<bool> EquipLoggingEnabled;
        
        private float _nextCharacterSelectionProbeTime;

        // Runtime root names
        internal const string LocalWeaponRootName = "NADA Weapon";
        internal const string EffectsRootName = "Effects";

        // Target/reference prefabs
        internal const string TargetPrefabName = "BWA_FlametalGreatSword";
        internal const string ReferencePrefabName = "SwordDyrnwyn";
        internal const string ReferenceRigPath = "attach/Burny vfx";
        internal const string DemisterPrefabName = "demister_ball";
        internal const string SparksName = "Sparks";
        internal const string SparksReferencePrefabName = "AtgeirHimminAfl";
        internal const string SparksReferencePath = "attach/equiped/Sparcs";
        internal const string AuraName = "Aura";
        internal const string AuraReferencePrefabName = "fi_vil_combs_props_bone_skull";
        internal const string AuraReferenceMaterialName = "glow_pulse_purple";

        // Core effect names
        internal const string InnerFlamesName = "Inner Flames";
        internal const string OuterFlamesName = "Outer Flames";
        internal const string FlareName = "Flare";

        // Orbitals names
        internal const string OrbitalsName = "Orbitals";
        internal const string OrbitalsRigRootName = "Orbitals Rig";
        internal const string OrbitalsMotionRootsName = "Orbitals Motion Roots";
        internal const string OrbitalsPoolsRootName = "Orbitals Pools";

        internal const string OrbitalsOrbsName = "Orbs";
        internal const string OrbitalsOrbsMotionRootName = "Orbs Motion Root";
        internal const string OrbitalsOrbsPoolName = "Orbs Pool";
        internal const int MaxOrbitalsOrbsVisuals = 40;

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
            
            DebugLoggingEnabled = Config.Bind(
                "Debug",
                "Enable Debug Logging",
                false,
                "Enables verbose NADA VFX structure/discovery logs.");

            Log.LogInfo($"{ModName} loaded! Version {ModVersion}");

            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo($"{ModName}: Applied Harmony patches.");

            StartCoroutine(NadaRigCache.CacheReferenceAssetsWhenReady());
        }
        
        private void Update()
        {
            if (PluginConfig.AttachHotkey.Value.IsDown())
            {
                TryAttachToEquipped();
            }

            if (PluginConfig.BindHotkey.Value.IsDown())
            {
                TryBindEquipped();
            }
            
            ApplyCharacterSelectionWeaponPreview();
        }
        
        private void TryAttachToEquipped()
        {
            Player player = Player.m_localPlayer;
            if (player == null)
                return;

            var controller = new NadaWeaponRigController();

            Transform rightHandAttach =
                FindDescendantByName(player.transform, "RightHand_Attach");

            Transform leftHandAttach =
                FindDescendantByName(player.transform, "LeftHand_Attach");

            global::ItemDrop.ItemData rightItem =
                NadaEquippedItemResolver.ResolveRightHandItem();

            global::ItemDrop.ItemData leftItem =
                NadaEquippedItemResolver.ResolveLeftHandItem();

            if (TryApplyToFirstAttachChild(rightHandAttach, controller, rightItem, requireMissingRig: true))
                return;

            if (TryApplyToFirstAttachChild(leftHandAttach, controller, leftItem, requireMissingRig: true))
                return;

            if (TryApplyToFirstAttachChild(rightHandAttach, controller, rightItem, requireMissingRig: false))
                return;

            TryApplyToFirstAttachChild(leftHandAttach, controller, leftItem, requireMissingRig: false);
        }

        private bool TryApplyToFirstAttachChild(
            Transform attachTransform,
            NadaWeaponRigController controller,
            global::ItemDrop.ItemData itemData,
            bool requireMissingRig)
        {
            if (attachTransform == null || controller == null || itemData == null)
                return false;

            foreach (Transform childTransform in attachTransform)
            {
                if (childTransform == null)
                    continue;

                Transform weaponVisualRootTransform =
                    NadaWeaponTargets.FindEquippedWeaponVisualRoot(childTransform);

                if (weaponVisualRootTransform == null)
                    continue;

                Transform attachTarget =
                    ResolveRigAttachTarget(weaponVisualRootTransform);

                if (attachTarget == null)
                    continue;

                Transform existingRig =
                    NadaRigPaths.FindDirectChild(
                        attachTarget,
                        LocalWeaponRootName);

                if (requireMissingRig && existingRig != null)
                    continue;

                if (!requireMissingRig && existingRig == null)
                    continue;

                return controller.TryApply(childTransform.gameObject, itemData);
            }

            return false;
        }
        
        private void TryBindEquipped()
        {
            Player player = Player.m_localPlayer;
            if (player == null)
                return;

            Transform rightHandAttach =
                FindDescendantByName(player.transform, "RightHand_Attach");

            Transform leftHandAttach =
                FindDescendantByName(player.transform, "LeftHand_Attach");

            global::ItemDrop.ItemData rightItem =
                NadaEquippedItemResolver.ResolveRightHandItem();

            global::ItemDrop.ItemData leftItem =
                NadaEquippedItemResolver.ResolveLeftHandItem();

            if (TryBindUnboundAttachedItem(rightHandAttach, rightItem))
                return;

            if (TryBindUnboundAttachedItem(leftHandAttach, leftItem))
                return;

            Plugin.Log.LogInfo(
                $"{ModName}: [Bind] no unbound equipped NADA rig found to bind.");
        }
        
        private bool TryBindUnboundAttachedItem(
            Transform attachTransform,
            global::ItemDrop.ItemData itemData)
        {
            if (attachTransform == null || itemData == null)
                return false;

            if (VfxStateIO.IsBound(itemData))
                return false;

            foreach (Transform childTransform in attachTransform)
            {
                if (childTransform == null)
                    continue;

                Transform weaponVisualRootTransform =
                    NadaWeaponTargets.FindEquippedWeaponVisualRoot(childTransform);

                if (weaponVisualRootTransform == null)
                    continue;

                Transform attachTarget =
                    ResolveRigAttachTarget(weaponVisualRootTransform);

                if (attachTarget == null)
                    continue;

                Transform existingRig =
                    NadaRigPaths.FindDirectChild(
                        attachTarget,
                        LocalWeaponRootName);

                if (existingRig == null)
                    continue;

                if (!NadaWeaponBinder.Bind(itemData))
                    return false;

                var controller = new NadaWeaponRigController();
                controller.TryApply(childTransform.gameObject, itemData);

                return true;
            }

            return false;
        }
        
        internal void TryUnbindEquipped()
        {
            bool unboundAny = false;

            global::ItemDrop.ItemData rightItem =
                NadaEquippedItemResolver.ResolveRightHandItem();

            global::ItemDrop.ItemData leftItem =
                NadaEquippedItemResolver.ResolveLeftHandItem();

            if (rightItem != null && VfxStateIO.IsBound(rightItem))
            {
                NadaWeaponBinder.Unbind(rightItem);
                unboundAny = true;
            }

            if (leftItem != null && leftItem != rightItem && VfxStateIO.IsBound(leftItem))
            {
                NadaWeaponBinder.Unbind(leftItem);
                unboundAny = true;
            }

            if (!unboundAny)
            {
                Plugin.Log.LogInfo(
                    $"{ModName}: [Unbind] no bound equipped items found.");
                return;
            }

            RefreshExistingEquippedRigsOnly();
        }
        
        internal void RefreshExistingEquippedRigsOnly()
        {
            Player player = Player.m_localPlayer;
            if (player == null)
                return;

            var controller = new NadaWeaponRigController();

            Transform rightHandAttach =
                FindDescendantByName(player.transform, "RightHand_Attach");

            Transform leftHandAttach =
                FindDescendantByName(player.transform, "LeftHand_Attach");

            global::ItemDrop.ItemData rightItem =
                NadaEquippedItemResolver.ResolveRightHandItem();

            global::ItemDrop.ItemData leftItem =
                NadaEquippedItemResolver.ResolveLeftHandItem();

            TryApplyToFirstAttachChild(rightHandAttach, controller, rightItem, requireMissingRig: false);
            TryApplyToFirstAttachChild(leftHandAttach, controller, leftItem, requireMissingRig: false);
        }
        
        internal void RefreshExistingUnboundEquippedRigsOnly()
        {
            Player player = Player.m_localPlayer;
            if (player == null)
                return;

            var controller = new NadaWeaponRigController();

            Transform rightHandAttach =
                FindDescendantByName(player.transform, "RightHand_Attach");

            Transform leftHandAttach =
                FindDescendantByName(player.transform, "LeftHand_Attach");

            global::ItemDrop.ItemData rightItem =
                NadaEquippedItemResolver.ResolveRightHandItem();

            global::ItemDrop.ItemData leftItem =
                NadaEquippedItemResolver.ResolveLeftHandItem();

            if (rightItem != null && !VfxStateIO.IsBound(rightItem))
                TryApplyToFirstAttachChild(rightHandAttach, controller, rightItem, requireMissingRig: false);

            if (leftItem != null && leftItem != rightItem && !VfxStateIO.IsBound(leftItem))
                TryApplyToFirstAttachChild(leftHandAttach, controller, leftItem, requireMissingRig: false);
        }
        
        private static Transform ResolveRigAttachTarget(Transform weaponVisualRootTransform)
        {
            if (weaponVisualRootTransform == null)
                return null;

            return NadaWeaponTargets.FindVisualMeshRoot(weaponVisualRootTransform)
                   ?? weaponVisualRootTransform;
        }
        
        private static Transform FindDescendantByName(Transform rootTransform, string targetName)
        {
            if (rootTransform == null)
                return null;

            foreach (Transform childTransform in rootTransform.GetComponentsInChildren<Transform>(true))
            {
                if (childTransform != null && childTransform.name == targetName)
                    return childTransform;
            }

            return null;
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
            if (!VfxStyleStore.TryGet(styleName, out VfxState state))
            {
                Log.LogInfo($"{ModName}: [Style] could not find style '{styleName}'.");
                return;
            }

            VfxStateIO.ApplyToConfig(state);
            Config.Save();

            RefreshExistingEquippedRigsOnly();

            Log.LogInfo($"{ModName}: [Style] loaded '{styleName}'.");
        }
        
        internal void DeleteStyleFromManager(string styleName)
        {
            if (!VfxStyleStore.Delete(styleName))
            {
                Log.LogInfo($"{ModName}: [Style] could not delete style '{styleName}'.");
                return;
            }

            PluginConfig.LoadStyle.Value = "Default";
            VfxStateIO.ApplyToConfig(VfxStateIO.FromDefaults());
            Config.Save();

            RefreshExistingEquippedRigsOnly();

            Log.LogInfo($"{ModName}: [Style] deleted '{styleName}'.");
        }

        private void ApplyCharacterSelectionWeaponPreview()
        {
            if (!PluginConfig.CharacterSelectionVisibility.Value)
            {
                RemoveCharacterSelectionWeaponPreview();
                return;
            }

            if (Player.m_localPlayer != null)
                return;
            
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "start")
                return;

            if (Time.time < _nextCharacterSelectionProbeTime)
                return;

            _nextCharacterSelectionProbeTime = Time.time + 1f;

            GameObject[] roots = UnityEngine.SceneManagement.SceneManager
                .GetActiveScene()
                .GetRootGameObjects();

            var controller = new NadaWeaponRigController();

            foreach (GameObject root in roots)
            {
                if (root == null || !root.name.StartsWith("Player", System.StringComparison.Ordinal))
                    continue;

                Transform rightHandAttach =
                    FindDescendantByName(root.transform, "RightHand_Attach");

                if (rightHandAttach == null)
                    continue;

                foreach (Transform childTransform in rightHandAttach)
                {
                    if (childTransform == null)
                        continue;

                    Transform weaponVisualRootTransform =
                        NadaWeaponTargets.FindEquippedWeaponVisualRoot(childTransform);

                    if (weaponVisualRootTransform == null)
                        continue;

                    Transform attachTarget =
                        ResolveRigAttachTarget(weaponVisualRootTransform);

                    if (attachTarget == null)
                        continue;

                    Transform existingRig =
                        NadaRigPaths.FindDirectChild(
                            attachTarget,
                            LocalWeaponRootName);

                    if (existingRig != null)
                        return;

                    global::ItemDrop.ItemData previewItem =
                        ResolvePreviewItemData(root.transform);

                    if (!VfxStateIO.IsBound(previewItem))
                        continue;

                    bool applied =
                        controller.TryApply(childTransform.gameObject, previewItem);

                    if (!applied)
                        continue;

                    Log.LogInfo(
                        $"{ModName}: [CharSelectPreview] applied preview rig to '{NadaWeaponTargets.FullPath(childTransform)}'.");

                    return;
                }
            }
        }
        
        private void RemoveCharacterSelectionWeaponPreview()
        {
            if (Player.m_localPlayer != null)
                return;

            if (SceneManager.GetActiveScene().name != "start")
                return;

            GameObject[] roots = SceneManager
                .GetActiveScene()
                .GetRootGameObjects();

            foreach (GameObject root in roots)
            {
                if (root == null || !root.name.StartsWith("Player", System.StringComparison.Ordinal))
                    continue;

                Transform rightHandAttach =
                    FindDescendantByName(root.transform, "RightHand_Attach");

                if (rightHandAttach == null)
                    continue;

                foreach (Transform childTransform in rightHandAttach)
                {
                    if (childTransform == null)
                        continue;

                    Transform weaponVisualRootTransform =
                        NadaWeaponTargets.FindEquippedWeaponVisualRoot(childTransform);

                    if (weaponVisualRootTransform == null)
                        continue;

                    Transform attachTarget =
                        ResolveRigAttachTarget(weaponVisualRootTransform);

                    if (attachTarget == null)
                        continue;

                    Transform existingRig =
                        NadaRigPaths.FindDirectChild(
                            attachTarget,
                            LocalWeaponRootName);

                    if (existingRig == null)
                        continue;

                    Destroy(existingRig.gameObject);

                    Log.LogInfo(
                        $"{ModName}: [CharSelectPreview] removed preview rig from '{NadaWeaponTargets.FullPath(weaponVisualRootTransform)}'.");

                    return;
                }
            }
        }
        
        internal void RefreshDroppedItemVisibility()
        {
            var drops = FindObjectsOfType<global::ItemDrop>(true);

            foreach (var itemDrop in drops)
            {
                if (itemDrop == null || itemDrop.gameObject == null)
                    continue;

                if (!VfxStateIO.IsBound(itemDrop.m_itemData))
                    continue;

                Transform attachTarget =
                    NadaWeaponTargets.FindVisualMeshRoot(itemDrop.transform)
                    ?? itemDrop.transform;

                Transform existingRig =
                    NadaRigPaths.FindDirectChild(
                        attachTarget,
                        LocalWeaponRootName);

                if (!PluginConfig.DroppedItemVisibility.Value)
                {
                    if (existingRig != null)
                        Destroy(existingRig.gameObject);

                    continue;
                }

                if (existingRig != null)
                    continue;

                var controller = new NadaWeaponRigController();
                controller.TryApplyDroppedItem(itemDrop.gameObject, itemDrop.m_itemData);
            }
        }
        
        private static global::ItemDrop.ItemData ResolvePreviewItemData(Transform previewRoot)
        {
            if (previewRoot == null)
                return null;

            Humanoid humanoid = previewRoot.GetComponent<Humanoid>();
            if (humanoid == null)
                return null;

            System.Reflection.FieldInfo rightItemField =
                typeof(Humanoid).GetField(
                    "m_rightItem",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic |
                    BindingFlags.Public);

            return rightItemField?.GetValue(humanoid) as global::ItemDrop.ItemData;
        }
    }
}