using UnityEngine;

namespace MyProject
{
    public class PlayerGunRotation : MonoBehaviour
    {
        private void Update()
        {
            Vector2 _position = transform.position;
            Vector2 _mousePositionWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 _direction = _mousePositionWorld - _position;
            float _angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, _angle));
        }
    }
}