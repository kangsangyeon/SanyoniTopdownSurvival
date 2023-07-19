using UnityEngine;

namespace MyProject.Event
{
    [System.Serializable]
    public struct PlayerGunShoot_Fire_EventParam
    {
        public uint tick;
        public int ownerConnectionId;
        public Vector2 position;
        public float rotationY;
    }
}