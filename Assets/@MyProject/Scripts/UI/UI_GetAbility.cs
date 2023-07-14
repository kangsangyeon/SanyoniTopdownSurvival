using System;
using System.Collections.Generic;
using MyProject.Event;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyProject
{
    public class UI_GetAbility : MonoBehaviour
    {
        [SerializeField] private Scene_Game m_GameScene;
        [SerializeField] private UIDocument m_Document;

        private VisualElement m_Parent;
        private readonly List<UI_GetAbilityElement> m_AbilityElemList = new List<UI_GetAbilityElement>();
        private Player m_Player;

        public event System.Action<AbilityDefinition> onSelectAbility;

        public void Initialize()
        {
            m_Parent = m_Document.rootVisualElement.Q("ability-list");
        }

        public void Uninitialize()
        {
            RemoveAllElem();
        }

        public void Show()
        {
            m_Document.rootVisualElement.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            m_Document.rootVisualElement.style.display = DisplayStyle.None;
            RemoveAllElem();
        }

        public void CreateElem(AbilityDefinition _abilityDefinition)
        {
            UI_GetAbilityElement _elem = new UI_GetAbilityElement()
                { name = $"ability-elem [name: {_abilityDefinition.name}]" };
            m_Parent.hierarchy.Add(_elem);
            m_AbilityElemList.Add(_elem);

            RefreshElem(_elem, _abilityDefinition);
        }

        private void RemoveAllElem()
        {
            foreach (var _elem in m_AbilityElemList)
                _elem.RemoveFromHierarchy();

            m_AbilityElemList.Clear();
        }

        private void RefreshElem(UI_GetAbilityElement _elem, AbilityDefinition _abilityDefinition)
        {
            Label _abilityNameLabel = _elem.Q<Label>("ability-name");
            Label _abilityDescriptionLabel = _elem.Q<Label>("ability-description");
            VisualElement _abilityThumbnail = _elem.Q("ability-thumbnail");

            _abilityNameLabel.text = _abilityDefinition.abilityName;
            _abilityDescriptionLabel.text = _abilityDefinition.description;

            if (_abilityDefinition.thumbnail)
            {
                _abilityThumbnail.style.backgroundColor = new Color(0, 0, 0, 0);
                _abilityThumbnail.style.backgroundImage = new StyleBackground(_abilityDefinition.thumbnail);
            }

            _elem.RegisterCallback<ClickEvent>(_evt => onSelectAbility?.Invoke(_abilityDefinition));
        }
    }
}