using UnityEngine.Events;

namespace MyProject
{
    public interface IGunWeapon : IWeapon
    {
        int currentMagazineCount { get; }
        int maxMagazineCount { get; }
        float reloadDuration { get; }
        float fireDelay { get; }
        float projectileSpeed { get; }

        event System.Action onCurrentMagazineCountChanged;
        event System.Action onFire;
        event System.Action onReloadStart;
        event System.Action onReloadFinished;
    }
}