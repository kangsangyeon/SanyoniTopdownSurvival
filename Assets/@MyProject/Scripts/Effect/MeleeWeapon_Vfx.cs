using System;
using FishNet;
using UnityEngine;

namespace MyProject
{
    public class MeleeWeapon_Vfx : MonoBehaviour
    {
        /* prefab reference */
        [SerializeField] private ParticleSystem m_Prefab_OnAttackParticle;
        [SerializeField] private ParticleSystem m_Prefab_OnAttackHitParticle;

        /* in this prefab reference */
        [SerializeField] private Transform m_OnAttackParticleSpawnPoint;

        private Player m_Player;
        public Player player => m_Player;

        private Action<IWeapon_OnAttack_EventParam> m_OnAttackAction;
        private Action<IMeleeWeapon_OnAttackHit_EventParam> m_OnAttackHitAction;

        private Vector3 m_CachedOnAttackParticleSpawnPosition;
        private Quaternion m_CachedOnAttackParticleSpawnRotation;

        private void InitializeMeleeWeapon(IMeleeWeapon _meleeWeapon)
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
            _meleeWeapon.onAttack += m_OnAttackAction;

            m_OnAttackHitAction = _param =>
            {
                ParticleSystem _particle =
                    GameObject.Instantiate(
                        m_Prefab_OnAttackHitParticle,
                        _param.hitPoint,
                        _param.hitRotation);
                Destroy(_particle, 2.0f);
            };
            _meleeWeapon.onAttackHit += m_OnAttackHitAction;
        }

        private void UninitializeMeleeWeapon(IMeleeWeapon _meleeWeapon)
        {
            _meleeWeapon.onAttack -= m_OnAttackAction;
            m_OnAttackAction = null;

            _meleeWeapon.onAttackHit -= m_OnAttackHitAction;
            m_OnAttackHitAction = null;
        }

        private void Start()
        {
            m_Player = GetComponentInParent<Player>(); // for test
            if (m_Player == null)
                return;

            if (player.weapon is IMeleeWeapon _meleeWeapon)
                InitializeMeleeWeapon(_meleeWeapon);

            player.onWeaponChanged_OnClient += _prevWeaponId =>
            {
                if (_prevWeaponId.HasValue)
                {
                    IWeapon _prevWeapon = InstanceFinder.ClientManager.Objects.Spawned[_prevWeaponId.Value] as IWeapon;

                    if (_prevWeapon is IMeleeWeapon _prevMeleeWeapon)
                        UninitializeMeleeWeapon(_prevMeleeWeapon);
                }

                if (player.weapon is IMeleeWeapon _meleeWeapon)
                    InitializeMeleeWeapon(_meleeWeapon);
            };
        }

        private void Update()
        {
            m_CachedOnAttackParticleSpawnPosition = m_OnAttackParticleSpawnPoint.position;
            m_CachedOnAttackParticleSpawnRotation = m_OnAttackParticleSpawnPoint.rotation;
        }
    }
}