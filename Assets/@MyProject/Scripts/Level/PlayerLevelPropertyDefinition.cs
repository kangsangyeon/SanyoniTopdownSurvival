using UnityEngine;

namespace MyProject
{
    [CreateAssetMenu(menuName = "MyProject/PlayerLevelPropertyDefinition")]
    public class PlayerLevelPropertyDefinition : ScriptableObject
    {
        [SerializeField] private int m_MaxLevel;
        public int maxLevel => m_MaxLevel;

        [SerializeField] private int[] m_RequiredExperienceArray;
        public int[] requiredExperienceArray => m_RequiredExperienceArray;
    }
}