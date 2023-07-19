using FishNet.Object;
using MyProject.Struct;
using UnityEngine;

namespace MyProject
{
    public class RebuildingWallObject : NetworkBehaviour, IRebuildingWallObject, IDamageableEntity
    {
        private DamageParam m_LastDamage;
        private MeshRenderer[] m_MeshRendererArray;
        private Collider[] m_ColliderArray;
        private Rigidbody[] m_RigidbodyArray;

        #region IRebuildingWallObject

        private EntityHealth m_Health;
        public EntityHealth health => m_Health;

        public bool isInvincible => false;

        private bool m_IsInvincibleTemporary;
        public bool isInvincibleTemporary => m_IsInvincibleTemporary;

        [SerializeField] private float m_RebuildingDelay;
        public float rebuildingDelay => m_RebuildingDelay;

        [SerializeField] private float m_InvincibleTimeWhenRebuilding;
        public float invincibleTimeWhenRebuilding => m_InvincibleTimeWhenRebuilding;

        private Vector3 m_RebuildingPosition;
        public Vector3 rebuildingPosition => m_RebuildingPosition;

        private Quaternion m_RebuildingRotation;
        public Quaternion rebuildingRotation => m_RebuildingRotation;

        private Vector3 m_RebuildingScale;
        public Vector3 rebuildingScale => m_RebuildingScale;

        #endregion

        #region IDamageableEntity;

        public void TakeDamage(in DamageParam _hitParam, out int _appliedDamage)
        {
            m_LastDamage = _hitParam;

            m_Health.ApplyModifier(_hitParam.healthModifier);
            _appliedDamage = _hitParam.healthModifier.magnitude;
        }

        #endregion

        [Server]
        private void Server_OnHealthIsZero(in DamageParam _damageParam)
        {
            Destruct(_damageParam);
            ObserversRpc_Destruct(_damageParam);

            this.Invoke(() =>
            {
                Rebuild();
                ObserversRpc_Rebuild();
            }, m_RebuildingDelay);
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_Destruct(DamageParam _damageParam)
        {
            Destruct(_damageParam);
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_Rebuild()
        {
            Rebuild();
        }

        private void Destruct(in DamageParam _damageParam)
        {
            foreach (var _meshRenderer in m_MeshRendererArray)
                _meshRenderer.enabled = false;

            foreach (var _collider in m_ColliderArray)
                _collider.enabled = false;

            foreach (var _rigidbody in m_RigidbodyArray)
                _rigidbody.isKinematic = true;
        }

        private void Rebuild()
        {
            foreach (var _meshRenderer in m_MeshRendererArray)
                _meshRenderer.enabled = true;

            foreach (var _collider in m_ColliderArray)
                _collider.enabled = true;

            foreach (var _rigidbody in m_RigidbodyArray)
                _rigidbody.isKinematic = false;

            m_Health.ApplyModifier(new HealthModifier()
            {
                magnitude = 99999,
                source = this,
                time = Time.time
            });

            transform.position = m_RebuildingPosition;
            transform.rotation = m_RebuildingRotation;
            transform.localScale = m_RebuildingScale;
        }

        private void Awake()
        {
            m_MeshRendererArray = GetComponentsInChildren<MeshRenderer>();
            m_ColliderArray = GetComponentsInChildren<Collider>();
            m_RigidbodyArray = GetComponentsInChildren<Rigidbody>();

            m_Health = GetComponent<EntityHealth>();
            m_RebuildingPosition = transform.position;
            m_RebuildingRotation = transform.rotation;
            m_RebuildingScale = transform.localScale;

            m_Health.onHealthIsZero_OnServer += () => { Server_OnHealthIsZero(m_LastDamage); };
        }
    }
}