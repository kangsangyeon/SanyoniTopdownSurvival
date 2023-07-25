using UnityEngine;
using UnityEngine.Serialization;

namespace MyProject
{
    [CreateAssetMenu(menuName = "MyProject/AbilityPropertyModifierDefinition")]
    public class AbilityPropertyModifierDefinition : ScriptableObject
    {
        /* gun attack */

        [SerializeField] private float m_ReloadDurationAddition;
        public float reloadDurationAddition => m_ReloadDurationAddition;

        [SerializeField] private float m_FireDelayAddition;
        public float fireDelayAddition => m_FireDelayAddition;

        [SerializeField] private int m_MaxMagazineAddition;
        public int maxMagazineAddition => m_MaxMagazineAddition;

        [SerializeField] private float m_ProjectileSpeedAddition;
        public float projectileSpeedAddition => m_ProjectileSpeedAddition;

        [SerializeField] private int m_ProjectileDamageAddition;
        public int projectileDamageAddition => m_ProjectileDamageAddition;

        [SerializeField] private float m_ProjectileSizeAddition;
        public float projectileSizeAddition => m_ProjectileSizeAddition;

        [SerializeField] private int m_ProjectileCountPerShotAddition = 0;
        public int projectileCountPerShotAddition => m_ProjectileCountPerShotAddition;

        [SerializeField] private float m_ProjectileSpreadAngleAddition;
        public float projectileSpreadAngleAddition => m_ProjectileSpreadAngleAddition;

        /* melee attack */

        [SerializeField] private float m_MeleeAttackDelayAddition;
        public float meleeAttackDelayAddition => m_MeleeAttackDelayAddition;

        [SerializeField] private int m_MeleeAttackDamageMagnitudeAddition;
        public int meleeAttackDamageMagnitudeAddition => m_MeleeAttackDamageMagnitudeAddition;

        /* sword */

        [SerializeField] private int m_SwordProjectileRequiredStackAddition;
        public int swordProjectileRequiredStackAddition => m_SwordProjectileRequiredStackAddition;
    }
}