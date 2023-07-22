namespace MyProject
{
    public interface IPickupItem
    {
        bool canPickup { get; }

        event System.Action<Player> onPickup;

        void Pickup(Player _player);
    }
}