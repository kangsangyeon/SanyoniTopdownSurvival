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
                ParticleSystem _particle =
                    GameObject.Instantiate(
                        m_Prefab_OnAttackParticle,
                        m_CachedOnAttackParticleSpawnPosition,
                        m_CachedOnAttackParticleSpawnRotation);
                Destroy(_particle, 2.0f);
            };
            _weapon.onAttack += m_OnAttackAction;

            m_OnAttackHitAction = _param =>
            {
                ParticleSystem _particle =
                    GameObject.Instantiate(
                        m_Prefab_OnAttackHitParticle,
                        _param.hitPoint,
                        _param.hitRotation);
                Destroy(_particle, 2.0f);
            };
            _weapon.onAttackHit += m_OnAttackHitAction;

            m_OnProjectileHitAction = _param =>
            {
                ParticleSystem _particle =
                    GameObject.Instantiate(
                        m_Prefab_OnProjectileHitParticle,
                        _param.hitPoint,
                        _param.hitRotation);
                Destroy(_particle, 2.0f);
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

        private void Start()
        {
            m_Player = GetComponentInParent<Player>(); // for test
            if (m_Player == null)
                return;

            if (player.weapon is Weapon_MagicSword _magicSword)
                InitializeMagicSword(_magicSword);

            player.onWeaponChanged_OnClient += _prevWeaponId =>
            {
                if (_prevWeaponId.HasValue)
                {
                    IWeapon _prevWeapon = InstanceFinder.ClientManager.Objects.Spawned[_prevWeaponId.Value] as IWeapon;

                    if (_prevWeapon is Weapon_MagicSword _prevMagicSword)
                        UninitializeMagicSword(_prevMagicSword);
                }

                if (player.weapon is Weapon_MagicSword _magicSword)
                    InitializeMagicSword(_magicSword);
            };
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