using UnityEngine;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    public class NadaFlamesRigAssembly
    {
        internal static Transform EnsureLocalFlameBranch(
            Transform localWeaponRootTransform,
            string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady)
                return null;

            if (NadaRigCache.RefRigTemplateInactive == null)
                return null;

            if (localWeaponRootTransform == null)
                return null;

            Transform localEffectsRootTransform =
                NadaRigPaths.FindLocalEffectsRoot(
                    localWeaponRootTransform);

            if (localEffectsRootTransform == null)
                return null;

            bool createdOuterFlames = false;

            Transform outerFlamesTransform =
                NadaRigPaths.FindDirectChild(
                    localEffectsRootTransform,
                    Plugin.OuterFlamesName);

            if (outerFlamesTransform == null)
            {
                GameObject outerFlamesObject =
                    Object.Instantiate(
                        NadaRigCache.RefRigTemplateInactive,
                        localEffectsRootTransform,
                        false);

                outerFlamesObject.name =
                    Plugin.OuterFlamesName;

                outerFlamesObject.SetActive(true);

                outerFlamesTransform =
                    outerFlamesObject.transform;

                createdOuterFlames = true;
            }

            if (createdOuterFlames)
            {
                NadaRigTransforms.ResetLocalTransform(
                    outerFlamesTransform);

                // Normalize the donor VFX once when the flame branch is created.
                // After this, each effect owns its own simulation space.
                NadaRigTransforms.NormalizeParticleSpacesUnder(
                    outerFlamesTransform,
                    ParticleSystemSimulationSpace.Local);
            }

            SplitFlameChildrenIntoEffects(
                localEffectsRootTransform,
                outerFlamesTransform);

            return outerFlamesTransform;
        }

        internal static Transform EnsureInnerFlamesBranch(
            Transform parentTransform,
            string ownerNameForLogs)
        {
            if (!NadaRigCache.CacheReady)
                return null;

            if (NadaRigCache.RefRigTemplateInactive == null)
                return null;

            if (parentTransform == null)
                return null;

            Transform existing =
                NadaRigPaths.FindDirectChild(
                    parentTransform,
                    Plugin.InnerFlamesName);

            if (existing != null)
                return existing;

            GameObject donorObject =
                Object.Instantiate(
                    NadaRigCache.RefRigTemplateInactive,
                    parentTransform,
                    false);

            donorObject.name =
                "NADA Inner Flames Donor";

            donorObject.SetActive(false);

            Transform donorTransform =
                donorObject.transform;

            NadaRigTransforms.ResetLocalTransform(
                donorTransform);

            NadaRigTransforms.NormalizeParticleSpacesUnder(
                donorTransform,
                ParticleSystemSimulationSpace.Local);

            Transform innerFlamesTransform =
                NadaRigPaths.FindDescendantByName(
                    donorTransform,
                    "fx_Torch_Basic");

            if (innerFlamesTransform == null)
            {
                donorObject.SetActive(false);
                donorTransform.SetParent(
                    null,
                    false);

                Object.Destroy(
                    donorObject);

                Plugin.Log.LogWarning(
                    $"{Plugin.ModName}: [InnerFlamesBranchCreateFailed] " +
                    $"owner='{ownerNameForLogs}' " +
                    $"parent='{parentTransform.name}' " +
                    $"reason='donor-inner-flames-not-found'");

                return null;
            }

            innerFlamesTransform.name =
                Plugin.InnerFlamesName;

            // The extracted visual stays inactive until block state has
            // reached its behavior component. This avoids one frame of
            // donor/default flame settings during ownership handoff.
            innerFlamesTransform
                .gameObject
                .SetActive(false);

            innerFlamesTransform.SetParent(
                parentTransform,
                true);

            // Destroy is deferred. Remove the temporary donor from the
            // owned hierarchy immediately so later reconciliation in the
            // same frame cannot rediscover it.
            donorObject.SetActive(false);

            donorTransform.SetParent(
                null,
                false);

            Object.Destroy(
                donorObject);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [InnerFlamesBranchCreated] " +
                $"owner='{ownerNameForLogs}' " +
                $"parent='{parentTransform.name}' " +
                $"root='{innerFlamesTransform.name}' " +
                $"rootId={innerFlamesTransform.GetInstanceID()}");

            return innerFlamesTransform;
        }

        internal static void RemoveDirectInnerFlamesBranch(
            Transform localEffectsRootTransform,
            string ownerNameForLogs)
        {
            if (localEffectsRootTransform == null)
                return;

            Transform innerFlamesTransform =
                NadaRigPaths.FindDirectChild(
                    localEffectsRootTransform,
                    Plugin.InnerFlamesName);

            if (innerFlamesTransform == null)
                return;

            int rootId =
                innerFlamesTransform.GetInstanceID();

            string rootName =
                innerFlamesTransform.name;

            // Destroy is deferred, so make ownership disappear immediately.
            innerFlamesTransform
                .gameObject
                .SetActive(false);

            innerFlamesTransform.SetParent(
                null,
                false);

            Object.Destroy(
                innerFlamesTransform.gameObject);

            Plugin.Log.LogInfo(
                $"{Plugin.ModName}: [InnerFlamesBranchRemoved] " +
                $"owner='{ownerNameForLogs}' " +
                $"parent='{localEffectsRootTransform.name}' " +
                $"root='{rootName}' " +
                $"rootId={rootId} " +
                $"reason='ownership-change'");
        }

        private static void SplitFlameChildrenIntoEffects(
            Transform localEffectsRootTransform,
            Transform outerFlamesTransform)
        {
            if (localEffectsRootTransform == null ||
                outerFlamesTransform == null)
            {
                return;
            }

            Transform flareTransform =
                NadaRigPaths.FindDescendantByName(
                    outerFlamesTransform,
                    "flare");

            if (flareTransform != null)
            {
                flareTransform.name =
                    Plugin.FlareName;

                if (flareTransform.parent !=
                    localEffectsRootTransform)
                {
                    flareTransform.SetParent(
                        localEffectsRootTransform,
                        true);
                }
            }

            Transform innerFlamesTransform =
                NadaRigPaths.FindDescendantByName(
                    outerFlamesTransform,
                    "fx_Torch_Basic");

            if (innerFlamesTransform != null)
            {
                innerFlamesTransform.name =
                    Plugin.InnerFlamesName;

                if (innerFlamesTransform.parent !=
                    localEffectsRootTransform)
                {
                    innerFlamesTransform.SetParent(
                        localEffectsRootTransform,
                        true);
                }
            }
        }
    }
}