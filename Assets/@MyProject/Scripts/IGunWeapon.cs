using UnityEngine.Events;

namespace MyProject
{
    public interface IGunWeapon : IWeapon
    {
        int currentMagazineCount { get; }
        int maxMagazineCount { get; }
        float reloadDuration { get; }

        UnityEvent onCurrentMagazineCountChanged { get; }
        UnityEvent onMaxMagazineCountChanged { get; }
        UnityEvent onFire { get; }
        UnityEvent onReloadStart { get; }
        UnityEvent onReloadFinished { get; }
    }
}