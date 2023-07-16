using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyProject
{
    public class UI_GetAbility : MonoBehaviour
    {
        [SerializeField] private Scene_Game m_GameScene;
        [SerializeField] private UIDocument m_Document;

        private ListView m_ListView;
        private readonly List<UI_GetAbilityElement> m_AbilityElemList = new List<UI_GetAbilityElement>();
        private readonly List<AbilityDefinition> m_AbilityDefinitionList = new List<AbilityDefinition>();
        private Player m_Player;

        public event System.Action<AbilityDefinition> onSelectAbility;

        public void Initialize()
        {
            m_ListView = m_Document.rootVisualElement.Q<ListView>("ability-list");
            m_ListView.itemsSource = m_AbilityDefinitionList;
            m_ListView.makeItem = () => new UI_GetAbilityElement();
            m_ListView.bindItem = (_visualElem, _index) =>
            {
                UI_GetAbilityElement _elem = _visualElem as UI_GetAbilityElement;
                _elem.Bind(m_AbilityDefinitionList[_index]);
            };
            m_ListView.itemsChosen += (_item) =>
            {
                AbilityDefinition _definition = _item.First() as AbilityDefinition;
                onSelectAbility?.Invoke(_definition);
            };
        }

        public void Uninitialize()
        {
            m_AbilityDefinitionList.Clear();
            m_ListView.Rebuild();
            m_ListView.itemsSource = null;
            m_ListView.makeItem = null;
            m_ListView.bindItem = null;
        }

        public void Show()
        {
            m_Document.rootVisualElement.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            m_Document.rootVisualElement.style.display = DisplayStyle.None;
            m_AbilityDefinitionList.Clear();
            m_ListView.Rebuild();
        }

        public void AddItem(AbilityDefinition _definition)
        {
            m_AbilityDefinitionList.Add(_definition);
        }
    }
}