using System.Collections.Generic;
using UnityEngine;

namespace MyProject
{
    [CreateAssetMenu(menuName = "MyProject/AbilityDatabaseDefine")]
    public class AbilityDatabaseDefinition : ScriptableObject
    {
        [SerializeField] private List<AbilityDatabaseDefinition> m_DefinitionList = new List<AbilityDatabaseDefinition>();
        public List<AbilityDatabaseDefinition> definitionList => m_DefinitionList;
    }
}