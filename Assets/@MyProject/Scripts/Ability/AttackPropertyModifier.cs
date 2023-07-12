using UnityEngine;

namespace MyProject
{
    [CreateAssetMenu(menuName = "MyProject/AttackPropertyDecorator")]
    public class AttackPropertyModifier : ScriptableObject
    {
        [SerializeField] private float m_ProjectileSpeedMultiplier = 1.0f;
        public float projectileSpeedMultiplier => m_ProjectileSpeedMultiplier;

        [SerializeField] private float m_ProjectileDamageMultiplier = 1.0f;
        public float projectileDamageMultiplier => m_ProjectileDamageMultiplier;

        [SerializeField] private float m_ProjectileSizeMultiplier = 1.0f;
        public float projectileSizeMultiplier => m_ProjectileSizeMultiplier;

        public void Modify(AttackProperty _property)
        {
            _property.projectileSpeedMultiplier =
                _property.projectileSpeedMultiplier * m_ProjectileSpeedMultiplier;
            _property.projectileDamageMultiplier =
                _property.projectileDamageMultiplier * m_ProjectileDamageMultiplier;
            _property.projectileSizeMultiplier =
                _property.projectileSizeMultiplier * m_ProjectileSizeMultiplier;
        }
    }
}