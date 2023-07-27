namespace MyProject
{
    [System.Serializable]
    public class AbilityProperty
    {
        /* character */
        public readonly int maxHealth = 200;
        public int maxHealthAddition;

        public readonly int maximumMaxHealth = 400;
        public readonly int minimumMaxHealth = 200;

        public readonly float moveSpeed = 4.0f;
        public float moveSpeedAddition;

        public readonly float maximumMoveSpeed = 10.0f;
        public readonly float minimumMoveSpeed = 2.0f;

        /* gun attack */

        public readonly float reloadDuration = 3.0f;
        public float reloadDurationAddition;

        public readonly float fireDelay = 0.2f;
        public float fireDelayAddition;

        public readonly float maximumFireDelay = 0.5f; // 최악
        public readonly float minimumFireDelay = 0.1f; // 최선

        public readonly int maxMagazine = 20;
        public int maxMagazineAddition;

        public readonly float projectileSpeed = 3.0f;
        public float projectileSpeedAddition;

        public readonly float maximumProjectileSpeed = 10;
        public readonly float minimumProjectileSpeed = 0.5f;

        public readonly int projectileDamage = -20;
        public int projectileDamageAddition;

        public readonly int maximumProjectileDamage = -10; // 최악
        public readonly int minimumProjectileDamage = -40; // 최선

        public float projectileSizeAddition;

        public readonly int projectileCountPerShot = 1;
        public int projectileCountPerShotAddition;

        public readonly int maximumProjectileCountPerShot = 5;
        public readonly int minimumProjectileCountPerShot = 1;

        public float projectileShotAngleRange =>
            projectileCountPerShotAddition * 36.0f;

        /* melee attack */

        public readonly float meleeAttackDelay = 0.2f;
        public float meleeAttackDelayAddition;

        public readonly float meleeAttackInterval = 0.7f;
        public float meleeAttackIntervalAddition;

        public readonly float maximumMeleeAttackInterval = 1.5f; // 최악
        public readonly float minimumMeleeAttackInterval = 0.2f; // 최선

        public readonly int meleeAttackDamageMagnitude = -40;
        public int meleeAttackDamageMagnitudeAddition;

        public readonly int maximumMeleeAttackDamageMagnitude = -20; // 최악
        public readonly int minimumMeleeAttackDamageMagnitude = -80; // 최선

        public float meleeAttackSizeAddition;

        public readonly float maximumMeleeAttackSizeAddition = 1.0f;
        public readonly float minimumMeleeAttackSizeAddition = 0.0f;

        /* sword */

        public readonly int swordProjectileRequiredStack = 5;
        public int swordProjectileRequiredStackAddition;

        public readonly int maximumSwordProjectileRequiredStack = 8; // 최악
        public readonly int minimumSwordProjectileRequiredStack = 2; // 최선
    }
}