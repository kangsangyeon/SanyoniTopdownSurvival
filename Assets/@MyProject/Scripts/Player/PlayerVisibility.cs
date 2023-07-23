using System;
using MyProject.Event;
using UnityEngine;

namespace MyProject
{
    public class PlayerVisibility : MonoBehaviour
    {
        private Player m_Player;

        private MeshRenderer[] m_MeshRenderers;
        private bool[] m_RendererEnables;

        private Action<Player_OnDead_EventParam> m_PlayerOnDeadAction;
        private Action m_PlayerOnRespawnAction;

        private void Start()
        {
            m_Player = GetComponent<Player>();
            m_MeshRenderers = GetComponentsInChildren<MeshRenderer>();
            m_RendererEnables = new bool[m_MeshRenderers.Length];

            m_PlayerOnDeadAction = _param =>
            {
                for (int i = 0; i < m_MeshRenderers.Length; ++i)
                {
                    m_RendererEnables[i] = m_MeshRenderers[i].enabled;
                    if (m_RendererEnables[i])
                        m_MeshRenderers[i].enabled = false;
                }
            };
            m_Player.onDead_OnClient += m_PlayerOnDeadAction;

            m_PlayerOnRespawnAction = () =>
            {
                for (int i = 0; i < m_MeshRenderers.Length; ++i)
                {
                    if (m_RendererEnables[i])
                        m_MeshRenderers[i].enabled = true;
                }
            };
            m_Player.onRespawn_OnClient += m_PlayerOnRespawnAction;
        }

        private void OnDestroy()
        {
            if (m_PlayerOnDeadAction != null)
            {
                m_Player.onDead_OnClient -= m_PlayerOnDeadAction;
                m_PlayerOnDeadAction = null;
            }

            if (m_PlayerOnRespawnAction != null)
            {
                m_Player.onRespawn_OnClient -= m_PlayerOnRespawnAction;
                m_PlayerOnRespawnAction = null;
            }
        }
    }
}