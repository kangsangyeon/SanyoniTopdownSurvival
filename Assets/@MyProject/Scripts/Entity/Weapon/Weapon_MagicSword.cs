using System;
using System.Collections.Generic;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Serializing.Helping;
using MyProject.Event;
using MyProject.Struct;
using UnityEngine;
using UnityEngine.Pool;

namespace MyProject
{
    public class Weapon_MagicSword : NetworkBehaviour, IMeleeWeapon
    {
        /// <summary>
        /// 발사체가 가질 수 있는 최대 경과 시간입니다.
        /// </summary>
        private const float MAX_PASSED_TIME = 0.3f;

        /// <summary>
        /// 검기 발사체 프리팹입니다.
        /// </summary>
        [SerializeField] private Projectile m_Prefab_Projectile;

        [SerializeField] private LayerMask m_CharacterLayer;
        [SerializeField] private LayerMask m_IgnoreCollisionCharacterLayer;
        [SerializeField] private LayerMask m_DamageableLayer;
        [SerializeField] private LayerMask m_BlockMovementLayer;

        private bool m_CanAttack;
        private float m_LastAttackTime;
        private bool m_AttackQueue;
        private Vector3 m_AttackQueuePosition;
        private Vector3 m_AttackQueueFootPosition;
        private float m_AttackQueueRotationY;
        private int m_MeleeAttackStack;

        private ObjectPool<Projectile> m_ProjectilePool;

        private Player m_Player;
        public Player player => m_Player;

        #region IMeleeWeapon

        public object owner => m_Player;

        public int ownerConnectionId => m_Player.OwnerId;

        public int attackDamageMagnitude =>
            Mathf.RoundToInt(
                Mathf.Clamp(m_Player.abilityProperty.meleeAttackDamageMagnitude
                            + m_Player.abilityProperty.meleeAttackDamageMagnitudeAddition,
                    m_Player.abilityProperty.minimumMeleeAttackDamageMagnitude,
                    m_Player.abilityProperty.maximumMeleeAttackDamageMagnitude)
            );

        public float attackDelay =>
            m_Player.abilityProperty.meleeAttackDelay;

        public float meleeAttackDelay =>
            m_Player.abilityProperty.meleeAttackDelay
            + m_Player.abilityProperty.meleeAttackDelayAddition;

        public float meleeAttackInterval =>
            Mathf.Clamp(m_Player.abilityProperty.meleeAttackInterval
                        + m_Player.abilityProperty.meleeAttackIntervalAddition,
                m_Player.abilityProperty.minimumMeleeAttackInterval,
                m_Player.abilityProperty.maximumMeleeAttackInterval);

        [SerializeField] private Collider m_AttackRange;
        public Collider meleeAttackRange => m_AttackRange;

        public event Action<IWeapon_OnAttack_EventParam> onAttack;
        public event Action<IMeleeWeapon_OnAttackHit_EventParam> onAttackHit;

        public void QueueAttack()
        {
            m_AttackQueue = true;
            m_AttackQueuePosition = transform.position;
            m_AttackQueueFootPosition = m_Player.footPoint.position;
            m_AttackQueueRotationY = transform.eulerAngles.y;
        }

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

        #region Sword

        public float projectileSpeed =>
            Mathf.Clamp(m_Player.abilityProperty.projectileSpeed
                        + m_Player.abilityProperty.projectileSpeedAddition,
                m_Player.abilityProperty.minimumProjectileSpeed,
                m_Player.abilityProperty.maximumProjectileSpeed);

        public float projectileScaleAddition =>
            1.0f + m_Player.abilityProperty.projectileSizeAddition;

        public int projectileCountPerShot =>
            Mathf.Clamp(m_Player.abilityProperty.projectileCountPerShot
                        + m_Player.abilityProperty.projectileCountPerShotAddition,
                m_Player.abilityProperty.minimumProjectileCountPerShot,
                m_Player.abilityProperty.maximumProjectileCountPerShot);

        public float projectileShotAngleRange =>
            m_Player.abilityProperty.projectileShotAngleRange;

        public int projectileDamageMagnitude =>
            Mathf.RoundToInt(
                Mathf.Clamp(m_Player.abilityProperty.projectileDamage
                            + m_Player.abilityProperty.projectileDamageAddition,
                    m_Player.abilityProperty.minimumProjectileDamage,
                    m_Player.abilityProperty.maximumProjectileDamage)
            );

        public int swordProjectileRequiredStack =>
            Mathf.Clamp(m_Player.abilityProperty.swordProjectileRequiredStack
                        + m_Player.abilityProperty.swordProjectileRequiredStackAddition,
                m_Player.abilityProperty.minimumSwordProjectileRequiredStack,
                m_Player.abilityProperty.maximumSwordProjectileRequiredStack);

        public event Action<Weapon_MagicSword_OnProjectileHit_EventParam> onProjectileHit;

        [Server]
        private void Server_OnProjectileHit(Weapon_MagicSword_OnProjectileHit_EventParam _param)
        {
            onProjectileHit?.Invoke(_param);
            ObserversRpc_OnProjectileHit(_param);
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnProjectileHit(Weapon_MagicSword_OnProjectileHit_EventParam _param)
        {
            onProjectileHit?.Invoke(_param);
        }

        #endregion

        [Client]
        private void Client_Attack()
        {
            m_AttackQueue = false;

            float _elapsedTimeSinceLastFire = Time.time - m_LastAttackTime;

            // 가장 마지막으로 공격한 뒤 일정 시간 뒤에 다시 공격할 수 있습니다.
            bool _canShoot = _elapsedTimeSinceLastFire >= meleeAttackInterval;

            if (_canShoot)
            {
                m_LastAttackTime = Time.time;

                if (m_MeleeAttackStack >= swordProjectileRequiredStack)
                {
                    m_MeleeAttackStack = 0;

                    if (base.IsServer == false)
                    {
                        // 공격한 자신이 서버이기도 한 경우,
                        // 서버 코드 내에서 투사체를 생성할 것이기 때문에 중복으로 생성하지 않기 위해 클라이언트 코드에서는 생성하지 않습니다.

                        // 투사체를 날립니다.

                        var _projectileDirections =
                            GetDirectionsByRotationY(
                                m_AttackQueueRotationY,
                                projectileCountPerShot,
                                projectileShotAngleRange);

                        _projectileDirections.ForEach(_direction =>
                        {
                            // 로컬에서 발사하고 즉시 생성하는 총알이기 때문에,
                            // 클라이언트에서 총알이 이동하는 위치가 곧 실제 위치입니다.
                            // 따라서 총알을 실제 위치까지 따라잡기 위해 가속할 필요가 없습니다.
                            var _projectile = SpawnProjectile(
                                base.OwnerId, 0.0f,
                                m_AttackQueueFootPosition, _direction);
                        });
                    }

                    // 서버에게 발사 사실을 알립니다.
                    ServerRpc_Attack(new Weapon_MagicSword_Attack_EventParam()
                    {
                        tick = base.TimeManager.Tick,
                        preciseTick = base.TimeManager.GetPreciseTick(TickType.Tick),
                        ownerConnectionId = base.OwnerId,
                        position = m_AttackQueueFootPosition,
                        rotationY = m_AttackQueueRotationY,
                        isProjectileAttack = true
                    });
                }
                else
                {
                    ++m_MeleeAttackStack;

                    // 일반적인 근접 공격을 실시합니다.

                    this.Invoke(() =>
                    {
                        onAttack?.Invoke(new IWeapon_OnAttack_EventParam()
                        {
                            tick = base.TimeManager.Tick,
                            ownerConnectionId = base.OwnerId,
                            position = m_AttackQueuePosition,
                            rotationY = m_AttackQueueRotationY
                        });

                        // 서버에게 공격 사실을 알립니다.
                        ServerRpc_Attack(new Weapon_MagicSword_Attack_EventParam()
                        {
                            tick = base.TimeManager.Tick,
                            preciseTick = base.TimeManager.GetPreciseTick(TickType.Tick),
                            ownerConnectionId = base.LocalConnection.ClientId,
                            position = m_AttackQueuePosition,
                            rotationY = m_AttackQueueRotationY,
                            isProjectileAttack = false
                        });
                    }, meleeAttackDelay);
                }
            }
        }

        [ServerRpc]
        private void ServerRpc_Attack(Weapon_MagicSword_Attack_EventParam _param)
        {
            // 클라이언트가 공격한 tick으로부터 현재 서버 tick까지
            // 얼만큼의 시간이 걸렸는지 얻습니다.
            float _passedTime = (float)base.TimeManager.TimePassed(_param.tick, false);
            _passedTime = Mathf.Min(MAX_PASSED_TIME, _passedTime);

            // 공격한 자신이 서버이기도 한 경우,
            // 이미 클라이언트 이벤트를 클라이언트 코드 내에서 실행했기 때문에 중복으로 실행하지 않습니다.
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

            Server_Attack(_param);

            // 다른 클라이언트들에게 공격 사실을 알립니다.
            ObserversRpc_Attack(_param);
        }

        [Server]
        private void Server_Attack(in Weapon_MagicSword_Attack_EventParam _param)
        {
            // 클라이언트가 공격한 tick으로부터 현재 서버 tick까지
            // 얼만큼의 시간이 걸렸는지 얻습니다.
            float _passedTime = (float)base.TimeManager.TimePassed(_param.tick, false);
            _passedTime = Mathf.Min(MAX_PASSED_TIME, _passedTime);

            if (_param.isProjectileAttack)
            {
                // 검기 발사체를 날립니다.
                Server_ProjectileAttack(_param, _passedTime);
            }
            else
            {
                // 일반적인 근접 공격을 합니다.
                Server_MeleeAttack(_param);
            }
        }

        [Server]
        private void Server_ProjectileAttack(
            Weapon_MagicSword_Attack_EventParam _param,
            float _passedTime)
        {
            var _projectileDirections =
                GetDirectionsByRotationY(_param.rotationY, projectileCountPerShot, projectileShotAngleRange);

            _projectileDirections.ForEach(_d =>
            {
                // 총알을 스폰합니다.
                var _projectile =
                    SpawnProjectile(
                        _param.ownerConnectionId, _passedTime,
                        _param.position, _d);
            });

            ObserversRpc_ProjectileAttack(_param);
        }

        [ObserversRpc(ExcludeServer = true, ExcludeOwner = true)]
        private void ObserversRpc_ProjectileAttack(
            Weapon_MagicSword_Attack_EventParam _param)
        {
            float _passedTime = (float)base.TimeManager.TimePassed(_param.tick);

            var _projectileDirections =
                GetDirectionsByRotationY(_param.rotationY, projectileCountPerShot, projectileShotAngleRange);

            _projectileDirections.ForEach(_d =>
            {
                // 총알을 스폰합니다.
                var _projectile =
                    SpawnProjectile(
                        _param.ownerConnectionId, _passedTime,
                        _param.position, _d);
            });
        }

        [Server]
        private void Server_MeleeAttack(in Weapon_MagicSword_Attack_EventParam _param)
        {
            List<IDamageableEntity> _alreadyDamagedList = new List<IDamageableEntity>();

            SetCharacterLayer(true);

            // Rollback only if a rollback time.
            bool rollingBack = Comparers.IsDefault(_param.preciseTick) == false;
            //If a rollbackTime exist then rollback colliders before firing.
            if (rollingBack)
                RollbackManager.Rollback(_param.preciseTick,
                    FishNet.Component.ColliderRollback.RollbackManager.PhysicsType.ThreeDimensional);

            var _others = Physics.OverlapBox(
                _param.position,
                m_AttackRange.bounds.extents,
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

                    Vector3 _attackDirection = GetDirectionsByRotationY(_param.rotationY, 1, 0)[0];
                    var _damageParam = new DamageParam()
                    {
                        direction = _attackDirection,
                        point = _collider.ClosestPoint(transform.position),
                        force = 10f,
                        time = Time.time,
                        healthModifier = new HealthModifier()
                        {
                            magnitude = attackDamageMagnitude,
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

            if (rollingBack)
                RollbackManager.Return();

            SetCharacterLayer(false);
        }

        [ObserversRpc(ExcludeOwner = true, ExcludeServer = true)]
        private void ObserversRpc_Attack(Weapon_MagicSword_Attack_EventParam _param)
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

        private List<Vector3> GetDirectionsByRotationY(float _rangeCenterRotation, int _count, float _angleRange)
        {
            if (_count == 1)
            {
                Vector3 _rangeCenterDirection =
                    Quaternion.Euler(new Vector3(0, _rangeCenterRotation, 0))
                    * Vector3.right;
                return new List<Vector3>() { _rangeCenterDirection };
            }

            List<Vector3> _outDirectionList = new List<Vector3>();

            float _startAngle = _rangeCenterRotation - (_angleRange / 2);

            for (int i = 0; i < _count; ++i)
            {
                float _delta = i / (float)(_count - 1);
                float _angleDelta = _angleRange * _delta;
                float _angle = _startAngle + _angleDelta;
                Quaternion _angleQuaternion = Quaternion.Euler(new Vector3(0, _angle, 0));
                Vector3 _bulletDirection = _angleQuaternion * Vector3.right;

                _outDirectionList.Add(_bulletDirection);
            }

            return _outDirectionList;
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

        private Projectile SpawnProjectile(
            int _ownerConnectionId, float _passedTime,
            Vector3 _position, Vector3 _direction)
        {
            Projectile _projectile = m_ProjectilePool.Get();
            _projectile.gameObject.layer = gameObject.layer;
            _projectile.Initialize(_position, _direction, _passedTime);
            _projectile.m_OwnerConnectionId = _ownerConnectionId;
            _projectile.m_StartTime = Time.time;
            _projectile.m_Speed = projectileSpeed;
            _projectile.m_DamageMagnitude = projectileDamageMagnitude;
            _projectile.scaleMultiplier = projectileScaleAddition;

            return _projectile;
        }

        private Projectile OnCreateProjectile()
        {
            Projectile _projectile = Instantiate(m_Prefab_Projectile);
            _projectile.gameObject.SetActive(false);
            _projectile.onLifeEnd += () => { m_ProjectilePool.Release(_projectile); };
            _projectile.onHit += (col) =>
            {
                Server_OnProjectileHit(new Weapon_MagicSword_OnProjectileHit_EventParam()
                {
                    tick = base.TimeManager.Tick,
                    ownerConnectionId = _projectile.m_OwnerConnectionId,
                    hitPoint = col.ClosestPoint(_projectile.transform.position),
                    hitDirection = _projectile.direction,
                    hitRotation = Quaternion.Euler(0, _projectile.transform.rotation.eulerAngles.y, 0)
                });
                m_ProjectilePool.Release(_projectile);
            };

            return _projectile;
        }

        private void OnGetProjectile(Projectile _projectile)
        {
            _projectile.gameObject.SetActive(true);
        }

        private void OnReleaseProjectile(Projectile _projectile)
        {
            _projectile.gameObject.SetActive(false);
        }

        private void Awake()
        {
            m_Player = GetComponentInParent<Player>();
            m_ProjectilePool = new ObjectPool<Projectile>(
                OnCreateProjectile, OnGetProjectile, OnReleaseProjectile, null,
                true, 20);
        }

        private void Start()
        {
            m_LastAttackTime = -9999;
            m_CanAttack = true;

            player.health.onHealthChanged_OnClient += _amount =>
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