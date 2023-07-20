using UnityEngine;

namespace MyProject
{
    [CreateAssetMenu(menuName = "MyProject/AbilityPropertyModifierDefinition")]
    public class AbilityPropertyModifierDefinition : ScriptableObject
    {
        /* gun attack */

        [SerializeField] private float m_ReloadDurationMultiplier = 1.0f;
        public float reloadDurationMultiplier => m_ReloadDurationMultiplier;

        [SerializeField] private float m_FireDelayMultiplier = 1.0f;
        public float fireDelayMultiplier => m_FireDelayMultiplier;

        [SerializeField] private float m_MaxMagazineMultiplier = 1.0f;
        public float maxMagazineMultiplier => m_MaxMagazineMultiplier;

        [SerializeField] private float m_ProjectileSpeedMultiplier = 1.0f;
        public float projectileSpeedMultiplier => m_ProjectileSpeedMultiplier;

        [SerializeField] private float m_ProjectileDamageMultiplier = 1.0f;
        public float projectileDamageMultiplier => m_ProjectileDamageMultiplier;

        [SerializeField] private float m_ProjectileSizeMultiplier = 1.0f;
        public float projectileSizeMultiplier => m_ProjectileSizeMultiplier;

        [SerializeField] private int m_ProjectileCountPerShotAdditional = 0;
        public int projectileCountPerShotAdditional => m_ProjectileCountPerShotAdditional;

        [SerializeField] private float m_ProjectileSpreadAngleMultiplier = 1.0f;
        public float projectileSpreadAngleMultiplier => m_ProjectileSpreadAngleMultiplier;

        /* melee attack */

        [SerializeField] private float m_MeleeAttackDelayMultiplier = 1.0f;
        public float meleeAttackDelayMultiplier => m_MeleeAttackDelayMultiplier;

        [SerializeField] private float m_MeleeAttackDamageMagnitudeMultiplier = 1.0f;
        public float meleeAttackDamageMagnitudeMultiplier => m_MeleeAttackDamageMagnitudeMultiplier;
    }
}