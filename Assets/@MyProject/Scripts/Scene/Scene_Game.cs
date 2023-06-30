using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MyProject
{
    public class Scene_Game : NetworkBehaviour
    {
        public static Scene_Game Instance;

        [SerializeField] private Transform m_RespawnPoint;
        [SerializeField] private int m_MaxTime = 60 * 5;
        [SerializeField] private int m_MaxKillCount = 30;
        [SerializeField] private int m_RespawnTime = 5;

        private readonly List<Player> m_PlayerList = new List<Player>();

        public IReadOnlyList<Player> playerList => m_PlayerList;

        private readonly Dictionary<Player, int> m_PlayerRankDict = new Dictionary<Player, int>();
        public IReadOnlyDictionary<Player, int> playerRankDict => m_PlayerRankDict;

        #region Events

        public UnityEvent<Player> onPlayerAdded_OnServer = new UnityEvent<Player>();
        public UnityEvent<Player> onPlayerAdded_OnClient = new UnityEvent<Player>();

        [ObserversRpc]
        private void ObserversRpc_OnPlayerAdded(Player _player) => onPlayerAdded_OnClient.Invoke(_player);

        public UnityEvent<Player> onPlayerRemoved_OnServer = new UnityEvent<Player>();
        public UnityEvent<Player> onPlayerRemoved_OnClient = new UnityEvent<Player>();

        [ObserversRpc]
        private void ObserversRpc_OnPlayerRemoved(Player _player) => onPlayerRemoved_OnClient.Invoke(_player);

        public UnityEvent<Player, Player> onPlayerKill_OnServer = new UnityEvent<Player, Player>();
        public UnityEvent<Player, Player> onPlayerKill_OnClient = new UnityEvent<Player, Player>();

        [ObserversRpc]
        private void ObserversRpc_OnPlayerKill(Player _killer, Player _target) =>
            onPlayerKill_OnClient.Invoke(_killer, _target);

        public UnityEvent onPlayerRankRefreshed = new UnityEvent(); // client event

        #endregion

        [ServerRpc]
        public void ServerRpc_RequestAddPlayer(Player _player) => Server_AddPlayer(_player);

        [ServerRpc]
        public void ServerRpc_RequestRemovePlayer(Player _player) => Server_RemovePlayer(_player);

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_AddPlayer(Player _player) => m_PlayerList.Add(_player);

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_RemovePlayer(Player _player) => m_PlayerList.Remove(_player);

        [ObserversRpc]
        private void ObserversRpc_RefreshPlayerRankList() => RefreshPlayerRankList();

        [Server]
        public void Server_AddPlayer(Player _player)
        {
            m_PlayerList.Add(_player);
            ObserversRpc_AddPlayer(_player);

            onPlayerAdded_OnServer.Invoke(_player);
            ObserversRpc_OnPlayerAdded(_player);

            _player.onKill.AddListener(target => ObserversRpc_RefreshPlayerRankList());

            _player.onDead.AddListener(source => this.Invoke(() =>
            {
                _player.transform.position = m_RespawnPoint.position;
                _player.Server_OnRespawn();
            }, m_RespawnTime));

            _player.onKill.AddListener(target =>
            {
                onPlayerKill_OnServer.Invoke(_player, target);
                ObserversRpc_OnPlayerKill(_player, target);
            });
        }

        [Server]
        public void Server_RemovePlayer(Player _player)
        {
            m_PlayerList.Remove(_player);
            ObserversRpc_RemovePlayer(_player);

            onPlayerRemoved_OnServer.Invoke(_player);
            ObserversRpc_OnPlayerRemoved(_player);
        }

        private void RefreshPlayerRankList()
        {
            if (m_PlayerList.Count == 0)
                return;

            var _playerListOrderByKillCount = m_PlayerList.OrderBy(p => p.killCount).Reverse().ToList();

            m_PlayerRankDict.Clear();
            m_PlayerRankDict.Add(_playerListOrderByKillCount[0], 1);

            int _lastKillCount = _playerListOrderByKillCount[0].killCount;
            int _lastRank = 1;

            for (int i = 1; i < _playerListOrderByKillCount.Count; i++)
            {
                Player _player = _playerListOrderByKillCount[i];
                if (_player.killCount < _lastKillCount)
                {
                    ++_lastRank;
                    _lastKillCount = _player.killCount;
                }

                m_PlayerRankDict.Add(_player, _lastRank);
            }

            onPlayerRankRefreshed.Invoke();
        }

        private void Awake()
        {
            Instance = this;
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            onPlayerAdded_OnServer.AddListener(p => RefreshPlayerRankList());
            onPlayerAdded_OnClient.AddListener(p => RefreshPlayerRankList());

            onPlayerRemoved_OnServer.AddListener(p => RefreshPlayerRankList());
            onPlayerRemoved_OnClient.AddListener(p => RefreshPlayerRankList());
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();

            onPlayerAdded_OnServer.RemoveAllListeners();
            onPlayerAdded_OnClient.RemoveAllListeners();

            onPlayerRemoved_OnServer.RemoveAllListeners();
            onPlayerRemoved_OnClient.RemoveAllListeners();
        }
    }
}