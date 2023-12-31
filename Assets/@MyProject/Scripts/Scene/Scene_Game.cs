using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using MyProject.Event;
using UnityEngine;

namespace MyProject
{
    public class Scene_Game : NetworkBehaviour
    {
        public static Scene_Game Instance;

        [SerializeField] private UI_PlayerList m_UI_PlayerList;
        [SerializeField] private Transform m_RespawnPoint;
        [SerializeField] private int m_MaxTime = 60 * 5;
        [SerializeField] private int m_MaxKillCount = 30;
        [SerializeField] private int m_RespawnTime = 5;

        #region Server Only

        private readonly List<Player> m_PlayerList = new List<Player>();
        public IReadOnlyList<Player> playerList => m_PlayerList;

        #endregion

        private readonly Dictionary<int, int> m_PlayerRankDict = new Dictionary<int, int>();
        public IReadOnlyDictionary<int, int> playerRankDict => m_PlayerRankDict;

        private readonly Dictionary<int, PlayerInfo> m_PlayerInfoDict = new Dictionary<int, PlayerInfo>();
        public IReadOnlyDictionary<int, PlayerInfo> playerInfoDict => m_PlayerInfoDict;

        #region Events

        public event System.Action<Player> onPlayerAdded_OnServer;
        public event System.Action<PlayerAddedEventParam> onPlayerAdded_OnClient;

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnPlayerAdded(PlayerAddedEventParam _param)
        {
            m_PlayerInfoDict.Add(_param.player.connectionId, _param.player);
            m_PlayerRankDict.Add(_param.player.connectionId, m_PlayerInfoDict.Count);
            onPlayerAdded_OnClient?.Invoke(_param);
        }

        public event System.Action<Player> onPlayerRemoved_OnServer;
        public event System.Action<PlayerRemovedEventParam> onPlayerRemoved_OnClient;

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnPlayerRemoved(PlayerRemovedEventParam _param)
        {
            m_PlayerInfoDict.Remove(_param.player.connectionId);
            m_PlayerRankDict.Remove(_param.player.connectionId);
            onPlayerRemoved_OnClient?.Invoke(_param);
        }

        public event System.Action<Player, Player> onPlayerKill_OnServer;
        public event System.Action<PlayerKillEventParam> onPlayerKill_OnClient;

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnPlayerKill(PlayerKillEventParam _param)
        {
            m_PlayerInfoDict[_param.killer.connectionId] = _param.killer;
            m_PlayerInfoDict[_param.target.connectionId] = _param.target;
            onPlayerKill_OnClient?.Invoke(_param);
        }

        public event System.Action onPlayerRankRefreshed; // client event

        #endregion

        private System.Action<PlayerAddedEventParam> m_OnPlayerAdded_OnClient;
        private System.Action<PlayerRemovedEventParam> m_OnPlayerRemoved_OnClient;

        /// <summary>
        /// * exclude server: 서버 PC에서 클라이언트로 플레이하던 중단하던 playerInfoList를 계속 관리하고 있어야 하기 때문에,
        /// 서버 PC에서 이 메세지를 받아 중복 처리해서는 안됩니다.
        /// </summary>
        [TargetRpc(ExcludeServer = true)]
        public void TargetRpc_JoinGame(NetworkConnection _conn, GameJoinedEventParam _param)
        {
            _param.playerInfoList.ForEach(p =>
            {
                m_PlayerInfoDict.Add(p.connectionId, p);
                Debug.Log($"player already joined: {p.connectionId}");
            });
        }

        [Server]
        public void Server_AddPlayer(Player _player)
        {
            _player.onKill_OnClient += target => RefreshPlayerRankList();

            _player.onDead_OnServer += (source =>
                this.Invoke(() => { _player.Server_Respawn(m_RespawnPoint.position); }, m_RespawnTime));

            _player.onKill_OnServer += (target =>
            {
                m_PlayerInfoDict[_player.OwnerId] = new PlayerInfo(_player);
                m_PlayerInfoDict[target.OwnerId] = new PlayerInfo(target);

                onPlayerKill_OnServer?.Invoke(_player, target);
                onPlayerKill_OnClient?.Invoke(new PlayerKillEventParam()
                {
                    killer = new PlayerInfo(_player),
                    target = new PlayerInfo(target)
                });
                ObserversRpc_OnPlayerKill(new PlayerKillEventParam()
                {
                    killer = new PlayerInfo(_player),
                    target = new PlayerInfo(target)
                });
            });

            m_PlayerList.Add(_player);
            m_PlayerInfoDict.Add(_player.OwnerId, new PlayerInfo(_player));
            m_PlayerRankDict.Add(_player.OwnerId, m_PlayerInfoDict.Count);

            onPlayerAdded_OnServer?.Invoke(_player);
            onPlayerAdded_OnClient?.Invoke(new PlayerAddedEventParam() { player = new PlayerInfo(_player) });
            ObserversRpc_OnPlayerAdded(new PlayerAddedEventParam() { player = new PlayerInfo(_player) });
        }

        [Server]
        public void Server_RemovePlayer(Player _player)
        {
            m_PlayerList.Remove(_player);
            m_PlayerInfoDict.Remove(_player.OwnerId);
            m_PlayerRankDict.Remove(_player.OwnerId);

            onPlayerRemoved_OnServer?.Invoke(_player);
            onPlayerRemoved_OnClient?.Invoke(new PlayerRemovedEventParam() { player = new PlayerInfo(_player) });
            ObserversRpc_OnPlayerRemoved(new PlayerRemovedEventParam() { player = new PlayerInfo(_player) });
        }

        private void RefreshPlayerRankList()
        {
            if (m_PlayerInfoDict.Count == 0)
                return;

            var _playerListOrderByKillCount = m_PlayerInfoDict.Values.OrderBy(p => p.killCount).Reverse().ToList();

            m_PlayerRankDict.Clear();
            m_PlayerRankDict.Add(_playerListOrderByKillCount[0].connectionId, 1);

            int _lastKillCount = _playerListOrderByKillCount[0].killCount;
            int _lastRank = 1;

            for (int i = 1; i < _playerListOrderByKillCount.Count; i++)
            {
                PlayerInfo _player = _playerListOrderByKillCount[i];
                if (_player.killCount < _lastKillCount)
                {
                    ++_lastRank;
                    _lastKillCount = _player.killCount;
                }

                m_PlayerRankDict.Add(_player.connectionId, _lastRank);
            }

            onPlayerRankRefreshed?.Invoke();
        }

        private void Awake()
        {
            Instance = this;
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            m_OnPlayerAdded_OnClient = p => RefreshPlayerRankList();
            onPlayerAdded_OnClient += m_OnPlayerAdded_OnClient;

            m_OnPlayerRemoved_OnClient = p => RefreshPlayerRankList();
            onPlayerRemoved_OnClient += m_OnPlayerRemoved_OnClient;

            m_UI_PlayerList.Initialize();
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();

            onPlayerAdded_OnClient -= m_OnPlayerAdded_OnClient;
            m_OnPlayerAdded_OnClient = null;

            onPlayerRemoved_OnClient -= m_OnPlayerRemoved_OnClient;
            m_OnPlayerRemoved_OnClient = null;

            m_PlayerList.Clear();
            m_PlayerInfoDict.Clear();
            m_PlayerRankDict.Clear();

            m_UI_PlayerList.Uninitialize();
        }
    }
}