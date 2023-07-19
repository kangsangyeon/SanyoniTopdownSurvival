using System.Collections.Generic;
using System.Collections.ObjectModel;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyProject;
using UnityEngine;

public class EntityHealth : NetworkBehaviour
{
    [SerializeField] private int m_MaxHealth = 100;
    public int MaxHealth => m_MaxHealth;

    [SyncVar(OnChange = nameof(SyncVar_OnHealthChanged), WritePermissions = WritePermission.ServerOnly)]
    private int m_Health;

    public int health => m_Health;

    private List<HealthModifier> m_DamageList = new List<HealthModifier>();
    public ReadOnlyCollection<HealthModifier> damageList => m_DamageList.AsReadOnly();

    public event System.Action onHealthIsZero_OnServer;
    public event System.Action onHealthIsZero_OnSync;
    public event System.Action<int> onHealthChanged_OnServer;
    public event System.Action<int> onHealthChanged_OnSync;

    public void SyncVar_OnHealthChanged(int _prev, int _next, bool _asServer)
    {
        int _amount = _next - _prev;
        onHealthChanged_OnSync?.Invoke(_amount);
        if (_next <= 0)
        {
            onHealthIsZero_OnSync?.Invoke();
        }
    }

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

        onHealthChanged_OnServer?.Invoke(_healthModifier.magnitude);

        if (m_Health <= 0)
        {
            m_Health = 0;
            onHealthIsZero_OnServer?.Invoke();
        }
    }

    private void Awake()
    {
        m_Health = m_MaxHealth;
    }
}