using System.Collections.Generic;
using UnityEngine;

namespace MyProject
{
    public class AbilityDatabase : MonoBehaviour
    {
        [SerializeField] private AbilityDatabaseDefinition m_Definition;

        private Dictionary<string, AbilityDefinition> m_AbilityDefinitionDict = new Dictionary<string, AbilityDefinition>();

        public AbilityDefinition Get(string _id) => m_AbilityDefinitionDict[_id];
    }
}