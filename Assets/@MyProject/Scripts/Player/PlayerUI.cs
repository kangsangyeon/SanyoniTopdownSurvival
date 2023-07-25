using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private EntityHealth m_PlayerHealth;
    [SerializeField] private UI_HealthBar m_HealthBar;

    private void Start()
    {
        m_HealthBar.fillRatio = (float)m_PlayerHealth.health / (float)m_PlayerHealth.maxHealth;
        m_PlayerHealth.onHealthChanged_OnSync += amount =>
            m_HealthBar.fillRatio = (float)m_PlayerHealth.health / (float)m_PlayerHealth.maxHealth;
        m_PlayerHealth.onHealthIsZero_OnSync += () => 
            m_HealthBar.fillRatio = 0f;
    }
}