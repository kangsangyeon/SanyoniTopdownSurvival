using System;
using System.Collections.Generic;
using FishNet;
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

        private Action<PlayerKillEventParam> m_OnPlayerKill_OnClient;

        public event System.Action<AbilityDefinition> onSelectAbility_OnClient;

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
            m_Document.gameObject.SetActive(true);
        }

        public void Hide()
        {
            m_Document.gameObject.SetActive(false);
        }

        private void RemoveAllElem()
        {
            foreach (var _elem in m_AbilityElemList)
                _elem.RemoveFromHierarchy();

            m_AbilityElemList.Clear();
        }

        private void CreateElem(AbilityDefinition _abilityDefinition)
        {
            UI_GetAbilityElement _elem = new UI_GetAbilityElement()
                { name = $"ability-elem [name: {_abilityDefinition.name}]" };
            m_Parent.hierarchy.Add(_elem);
            m_AbilityElemList.Add(_elem);

            RefreshElem(_elem, _abilityDefinition);
        }

        private void RefreshElem(UI_GetAbilityElement _elem, AbilityDefinition _abilityDefinition)
        {
            Label _abilityNameLabel = _elem.Q<Label>("ability-name");
            VisualElement _abilityThumbnail = _elem.Q<Label>("ability-thumbnail");
            Label _abilityDescriptionLabel = _elem.Q<Label>("ability-description");

            _abilityNameLabel.text = _abilityDefinition.abilityName;
            _abilityThumbnail.style.backgroundImage = new StyleBackground(_abilityDefinition.thumbnail);
            _abilityDescriptionLabel.text = _abilityDefinition.description;

            _elem.RegisterCallback<ClickEvent>(_evt =>
            {
                m_Player.AddAbility(_abilityDefinition);
                m_Document.gameObject.SetActive(false);
            });
        }
    }
}