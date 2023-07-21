using UnityEngine;

namespace MyProject.Event
{
    [System.Serializable]
    public struct PlayerMeleeAttack_Attack_EventParam
    {
        public uint tick;
        public int ownerConnectionId;
        public Vector3 position;
        public float rotationY;
    }

    [SerializeField]
    public struct PlayerMeleeAttack_OnAttack_EventParam
    {
        public uint tick;
        public int ownerConnectionId;
        public Vector3 position;
        public float rotationY;
    }

    [SerializeField]
    public struct PlayerMeleeAttack_OnAttackHit_EventParam
    {
        public Vector3 hitPoint;
        public Vector3 hitDirection;
    }
}