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

        private Action<Player> m_OnPlayerAddedAction;

        private Vector3 GetSpawnPosition(Vector3 _origin, float _distance)
        {
            float _rotation = Random.Range(0.0f, 360.0f);
            Vector3 _directionToSpawnPoint = new Vector3(
                Mathf.Cos(_rotation * Mathf.Deg2Rad), 0, Mathf.Sin(_rotation * Mathf.Deg2Rad));
            Vector3 _position = _origin + _directionToSpawnPoint * _distance;
            return _position;
        }

        private void OnEnable()
        {
            m_OnPlayerAddedAction = _player =>
            {
                _player.GetComponent<ILevelable>().onCurrentExperienceOrLevelChanged += _param =>
                {
                    if (_param.becomeNewLevel)
                    {
                        // 레벨업 상자를 스폰합니다.
                        var _go = GameObject.Instantiate(
                            m_Prefab_LevelUpCrate,
                            GetSpawnPosition(_player.transform.position, m_SpawnDistance),
                            Quaternion.identity);

                        InstanceFinder.ServerManager.Spawn(_go);
                    }
                };
            };
            OfflineGameplayDependencies.gameScene.onPlayerAdded_OnServer += m_OnPlayerAddedAction;
        }

        private void OnDisable()
        {
            if (m_OnPlayerAddedAction != null)
            {
                OfflineGameplayDependencies.gameScene.onPlayerAdded_OnServer -= m_OnPlayerAddedAction;
                m_OnPlayerAddedAction = null;
            }
        }
    }
}