using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using MyProject.Event;
using UnityEngine;
using UnityEngine.Events;

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

        public UnityEvent<Player> onPlayerAdded_OnServer = new UnityEvent<Player>();
        public UnityEvent<PlayerAddedEventParam> onPlayerAdded_OnClient = new UnityEvent<PlayerAddedEventParam>();

        [ObserversRpc]
        public void ObserversRpc_OnPlayerAdded(PlayerAddedEventParam _param)
        {
            m_PlayerInfoDict[_param.player.connectionId] = _param.player;
            m_PlayerRankDict[_param.player.connectionId] = m_PlayerInfoDict.Count;
            onPlayerAdded_OnClient.Invoke(_param);
        }

        public UnityEvent<Player> onPlayerRemoved_OnServer = new UnityEvent<Player>();
        public UnityEvent<PlayerRemovedEventParam> onPlayerRemoved_OnClient = new UnityEvent<PlayerRemovedEventParam>();

        [ObserversRpc]
        public void ObserversRpc_OnPlayerRemoved(PlayerRemovedEventParam _param)
        {
            m_PlayerInfoDict.Remove(_param.player.connectionId);
            m_PlayerRankDict.Remove(_param.player.connectionId);
            onPlayerRemoved_OnClient.Invoke(_param);
        }

        public UnityEvent<Player, Player> onPlayerKill_OnServer = new UnityEvent<Player, Player>();
        public UnityEvent<PlayerKillEventParam> onPlayerKill_OnClient = new UnityEvent<PlayerKillEventParam>();

        [ObserversRpc]
        public void ObserversRpc_OnPlayerKill(PlayerKillEventParam _param)
        {
            m_PlayerInfoDict[_param.killer.connectionId] = _param.killer;
            m_PlayerInfoDict[_param.target.connectionId] = _param.target;
            onPlayerKill_OnClient.Invoke(_param);
        }

        public UnityEvent onPlayerRankRefreshed = new UnityEvent(); // client event

        #endregion

        [ObserversRpc]
        private void ObserversRpc_RefreshPlayerRankList() => RefreshPlayerRankList();

        [Server]
        public void Server_AddPlayer(Player _player)
        {
            _player.onKill.AddListener(target => ObserversRpc_RefreshPlayerRankList());

            _player.onDead.AddListener(source => this.Invoke(() =>
            {
                _player.transform.position = m_RespawnPoint.position;
                _player.Server_OnRespawn();
            }, m_RespawnTime));

            _player.onKill.AddListener(target =>
            {
                onPlayerKill_OnServer.Invoke(_player, target);
                ObserversRpc_OnPlayerKill(new PlayerKillEventParam()
                {
                    killer = new PlayerInfo(_player),
                    target = new PlayerInfo(target)
                });
            });

            m_PlayerList.Add(_player);
            onPlayerAdded_OnServer.Invoke(_player);
            ObserversRpc_OnPlayerAdded(new PlayerAddedEventParam() { player = new PlayerInfo(_player) });
        }

        [Server]
        public void Server_RemovePlayer(Player _player)
        {
            m_PlayerList.Remove(_player);
            onPlayerRemoved_OnServer.Invoke(_player);
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

            onPlayerRankRefreshed.Invoke();
        }

        private void Awake()
        {
            Instance = this;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            onPlayerAdded_OnServer.AddListener(p => RefreshPlayerRankList());
            onPlayerRemoved_OnServer.AddListener(p => RefreshPlayerRankList());
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            onPlayerAdded_OnServer.RemoveAllListeners();
            onPlayerRemoved_OnServer.RemoveAllListeners();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            onPlayerAdded_OnClient.AddListener(p => RefreshPlayerRankList());
            onPlayerRemoved_OnClient.AddListener(p => RefreshPlayerRankList());

            m_UI_PlayerList.Initialize();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            onPlayerAdded_OnClient.RemoveAllListeners();
            onPlayerRemoved_OnClient.RemoveAllListeners();
        }
    }
}