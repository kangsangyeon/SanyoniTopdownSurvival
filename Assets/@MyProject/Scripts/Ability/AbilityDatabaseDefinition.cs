using System.Collections.Generic;
using UnityEngine;

namespace MyProject
{
    [CreateAssetMenu(menuName = "MyProject/AbilityDatabaseDefine")]
    public class AbilityDatabaseDefinition : ScriptableObject
    {
        [SerializeField] private List<AbilityDefinition> m_DefinitionList = new List<AbilityDefinition>();
        public IReadOnlyList<AbilityDefinition> definitionList => m_DefinitionList;
    }
}