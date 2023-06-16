using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int m_MaxHealth = 100;
    public int MaxHealth => m_MaxHealth;

    private int m_Health;
    public int Health => m_Health;

    public UnityEvent onHealthIsZero = new UnityEvent();
    public UnityEvent<int> onHealthChanged = new UnityEvent<int>();

    public void AddHealth(int _amount)
    {
        m_Health = m_Health + _amount;
        onHealthChanged.Invoke(_amount);

        if (m_Health <= 0)
        {
            m_Health = 0;
            onHealthIsZero.Invoke();
        }
    }

    private void Start()
    {
        m_Health = m_MaxHealth;
    }
}