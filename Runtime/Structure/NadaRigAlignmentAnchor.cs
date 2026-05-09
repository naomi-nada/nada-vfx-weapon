using UnityEngine;

namespace NADA.VFX.Runtime.Structure
{
    internal sealed class NadaRigAlignmentAnchor : MonoBehaviour
    {
        internal Vector3 BaseLocalPosition;
        internal Vector3 BaseLocalEulerAngles;
        internal Vector3 BaseLocalScale;

        internal void Set(NADA.VFX.Weapons.Runtime.NadaWeaponRigAlignment alignment)
        {
            BaseLocalPosition = alignment.LocalPosition;
            BaseLocalEulerAngles = alignment.LocalEulerAngles;
            BaseLocalScale = alignment.LocalScale;
        }
    }
}