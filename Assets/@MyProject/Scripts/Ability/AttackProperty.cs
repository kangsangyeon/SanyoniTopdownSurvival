namespace MyProject
{
    public class AttackProperty
    {
        public readonly float reloadDuration = 3.0f;
        public float reloadDurationMultiplier = 1.0f;

        public readonly float fireDelay = 0.2f;
        public float fireDelayMultiplier = 1.0f;

        public readonly int maxMagazine = 20;
        public float maxMagazineMultiplier = 1.0f;

        public readonly float projectileSpeed = 10.0f;
        public float projectileSpeedMultiplier = 1.0f;

        public readonly int projectileDamage = -15;
        public float projectileDamageMultiplier = 1.0f;

        public float projectileSizeMultiplier = 1.0f;

        public int projectileCountPerShot = 1;

        public float projectileSpreadAngle = 20.0f;
    }
}