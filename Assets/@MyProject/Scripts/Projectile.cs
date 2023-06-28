using FishNet;
using UnityEngine;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    [SerializeField] private Collider2D m_Collider;
    [SerializeField] private float m_Speed = 8f;
    [SerializeField] private float m_LiveDuration = 1f;

    private Vector3 m_Direction;
    private bool m_AlreadyHit = false;
    private float m_PassedTime;

    public float m_StartTime;
    public UnityEvent onLifeEnd = new UnityEvent();
    public UnityEvent<Collider2D> onHit = new UnityEvent<Collider2D>();

    public void Initialize(Vector3 _position, Vector3 _direction, float _passedTime)
    {
        transform.position = _position;
        m_Direction = _direction;
        m_AlreadyHit = false;
        m_PassedTime = _passedTime;
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

        if (InstanceFinder.IsServer)
        {
            if (Time.time - m_StartTime >= m_LiveDuration)
            {
                onLifeEnd.Invoke();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (m_AlreadyHit)
            return;

        if (col.gameObject.CompareTag("Player"))
        {
            m_AlreadyHit = true;
            onHit.Invoke(col);
        }
    }
}