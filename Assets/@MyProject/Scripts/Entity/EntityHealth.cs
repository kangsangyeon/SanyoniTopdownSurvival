using System.Collections.Generic;
using System.Collections.ObjectModel;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyProject;
using UnityEngine;

public class EntityHealth : NetworkBehaviour
{
    [SerializeField] private int m_MaxHealth = 100;

    public int maxHealth
    {
        get => m_MaxHealth;
        set
        {
            if (m_MaxHealth != value)
            {
                m_MaxHealth = value;
                onMaxHealthChanged_OnClient?.Invoke(value);
            }
        }
    }

    [SyncVar(WritePermissions = WritePermission.ServerOnly)]
    private int m_HealthSynced;

    private int m_Health;
    public int health => m_Health;

    private List<HealthModifier> m_DamageList = new List<HealthModifier>();
    public ReadOnlyCollection<HealthModifier> damageList => m_DamageList.AsReadOnly();

    public event System.Action onHealthIsZero_OnServer;
    public event System.Action onHealthIsZero_OnClient;
    public event System.Action<int> onHealthChanged_OnServer;
    public event System.Action<int> onHealthChanged_OnClient;
    public event System.Action<int> onMaxHealthChanged_OnClient;

    private bool m_Initialized;

    public void InitializeHealth()
    {
        if (m_Initialized)
            return;

        m_Initialized = true;
        if (InstanceFinder.IsServer)
        {
            ApplyModifier(new HealthModifier()
            {
                magnitude = maxHealth,
                source = this,
                sourceOwnerObject = null,
                time = Time.time
            });
        }
        else if (InstanceFinder.IsClient)
        {
            m_Health = m_HealthSynced;
            onHealthChanged_OnClient?.Invoke(m_HealthSynced);
        }
    }

    [Server]
    public void ApplyModifier(HealthModifier _healthModifier)
    {
        ObserversRpc_ChangeHealth(_healthModifier.magnitude);

        if (_healthModifier.magnitude < 0)
        {
            m_DamageList.Add(_healthModifier);
        }

        m_Health = m_Health + _healthModifier.magnitude;
        if (m_Health > m_MaxHealth)
            m_Health = m_MaxHealth;

        onHealthChanged_OnServer?.Invoke(_healthModifier.magnitude);
        onHealthChanged_OnClient?.Invoke(_healthModifier.magnitude);

        if (m_Health <= 0)
        {
            m_Health = 0;
            onHealthIsZero_OnServer?.Invoke();
            onHealthIsZero_OnClient?.Invoke();
        }

        m_HealthSynced = m_Health;
    }

    [ObserversRpc(ExcludeServer = true)]
    private void ObserversRpc_ChangeHealth(int _magnitude)
    {
        m_Health = m_Health + _magnitude;
        if (m_Health > m_MaxHealth)
            m_Health = m_MaxHealth;

        onHealthChanged_OnClient?.Invoke(_magnitude);
        if (m_Health <= 0)
        {
            m_Health = 0;
            onHealthIsZero_OnClient?.Invoke();
        }
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        InitializeHealth();
    }
}