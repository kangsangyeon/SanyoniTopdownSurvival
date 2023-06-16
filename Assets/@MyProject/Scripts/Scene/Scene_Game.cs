using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

            _player.health.onHealthIsZero.AddListener(() =>
            {
                // 죽은 플레이어의 가장 최근 데미지 요인들부터 순회하여
                // 가장 마지막으로 자신에게 피해를 입힌 플레이어를 얻습니다.
                HealthModifier _healthModifier =
                    _player.health.damageList
                        .Reverse().FirstOrDefault(m =>
                            m.source is IWeapon _weapon
                            && _weapon.owner is Player);

                if (_healthModifier != null)
                {
                    if (Time.time - 10 > _healthModifier.time)
                    {
                        // 가장 마지막으로 플레이어로부터 받은 피해가 조금 오래 된 경우,
                        // 플레이어로부터 죽었다고 판정하지 않습니다.
                    }
                    else
                    {
                        // 플레이어로 인해 죽은 경우 실행됩니다.
                        var _weapon = _healthModifier.source as IWeapon;
                        var _killer = _weapon.owner as Player;
                        onPlayerKill.Invoke(_killer, _player);
                    }
                }

                // 플레이어로부터 죽은 것이 아닐 때 실행됩니다.
                HealthModifier _lastHealthModifier = _player.health.damageList.Last();

                // TODO
            });
        }
    }
}