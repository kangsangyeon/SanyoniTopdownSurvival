using System;
using FishNet;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MyProject
{
    public class SpawnLevelUpCrateOnPlayerLevelUp : MonoBehaviour
    {
        [SerializeField] private GameObject m_Prefab_LevelUpCrate;
        [SerializeField] private float m_SpawnDistance = 1.2f;

        private int m_ReservedCreationCount;

        private Action<Player> m_OnPlayerAddedAction;
        private Action<Player> m_OnPlayerRemovedAction;

        private Vector3 GetSpawnPosition(Vector3 _origin, float _distance)
        {
            float _rotation = Random.Range(0.0f, 360.0f);
            Vector3 _directionToSpawnPoint = new Vector3(
                Mathf.Cos(_rotation * Mathf.Deg2Rad), 0, Mathf.Sin(_rotation * Mathf.Deg2Rad));
            Vector3 _position = _origin + _directionToSpawnPoint * _distance;
            return _position;
        }

        private void SpawnLevelUpCrates(Vector3 _center, int _count)
        {
            for (int i = 0; i < _count; ++i)
            {
                // 레벨업 상자를 스폰합니다.
                var _go = GameObject.Instantiate(
                    m_Prefab_LevelUpCrate,
                    GetSpawnPosition(_center, m_SpawnDistance),
                    Quaternion.identity);

                InstanceFinder.ServerManager.Spawn(_go);
            }
        }

        private void InitializePlayerEvents(Player _player)
        {
            if (InstanceFinder.IsServer == false)
            {
                // 이벤트 부착은 서버측에서만 실시합니다.
                // 아래 동작들은 전부 서버에서 이루어져야 하는 게임 로직이기 때문입니다.
                return;
            }

            _player.GetComponent<ILevelable>().onCurrentExperienceOrLevelChanged += _param =>
            {
                if (_param.becomeNewLevel)
                {
                    if (_player.health.health == 0)
                    {
                        // 플레이어가 죽은 상태에서 레벨업했다면,
                        // 다음 부활 시점에 생성되도록 생성 예약 카운트를 증가시킵니다.
                        ++m_ReservedCreationCount;
                    }
                    else
                    {
                        // 플레이어가 살아있는 상태에서 레벨업했다면,
                        // 상자를 즉시 스폰합니다.
                        SpawnLevelUpCrates(_player.transform.position, 1);
                    }
                }
            };

            _player.onRespawn_OnServer += () =>
            {
                if (m_ReservedCreationCount > 0)
                {
                    // 플레이어가 사망한 상태에서 레벨업했다면,
                    // 리스폰되었을 때 생성합니다.
                    SpawnLevelUpCrates(_player.transform.position, m_ReservedCreationCount);
                }
            };
        }

        private void Awake()
        {
            m_OnPlayerAddedAction = InitializePlayerEvents;
            OfflineGameplayDependencies.gameScene.onPlayerAdded_OnServer += m_OnPlayerAddedAction;
        }

        private void OnDestroy()
        {
            if (m_OnPlayerAddedAction != null)
            {
                OfflineGameplayDependencies.gameScene.onPlayerAdded_OnServer -= m_OnPlayerAddedAction;
                m_OnPlayerAddedAction = null;
            }
        }
    }
}