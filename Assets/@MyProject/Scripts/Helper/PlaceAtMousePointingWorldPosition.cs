using UnityEngine;

namespace MyProject
{
    public class PlaceAtMousePointingWorldPosition : MonoBehaviour
    {
        [SerializeField] private LayerMask m_PointableLayer = int.MaxValue;

        private void LateUpdate()
        {
            var _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit _hit;
            Physics.Raycast(_ray, out _hit, 1000.0f, m_PointableLayer);
            if (_hit.collider != null)
            {
                transform.position = _hit.point;
                transform.rotation = Quaternion.LookRotation(_hit.normal);
            }
        }
    }
}