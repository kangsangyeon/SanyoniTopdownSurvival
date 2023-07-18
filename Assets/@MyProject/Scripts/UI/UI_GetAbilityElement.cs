using UnityEngine;
using UnityEngine.UIElements;

namespace MyProject
{
    public class UI_GetAbilityElement : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<UI_GetAbilityElement, UxmlTraits>
        {
        }

        public UI_GetAbilityElement()
        {
            var _visualTree = Resources.Load<VisualTreeAsset>("UI/UI_GetAbilityElem");
            _visualTree.CloneTree(this);
        }

        private AbilityDefinition m_Definition;
        public AbilityDefinition definition => m_Definition;

        public void Bind(AbilityDefinition _abilityDefinition)
        {
            m_Definition = _abilityDefinition;

            Label _abilityNameLabel = this.Q<Label>("ability-name");
            Label _abilityDescriptionLabel = this.Q<Label>("ability-description");
            VisualElement _abilityThumbnail = this.Q("ability-thumbnail");

            _abilityNameLabel.text = _abilityDefinition.abilityName;
            _abilityDescriptionLabel.text = _abilityDefinition.description;

            if (_abilityDefinition.thumbnail)
            {
                _abilityThumbnail.style.backgroundColor = new Color(0, 0, 0, 0);
                _abilityThumbnail.style.backgroundImage = new StyleBackground(_abilityDefinition.thumbnail);
            }

            // this.RegisterCallback<ClickEvent>(_evt => onSelectAbility?.Invoke(_abilityDefinition));
        }
    }
}