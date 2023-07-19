using FishNet.Object;
using MyProject.Struct;

namespace MyProject
{
    public class InvincibleWallObject : NetworkBehaviour, IDamageableEntity, IWallObject
    {
        #region IDamageableEntity

        public int maxTakableDamage => 0;
        public bool useConstantDamage => true;
        public int constantDamage => 0;

        public void TakeDamage(in DamageParam _hitParam, out int _appliedDamage)
        {
            _hitParam.healthModifier.magnitude = 0;
            _appliedDamage = 0;
        }

        #endregion

        #region IWallObject

        public EntityHealth health => null;
        public bool isInvincible => true;
        public bool isInvincibleTemporary => true;

        #endregion
    }
}