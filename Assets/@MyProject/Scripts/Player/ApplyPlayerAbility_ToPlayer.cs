using UnityEngine;

namespace MyProject
{
    public class ApplyPlayerAbility_ToPlayer : MonoBehaviour
    {
        [SerializeField] private Player m_Player;

        private void Awake()
        {
            m_Player.health.maxHealth =
                Mathf.Clamp(m_Player.abilityProperty.maxHealth
                            + m_Player.abilityProperty.maxHealthAddition,
                    m_Player.abilityProperty.minimumMaxHealth,
                    m_Player.abilityProperty.maximumMaxHealth);

            m_Player.movement.moveSpeed =
                Mathf.Clamp(m_Player.abilityProperty.moveSpeed
                            + m_Player.abilityProperty.moveSpeedAddition,
                    m_Player.abilityProperty.minimumMoveSpeed,
                    m_Player.abilityProperty.maximumMoveSpeed);

            m_Player.onAbilityPropertyRefreshed_OnClient += () =>
            {
                m_Player.health.maxHealth =
                    Mathf.Clamp(m_Player.abilityProperty.maxHealth
                                + m_Player.abilityProperty.maxHealthAddition,
                        m_Player.abilityProperty.minimumMaxHealth,
                        m_Player.abilityProperty.maximumMaxHealth);

                m_Player.movement.moveSpeed =
                    Mathf.Clamp(m_Player.abilityProperty.moveSpeed
                                + m_Player.abilityProperty.moveSpeedAddition,
                        m_Player.abilityProperty.minimumMoveSpeed,
                        m_Player.abilityProperty.maximumMoveSpeed);
            };
        }
    }
}