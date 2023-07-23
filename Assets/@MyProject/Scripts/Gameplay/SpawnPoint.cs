using UnityEngine;

namespace MyProject
{
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private float m_CheckCanSpawnRadius = 3.0f;
        [SerializeField] private LayerMask m_DisallowSpawnNearbyLayer;

        public bool canSpawn
        {
            get => Physics.CheckSphere(transform.position, m_CheckCanSpawnRadius, m_DisallowSpawnNearbyLayer) == false;
        }
    }
}