using UnityEngine;

namespace MyProject
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private float m_Speed = 10.0f;
        private Quaternion m_TargetRotation;

        private void Rotate(float _rotationY)
        {
            m_TargetRotation =
                m_TargetRotation * Quaternion.Euler(0, _rotationY, 0);
        }

        private void Awake()
        {
            m_TargetRotation = transform.rotation;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Rotate(90.0f);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                Rotate(-90.0f);
            }

            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, m_TargetRotation, m_Speed);
        }
    }
}