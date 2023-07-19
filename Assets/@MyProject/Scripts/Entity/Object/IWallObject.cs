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
        public int rebuildingDelay { get; }
        public float invincibleTimeWhenRebuilding { get; }
        public Vector3 rebuildingPosition { get; }
        public Quaternion rebuildingRotation { get; }
    }
}