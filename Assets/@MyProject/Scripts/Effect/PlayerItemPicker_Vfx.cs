using System;
using FishNet;
using FishNet.Object;
using MyProject.Event;
using UnityEngine;

namespace MyProject
{
    public class PlayerItemPicker_Vfx : MonoBehaviour
    {
        [SerializeField] private PlayerItemPicker m_ItemPicker;
        [SerializeField] private ParticleSystem m_Prefab_Particle;

        private Action<PlayerItemPicker_OnPickItem_EventParam> m_OnPickItemAction;

        private void OnEnable()
        {
            m_OnPickItemAction = _param =>
            {
                IObtainableItem _item = null;

                if (InstanceFinder.IsServer
                    && InstanceFinder.ServerManager.Objects.Spawned.ContainsKey(_param.itemNetworkObjectId))
                {
                    _item =
                        InstanceFinder.ServerManager.Objects.Spawned[_param.itemNetworkObjectId]
                            .GetComponent<IObtainableItem>();
                }
                else if (InstanceFinder.ClientManager.Objects.Spawned.ContainsKey(_param.itemNetworkObjectId))
                {
                    _item =
                        InstanceFinder.ClientManager.Objects.Spawned[_param.itemNetworkObjectId]
                            .GetComponent<IObtainableItem>();
                }

                if (_item != null)
                {
                    var _itemNetworkBehaviour = _item as NetworkBehaviour;
                    var _hider = _itemNetworkBehaviour.GetComponent<GameObjectHider>();
                    _hider.Hide();
                }

                var _fx =
                    GameObject.Instantiate(m_Prefab_Particle, _param.itemPosition, Quaternion.identity);
                Destroy(_fx, 2.0f);
            };
            m_ItemPicker.onPickItem_OnClient += m_OnPickItemAction;
        }

        private void OnDisable()
        {
            m_ItemPicker.onPickItem_OnClient -= m_OnPickItemAction;
            m_OnPickItemAction = null;
        }
    }
}