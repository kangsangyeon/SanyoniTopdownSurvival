using FishNet.Object;
using UnityEngine;

namespace MyProject
{
    [RequireComponent(typeof(EntityHealth))]
    public class BreakableWallObject : NetworkBehaviour, IWallObject, IDamageableEntity
    {
        private Collider2D m_Collider;
        private Rigidbody2D m_RigidBody;

        #region IWallObject

        private EntityHealth m_Health;
        public EntityHealth health => m_Health;

        public bool isInvincible => false;

        private bool m_IsInvincibleTemporary = false;
        public bool isInvincibleTemporary => m_IsInvincibleTemporary;

        #endregion

        #region IDamageableEntity

        public void TakeDamage(int _magnitude, object _source, float _time, out int _appliedDamage)
        {
            m_Health.ApplyModifier(new HealthModifier() { magnitude = _magnitude, source = _source, time = _time });
            _appliedDamage = _magnitude;
        }

        #endregion

        [Server]
        public void Server_OnBreak()
        {
            // 상자를 부숩니다.
            // Break();

            // 모든 이들에게 전파합니다.
            // ObserversRpc_OnBreak();
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnBreak(Vector2 _point, Vector2 _forceDirection, float _forceAmount)
        {
            // 상자를 부숩니다.
            // Break();
        }

        private void Break(Vector2 _point, Vector2 _forceDirection, float _forceAmount)
        {
            m_Collider.enabled = false;
            m_RigidBody.AddForce(_forceDirection * _forceAmount, ForceMode2D.Impulse); // 3d인 경우 AddExplosionForce 함수 사용
        }

        private void Awake()
        {
            m_Health = GetComponent<EntityHealth>();
            m_Health.onHealthIsZero_OnServer += Server_OnBreak;
        }
    }
}