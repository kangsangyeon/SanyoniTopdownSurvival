using System.Collections.Generic;
using System.Collections.ObjectModel;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyProject;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private int m_MaxHealth = 100;
    public int MaxHealth => m_MaxHealth;

    [SyncVar] private int m_Health;
    public int health => m_Health;

    private List<HealthModifier> m_DamageList = new List<HealthModifier>();
    public ReadOnlyCollection<HealthModifier> damageList => m_DamageList.AsReadOnly();

    public UnityEvent onHealthIsZero = new UnityEvent();
    public UnityEvent<int> onHealthChanged = new UnityEvent<int>();

    [Server]
    public void Server_OnHealthIsZero()
    {
        onHealthIsZero.Invoke();
        Observers_OnHealthIsZero();
    }

    [Server]
    public void Server_OnHealthChanged(int _amount)
    {
        onHealthChanged.Invoke(_amount);
        Observers_OnHealthChanged(_amount);
    }

    [ObserversRpc(ExcludeServer = true)]
    public void Observers_OnHealthIsZero() => onHealthIsZero.Invoke();

    [ObserversRpc(ExcludeServer = true)]
    public void Observers_OnHealthChanged(int _amount) => onHealthChanged.Invoke(_amount);

    [Server]
    public void ApplyModifier(HealthModifier _healthModifier)
    {
        if (_healthModifier.magnitude < 0)
        {
            m_DamageList.Add(_healthModifier);
        }

        m_Health = m_Health + _healthModifier.magnitude;
        if (m_Health > m_MaxHealth)
            m_Health = m_MaxHealth;

        Server_OnHealthChanged(_healthModifier.magnitude);

        if (m_Health <= 0)
        {
            m_Health = 0;
            Server_OnHealthIsZero();
        }
    }

    private void Awake()
    {
        m_Health = m_MaxHealth;
    }
}