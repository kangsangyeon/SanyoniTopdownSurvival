using UnityEngine;

namespace MyProject
{
    public class ApplyPlayerAbility_ToPlayer : MonoBehaviour
    {
        [SerializeField] private Player m_Player;

        private void Awake()
        {
            m_Player.onAbilityPropertyRefreshed_OnClient += () =>
            {
                m_Player.health.maxHealth =
                    m_Player.abilityProperty.maxHealth
                    + m_Player.abilityProperty.maxHealthAddition;

                m_Player.movement.moveSpeed =
                    m_Player.abilityProperty.moveSpeed
                    + m_Player.abilityProperty.moveSpeedAddition;
            };
        }
    }
}