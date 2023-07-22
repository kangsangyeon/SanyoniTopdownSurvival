using FishNet;
using FishNet.Object;
using MyProject.Event;
using UnityEngine;

namespace MyProject
{
    public class PlayerItemPicker : NetworkBehaviour
    {
        [SerializeField] private Player m_Player;

        #region Events

        public event System.Action<PlayerItemPicker_OnPickItem_EventParam> onPickItem_OnClient; // param: <item> 

        [Server]
        private void Server_OnPickItem(IPickupItem _item)
        {
            var _itemNetworkBehaviour = (_item as NetworkBehaviour);
            var _itemTransform = _itemNetworkBehaviour.transform;

            var _param = new PlayerItemPicker_OnPickItem_EventParam()
            {
                tick = InstanceFinder.TimeManager.Tick,
                itemNetworkObjectId = _itemNetworkBehaviour.ObjectId,
                itemPosition = _itemTransform.position,
                itemRotation = _itemTransform.rotation
            };

            onPickItem_OnClient?.Invoke(_param);
            ObserversRpc_OnPickItem(_param);
        }

        [ObserversRpc]
        private void ObserversRpc_OnPickItem(PlayerItemPicker_OnPickItem_EventParam _param)
        {
            onPickItem_OnClient?.Invoke(_param);
        }

        #endregion

        private void OnTriggerStay(Collider _other)
        {
            if (base.IsServer == false)
            {
                // collision 판정은 서버에서만 합니다.
                return;
            }

            if (_other.GetComponent<IPickupItem>() is IPickupItem _item)
            {
                if (_item.canPickup)
                {
                    _item.Pickup(m_Player);
                    Server_OnPickItem(_item);
                }
            }
        }
    }
}