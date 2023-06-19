using System;
using UnityEngine;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    [SerializeField] private Collider2D m_Collider;
    [SerializeField] private float m_Speed = 8f;
    [SerializeField] private float m_LiveDuration = 1f;

    private Vector3 m_Direction;
    private float m_StartTime;
    private bool m_AlreadyHit = false;

    public UnityEvent onLifeEnd = new UnityEvent();
    public UnityEvent<Collider2D> onHit = new UnityEvent<Collider2D>();

    public void Refresh(Vector3 _direction, float _startTime)
    {
        m_Direction = _direction;
        m_StartTime = _startTime;
        m_AlreadyHit = false;
    }

    private void Update()
    {
        transform.position = transform.position + m_Direction * m_Speed * Time.deltaTime;

        if (Time.time - m_StartTime >= m_LiveDuration)
        {
            onLifeEnd.Invoke();
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