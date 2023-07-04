using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;
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
        [SerializeField] private int m_Damage = -10;
        [SerializeField] private int m_MaxMagazineCount = 20;
        [SerializeField] private float m_ReloadDuration = 1.5f;
        [SerializeField] private float m_FireDelay = 0.2f;

        private Player m_Player;

        public Player player
        {
            get => m_Player;
            set => m_Player = value;
        }

        private ObjectPool<Projectile> m_ProjectilePool;
        private Vector2 m_Direction;
        private float m_LastReloadStartTime;
        private float m_LastFireTime;
        private int m_CurrentMagazineCount;
        private bool m_ShootQueue;

        #region IGunWeapon

        public int damageMagnitude => m_Damage;
        public object owner => player;

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

        public int maxMagazineCount
        {
            get => m_MaxMagazineCount;
            set
            {
                if (m_MaxMagazineCount != value)
                {
                    m_MaxMagazineCount = value;
                    onMaxMagazineCountChanged?.Invoke();
                }
            }
        }

        public float reloadDuration
        {
            get => m_ReloadDuration;
            private set => m_ReloadDuration = value;
        }

        public event System.Action onCurrentMagazineCountChanged;
        public event System.Action onMaxMagazineCountChanged;
        public event System.Action onFire;
        public event System.Action onReloadStart;
        public event System.Action onReloadFinished;

        #endregion

        private Projectile SpawnProjectile(Vector2 _position, Vector2 _direction, float _passedTime)
        {
            Projectile _projectile = m_ProjectilePool.Get();
            _projectile.gameObject.layer = gameObject.layer;
            _projectile.Initialize(_position, _direction, _passedTime);

            return _projectile;
        }

        [Client]
        private void ClientFire()
        {
            m_ShootQueue = false;

            float _elapsedTimeSinceLastReloadStart = Time.time - m_LastReloadStartTime;
            float _elapsedTimeSinceLastFire = Time.time - m_LastFireTime;

            // 재장전 중에 총알을 발사할 수 없습니다.
            bool _canShoot = _elapsedTimeSinceLastReloadStart >= m_ReloadDuration;

            // 가장 마지막으로 발사한 뒤 일정 시간 뒤에 다시 발사할 수 있습니다.
            _canShoot = _canShoot && _elapsedTimeSinceLastFire >= m_FireDelay;

            // 총알이 탄창에 없을 때 발사할 수 없습니다.
            _canShoot = _canShoot && currentMagazineCount > 0;

            if (_canShoot)
            {
                Vector2 _position = m_Player.transform.position;
                Vector2 _mousePositionWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                m_Direction = _mousePositionWorld - _position;
                m_Direction.Normalize();

                // 로컬에서 발사하고 즉시 생성하는 총알이기 때문에,
                // 클라이언트에서 총알이 이동하는 위치가 곧 실제 위치입니다.
                // 따라서 총알을 실제 위치까지 따라잡기 위해 가속할 필요가 없습니다.
                var _projectile = SpawnProjectile(m_FirePoint.position, m_Direction, 0.0f);
                _projectile.m_StartTime = Time.time;

                // 서버에게 발사 사실을 알립니다.
                ServerRpcFire(m_FirePoint.position, m_Direction, base.TimeManager.Tick);

                m_LastFireTime = Time.time;
                --currentMagazineCount;
                onFire?.Invoke();
            }
        }

        [ServerRpc]
        private void ServerRpcFire(Vector2 _position, Vector2 _direction, uint _tick)
        {
            // 클라이언트가 총알을 발사한 tick으로부터 현재 서버 tick까지
            // 얼만큼의 시간이 걸렸는지 얻습니다.
            float _passedTime = (float)base.TimeManager.TimePassed(_tick, false);

            _passedTime = Mathf.Min(MAX_PASSED_TIME, _passedTime);

            // 총알을 스폰합니다.
            var _projectile = SpawnProjectile(_position, _direction, _passedTime);
            _projectile.m_StartTime = Time.time;

            // 다른 클라이언트들에게 발사 사실을 알립니다.
            ObserversRpcFire(_position, _direction, _tick);
        }

        [ObserversRpc(ExcludeOwner = true)]
        private void ObserversRpcFire(Vector2 _position, Vector2 _direction, uint _tick)
        {
            // 총을 발사한 클라이언트가 총알을 발사한 tick으로부터
            // 이 클라이언트의 현재 tick까지 얼만큼의 시간이 걸렸는지 얻습니다.
            float passedTime = (float)base.TimeManager.TimePassed(_tick, false);
            passedTime = Mathf.Min(MAX_PASSED_TIME, passedTime);

            SpawnProjectile(_position, _direction, passedTime);
        }

        private void Awake()
        {
            player = GetComponentInParent<Player>();
            m_ProjectilePool = new ObjectPool<Projectile>(
                OnCreateProjectile, OnGetProjectile, OnReleaseProjectile, null,
                true, 20);

            currentMagazineCount = m_MaxMagazineCount;
            m_LastFireTime = -9999;
            m_LastReloadStartTime = -9999;
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
                if (m_ShootQueue)
                    ClientFire();
            }
        }

        private Projectile OnCreateProjectile()
        {
            Projectile _projectile = Instantiate(m_Prefab_Projectile);
            _projectile.gameObject.SetActive(false);
            _projectile.onLifeEnd += () => { m_ProjectilePool.Release(_projectile); };
            _projectile.onHit += (col) =>
            {
                if (base.IsServer)
                {
                    var _health = col.GetComponent<PlayerHealth>();
                    if (_health.health == 0)
                    {
                        // 이미 죽은 대상에게는 아무런 영향을 주지 않고 무시합니다.
                        return;
                    }
                }

                m_ProjectilePool.Release(_projectile);

                if (base.IsServer)
                {
                    // 서버에서 생성된 총알만 게임에 영향을 끼치는 동작을 합니다.

                    var _health = col.GetComponent<PlayerHealth>();
                    _health.ApplyModifier(new HealthModifier() { magnitude = m_Damage, source = this, time = Time.time });
                    Debug.Log(
                        $"{gameObject.name}: player {col.gameObject.name} hit! now health is {_health.health}/{_health.MaxHealth}.");
                }
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

        private void Update()
        {
            if (base.IsOwner == false)
                return;

            if (Input.GetMouseButton(0))
            {
                m_ShootQueue = true;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                m_LastReloadStartTime = Time.time;
                onReloadStart?.Invoke();

                this.Invoke(() =>
                {
                    currentMagazineCount = maxMagazineCount;
                    onReloadFinished?.Invoke();
                }, m_ReloadDuration);
            }
        }
    }
}