using UnityEngine;
using UnityEngine.UIElements;

namespace MyProject
{
    public class UI_PlayerElement : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<UI_PlayerElement, UxmlTraits>
        {
        }

        public UI_PlayerElement()
        {
            var _visualTree = Resources.Load<VisualTreeAsset>("UI/UI_Player");
            _visualTree.CloneTree(this);
        }
    }
}