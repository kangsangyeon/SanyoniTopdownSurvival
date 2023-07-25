using System;
using System.Collections.Generic;
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

        [SerializeField] private LayerMask m_CharacterLayer;
        [SerializeField] private LayerMask m_IgnoreCollisionCharacterLayer;
        [SerializeField] private LayerMask m_DamageableLayer;

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
            + player.abilityProperty.meleeAttackDamageMagnitudeAddition);

        public float attackDelay =>
            player.abilityProperty.meleeAttackDelay
            + player.abilityProperty.meleeAttackDelayAddition;

        public event Action<IWeapon_OnAttack_EventParam> onAttack;
        public event Action<IMeleeWeapon_OnAttackHit_EventParam> onAttackHit;

        [Client(RequireOwnership = true)]
        public void QueueAttack()
        {
            m_AttackQueue = true;
            m_AttackQueuePosition = transform.position;
            m_AttackQueueRotationY = transform.eulerAngles.y;
        }

        #endregion

        #region Events

        [Server]
        private void Server_OnAttackHit(Vector3 _hitPoint, Vector3 _hitDirection, in DamageParam _hitDamage, uint _tick)
        {
            var _param = new IMeleeWeapon_OnAttackHit_EventParam()
            {
                tick = _tick,
                ownerConnectionId = base.OwnerId,
                hitPoint = _hitPoint,
                hitDirection = _hitDirection,
                hitRotation = Quaternion.FromToRotation(Vector3.right, _hitDirection),
                hitDamage = _hitDamage
            };

            onAttackHit?.Invoke(_param);
            ObserversRpc_OnAttackHit(_param);
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnAttackHit(IMeleeWeapon_OnAttackHit_EventParam _param)
        {
            onAttackHit?.Invoke(_param);
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
            // 클라이언트가 총알을 발사한 tick으로부터 현재 서버 tick까지
            // 얼만큼의 시간이 걸렸는지 얻습니다.
            float _passedTime = (float)base.TimeManager.TimePassed(_param.tick, false);
            _passedTime = Mathf.Min(MAX_PASSED_TIME, _passedTime);

            List<IDamageableEntity> _alreadyDamagedList = new List<IDamageableEntity>();

            SetCharacterLayer(true);

            var _others = Physics.OverlapBox(
                _param.position,
                m_AttackRangeCollider.bounds.extents,
                Quaternion.Euler(0, _param.rotationY, 0),
                m_DamageableLayer);

            foreach (var _collider in _others)
            {
                var _damageableEntity = _collider.GetComponent<IDamageableEntity>();
                if (_damageableEntity != null)
                {
                    if (_alreadyDamagedList.Contains(_damageableEntity))
                    {
                        // 이미 동일한 공격에 피격된 대상이라면
                        // 피격 처리를 중복으로 하지 않습니다.
                        continue;
                    }

                    _alreadyDamagedList.Add(_damageableEntity);

                    Vector3 _attackDirection = GetDirectionsByRotationY(_param.rotationY);
                    var _damageParam = new DamageParam()
                    {
                        direction = _attackDirection,
                        point = _collider.ClosestPoint(transform.position),
                        force = 10f,
                        time = Time.time,
                        healthModifier = new HealthModifier()
                        {
                            magnitude = _attackDamage,
                            source = this,
                            sourceOwnerObject = player,
                            time = Time.time
                        }
                    };

                    _damageableEntity.TakeDamage(
                        _damageParam, out int _appliedDamage);

                    Vector3 _hitDirection = _collider.transform.position - _param.position;
                    _hitDirection.y = 0;
                    _hitDirection.Normalize();

                    Server_OnAttackHit(
                        _collider.ClosestPoint(_param.position), _hitDirection,
                        _damageParam, _param.tick);
                }
            }

            SetCharacterLayer(false);
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

            m_Player.gameObject.layer = _layer;
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
    }
}