using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MyProject
{
    public class Scene_Game : MonoBehaviour
    {
        [SerializeField] private Transform m_RespawnPoint;
        [SerializeField] private int m_MaxTime = 60 * 5;
        [SerializeField] private int m_MaxKillCount = 30;
        [SerializeField] private int m_RespawnTime = 5;

        private readonly List<Player> m_PlayerList = new List<Player>();
        public ReadOnlyCollection<Player> playerList => m_PlayerList.AsReadOnly();

        private readonly Dictionary<Player, int> m_PlayerRankDict = new Dictionary<Player, int>();
        public IReadOnlyDictionary<Player, int> playerRankDict => m_PlayerRankDict;

        public UnityEvent<Player> onPlayerAdded = new UnityEvent<Player>();
        public UnityEvent<Player> onPlayerRemoved = new UnityEvent<Player>();
        public UnityEvent<Player, Player> onPlayerKill = new UnityEvent<Player, Player>();
        public UnityEvent onPlayerRankRefreshed = new UnityEvent();

        private void Start()
        {
            var _players = GameObject.FindObjectsOfType<Player>();

            foreach (var _player in _players)
                AddPlayer(_player);

            RefreshPlayerRankList();
        }

        private void AddPlayer(Player _player)
        {
            m_PlayerList.Add(_player);
            onPlayerAdded.Invoke(_player);

            _player.onKill.AddListener(target =>
            {
                RefreshPlayerRankList();
                onPlayerKill.Invoke(_player, target);
            });
            _player.onDead.AddListener(source => this.Invoke(() =>
            {
                _player.transform.position = m_RespawnPoint.position;
                _player.onRespawn.Invoke();
            }, m_RespawnTime));
        }

        private void RemovePlayer(Player _player)
        {
            m_PlayerList.Remove(_player);
            onPlayerRemoved.Invoke(_player);
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
    }
}