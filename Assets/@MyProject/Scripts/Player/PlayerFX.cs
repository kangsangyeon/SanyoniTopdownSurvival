using UnityEngine;

namespace MyProject
{
    public class PlayerFX : MonoBehaviour
    {
        [SerializeField] private GameObject m_Prefab_DeadFX;

        private void Start()
        {
            Player _player = GetComponent<Player>();

            _player.onDead += killer =>
            {
                GameObject _fx = Instantiate(m_Prefab_DeadFX, transform.position, Quaternion.identity);
                Destroy(_fx, 1.0f);
            };
        }
    }
}