using System;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace MyProject
{
    public class AbilityItem : NetworkBehaviour, IPickupItem
    {
        [SerializeField] private float m_CanPickupDelay = 1.0f;

        [SerializeField] private Collider m_Collider;
        [SerializeField] private Transform m_ModelParent;

        private float m_SpawnTime;
        private AbilityDefinition m_Ability;

        private bool m_AlreadyClaimed;
        public bool alreadyClaimed => m_AlreadyClaimed;

        public event System.Action<AbilityDefinition> onSetAbilityDefinition_onClient;

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

        public void SetModel(GameObject _go)
        {
            _go.transform.SetParent(_go.transform);
        }

        [Server]
        private void Server_SetAbility(AbilityDefinition _abilityDefinition)
        {
            m_Ability = _abilityDefinition;
            onSetAbilityDefinition_onClient?.Invoke(_abilityDefinition);
            ObserversRpc_SetAbility(_abilityDefinition.abilityId);
        }

        [ObserversRpc]
        private void ObserversRpc_SetAbility(string _abilityId)
        {
            var _abilityDefinition = OfflineGameplayDependencies.abilityDatabase.GetAbility(_abilityId);
            m_Ability = _abilityDefinition;
            onSetAbilityDefinition_onClient?.Invoke(_abilityDefinition);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            m_SpawnTime = Time.time;
            m_Collider.enabled = true;

            var _randomAbility = OfflineGameplayDependencies.abilityDatabase.GetRandomAbility();
            Server_SetAbility(_randomAbility);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            m_AlreadyClaimed = false;
        }
    }
}