using UnityEngine;
using UnityEngine.Pool;

public class PlayerGunShoot : MonoBehaviour
{
    [SerializeField] private Projectile m_Prefab_Projectile;
    [SerializeField] private Transform m_FirePoint;
    [SerializeField] private int m_Damage = -10;

    private ObjectPool<Projectile> m_ProjectilePool;
    private Vector2 m_Direction;

    private void Start()
    {
        m_ProjectilePool = new ObjectPool<Projectile>(
            OnCreateProjectile, OnGetProjectile, OnReleaseProjectile, null,
            true, 20);
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
            _health.AddHealth(m_Damage);
            Debug.Log($"{gameObject.name}: player {col.gameObject.name} hit! now health is {_health.Health}/{_health.MaxHealth}.");
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