using NADA.VFX.Weapon.Weapons.Runtime;
using UnityEngine;

namespace NADA.VFX.Weapon.Runtime.Structure
{
    internal sealed class NadaRigAlignmentAnchor : MonoBehaviour
    {
        internal Vector3 BaseLocalPosition;
        internal Vector3 BaseLocalEulerAngles;
        internal Vector3 BaseLocalScale;

        internal void Set(NadaWeaponRigAlignment alignment)
        {
            BaseLocalPosition = alignment.LocalPosition;
            BaseLocalEulerAngles = alignment.LocalEulerAngles;
            BaseLocalScale = alignment.LocalScale;
        }
    }
}