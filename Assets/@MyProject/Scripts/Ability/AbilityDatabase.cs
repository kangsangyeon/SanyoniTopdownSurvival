using System.Collections.Generic;
using UnityEngine;

namespace MyProject
{
    public class AbilityDatabase : MonoBehaviour
    {
        [SerializeField] private AbilityDatabaseDefinition m_Definition;

        private Dictionary<string, AbilityDefinition> m_AbilityDefinitionDict =
            new Dictionary<string, AbilityDefinition>();

        public void Initialize()
        {
            foreach (var _abilityDefinition in m_Definition.definitionList)
                m_AbilityDefinitionDict.Add(_abilityDefinition.abilityId, _abilityDefinition);
        }

        public void Uninitialize()
        {
            m_AbilityDefinitionDict.Clear();
        }

        public AbilityDefinition GetAbility(string _id) => m_AbilityDefinitionDict[_id];

        public AbilityDefinition GetRandomAbility()
        {
            int _index = Random.Range(0, m_Definition.definitionList.Count);
            return m_Definition.definitionList[_index];
        }
    }
}