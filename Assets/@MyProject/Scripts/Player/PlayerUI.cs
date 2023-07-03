using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth m_PlayerHealth;
    [SerializeField] private HealthBar m_HealthBar;

    private void Start()
    {
        m_HealthBar.fillRatio = (float)m_PlayerHealth.health / (float)m_PlayerHealth.MaxHealth;
        m_PlayerHealth.onHealthChanged_OnSync += amount =>
            m_HealthBar.fillRatio = (float)m_PlayerHealth.health / (float)m_PlayerHealth.MaxHealth;
        m_PlayerHealth.onHealthIsZero += () => 
            m_HealthBar.fillRatio = 0f;
    }
}