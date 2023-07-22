namespace MyProject
{
    public interface IObtainableItem
    {
        bool canObtain { get; }

        event System.Action<Player> onObtain;
        
        void Obtain(Player _player);
    }
}