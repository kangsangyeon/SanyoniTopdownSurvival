using System;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace MyProject
{
    public class AbilityItem : NetworkBehaviour, IPickupItem
    {
        [SerializeField] private AbilityDefinition m_Ability;
        [SerializeField] private float m_CanPickupDelay = 1.0f;

        [SerializeField] private Collider m_Collider;

        private float m_SpawnTime;

        private bool m_AlreadyClaimed;
        public bool alreadyClaimed => m_AlreadyClaimed;

        #region IObtainableItem

        public bool canPickup =>
            m_AlreadyClaimed == false && Time.time - m_SpawnTime >= m_CanPickupDelay;

        public event Action<Player> onPickup;

        [Server]
        public void Pickup(Player _player)
        {
            m_AlreadyClaimed = true;
            m_Collider.enabled = false;

            _player.Server_AddAbility(m_Ability);
            onPickup?.Invoke(_player);
            InstanceFinder.ServerManager.Despawn(gameObject);
        }

        #endregion

        private void OnEnable()
        {
            m_SpawnTime = Time.time;
            m_Collider.enabled = true;
        }

        private void OnDisable()
        {
            m_AlreadyClaimed = false;
        }
    }
}