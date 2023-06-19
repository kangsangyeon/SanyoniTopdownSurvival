using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

namespace MyProject
{
    public class PlayerGunShoot : MonoBehaviour, IGunWeapon
    {
        [SerializeField] private Projectile m_Prefab_Projectile;
        [SerializeField] private Transform m_FirePoint;
        [SerializeField] private int m_Damage = -10;
        [SerializeField] private int m_MaxMagazineCount = 20;
        [SerializeField] private float m_ReloadDuration = 1.5f;

        private Player m_Player;

        public Player player
        {
            get => m_Player;
            set => m_Player = value;
        }

        private ObjectPool<Projectile> m_ProjectilePool;
        private Vector2 m_Direction;
        private float m_LastReloadStartTime;
        private int m_CurrentMagazineCount;

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
                    onCurrentMagazineCountChanged.Invoke();
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
                    onMaxMagazineCountChanged.Invoke();
                }
            }
        }

        public UnityEvent onCurrentMagazineCountChanged { get; } = new UnityEvent();
        public UnityEvent onMaxMagazineCountChanged { get; } = new UnityEvent();

        #endregion

        private void Awake()
        {
            m_ProjectilePool = new ObjectPool<Projectile>(
                OnCreateProjectile, OnGetProjectile, OnReleaseProjectile, null,
                true, 20);

            currentMagazineCount = m_MaxMagazineCount;
        }

        private void Start()
        {
            player = GetComponentInParent<Player>();
        }

        private Projectile OnCreateProjectile()
        {
            Projectile _projectile = Instantiate(m_Prefab_Projectile);
            _projectile.gameObject.SetActive(false);
            _projectile.onLifeEnd.AddListener(() => { m_ProjectilePool.Release(_projectile); });
            _projectile.onHit.AddListener((col) =>
            {
                m_ProjectilePool.Release(_projectile);

                var _health = col.GetComponent<PlayerHealth>();
                _health.ApplyModifier(new HealthModifier() { magnitude = m_Damage, source = this, time = Time.time });
                Debug.Log(
                    $"{gameObject.name}: player {col.gameObject.name} hit! now health is {_health.Health}/{_health.MaxHealth}.");
            });

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
            if (Input.GetMouseButtonDown(0))
            {
                float _elapsedTimeSinceLastReloadStart = Time.time - m_LastReloadStartTime;

                // 재장전 중에 총알을 발사할 수 없습니다.
                bool _canShoot = _elapsedTimeSinceLastReloadStart >= m_ReloadDuration;

                // 총알이 탄창에 없을 때 발사할 수 없습니다.
                _canShoot = _canShoot && currentMagazineCount > 0;

                if (_canShoot)
                {
                    Vector2 _position = transform.position;
                    Vector2 _mousePositionWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    m_Direction = _mousePositionWorld - _position;
                    m_Direction.Normalize();

                    Projectile _projectile = m_ProjectilePool.Get();
                    _projectile.transform.position = m_FirePoint.position;
                    _projectile.gameObject.layer = gameObject.layer;
                    _projectile.Refresh(m_Direction, Time.time);

                    --currentMagazineCount;
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                currentMagazineCount = maxMagazineCount;
            }
        }
    }
}