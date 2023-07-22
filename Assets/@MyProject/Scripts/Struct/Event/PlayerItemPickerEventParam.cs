using UnityEngine;

namespace MyProject.Event
{
    [System.Serializable]
    public struct PlayerItemPicker_OnPickItem_EventParam
    {
        public uint tick;
        public int itemNetworkObjectId;
        public Vector3 itemPosition;
        public Quaternion itemRotation;
    }
}