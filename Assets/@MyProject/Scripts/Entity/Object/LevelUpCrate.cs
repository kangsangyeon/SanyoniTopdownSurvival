using System;
using FishNet.Object;
using MyProject.Struct;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MyProject
{
    public class LevelUpCrate : NetworkBehaviour, IDamageableEntity
    {
        [SerializeField] private GameObject m_Prefab_AbilityItem;
        [SerializeField] private float m_SpawnItemDistance = 1.2f;
        [SerializeField] private int m_SpawnItemCount = 3;

        private EntityHealth m_Health;

        private DamageParam m_LastDamage;
        public DamageParam lastDamage => m_LastDamage;

        private Action m_OnHealthIsZeroAction;

        #region IDamageableEntity

        [SerializeField] private int m_MaxTakableDamage = int.MaxValue;
        public int maxTakableDamage => m_MaxTakableDamage;

        [SerializeField] private bool m_UseConstantDamage = true;
        public bool useConstantDamage => m_UseConstantDamage;

        [SerializeField] private int m_ConstantDamage = -1;
        public int constantDamage => m_ConstantDamage;

        public void TakeDamage(in DamageParam _hitParam, out int _appliedDamage)
        {
            if (m_UseConstantDamage)
                _hitParam.healthModifier.magnitude = m_ConstantDamage;
            else if (_hitParam.healthModifier.magnitude > m_MaxTakableDamage)
                _hitParam.healthModifier.magnitude = m_MaxTakableDamage;

            m_LastDamage = _hitParam;

            m_Health.ApplyModifier(_hitParam.healthModifier);
            _appliedDamage = _hitParam.healthModifier.magnitude;
        }

        #endregion

        private Vector3[] GetSpawnItemPositions(Vector3 _origin, int _count, float _distance)
        {
            float _startRotation = Random.Range(0.0f, 360.0f);

            Vector3[] _positions = new Vector3[_count];
            for (int i = 0; i < _count; ++i)
            {
                float _rotation = (360.0f / _count) * i;
                _rotation = _rotation + _startRotation;
                Vector3 _directionToSpawnPoint = new Vector3(
                    Mathf.Cos(_rotation), 0, Mathf.Sin(_rotation * Mathf.Deg2Rad));
                _positions[i] = _origin + _directionToSpawnPoint * _distance;
            }

            return _positions;
        }

        private void Awake()
        {
            m_Health = GetComponent<EntityHealth>();

            m_OnHealthIsZeroAction = () =>
            {
                Vector3[] _itemPositions =
                    GetSpawnItemPositions(transform.position, m_SpawnItemCount, m_SpawnItemDistance);
                for (int i = 0; i < m_SpawnItemCount; ++i)
                {
                    var _itemGO =
                        GameObject.Instantiate(m_Prefab_AbilityItem, _itemPositions[i], Quaternion.identity);
                    base.ServerManager.Spawn(_itemGO);
                }

                base.ServerManager.Despawn(gameObject);
            };
            m_Health.onHealthIsZero_OnServer += m_OnHealthIsZeroAction;
        }

        private void OnDestroy()
        {
            if (m_OnHealthIsZeroAction != null)
            {
                m_Health.onHealthIsZero_OnServer -= m_OnHealthIsZeroAction;
                m_OnHealthIsZeroAction = null;
            }
        }
    }
}