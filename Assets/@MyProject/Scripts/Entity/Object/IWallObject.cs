using UnityEngine;

namespace MyProject
{
    public interface IWallObject
    {
        public EntityHealth health { get; }
        public bool isInvincible { get; }
        public bool isInvincibleTemporary { get; }
    }

    public interface IRebuildingWallObject : IWallObject
    {
        public float rebuildingDelay { get; }
        public float invincibleTimeWhenRebuilding { get; }
        public Vector3 rebuildingPosition { get; }
        public Quaternion rebuildingRotation { get; }
        public Vector3 rebuildingScale { get; }
    }
}