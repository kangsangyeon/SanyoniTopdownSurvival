using UnityEngine;

namespace MyProject
{
    public class GameObjectHider : MonoBehaviour
    {
        private MeshRenderer[] m_MeshRenderers;
        private Collider[] m_Colliders;

        private void Awake()
        {
            m_MeshRenderers = GetComponentsInChildren<MeshRenderer>();
            m_Colliders = GetComponentsInChildren<Collider>();
        }

        public void Hide()
        {
            foreach (var _meshRenderer in m_MeshRenderers)
                _meshRenderer.enabled = false;

            foreach (var _collider in m_Colliders)
                _collider.enabled = false;
        }

        public void Show()
        {
            foreach (var _meshRenderer in m_MeshRenderers)
                _meshRenderer.enabled = true;

            foreach (var _collider in m_Colliders)
                _collider.enabled = true;
        }
    }
}