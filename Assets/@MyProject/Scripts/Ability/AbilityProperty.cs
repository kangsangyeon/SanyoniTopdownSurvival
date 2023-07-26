namespace MyProject
{
    [System.Serializable]
    public class AbilityProperty
    {
        /* character */
        public readonly int maxHealth = 200;
        public int maxHealthAddition;

        public readonly float moveSpeed = 4.0f;
        public float moveSpeedAddition;

        /* gun attack */

        public readonly float reloadDuration = 3.0f;
        public float reloadDurationAddition;

        public readonly float fireDelay = 0.2f;
        public float fireDelayAddition;

        public readonly int maxMagazine = 20;
        public int maxMagazineAddition;

        public readonly float projectileSpeed = 10.0f;
        public float projectileSpeedAddition;

        public readonly int projectileDamage = -15;
        public int projectileDamageAddition;

        public float projectileSizeAddition;

        public readonly int projectileCountPerShot = 1;
        public int projectileCountPerShotAddition;

        public readonly float projectileShotAngleRange = 20.0f;
        public float projectileShotAngleRangeAddition;

        /* melee attack */

        public readonly float meleeAttackDelay = 0.1f;
        public float meleeAttackDelayAddition;

        public readonly float meleeAttackInterval = 0.5f;
        public float meleeAttackIntervalAddition;
        
        public readonly int meleeAttackDamageMagnitude = -40;
        public int meleeAttackDamageMagnitudeAddition;

        public float meleeAttackSizeAddition;

        /* sword */

        public readonly int swordProjectileRequiredStack = 3;
        public int swordProjectileRequiredStackAddition;
    }
}