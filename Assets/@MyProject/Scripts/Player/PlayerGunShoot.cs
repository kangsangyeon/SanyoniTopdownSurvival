using System;
using UnityEngine;
using UnityEngine.Pool;

namespace MyProject
{
    public class PlayerGunShoot : MonoBehaviour, IWeapon
    {
        [SerializeField] private Projectile m_Prefab_Projectile;
        [SerializeField] private Transform m_FirePoint;
        [SerializeField] private int m_Damage = -10;

        private Player m_Player;

        public Player player
        {
            get => m_Player;
            set => m_Player = value;
        }

        private ObjectPool<Projectile> m_ProjectilePool;
        private Vector2 m_Direction;

        #region IWeapon

        public int damageMagnitude => m_Damage;
        public object owner => player;

        #endregion

        private void Awake()
        {
            m_ProjectilePool = new ObjectPool<Projectile>(
                OnCreateProjectile, OnGetProjectile, OnReleaseProjectile, null,
                true, 20);
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
                Vector2 _position = transform.position;
                Vector2 _mousePositionWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                m_Direction = _mousePositionWorld - _position;
                m_Direction.Normalize();

                Projectile _projectile = m_ProjectilePool.Get();
                _projectile.transform.position = m_FirePoint.position;
                _projectile.gameObject.layer = gameObject.layer;
                _projectile.Direction = m_Direction;
                _projectile.StartTime = Time.time;
            }
        }
    }
}