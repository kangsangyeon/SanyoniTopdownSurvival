using System;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace MyProject
{
    public class AbilityItem : NetworkBehaviour, IPickupItem
    {
        [SerializeField] private float m_CanPickupDelay = 1.0f;

        [SerializeField] private Collider m_Collider;
        [SerializeField] private Transform m_ModelParent;

        [SyncVar(WritePermissions = WritePermission.ServerOnly)]
        private string m_AbilityId;

        private AbilityDefinition m_Ability;

        private float m_SpawnTime;

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

        // Instantiate한 직후 NetworkObject가 초기화되지 않았기 때문에 Server 검증을 정상적으로 통과할 수 없습니다.
        // 따라서 주석 처리하지만, 서버에서만 불러야 합니다.
        // [Server]
        public void Server_Initialize()
        {
            m_Ability =
                OfflineGameplayDependencies.abilityDatabase.GetRandomAbility();
            m_AbilityId =
                m_Ability.abilityId;
            SetModelPrefab(m_Ability.prefabModel);
        }

        private void SetModelPrefab(GameObject _prefab)
        {
            var _go = GameObject.Instantiate(_prefab, m_ModelParent);
            _go.transform.localPosition = Vector3.zero;
            _go.transform.rotation = Quaternion.identity;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            m_SpawnTime = Time.time;
            m_Collider.enabled = true;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            m_AlreadyClaimed = false;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (base.IsServer == false)
            {
                m_Ability =
                    OfflineGameplayDependencies.abilityDatabase.GetAbility(m_AbilityId);
                SetModelPrefab(m_Ability.prefabModel);
            }
        }
    }
}