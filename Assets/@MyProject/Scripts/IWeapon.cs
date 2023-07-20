using UnityEngine;

namespace MyProject
{
    [System.Serializable]
    public struct IWeapon_OnAttack_EventParam
    {
        public uint tick;
        public int ownerConnectionId;
        public Vector3 position;
        public float rotationY;
    }

    public interface IWeapon
    {
        object owner { get; }
        int ownerConnectionId { get; }
        int attackDamageMagnitude { get; }
        float attackDelay { get; }

        event System.Action<IWeapon_OnAttack_EventParam> onAttack;
    }
}