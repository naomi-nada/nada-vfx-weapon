using System.Collections.Generic;
using UnityEngine;
using NADA.VFX.Weapon.Core.Config;
using NADA.VFX.Weapon.Weapons.Targets;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    internal static class NadaRigMaintenance
    {
        internal static void DisableBrokenFlameRenderer(Transform sword15LavaTf, string ownerNameForLogs)
        {
            if (sword15LavaTf == null) return;

            var psChild = sword15LavaTf.Find("Particle System");
            if (psChild == null) return;

            var psr = psChild.GetComponent<ParticleSystemRenderer>();
            if (psr != null && psr.enabled)
            {
                psr.enabled = false;
                Plugin.Log.LogInfo(
                    $"{Plugin.ModName}: Disabled broken flame renderer at '{NadaWeaponTargets.FullPath(psChild)}' (owner='{ownerNameForLogs}').");
            }

            var ps = psChild.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                try { ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); } catch { }
            }
        }

        internal static void ApplyPickupFix(GameObject go)
        {
            if (!NadaRigCache.CacheReady) return;
            if (NadaRigCache.PickupMat == null) return;
            if (!NadaWeaponTargets.IsTargetRoot(go)) return;

            var psr = go.GetComponent<ParticleSystemRenderer>();
            if (psr == null) return;
            if (psr.sharedMaterial == NadaRigCache.PickupMat) return;

            psr.sharedMaterial = NadaRigCache.PickupMat;
            Plugin.Log.LogInfo($"{Plugin.ModName}: Fixed ItemDrop pickup particle material on '{go.name}' (root PSR).");
        }
    }
}