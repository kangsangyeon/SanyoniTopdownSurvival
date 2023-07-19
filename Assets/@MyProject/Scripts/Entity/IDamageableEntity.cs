using MyProject.Struct;

namespace MyProject
{
    public interface IDamageableEntity
    {
        public void TakeDamage(in DamageParam _hitParam, out int _appliedDamage);
    }
}