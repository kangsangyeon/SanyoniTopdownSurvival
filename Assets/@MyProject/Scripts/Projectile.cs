using FishNet;
using MyProject;
using MyProject.Struct;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float m_LiveDuration = 1f;

    private float m_ScaleOrigin;
    private Vector3 m_Direction;
    private bool m_AlreadyHit = false;
    private float m_PassedTime;
    private float m_ActualLiveDuration;

    private float m_ScaleMultiplier;

    public float scaleMultiplier
    {
        get => m_ScaleMultiplier;
        set
        {
            m_ScaleMultiplier = value;
            transform.localScale = Vector3.one * m_ScaleOrigin * value;
        }
    }

    public float m_StartTime;
    public float m_Speed = 10f;

    public int m_OwnerConnectionId;

    public event System.Action onLifeEnd;
    public event System.Action<Collider> onHit;

    public void Initialize(Vector3 _position, Vector3 _direction, float _passedTime)
    {
        transform.position = _position;
        m_Direction = _direction;
        m_AlreadyHit = false;
        m_PassedTime = _passedTime;
        m_ActualLiveDuration = m_LiveDuration - _passedTime;
    }

    private void Awake()
    {
        m_ScaleOrigin = transform.localScale.x;
    }

    private void Update()
    {
        float _delta = Time.deltaTime;

        float _passedTimeDelta = 0f;
        if (m_PassedTime > 0f)
        {
            float _step = m_PassedTime * 0.08f;
            m_PassedTime = m_PassedTime - _step;

            if (m_PassedTime <= _delta / 2f)
            {
                _step += m_PassedTime;
                m_PassedTime = 0f;
            }

            _passedTimeDelta = _step;
        }

        transform.position =
            transform.position
            + m_Direction * m_Speed * (_delta + _passedTimeDelta);

        if (Time.time - m_StartTime >= m_ActualLiveDuration)
        {
            onLifeEnd?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (m_AlreadyHit)
            return;

        var _damageableEntity = col.GetComponent<IDamageableEntity>();
        if (_damageableEntity != null)
        {
            if (col.gameObject.CompareTag("Player"))
            {
                // 발사한 총알이 나 자신이 발사한 총알이라면 무시합니다.
                var _player = col.GetComponent<Player>();
                if (_player.OwnerId == m_OwnerConnectionId)
                    return;
            }

            if (InstanceFinder.IsServer)
            {
                _damageableEntity.TakeDamage(
                    new DamageParam()
                    {
                        direction = m_Direction,
                        point = col.ClosestPoint(transform.position),
                        force = 10f,
                        time = Time.time,
                        healthModifier = new HealthModifier()
                        {
                            magnitude = -10,
                            source = this,
                            time = Time.time
                        }
                    }, out int _appliedDamage);

                m_AlreadyHit = true;
                onHit?.Invoke(col);
            }
        }
    }
}