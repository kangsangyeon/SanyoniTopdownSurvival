using FishNet.Managing.Timing;
using UnityEngine;

namespace MyProject.Event
{
    [System.Serializable]
    public struct Weapon_MagicSword_Attack_EventParam
    {
        public uint tick;
        public PreciseTick preciseTick;
        public int ownerConnectionId;
        public Vector3 position;
        public float rotationY;
        public bool isProjectileAttack;
    }

    [System.Serializable]
    public struct Weapon_MagicSword_OnProjectileHit_EventParam
    {
        public uint tick;
        public int ownerConnectionId;
        public Vector3 hitPoint;
        public Vector3 hitDirection;
        public Quaternion hitRotation;
    }
}