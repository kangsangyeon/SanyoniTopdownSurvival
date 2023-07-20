using System;
using System.Collections.Generic;
using FishNet.Object;
using MyProject.Event;
using UnityEngine;
using UnityEngine.Pool;

namespace MyProject
{
    public class PlayerGunShoot : NetworkBehaviour, IGunWeapon
    {
        /// <summary>
        /// 발사체가 가질 수 있는 최대 경과 시간입니다.
        /// </summary>
        private const float MAX_PASSED_TIME = 0.3f;

        [SerializeField] private Projectile m_Prefab_Projectile;
        [SerializeField] private Transform m_FirePoint;

        private Player m_Player;

        public Player player
        {
            get => m_Player;
            set => m_Player = value;
        }

        private ObjectPool<Projectile> m_ProjectilePool;
        private float m_LastReloadStartTime;
        private float m_LastFireTime;
        private int m_CurrentMagazineCount;
        private bool m_ShootQueue;
        private Vector3 m_ShotQueuePosition;
        private float m_ShotQueueRotationY;
        private bool m_CanShoot;

        #region IGunWeapon

        public object owner => player;

        public int ownerConnectionId => player.OwnerId;

        public int attackDamageMagnitude =>
            Mathf.RoundToInt(m_Player.abilityProperty.projectileDamage *
                             m_Player.abilityProperty.projectileDamageMultiplier);

        public float attackDelay =>
            m_Player.abilityProperty.fireDelay * m_Player.abilityProperty.fireDelayMultiplier;

        public int currentMagazineCount
        {
            get => m_CurrentMagazineCount;
            private set
            {
                if (m_CurrentMagazineCount != value)
                {
                    m_CurrentMagazineCount = value;
                    onCurrentMagazineCountChanged?.Invoke();
                }
            }
        }

        public int maxMagazineCount =>
            Mathf.RoundToInt(m_Player.abilityProperty.maxMagazine * m_Player.abilityProperty.maxMagazineMultiplier);

        public float reloadDuration =>
            m_Player.abilityProperty.reloadDuration * m_Player.abilityProperty.reloadDurationMultiplier;

        public float fireDelay =>
            m_Player.abilityProperty.fireDelay * m_Player.abilityProperty.fireDelayMultiplier;

        public float projectileSpeed =>
            m_Player.abilityProperty.projectileSpeed * m_Player.abilityProperty.projectileSpeedMultiplier;

        public float projectileScaleMultiplier =>
            m_Player.abilityProperty.projectileSizeMultiplier;

        public int projectileCountPerShot =>
            m_Player.abilityProperty.projectileCountPerShot;

        public float projectileShotAngleRange =>
            m_Player.abilityProperty.projectileShotAngleRange;

        public event System.Action onCurrentMagazineCountChanged;
        public event Action<IWeapon_OnAttack_EventParam> onAttack;
        public event System.Action onReloadStart;
        public event System.Action onReloadFinished;

        #endregion

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
            _projectile.scaleMultiplier = projectileScaleMultiplier;

            return _projectile;
        }

        [Client]
        private void ClientFire()
        {
            m_ShootQueue = false;

            float _elapsedTimeSinceLastReloadStart = Time.time - m_LastReloadStartTime;
            float _elapsedTimeSinceLastFire = Time.time - m_LastFireTime;

            // 재장전 중에 총알을 발사할 수 없습니다.
            bool _canShoot = _elapsedTimeSinceLastReloadStart >= reloadDuration;

            // 가장 마지막으로 발사한 뒤 일정 시간 뒤에 다시 발사할 수 있습니다.
            _canShoot = _canShoot && _elapsedTimeSinceLastFire >= fireDelay;

            // 총알이 탄창에 없을 때 발사할 수 없습니다.
            _canShoot = _canShoot && currentMagazineCount > 0;

            if (_canShoot)
            {
                var _projectileDirections =
                    GetProjectileDirections(m_ShotQueueRotationY, projectileCountPerShot, projectileShotAngleRange);

                _projectileDirections.ForEach(_direction =>
                {
                    // 로컬에서 발사하고 즉시 생성하는 총알이기 때문에,
                    // 클라이언트에서 총알이 이동하는 위치가 곧 실제 위치입니다.
                    // 따라서 총알을 실제 위치까지 따라잡기 위해 가속할 필요가 없습니다.
                    var _projectile = SpawnProjectile(
                        base.OwnerId, 0.0f,
                        m_ShotQueuePosition, _direction);
                });

                // 서버에게 발사 사실을 알립니다.
                ServerRpcFire(new PlayerGunShoot_Fire_EventParam()
                {
                    tick = base.TimeManager.Tick,
                    ownerConnectionId = base.OwnerId,
                    position = m_ShotQueuePosition,
                    rotationY = m_ShotQueueRotationY
                });

                m_LastFireTime = Time.time;
                --currentMagazineCount;
                onAttack?.Invoke(new IWeapon_OnAttack_EventParam()
                {
                    tick = base.TimeManager.Tick,
                    ownerConnectionId = base.OwnerId,
                    position = m_ShotQueuePosition,
                    rotationY = m_ShotQueueRotationY
                });
            }
        }

        [ServerRpc]
        private void ServerRpcFire(PlayerGunShoot_Fire_EventParam _param)
        {
            // 발사한 자신이 서버이기도 한 경우,
            // 이미 총알을 클라이언트 코드 내에서 스폰했기 때문에 중복으로 생성하지 않습니다.
            if (base.IsOwner == false)
            {
                // 클라이언트가 총알을 발사한 tick으로부터 현재 서버 tick까지
                // 얼만큼의 시간이 걸렸는지 얻습니다.
                float _passedTime = (float)base.TimeManager.TimePassed(_param.tick, false);

                _passedTime = Mathf.Min(MAX_PASSED_TIME, _passedTime);

                var _projectileDirections =
                    GetProjectileDirections(_param.rotationY, projectileCountPerShot, projectileShotAngleRange);

                _projectileDirections.ForEach(_d =>
                {
                    // 총알을 스폰합니다.
                    var _projectile = SpawnProjectile(
                        _param.ownerConnectionId, _passedTime,
                        _param.position, _d);
                });
            }

            // 다른 클라이언트들에게 발사 사실을 알립니다.
            ObserversRpcFire(_param);
        }

        [ObserversRpc(ExcludeOwner = true, ExcludeServer = true)]
        private void ObserversRpcFire(PlayerGunShoot_Fire_EventParam _param)
        {
            // 총을 발사한 클라이언트가 총알을 발사한 tick으로부터
            // 이 클라이언트의 현재 tick까지 얼만큼의 시간이 걸렸는지 얻습니다.
            float passedTime = (float)base.TimeManager.TimePassed(_param.tick, false);
            passedTime = Mathf.Min(MAX_PASSED_TIME, passedTime);

            var _projectileDirections =
                GetProjectileDirections(_param.rotationY, projectileCountPerShot, projectileShotAngleRange);

            _projectileDirections.ForEach(_d =>
            {
                // 총알을 스폰합니다.
                var _projectile = SpawnProjectile(
                    _param.ownerConnectionId, passedTime,
                    _param.position, _d);
            });
        }

        private List<Vector3> GetProjectileDirections(float _rangeCenterRotation, int _count, float _angleRange)
        {
            if (_count == 1)
            {
                Vector3 _rangeCenterDirection =
                    Quaternion.Euler(new Vector3(0, _rangeCenterRotation, 0))
                    * Vector3.forward;
                return new List<Vector3>() { _rangeCenterDirection };
            }

            List<Vector3> _outDirectionList = new List<Vector3>();

            float _startAngle = _rangeCenterRotation - (_angleRange / 2);

            for (int i = 0; i < _count; ++i)
            {
                float _delta = i / (float)(_count - 1);
                float _angleDelta = _angleRange * _delta;
                float _angle = _startAngle + _angleDelta;
                Quaternion _angleQuaternion = Quaternion.Euler(new Vector3(0, 0, _angle));
                Vector3 _bulletDirection = _angleQuaternion * Vector3.right;

                _outDirectionList.Add(_bulletDirection);
            }

            return _outDirectionList;
        }

        private void Awake()
        {
            player = GetComponentInParent<Player>();
            m_ProjectilePool = new ObjectPool<Projectile>(
                OnCreateProjectile, OnGetProjectile, OnReleaseProjectile, null,
                true, 20);
        }

        private void Start()
        {
            currentMagazineCount = maxMagazineCount;
            m_LastFireTime = -9999;
            m_LastReloadStartTime = -9999;
            m_CanShoot = true;

            player.health.onHealthIsZero_OnSync += () => { m_CanShoot = false; };

            player.health.onHealthChanged_OnSync += _amount =>
            {
                if (player.health.health > 0)
                    m_CanShoot = true;
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
                if (m_ShootQueue && m_CanShoot)
                    ClientFire();
            }
        }

        private Projectile OnCreateProjectile()
        {
            Projectile _projectile = Instantiate(m_Prefab_Projectile);
            _projectile.gameObject.SetActive(false);
            _projectile.onLifeEnd += () => { m_ProjectilePool.Release(_projectile); };
            _projectile.onHit += (col) => { m_ProjectilePool.Release(_projectile); };

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

        private void Update()
        {
            if (base.IsOwner == false)
                return;

            if (Input.GetMouseButton(0) && m_CanShoot)
            {
                m_ShootQueue = true;
                m_ShotQueuePosition = m_FirePoint.position;
                m_ShotQueueRotationY = transform.eulerAngles.y;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                m_LastReloadStartTime = Time.time;
                onReloadStart?.Invoke();

                this.Invoke(() =>
                {
                    currentMagazineCount = maxMagazineCount;
                    onReloadFinished?.Invoke();
                }, reloadDuration);
            }
        }
    }
}