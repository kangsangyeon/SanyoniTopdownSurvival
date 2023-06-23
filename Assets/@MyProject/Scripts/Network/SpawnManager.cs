using UnityEngine;

namespace MyProject
{
    public class SpawnManager : MonoBehaviour
    {
        [SerializeField] private Transform m_SpawnPoint;

        public Transform ReturnSpawnPoint()
        {
            return m_SpawnPoint;
        }
    }
}