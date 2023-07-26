using DamageNumbersPro;
using UnityEngine;

namespace MyProject
{
    public class EntityHealth_Vfx : MonoBehaviour
    {
        [SerializeField] private EntityHealth m_Health;
        [SerializeField] private DamageNumber m_Prefab_Damage;
        [SerializeField] private Transform m_DamagePoint;

        private void SpawnDamageText(int _magnitude)
        {
            DamageNumber damageNumber = m_Prefab_Damage.Spawn(m_DamagePoint.position, _magnitude);
            damageNumber.spamGroup = $"damage_group_{m_Health.gameObject.name}";
            damageNumber.followedTarget = m_DamagePoint;
        }

        private void Awake()
        {
            m_Health.onHealthChanged_OnClient += _magnitude =>
            {
                if (_magnitude < 0)
                {
                    // 데미지입니다.
                    SpawnDamageText(_magnitude);
                }
                else
                {
                    // 힐입니다.
                }
            };
        }
    }
}