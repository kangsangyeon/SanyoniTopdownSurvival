using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Events;

namespace MyProject
{
    public class Scene_Game : MonoBehaviour
    {
        [SerializeField] private int m_MaxTime = 60 * 5;
        [SerializeField] private int m_MaxKillCount = 30;

        private List<Player> m_PlayerList = new List<Player>();
        public ReadOnlyCollection<Player> playerList => m_PlayerList.AsReadOnly();

        public UnityEvent<Player> onPlayerAdded = new UnityEvent<Player>();
        public UnityEvent<Player> onPlayerRemoved = new UnityEvent<Player>();
        public UnityEvent<Player, Player> onPlayerKill = new UnityEvent<Player, Player>();

        private void Start()
        {
            var _players = GameObject.FindObjectsOfType<Player>();
            foreach (var _player in _players)
                AddPlayer(_player);
        }

        private void AddPlayer(Player _player)
        {
            m_PlayerList.Add(_player);
            onPlayerAdded.Invoke(_player);

            _player.onKill.AddListener(target => onPlayerKill.Invoke(_player, target));
        }

        private void RemovePlayer(Player _player)
        {
            m_PlayerList.Remove(_player);
            onPlayerRemoved.Invoke(_player);
        }
    }
}