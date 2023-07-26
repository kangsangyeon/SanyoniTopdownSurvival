using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MyProject
{
    public class SpawnManager : MonoBehaviour
    {
        private SpawnPoint[] m_SpawnPoints;

        public Transform ReturnSpawnPoint()
        {
            for (int i = 0; i < 10; ++i)
            {
                // 스폰 가능한 위치를 최대 10번 찾습니다.
                int _index = Random.Range(0, m_SpawnPoints.Length);
                if (m_SpawnPoints[_index].canSpawn)
                    return m_SpawnPoints[_index].transform;
            }

            return null;
        }

        private void Start()
        {
            m_SpawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint")
                .Select(_go => _go.GetComponent<SpawnPoint>())
                .ToArray();
        }
    }
}