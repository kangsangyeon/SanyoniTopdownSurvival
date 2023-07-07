using System;
using FishNet;
using MyProject.Event;
using UnityEngine;

namespace MyProject
{
    public class PlayerFX : MonoBehaviour
    {
        [SerializeField] private GameObject m_Prefab_DeadFX;

        private Action<Player_OnDead_EventParam> m_PlayerOnDead_OnClient;

        private void Start()
        {
            Player _player = GetComponent<Player>();

            m_PlayerOnDead_OnClient = _param =>
            {
                GameObject _fx = Instantiate(m_Prefab_DeadFX, transform.position, Quaternion.identity);
                Destroy(_fx, 1.0f);
            };

            _player.onDead_OnClient += m_PlayerOnDead_OnClient;
            if (Application.isBatchMode == false)
            {
                _player.onDead_OnServer += _source =>
                {
                    if (InstanceFinder.IsClient == false)
                        m_PlayerOnDead_OnClient(new Player_OnDead_EventParam() { });
                };
            }
        }
    }
}