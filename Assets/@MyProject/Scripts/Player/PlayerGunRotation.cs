using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

namespace MyProject
{
    public class PlayerGunRotation : NetworkBehaviour
    {
        public struct RotationData : IReplicateData
        {
            public float angle;

            /* Everything below this is required for
            * the interface. You do not need to implement
            * Dispose, it is there if you want to clean up anything
            * that may allocate when this structure is discarded. */
            private uint tick;

            public void Dispose()
            {
            }

            public uint GetTick() => tick;
            public void SetTick(uint value) => tick = value;
        }

        public struct ReconcileData : IReconcileData
        {
            public float angle;

            /* Everything below this is required for
            * the interface. You do not need to implement
            * Dispose, it is there if you want to clean up anything
            * that may allocate when this structure is discarded. */
            private uint tick;

            public void Dispose()
            {
            }

            public uint GetTick() => tick;
            public void SetTick(uint value) => tick = value;
        }

        [SerializeField] private Player m_Player;
        public Player player => m_Player;

        private float m_Angle;

        private void BuildData(out RotationData _moveData)
        {
            _moveData = default;
            _moveData.angle = m_Angle;
        }

        [Replicate]
        private void Rotate(
            RotationData _rotationData,
            bool _asServer, Channel _channel = Channel.Unreliable, bool _replaying = false)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, _rotationData.angle, 0));
        }

        [Reconcile]
        private void Reconcile(
            ReconcileData _reconcileData,
            bool _asServer, Channel _channel = Channel.Unreliable)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, _reconcileData.angle, 0));
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            // if (base.Owner.IsLocalClient || base.IsServer)
            base.TimeManager.OnTick += TimeManager_OnTick;
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();

            // if (base.Owner.IsLocalClient || base.IsServer)
            base.TimeManager.OnTick -= TimeManager_OnTick;
        }

        private void TimeManager_OnTick()
        {
            if (base.IsOwner)
            {
                Reconcile(default, false);

                BuildData(out RotationData _rotationData);

                Rotate(_rotationData, false);
            }

            if (base.IsServer)
            {
                Rotate(default, true);

                float _angle = Vector2.SignedAngle(transform.up, Vector2.up);
                // if (_angle < 0)
                //     _angle += 360f;

                ReconcileData _reconcileData = new ReconcileData() { angle = _angle };
                Reconcile(_reconcileData, true);
            }
        }

        private void Update()
        {
            if (base.IsOwner == false)
                return;

            if (Application.isFocused == false)
                return;

            Vector3 _position = transform.position;
            Vector3 _mousePositionWorld = _position;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                _mousePositionWorld = hit.point;
            }

            if (_position != _mousePositionWorld)
            {
                Vector3 _positionDiff = _mousePositionWorld - _position;
                _positionDiff.y = 0.0f;
                m_Angle = Mathf.Atan2(_positionDiff.z, _positionDiff.x) * Mathf.Rad2Deg * -1;
                transform.rotation = Quaternion.Euler(new Vector3(0, m_Angle, 0));
            }
        }
    }
}