using FishNet.Connection;
using FishNet.Managing.Logging;
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
            public void SetTick(uint _value) => tick = _value;
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
            public void SetTick(uint _value) => tick = _value;
        }

        [SerializeField] private LayerMask m_CharacterLayer;
        [SerializeField] private LayerMask m_IgnoreCollisionCharacterLayer;
        [SerializeField] private LayerMask m_BlockMovementLayer;
        [SerializeField] private float m_GravityMultiplier = 3f;


        [SerializeField] private float m_MoveSpeed = 4f;
        [SerializeField] private float m_JumpVelocity = 7f;
        [SerializeField] private Player m_Player;

        private CharacterController m_CharacterController;
        public CharacterController characterController => m_CharacterController;

        private Vector3 m_Movement;
        private float m_VerticalVelocity;
        private bool m_QueueJump = false; // client only
        private bool m_CanMove = true;
        private bool m_TeleportQueue; // client only
        private Vector3 m_TeleportPosition;

        private bool m_IsGrounded;

        private float m_DefaultStepOffset;

        [Client]
        private void UpdateQueueJump()
        {
            if (Input.GetKeyDown(KeyCode.Space) == false)
                return;

            if (CanJump() == false)
                return;

            m_QueueJump = true;
        }

        private void UpdateIsGrounded(bool _replaying, out bool _isChanged)
        {
            bool _previousGrounded = m_IsGrounded;
            m_IsGrounded = CastForGround();
            _isChanged = (_previousGrounded != m_IsGrounded);
        }

        /// <summary>
        /// 지면에 붙어있을 때 velocity를 0으로 설정합니다.
        /// </summary>
        private void UpdateGroundedVelocity(float _deltaTime, bool _asServer, bool _replaying)
        {
            /*
             * 갑자기 땅을 뚫고 사라지지 않도록 지면에 붙어있을 때 빠르게 gravity를 감소시킵니다.
             * 또한 시간이 지남에 따라 이 gravity amount를 향해 이동하여 착지 시 중력이 즉시 초기화되지 않고
             * 서서히 추진력을 잃는 듯한 느낌을 주도록 합니다.
             */
            if (m_CharacterController.isGrounded && m_VerticalVelocity < -1f)
                m_VerticalVelocity = Mathf.MoveTowards(m_VerticalVelocity, -1f,
                    (-Physics.gravity.y * m_GravityMultiplier * 2f) * _deltaTime);
        }

        /// <summary>
        /// 상황에 따라 step height를 변경합니다.
        /// </summary>
        private void UpdateStepOffset()
        {
            /*
             * 공중에 있을 때 stepping을 허용하지 않습니다.
             * 이는 떨어지고 있는 상태에서 플레이어보다 약간 위쪽에 있는 지면을 올라가지 못하게 막기 위함입니다.
             * 이는 unity character controller의 문제이며 ledge grabbing에는 좋을 지 모르겟지만, 일반적인 게임에서는 그렇지 않습니다.
             */
            m_CharacterController.stepOffset = m_IsGrounded && m_VerticalVelocity <= 0f ? m_DefaultStepOffset : 0f;
        }

        /// <summary>
        /// 캐릭터의 레이어를 설정합니다.
        /// 이는 캐릭터의 히트박스가 어떤 레이어 오브젝트와의 충돌을 허용할 것인지 설정합니다.
        /// </summary>
        /// <param name="_toIgnoreCollision">true를 건네주면 character간 충돌을 중단합니다.</param>
        private void SetCharacterLayer(bool _toIgnoreCollision)
        {
            int _layer = _toIgnoreCollision
                ? LayerMaskHelper.LayerMaskToLayerNumber(m_IgnoreCollisionCharacterLayer)
                : LayerMaskHelper.LayerMaskToLayerNumber(m_CharacterLayer);

            gameObject.layer = _layer;
        }

        /// <summary>
        /// 점프할 수 있는지에 대한 여부를 반환합니다.
        /// </summary>
        private bool CanJump()
        {
            if (base.IsOwner)
            {
                // 클라이언트에서는 이 함수를 m_QueueJump를 활성화할 수 있는지 여부를 판단하기 위해 사용합니다.
                // 클라이언트 측에서 이미 jump가 queue되었다는 것은 이미 CanJump로 점프 가능 여부를 확인하고 jump를 예약했기 때문에
                // 더 이상 확인할 필요가 없다는 것을 의미합니다.
                if (m_QueueJump)
                    return false;
            }

            if (m_IsGrounded == false)
            {
                // 플레이어가 지면에 붙어있을 때에만 점프할 수 있습니다.
                return false;
            }

            // /* If owner require exact jump intervals, if server allow a little leanancy.
            //  * When server allow to jump 150ms sooner to compensate for slower packets. */
            // float nextAllowedJumpTime = (base.IsServer && !base.IsOwner) ? _nextAllowedJumpTime - 0.15f : _nextAllowedJumpTime;
            // if (Time.time < nextAllowedJumpTime)
            //     return false;

            return true;
        }


        /// <summary>
        /// 플레이어의 아래쪽으로 cast하여 지면을 밣고있는지 확인합니다.
        /// </summary>
        /// <param name="_extraDistance"></param>
        private bool CastForGround(float _extraDistance = 0.05f)
        {
            float _radius = m_CharacterController.radius + (m_CharacterController.skinWidth / 2f);
            Vector3 _start = m_CharacterController.bounds.center;
            float _distance = (m_CharacterController.height / 2f) - (_radius / 2f);

            // 지면을 향해 레이캐스트를 실시합니다.
            // 실시 전 나 자신은 레이캐스트 대상에서 제외합니다.
            SetCharacterLayer(true);

            Ray _ray = new Ray(_start, Vector3.down);
            RaycastHit _hit;

            bool _isGrounded =
                Physics.SphereCast(_ray, _radius, out _hit, _distance + _extraDistance,
                    m_BlockMovementLayer | m_CharacterLayer);

            SetCharacterLayer(false);
            return _isGrounded;
        }

        /// <summary>
        /// 플레이어가 movement 방향으로 이동할 수 없는지에 대한 여부를 반환합니다.
        /// 컨트롤러가 허용해서는 안 되는 경로로 이동할 때 true일 수 있습니다.
        /// </summary>
        /// <param name="_input"></param>
        /// <returns></returns>
        private bool CastMovementIsBlocked(in MoveData _input, float _moveRate, float _deltaTime)
        {
            /*
             * 경사가 너무 가파르지 않은지 확인합니다. character controller가 망가져 평소에는 올라갈 수 없는 경사면을 올라갈 수도 있습니다.
             * 점프할 때 경사가 너무 가파르거나 지면과 닿지 않고 있는 경우에만 확인합니다.
             */
            if (m_IsGrounded == false || m_VerticalVelocity > 0f)
            {
                // Start in the center of the character controller.
                Vector3 _start = transform.position + new Vector3(0f, m_CharacterController.height / 2f, 0f);
                Vector3 _estimatedImpact =
                    transform.position
                    + (_input.movement * (m_CharacterController.radius + m_CharacterController.skinWidth + _moveRate) *
                       _deltaTime);
                float _distance = (_start - _estimatedImpact).magnitude;
                Vector3 _direction = (_estimatedImpact - _start).normalized;
                Ray _ray = new Ray(_start, _direction);
                RaycastHit _hit;

                if (Physics.Raycast(_ray, out _hit, _distance, m_BlockMovementLayer))
                {
                    float _angle = Vector3.Angle(_hit.normal, Vector3.up);
                    if (_angle > m_CharacterController.slopeLimit)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Applies gravity to verticalVelocity.
        /// </summary>
        private void ApplyGravity(ref float _verticalVelocity, float _deltaTime)
        {
            //Multiply gravity by 2 for snappier jumps.
            _verticalVelocity += (Physics.gravity.y * m_GravityMultiplier) * _deltaTime;
            _verticalVelocity = Mathf.Max(_verticalVelocity, Physics.gravity.y * m_GravityMultiplier);
        }

        /// <summary>
        /// 점프합니다.
        /// </summary>
        private void Jump(bool _replaying)
        {
            m_VerticalVelocity = m_JumpVelocity;

            // replay중이 아닐 때에만 time을 설정하고 queue jump를 비활성화합니다.
            if (!_replaying)
            {
                m_QueueJump = false;
                // _nextAllowedJumpTime = Time.time + _jumpInterval;
                // _animatorController.Jump();
            }
        }

        private void BuildData(out MoveData _moveData)
        {
            _moveData = default;
            _moveData.movement = m_Movement;
            _moveData.queueJump = m_QueueJump && m_IsGrounded;

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
            bool _isDefaultData = _moveData.Equals(default(MoveData));

            UpdateIsGrounded(_replaying, out bool _isGroundedChanged);
            ApplyGravity(ref m_VerticalVelocity, _delta);
            UpdateGroundedVelocity(_delta, _asServer, _replaying);
            UpdateStepOffset();

            // /* 서버이거나 replay가 아닐 때에만 회전을 갱신합니다.
            //  * 클라이언트가 입력을 replay하는 동안 회전을 갱신하면 동기화가 쉽게 깨져 camera가 jittering됩니다.
            //  * 대신 캐릭터는 world position을 사용하여 움직이고 캐릭터는 회전을 유지합니다.
            //  *
            //  * 또한 데이터가 기본값인 경우에는 적용하지 않습니다.
            //  * 클라이언트의 회전을 계속 Vector3.Zero로 재설정하기 때문입니다. */
            // if (!_isDefaultData && (_asServer || !_replaying))
            //     transform.eulerAngles = new Vector3(transform.eulerAngles.x, input.Rotation, transform.eulerAngles.z);

            if (_moveData.queueJump)
            {
                if (_asServer)
                {
                    // 서버에서 점프할 수 있는지에 대한 여부를 확인한 뒤 점프합니다.
                    if (CanJump())
                    {
                        Jump(_replaying);
                    }
                }
                else
                {
                    // 이미 클라이언트 유효성 검사를 마쳤기 때문에, 검사하지 않고 그냥 점프합니다.
                    Jump(_replaying);
                }
            }

            if (!_replaying && _moveData.movement != Vector3.zero)
            {
                if (CastMovementIsBlocked(_moveData, m_MoveSpeed, _delta))
                    _moveData.movement = Vector3.zero;
            }

            Vector3 _movement = _moveData.movement * m_MoveSpeed;
            _movement.y = _movement.y + m_VerticalVelocity;
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
            m_DefaultStepOffset = m_CharacterController.stepOffset;

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
                Transform _camTransform = Camera.main.transform;
                Vector3 _camForwardXZ = new Vector3(_camTransform.forward.x, 0, _camTransform.forward.z).normalized;
                Vector3 _camRightXZ = new Vector3(_camTransform.right.x, 0, _camTransform.right.z).normalized;
                
                m_Movement =
                    _camForwardXZ * Input.GetAxisRaw("Vertical")
                    + _camRightXZ * Input.GetAxisRaw("Horizontal");
                
                if (m_Movement.magnitude >= 1.0f)
                    m_Movement.Normalize();

                UpdateQueueJump();
            }
            else
            {
                m_Movement = Vector3.zero;
            }
        }
    }
}