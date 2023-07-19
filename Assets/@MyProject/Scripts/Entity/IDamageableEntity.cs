using MyProject.Struct;

namespace MyProject
{
    public interface IDamageableEntity
    {
        public int maxTakableDamage { get; }
        public bool useConstantDamage { get; }
        public int constantDamage { get; }
        public void TakeDamage(in DamageParam _hitParam, out int _appliedDamage);
    }
}