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
    }
}
