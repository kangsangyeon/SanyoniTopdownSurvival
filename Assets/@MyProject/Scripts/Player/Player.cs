using UnityEngine;

namespace MyProject
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerHealth m_Health;
        public PlayerHealth health => m_Health;

        private int m_KillCount = 0;
        public int killCount => m_KillCount;
    }
}