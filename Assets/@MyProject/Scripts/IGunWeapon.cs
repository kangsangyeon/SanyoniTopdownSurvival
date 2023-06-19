using UnityEngine.Events;

namespace MyProject
{
    public interface IGunWeapon : IWeapon
    {
        int currentMagazineCount { get; }
        int maxMagazineCount { get; }

        UnityEvent onCurrentMagazineCountChanged { get; }
        UnityEvent onMaxMagazineCountChanged { get; }
    }
}