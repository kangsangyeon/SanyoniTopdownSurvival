using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyProject
{
    [CreateAssetMenu(menuName = "MyProject/AbilityDefinition")]
    public class AbilityDefinition : ScriptableObject
    {
        [SerializeField] private string m_AbilityId = Guid.NewGuid().ToString();
        public string abilityId => m_AbilityId;

        [SerializeField] private string m_AbilityName;
        public string abilityName => m_AbilityName;

        [SerializeField] private Sprite m_Thumbnail;
        public Sprite thumbnail => m_Thumbnail;

        [SerializeField] private string m_Description;
        public string description => m_Description;

        [SerializeField] private GameObject m_Prefab_Model;
        public GameObject prefabModel => m_Prefab_Model;

        [SerializeField] private List<AbilityPropertyModifierDefinition> m_AbilityPropertyModifierDefinitionList =
            new List<AbilityPropertyModifierDefinition>();

        public IReadOnlyList<AbilityPropertyModifierDefinition> abilityPropertyModifierDefinitionList =>
            m_AbilityPropertyModifierDefinitionList;
    }
}