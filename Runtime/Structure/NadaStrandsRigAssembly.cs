using UnityEngine;

namespace NADA.VFX.Runtime.Structure
{
    public class NadaStrandsRigAssembly
    {
        internal static Transform EnsureLocalStrandsBranch(
            Transform localWeaponRootTransform,
            string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady)
                return null;

            if (NadaRigCache.StrandsTemplateInactive == null)
                return null;

            if (localWeaponRootTransform == null)
                return null;

            Transform localEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(localWeaponRootTransform);

            if (localEffectsRootTransform == null)
                return null;

            Transform strandsTransform =
                NadaRigPaths.FindDirectChild(localEffectsRootTransform, Plugin.StrandsName);

            if (strandsTransform != null)
                return strandsTransform;

            GameObject strandsObject =
                Object.Instantiate(NadaRigCache.StrandsTemplateInactive, localEffectsRootTransform, false);

            strandsObject.name = Plugin.StrandsName;
            strandsObject.SetActive(true);

            NadaRigTransforms.ResetLocalTransform(strandsObject.transform);
            
            strandsObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            
            return strandsObject.transform;
        }
    }
}