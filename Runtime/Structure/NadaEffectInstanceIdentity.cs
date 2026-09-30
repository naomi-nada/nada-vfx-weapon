using UnityEngine;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    /// <summary>
    /// Runtime identity for one effect-block instance.
    ///
    /// The GameObject name is only for humans/debugging.
    /// InstanceId + TypeId are the actual identity.
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class NadaEffectInstanceIdentity :
        MonoBehaviour
    {
        [SerializeField]
        private uint _instanceId;

        [SerializeField]
        private string _typeId;

        internal uint InstanceId =>
            _instanceId;

        internal string TypeId =>
            _typeId;

        internal void Configure(
            uint instanceId,
            string typeId)
        {
            _instanceId = instanceId;
            _typeId = typeId;
        }

        internal bool Matches(
            uint instanceId,
            string typeId)
        {
            return
                _instanceId == instanceId &&
                string.Equals(
                    _typeId,
                    typeId,
                    System.StringComparison.Ordinal);
        }
    }
}