namespace MyProject
{
    public interface IObtainableItem
    {
        bool canObtain { get; }
        
        void OnObtain(Player _player);
    }
}