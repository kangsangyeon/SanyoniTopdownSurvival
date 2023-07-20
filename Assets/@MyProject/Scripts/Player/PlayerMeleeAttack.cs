using System;
using FishNet.Object;
using MyProject.Event;
using MyProject.Struct;
using UnityEngine;

namespace MyProject
{
    public class PlayerMeleeAttack : NetworkBehaviour, IMeleeWeapon
    {
        /// <summary>
        /// 발사체가 가질 수 있는 최대 경과 시간입니다.
        /// </summary>
        private const float MAX_PASSED_TIME = 0.3f;

        [SerializeField] private Collider m_AttackRangeCollider;

        private Player m_Player;

        public Player player
        {
            get => m_Player;
            set => m_Player = value;
        }

        private float m_LastAttackTime;
        private bool m_AttackQueue;
        private Vector3 m_AttackQueuePosition;
        private float m_AttackQueueRotationY;
        private bool m_CanAttack;

        #region IMeleeWeapon

        public object owner => player;

        public int ownerConnectionId => player.OwnerId;

        public int attackDamageMagnitude => Mathf.RoundToInt(
            player.abilityProperty.meleeAttackDamageMagnitude
            * player.abilityProperty.meleeAttackDamageMagnitudeMultiplier);

        public float attackDelay =>
            player.abilityProperty.meleeAttackDelay * player.abilityProperty.meleeAttackDelayMultiplier;

        public event Action<IWeapon_OnAttack_EventParam> onAttack;

        #endregion

        #region Events

        public event System.Action<PlayerMeleeAttack_OnAttackHit_EventParam> onAttackHit_OnClient;

        [Server]
        private void Server_OnAttackHit(Vector3 _hitPoint, Vector3 _hitDirection)
        {
            onAttackHit_OnClient?.Invoke(new PlayerMeleeAttack_OnAttackHit_EventParam()
                { hitPoint = _hitPoint, hitDirection = _hitDirection });
            ObserversRpc_OnAttackHit(new PlayerMeleeAttack_OnAttackHit_EventParam()
                { hitPoint = _hitPoint, hitDirection = _hitDirection });
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnAttackHit(PlayerMeleeAttack_OnAttackHit_EventParam _param)
        {
            onAttackHit_OnClient?.Invoke(_param);
        }

        #endregion

        [Client]
        private void Client_Attack()
        {
            m_AttackQueue = false;

            float _elapsedTimeSinceLastFire = Time.time - m_LastAttackTime;

            // 가장 마지막으로 발사한 뒤 일정 시간 뒤에 다시 발사할 수 있습니다.
            bool _canShoot = _elapsedTimeSinceLastFire >= attackDelay;

            if (_canShoot)
            {
                m_LastAttackTime = Time.time;
                onAttack?.Invoke(new IWeapon_OnAttack_EventParam()
                {
                    tick = base.TimeManager.Tick,
                    ownerConnectionId = base.OwnerId,
                    position = m_AttackQueuePosition,
                    rotationY = m_AttackQueueRotationY
                });

                // 서버에게 공격 사실을 알립니다.
                ServerRpc_Attack(new PlayerMeleeAttack_Attack_EventParam()
                {
                    tick = base.TimeManager.Tick,
                    ownerConnectionId = base.LocalConnection.ClientId,
                    position = m_AttackQueuePosition,
                    rotationY = m_AttackQueueRotationY
                });
            }
        }

        [ServerRpc]
        private void ServerRpc_Attack(PlayerMeleeAttack_Attack_EventParam _param)
        {
            // 클라이언트가 총알을 발사한 tick으로부터 현재 서버 tick까지
            // 얼만큼의 시간이 걸렸는지 얻습니다.
            float _passedTime = (float)base.TimeManager.TimePassed(_param.tick, false);
            _passedTime = Mathf.Min(MAX_PASSED_TIME, _passedTime);

            // 공격한 자신이 서버이기도 한 경우,
            // 이미 pred 이벤트를 클라이언트 코드 내에서 실행했기 때문에 중복으로 실행하지 않습니다.
            if (base.IsOwner == false)
            {
                onAttack?.Invoke(new IWeapon_OnAttack_EventParam()
                {
                    tick = _param.tick,
                    ownerConnectionId = _param.ownerConnectionId,
                    position = _param.position,
                    rotationY = _param.rotationY
                });
            }

            Server_Attack(_param, attackDamageMagnitude);

            // 다른 클라이언트들에게 발사 사실을 알립니다.
            ObserversRpc_Attack(_param);
        }

        [Server]
        private void Server_Attack(in PlayerMeleeAttack_Attack_EventParam _param, int _attackDamage)
        {
            var _others = Physics.OverlapBox(
                m_AttackRangeCollider.transform.position,
                m_AttackRangeCollider.bounds.extents,
                m_AttackRangeCollider.transform.rotation);

            foreach (var _collider in _others)
            {
                var _damageableEntity = _collider.GetComponent<IDamageableEntity>();
                if (_damageableEntity != null)
                {
                    if (_collider.gameObject.CompareTag("Player"))
                    {
                        // 나 자신이 내 공격에 데미지를 받지 않습니다.
                        var _player = _collider.GetComponent<Player>();
                        if (_player.OwnerId == base.OwnerId)
                            continue;
                    }

                    Vector3 _attackDirection = GetDirectionsByRotationY(_param.rotationY);

                    _damageableEntity.TakeDamage(
                        new DamageParam()
                        {
                            direction = _attackDirection,
                            point = _collider.ClosestPoint(transform.position),
                            force = 10f,
                            time = Time.time,
                            healthModifier = new HealthModifier()
                            {
                                magnitude = _attackDamage,
                                source = this,
                                time = Time.time
                            }
                        }, out int _appliedDamage);

                    Debug.Log(_appliedDamage);

                    Vector3 _hitDirection = _collider.transform.position - _param.position;
                    _hitDirection.y = 0;
                    _hitDirection.Normalize();

                    Server_OnAttackHit(_collider.ClosestPoint(_param.position), _hitDirection);
                }
            }
        }

        [ObserversRpc(ExcludeOwner = true, ExcludeServer = true)]
        private void ObserversRpc_Attack(PlayerMeleeAttack_Attack_EventParam _param)
        {
            // 총을 발사한 클라이언트가 총알을 발사한 tick으로부터
            // 이 클라이언트의 현재 tick까지 얼만큼의 시간이 걸렸는지 얻습니다.
            float passedTime = (float)base.TimeManager.TimePassed(_param.tick, false);
            passedTime = Mathf.Min(MAX_PASSED_TIME, passedTime);

            onAttack?.Invoke(new IWeapon_OnAttack_EventParam()
            {
                tick = _param.tick,
                ownerConnectionId = _param.ownerConnectionId,
                position = _param.position,
                rotationY = _param.rotationY
            });
        }

        private Vector3 GetDirectionsByRotationY(float _rotationY)
        {
            Vector3 _rangeCenterDirection =
                Quaternion.Euler(new Vector3(0, _rotationY, 0))
                * Vector3.forward;

            return _rangeCenterDirection;
        }

        private void Awake()
        {
            player = GetComponentInParent<Player>();
        }

        private void Start()
        {
            m_LastAttackTime = -9999;
            m_CanAttack = true;

            player.health.onHealthChanged_OnSync += _amount =>
            {
                if (player.health.health > 0)
                    m_CanAttack = true;
                else
                    m_CanAttack = false;
            };
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            base.TimeManager.OnTick += TimeManager_OnTick;
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            base.TimeManager.OnTick -= TimeManager_OnTick;
        }

        private void TimeManager_OnTick()
        {
            if (base.IsOwner)
            {
                if (m_AttackQueue && m_CanAttack)
                    Client_Attack();
            }
        }

        private void Update()
        {
            if (base.IsOwner == false)
                return;

            if (Input.GetMouseButton(1) && m_CanAttack)
            {
                m_AttackQueue = true;
                m_AttackQueuePosition = transform.position;
                m_AttackQueueRotationY = transform.eulerAngles.y;
            }
        }
    }
}