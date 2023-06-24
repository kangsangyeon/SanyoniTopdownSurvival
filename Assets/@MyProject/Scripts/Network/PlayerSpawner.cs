using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;

namespace MyProject
{
    public class PlayerSpawner : NetworkBehaviour
    {
        public class PlayerData
        {
            public NetworkObject networkObject;
            public Player player;
        }

        /// <summary>
        /// 스폰할 캐릭터 프리팹입니다.
        /// </summary>
        [SerializeField] private GameObject m_CharacterPrefab;

        /// <summary>
        /// 현재 스폰된 플레이어에 대한 데이터입니다.
        /// </summary>
        public PlayerData spawnedCharacterData { get; private set; } = new PlayerData();

        /// <summary>
        /// 캐릭터가 변경될 때 디스패치됩니다.
        /// </summary>
        public static UnityEvent<GameObject> onCharacterUpdated = new UnityEvent<GameObject>();

        /// <summary>
        /// 리스폰을 시도합니다.
        /// </summary>
        [Client]
        public void TryRespawn()
        {
            CmdRespawn();
        }

        /// <summary>
        /// 클라이언트가 리스폰을 요청했을 때 서버에서 호출됩니다.
        /// </summary>
        [ServerRpc]
        private void CmdRespawn()
        {
            Transform _spawn = OfflineGameplayDependencies.spawnManager.ReturnSpawnPoint();
            if (_spawn == null)
            {
                Debug.LogError("스폰 포인트를 찾을 수 없습니다.");
            }
            else
            {
                if (spawnedCharacterData.networkObject == null)
                {
                    // 캐릭터가 최초로 스폰될 때 실행됩니다.

                    GameObject _spawned = Instantiate(m_CharacterPrefab, _spawn.position, Quaternion.Euler(0f, _spawn.eulerAngles.y, 0f));
                    base.Spawn(_spawned, base.Owner);

                    spawnedCharacterData = GetSpawnedCharacterDataFrom(_spawned);
                    TargetCharacterSpawned(base.Owner, spawnedCharacterData.networkObject);
                }
                else
                {
                    // 캐릭터가 이미 소환된 상태일 때 실행됩니다.

                    spawnedCharacterData.networkObject.transform.position = _spawn.position;
                    spawnedCharacterData.networkObject.transform.rotation = Quaternion.Euler(0f, _spawn.eulerAngles.y, 0f);
                    Physics.SyncTransforms();
                    //Restore health and set respawned.
                    // spawnedCharacterData.Health.RestoreHealth();
                    // spawnedCharacterData.Health.Respawned();
                }
            }
        }

        /// <summary>
        /// 서버가 캐릭터를 스폰했을 때, 스폰된 캐릭터의 소유자 클라이언트에서 호출됩니다.
        /// </summary>
        [TargetRpc]
        private void TargetCharacterSpawned(NetworkConnection _conn, NetworkObject _character)
        {
            GameObject _playerObj = (_character == null) ? null : _playerObj = _character.gameObject;
            onCharacterUpdated?.Invoke(_playerObj);

            //If player was spawned.
            if (_playerObj != null)
                spawnedCharacterData = GetSpawnedCharacterDataFrom(_character.gameObject);
        }

        private PlayerData GetSpawnedCharacterDataFrom(GameObject _go) => new PlayerData()
        {
            networkObject = _go.GetComponent<NetworkObject>(),
            player = _go.GetComponent<Player>()
        };
    }
}