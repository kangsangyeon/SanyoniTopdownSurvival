using UnityEngine;
using UnityEngine.Serialization;

namespace MyProject
{
    [CreateAssetMenu(menuName = "MyProject/PlayerLevelPropertyDefinition")]
    public class PlayerLevelPropertyDefinition : ScriptableObject
    {
        [SerializeField] private int m_MaxLevel;
        public int maxLevel => m_MaxLevel;

        [SerializeField] private int[] m_RequiredExperienceToNextLevelGapArray;
        public int[] requiredExperienceToNextLevelGapArray => m_RequiredExperienceToNextLevelGapArray;
    }
}