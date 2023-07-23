using System;
using UnityEngine;

namespace MyProject
{
    public class AbilityItem_Vfx : MonoBehaviour
    {
        [SerializeField] private AbilityItem m_AbilityItem;

        private Action<AbilityDefinition> m_OnSetAbilityDefinitionAction;

        private void Awake()
        {
            m_OnSetAbilityDefinitionAction = _abilityDefinition =>
            {
                m_AbilityItem.SetModelPrefab(_abilityDefinition.prefabModel);
            };
            m_AbilityItem.onSetAbilityDefinition_onClient += m_OnSetAbilityDefinitionAction;
        }

        private void OnDestroy()
        {
            if (m_OnSetAbilityDefinitionAction != null)
            {
                m_AbilityItem.onSetAbilityDefinition_onClient -= m_OnSetAbilityDefinitionAction;
                m_OnSetAbilityDefinitionAction = null;
            }
        }
    }
}