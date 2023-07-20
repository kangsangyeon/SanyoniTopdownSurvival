namespace MyProject
{
    public interface IGunWeapon : IWeapon
    {
        int currentMagazineCount { get; }
        int maxMagazineCount { get; }
        float reloadDuration { get; }
        float projectileSpeed { get; }
        float projectileScaleMultiplier { get; }
        int projectileCountPerShot { get; }
        float projectileShotAngleRange { get; }

        event System.Action onCurrentMagazineCountChanged;
        event System.Action onReloadStart;
        event System.Action onReloadFinished;

        void QueueReload();
    }
}