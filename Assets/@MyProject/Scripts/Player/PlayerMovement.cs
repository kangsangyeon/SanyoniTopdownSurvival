using UnityEngine;

namespace MyProject
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 4f;

        private Rigidbody2D m_Rigidbody;
        private Vector2 m_Movement;

        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            m_Movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (m_Movement.magnitude >= 1.0f)
            {
                m_Movement.Normalize();
            }
        }

        private void FixedUpdate()
        {
            m_Rigidbody.MovePosition(
                m_Rigidbody.position
                + m_Movement * moveSpeed * Time.fixedDeltaTime);
        }
    }
}