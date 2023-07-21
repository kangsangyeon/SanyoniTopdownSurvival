using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

namespace MyProject
{
    public class PlayerMovement : NetworkBehaviour
    {
        public struct MoveData : IReplicateData
        {
            public Vector3 movement;
            public bool queueJump;

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
            public Vector3 position;
            public float verticalVelocity;

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

        [SerializeField] private float m_GravityMultiplier = 3f;
        [SerializeField] private float m_MinVerticalVelocity = -20f;
        [SerializeField] private float m_MoveSpeed = 4f;
        [SerializeField] private float m_JumpVelocity = 7f;
        [SerializeField] private Player m_Player;

        private CharacterController m_CharacterController;
        private Vector3 m_Movement;
        private float m_VerticalVelocity;
        private bool m_QueueJump = false;
        private bool m_CanMove = true;
        private bool m_TeleportQueue;
        private Vector3 m_TeleportPosition;

        private void BuildData(out MoveData _moveData)
        {
            _moveData = default;
            _moveData.movement = m_Movement;
            _moveData.queueJump = m_QueueJump;

            m_QueueJump = false;
        }

        [TargetRpc(RunLocally = true)]
        public void Teleport(NetworkConnection _conn, Vector3 _position)
        {
            transform.position = _position;
        }

        [Replicate]
        private void Move(
            MoveData _moveData,
            bool _asServer, Channel _channel = Channel.Unreliable, bool _replaying = false)
        {
            // reconcile이 수행된 후 이전에 캐시된 input이 replay됩니다. (이는 오직 클라이언트에서만 일어납니다.)
            // 지연 시간으로 인해 input이 캐시되지만, 이는 클라이언트가 서버와 동기화 상태를 유지하는 방법이기도 합니다.
            // 클라이언트가 매 틱마다 움직이고, 10, 11, 12, 13 등의 틱에 입력을 전송한다고 가정해 봅시다.
            // 서버는 클라이언트 틱 10에서 첫 번째 작업을 가져와 실행하고 reconcile 데이터를 보냅니다.
            // 클라이언트가 reconcile 데이터를 수신할 때쯤이면 이미 11, 12, 13 등을 서버에 전송한 상태입니다.
            // 이렇게 추가로 전송된 값은 클라이언트 내부에 캐시됩니다.
            // 클라이언트가 reconcile을 수신하면 reconcile 메소드에서 필요한 조정을 수행한 다음 캐시된 모든 작업을 replay합니다.
            // 이 경우 해당 액션은 틱 11, 12, 13 등이 됩니다.
            // 이것이 클라이언트 액션이 클라이언트에서 거의 확실하게 여러 번 수행되는 이유입니다.
            // 로컬에서 처리될 때 한 번, 그리고 replay 될 때마다 다시 한 번 수행됩니다.

            float _delta = (float)base.TimeManager.TickDelta;

            Vector3 _movement = _moveData.movement * m_MoveSpeed;

            if (_moveData.queueJump && m_CharacterController.isGrounded)
            {
                // 땅에 붙어있을 때 점프를 하여 vertical velocity를 위로 올릴 수 있습니다.
                m_VerticalVelocity = m_JumpVelocity;
            }
            // else if (m_CharacterController.isGrounded && m_VerticalVelocity < 0.0f)
            // {
            //     // 땅에 붙어있을 때 vertical velocity를 -1로 고정합니다.
            //     // 점프한 프레임에는 이 처리를 실시하지 않습니다.
            //     m_VerticalVelocity = -1.0f;
            // }

            // vertical velocity를 적용합니다.

            m_VerticalVelocity = m_VerticalVelocity + Physics.gravity.y * m_GravityMultiplier * _delta;
            m_VerticalVelocity = Mathf.Max(m_MinVerticalVelocity, m_VerticalVelocity);

            _movement = _movement + Vector3.up * m_VerticalVelocity;

            m_CharacterController.Move(_movement * _delta);
        }

        [Reconcile]
        private void Reconcile(
            ReconcileData _reconcileData,
            bool _asServer, Channel _channel = Channel.Unreliable)
        {
            transform.position = _reconcileData.position;
            m_VerticalVelocity = _reconcileData.verticalVelocity;
        }

        private void Awake()
        {
            m_CharacterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            m_CanMove = true;

            m_Player.health.onHealthIsZero_OnSync += () => m_CanMove = false;

            m_Player.health.onHealthChanged_OnSync += _amount =>
            {
                if (m_Player.health.health > 0)
                    m_CanMove = true;
            };
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            if (base.Owner.IsLocalClient || base.IsServer)
                base.TimeManager.OnTick += TimeManager_OnTick;

            if (base.IsServer)
                base.TimeManager.OnPostTick += TimeManager_OnPostTick;
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();

            if (base.Owner.IsLocalClient || base.IsServer)
                base.TimeManager.OnTick -= TimeManager_OnTick;

            if (base.IsServer)
                base.TimeManager.OnPostTick -= TimeManager_OnPostTick;
        }

        private void TimeManager_OnTick()
        {
            if (base.IsOwner)
            {
                // 소유자 클라이언트에서 첫 번째 매개변수로 default를,
                // 두 번째 매개변수로 false를 건네주며 Reconcile를 호출해야 합니다.
                // 이렇게 해야 새 action을 처리하기 전에 발생했을 수 있는 동기화 어긋남을 reconcile할 수 있습니다.
                // 첫 번째 매개변수는 fishnet에 의해 서버에서 받아온 값이 자동으로 대체되어 건네주어질 것이기 때문에 default로 작성합니다.
                // 두 번째 매개변수는 이 메소드가 클라이언트에서 호출되었다는 것을 지시합니다.
                Reconcile(default, false);

                BuildData(out MoveData _moveData);

                Move(_moveData, false);
            }

            if (base.IsServer)
            {
                // 서버에서 첫 번째 매개변수로 default를, 
                // 두 번째 매개변수로 true를 건네주며 Move를 호출해야 합니다.
                // 첫 번째 매개변수는 fishnet에 의해 클라이언트에서 받아온 값이 자동으로 대체되어 건네주어질 것이기 때문에 default로 작성합니다.
                Move(default, true);
            }
        }

        private void TimeManager_OnPostTick()
        {
            if (base.IsServer)
            {
                ReconcileData _reconcileData = new ReconcileData()
                {
                    position = transform.position,
                    verticalVelocity = m_VerticalVelocity
                };
                Reconcile(_reconcileData, true);
            }
        }


        private void Update()
        {
            if (base.IsOwner == false)
                return;

            if (m_CanMove)
            {
                m_Movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
                if (m_Movement.magnitude >= 1.0f)
                    m_Movement.Normalize();

                if (Input.GetKeyDown(KeyCode.Space))
                    m_QueueJump = true;
            }
            else
            {
                m_Movement = Vector3.zero;
            }
        }
    }
}