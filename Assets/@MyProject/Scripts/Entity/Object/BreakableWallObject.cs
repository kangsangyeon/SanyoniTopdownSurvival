using FishNet.Object;
using MyProject.Struct;
using UnityEngine;

namespace MyProject
{
    [RequireComponent(typeof(EntityHealth))]
    public class BreakableWallObject : NetworkBehaviour, IWallObject, IDamageableEntity
    {
        [SerializeField] private LayerMask m_IgnorePlayerMask;
        [SerializeField] private Collider m_IgnorePlayerCollider;
        private Collider m_Collider;
        private Rigidbody m_RigidBody;

        private DamageParam m_LastDamage;

        #region IWallObject

        private EntityHealth m_Health;
        public EntityHealth health => m_Health;

        public bool isInvincible => false;

        private bool m_IsInvincibleTemporary = false;
        public bool isInvincibleTemporary => m_IsInvincibleTemporary;

        #endregion

        #region IDamageableEntity

        public void TakeDamage(in DamageParam _hitParam, out int _appliedDamage)
        {
            m_Health.ApplyModifier(_hitParam.healthModifier);
            _appliedDamage = _hitParam.healthModifier.magnitude;

            m_LastDamage = _hitParam;
        }

        #endregion

        [Server]
        private void Server_OnHealthIsZero(in DamageParam _damageParam)
        {
            // 상자를 부숩니다.
            Break(_damageParam);

            // 모든 이들에게 전파합니다.
            ObserversRpc_Break(_damageParam);
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_Break(DamageParam _damageParam)
        {
            // 상자를 부숩니다.
            Break(_damageParam);
        }

        private void Break(in DamageParam _damageParam)
        {
            m_Collider.enabled = false;
            m_IgnorePlayerCollider.enabled = true;
            m_RigidBody.AddForceAtPosition(_damageParam.direction * _damageParam.force, _damageParam.point, ForceMode.Impulse); // 3d 폭발인 경우 AddExplosionForce 함수 사용
        }

        private void Awake()
        {
            m_Collider = GetComponent<Collider>();
            m_RigidBody = GetComponent<Rigidbody>();

            m_Health = GetComponent<EntityHealth>();
            m_Health.onHealthIsZero_OnServer += () => { Server_OnHealthIsZero(m_LastDamage); };
        }
    }
}