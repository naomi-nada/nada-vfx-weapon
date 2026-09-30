using UnityEngine;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    /// <summary>
    /// Lightweight runtime handle for one instantiated effect block.
    /// </summary>
    internal sealed class NadaEffectInstance
    {
        internal NadaEffectInstanceIdentity Identity { get; }

        internal Transform RootTransform { get; }

        internal GameObject RootObject =>
            RootTransform != null
                ? RootTransform.gameObject
                : null;

        internal uint InstanceId =>
            Identity != null
                ? Identity.InstanceId
                : 0u;

        internal string TypeId =>
            Identity != null
                ? Identity.TypeId
                : null;

        internal bool IsValid =>
            Identity != null &&
            RootTransform != null;

        internal NadaEffectInstance(
            NadaEffectInstanceIdentity identity)
        {
            Identity = identity;

            RootTransform =
                identity != null
                    ? identity.transform
                    : null;
        }
    }
}