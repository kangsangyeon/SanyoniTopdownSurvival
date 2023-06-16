using UnityEngine;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float m_Speed = 8f;
    [SerializeField] private float m_LiveDuration = 1f;

    public Vector3 Direction;
    public float StartTime;
    public UnityEvent onLifeEnd = new UnityEvent();
    public UnityEvent<Collider2D> onHit = new UnityEvent<Collider2D>();

    private void Update()
    {
        transform.position = transform.position + Direction * m_Speed * Time.deltaTime;

        if (Time.time - StartTime >= m_LiveDuration)
        {
            onLifeEnd.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            onHit.Invoke(col);
        }
    }
}