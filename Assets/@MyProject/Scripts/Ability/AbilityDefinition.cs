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

        [SerializeField] private List<AttackPropertyModifierDefine> m_AttackPropertyModifierDefineList = new List<AttackPropertyModifierDefine>();
        public IReadOnlyList<AttackPropertyModifierDefine> attackPropertyModifierDefineList => m_AttackPropertyModifierDefineList;
    }
}