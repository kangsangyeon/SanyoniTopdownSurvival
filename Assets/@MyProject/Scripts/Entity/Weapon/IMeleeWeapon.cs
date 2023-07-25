using MyProject.Struct;
using UnityEngine;

namespace MyProject
{
    [System.Serializable]
    public struct IMeleeWeapon_OnAttackHit_EventParam
    {
        public uint tick;
        public int ownerConnectionId;
        public Vector3 hitPoint;
        public Vector3 hitDirection;
        public Quaternion hitRotation;
        public DamageParam hitDamage;
    }

    public interface IMeleeWeapon : IWeapon
    {
        float meleeAttackDelay { get; }
        float meleeAttackInterval { get; }
        Collider meleeAttackRange { get; }
        event System.Action<IMeleeWeapon_OnAttackHit_EventParam> onAttackHit;
    }
}