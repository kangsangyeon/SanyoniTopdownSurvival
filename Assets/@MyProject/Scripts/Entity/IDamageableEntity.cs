namespace MyProject
{
    public interface IDamageableEntity
    {
        public void TakeDamage(int _magnitude, object _source, float _time, out int _appliedDamage);
    }
}