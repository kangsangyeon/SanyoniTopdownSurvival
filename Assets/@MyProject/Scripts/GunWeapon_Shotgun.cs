using DG.Tweening;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Pool;

namespace MyProject
{
    public class GunWeapon_Shotgun : NetworkBehaviour, IGunWeapon
    {
        [SerializeField] private TrailRenderer m_Prefab_Trail;
        [SerializeField] private Transform m_FirePoint;
        [SerializeField] private int m_Damage = -10;
        [SerializeField] private int m_MaxMagazineCount = 20;
        [SerializeField] private float m_ReloadDuration = 1.5f;
        [SerializeField] private float m_FireDelay = 0.6f;
        [SerializeField] private int m_BulletsPerShot = 10;
        [SerializeField] private float m_ShotAngle = 30.0f;
        [SerializeField] private float m_ShotLength = 3.0f;
        [SerializeField] private float m_BulletToEndDuration = 0.15f;

        private Player m_Player;

        public Player player
        {
            get => m_Player;
            set => m_Player = value;
        }

        private ObjectPool<TrailRenderer> m_ProjectilePool;
        private Vector2 m_Direction;
        private float m_LastReloadStartTime;
        private float m_LastFireTime;
        private int m_CurrentMagazineCount;
        private bool m_ShootQueue;
        private bool m_CanShoot;

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

        [Client]
        public void Client_Fire()
        {
            float _elapsedTimeSinceLastReloadStart = Time.time - m_LastReloadStartTime;
            float _elapsedTimeSinceLastFire = Time.time - m_LastFireTime;

            // 재장전 중에 총알을 발사할 수 없습니다.
            bool _canShoot = _elapsedTimeSinceLastReloadStart >= m_ReloadDuration;

            // 가장 마지막으로 발사한 뒤 일정 시간 뒤에 다시 발사할 수 있습니다.
            _canShoot = _canShoot && _elapsedTimeSinceLastFire >= m_FireDelay;

            // 총알이 탄창에 없을 때 발사할 수 없습니다.
            _canShoot = _canShoot && currentMagazineCount > 0;

            if (_canShoot == false)
                return;

            // Vector2 _position = m_Player.transform.position;
            // Vector2 _mousePositionWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // m_Direction = _mousePositionWorld - _position;
            // m_Direction.Normalize();

            float _playerLookAngle = Vector2.SignedAngle(transform.right, Vector2.right) * -1;

            Debug.DrawLine(
                transform.position,
                transform.position + Quaternion.Euler(new Vector3(0, 0, _playerLookAngle)) * Vector3.right * 10,
                Color.red,
                2.0f);

            float _startAngle = _playerLookAngle - (m_ShotAngle / 2);

            for (int i = 0; i < m_BulletsPerShot; ++i)
            {
                float _delta = i / (float)(m_BulletsPerShot - 1);
                float _angleDelta = m_ShotAngle * _delta;
                float _angle = _startAngle + _angleDelta;
                Quaternion _angleQuaternion = Quaternion.Euler(new Vector3(0, 0, _angle));
                Vector3 _bulletDirection = _angleQuaternion * Vector2.right;
                Vector2 _endPosition = m_FirePoint.position + _bulletDirection * m_ShotLength;

                var _trail = m_ProjectilePool.Get();
                _trail.transform.position = m_FirePoint.position;
                _trail.transform
                    .DOMove(_endPosition, m_BulletToEndDuration)
                    .SetEase(Ease.Linear)
                    .OnComplete(() => m_ProjectilePool.Release(_trail));
            }
        }

        [ServerRpc]
        private void ServerRpc_Fire()
        {
        }

        [ObserversRpc(ExcludeOwner = true, ExcludeServer = true)]
        private void ObserversRpc_Fire()
        {
        }

        private void Awake()
        {
            player = GetComponentInParent<Player>();
            m_ProjectilePool = new ObjectPool<TrailRenderer>(
                OnCreateTrail, OnGetTrail, OnReleaseTrail, null,
                true, 20);
        }

        private void Start()
        {
            currentMagazineCount = m_MaxMagazineCount;
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
                    Client_Fire();

                m_ShootQueue = false;
            }
        }

        private TrailRenderer OnCreateTrail()
        {
            TrailRenderer _trail = Instantiate(m_Prefab_Trail);
            _trail.gameObject.SetActive(false);

            return _trail;
        }

        private void OnGetTrail(TrailRenderer _trail)
        {
            _trail.Clear();
            _trail.gameObject.SetActive(true);
        }

        private void OnReleaseTrail(TrailRenderer _trail)
        {
            _trail.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (base.IsOwner == false)
                return;

            if (Input.GetMouseButton(0) && m_CanShoot)
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