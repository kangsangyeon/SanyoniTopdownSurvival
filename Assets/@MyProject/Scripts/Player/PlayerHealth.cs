using System.Collections.Generic;
using System.Collections.ObjectModel;
using MyProject;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int m_MaxHealth = 100;
    public int MaxHealth => m_MaxHealth;

    private int m_Health;
    public int Health => m_Health;

    private List<HealthModifier> m_DamageList = new List<HealthModifier>();
    public ReadOnlyCollection<HealthModifier> damageList => m_DamageList.AsReadOnly();

    public UnityEvent onHealthIsZero = new UnityEvent();
    public UnityEvent<int> onHealthChanged = new UnityEvent<int>();

    public void ApplyModifier(HealthModifier _healthModifier)
    {
        if (_healthModifier.magnitude < 0)
        {
            m_DamageList.Add(_healthModifier);
        }

        m_Health = m_Health + _healthModifier.magnitude;
        onHealthChanged.Invoke(_healthModifier.magnitude);

        if (m_Health <= 0)
        {
            m_Health = 0;
            onHealthIsZero.Invoke();
        }
    }

    private void Awake()
    {
        m_Health = m_MaxHealth;
    }
}