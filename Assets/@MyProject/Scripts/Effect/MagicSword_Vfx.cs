using System;
using FishNet;
using MyProject.Event;
using UnityEngine;

namespace MyProject
{
    public class MagicSword_Vfx : MonoBehaviour
    {
        /* prefab reference */
        [SerializeField] private ParticleSystem m_Prefab_OnAttackParticle;
        [SerializeField] private ParticleSystem m_Prefab_OnAttackHitParticle;
        [SerializeField] private ParticleSystem m_Prefab_OnProjectileHitParticle;

        /* in this prefab reference */
        [SerializeField] private Transform m_OnAttackParticleSpawnPoint;

        private Player m_Player;
        public Player player => m_Player;

        private Action<IWeapon_OnAttack_EventParam> m_OnAttackAction;
        private Action<IMeleeWeapon_OnAttackHit_EventParam> m_OnAttackHitAction;
        private Action<Weapon_MagicSword_OnProjectileHit_EventParam> m_OnProjectileHitAction;

        private Vector3 m_CachedOnAttackParticleSpawnPosition;
        private Quaternion m_CachedOnAttackParticleSpawnRotation;

        private void InitializeMagicSword(Weapon_MagicSword _weapon)
        {
            m_OnAttackAction = _param =>
            {
                GameObject _particle = OfflineGameplayDependencies.objectPoolManager
                    .Get(m_Prefab_OnAttackParticle.gameObject);
                _particle.transform.SetPositionAndRotation(
                    m_CachedOnAttackParticleSpawnPosition,
                    m_CachedOnAttackParticleSpawnRotation);
                OfflineGameplayDependencies.objectPoolManager
                    .Release(m_Prefab_OnAttackParticle.gameObject, _particle, 2.0f);
            };
            _weapon.onAttack += m_OnAttackAction;

            m_OnAttackHitAction = _param =>
            {
                GameObject _particle = OfflineGameplayDependencies.objectPoolManager
                    .Get(m_Prefab_OnAttackHitParticle.gameObject);
                _particle.transform.SetPositionAndRotation(
                    _param.hitPoint,
                    _param.hitRotation);
                OfflineGameplayDependencies.objectPoolManager
                    .Release(m_Prefab_OnAttackHitParticle.gameObject, _particle, 2.0f);
            };
            _weapon.onAttackHit += m_OnAttackHitAction;

            m_OnProjectileHitAction = _param =>
            {
                GameObject _particle = OfflineGameplayDependencies.objectPoolManager
                    .Get(m_Prefab_OnProjectileHitParticle.gameObject);
                _particle.transform.SetPositionAndRotation(
                    _param.hitPoint,
                    _param.hitRotation);
                OfflineGameplayDependencies.objectPoolManager
                    .Release(m_Prefab_OnProjectileHitParticle.gameObject, _particle, 2.0f);
            };
            _weapon.onProjectileHit += m_OnProjectileHitAction;
        }

        private void UninitializeMagicSword(Weapon_MagicSword _weapon)
        {
            _weapon.onAttack -= m_OnAttackAction;
            m_OnAttackAction = null;

            _weapon.onAttackHit -= m_OnAttackHitAction;
            m_OnAttackHitAction = null;

            _weapon.onProjectileHit -= m_OnProjectileHitAction;
            m_OnProjectileHitAction = null;
        }

        private void RegisterPrefab()
        {
            OfflineGameplayDependencies.objectPoolManager.Register(
                m_Prefab_OnAttackParticle.gameObject, 30,
                () =>
                {
                    var _particle = GameObject.Instantiate(m_Prefab_OnAttackParticle);
                    _particle.gameObject.SetActive(false);
                    return _particle.gameObject;
                },
                null,
                (_go) => { _go.SetActive(true); },
                (_go) => { _go.SetActive(false); });
            OfflineGameplayDependencies.objectPoolManager.Register(
                m_Prefab_OnAttackHitParticle.gameObject, 30,
                () =>
                {
                    var _particle = GameObject.Instantiate(m_Prefab_OnAttackHitParticle);
                    _particle.gameObject.SetActive(false);
                    return _particle.gameObject;
                },
                null,
                (_go) => { _go.SetActive(true); },
                (_go) => { _go.SetActive(false); });
            OfflineGameplayDependencies.objectPoolManager.Register(
                m_Prefab_OnProjectileHitParticle.gameObject, 30,
                () =>
                {
                    var _particle = GameObject.Instantiate(m_Prefab_OnAttackParticle);
                    _particle.gameObject.SetActive(false);
                    return _particle.gameObject;
                },
                null,
                (_go) => { _go.SetActive(true); },
                (_go) => { _go.SetActive(false); });
        }

        private void Awake()
        {
            m_Player = GetComponentInParent<Player>(); // for test
            if (m_Player == null)
                return;

            if (player.weapon is Weapon_MagicSword _magicSword)
                InitializeMagicSword(_magicSword);

            player.onWeaponChanged_OnClient += (_prevWeapon, _currentWeapon) =>
            {
                if (_prevWeapon != null)
                {
                    if (_prevWeapon is Weapon_MagicSword _prevMagicSword)
                        UninitializeMagicSword(_prevMagicSword);
                }

                if (player.weapon is Weapon_MagicSword _magicSword)
                    InitializeMagicSword(_magicSword);
            };

            RegisterPrefab();
        }

        private void Update()
        {
            // weapon.onAttack이 호출되는 시점에 transform이 이상하게 변하는 문제가 있기 때문에
            // 이렇게 transform을 매 프레임마다 캐싱하여 사용해야 합니다.
            // 아마 FishNet의 구현 때문에 이런 문제가 발생하는 것으로 추정됩니다.
            m_CachedOnAttackParticleSpawnPosition = m_OnAttackParticleSpawnPoint.position;
            m_CachedOnAttackParticleSpawnRotation = m_OnAttackParticleSpawnPoint.rotation;
        }
    }
}