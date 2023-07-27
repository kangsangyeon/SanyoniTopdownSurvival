using System;
using TMPro;
using UnityEngine;

namespace MyProject
{
    public class UI_PlayerInfo : MonoBehaviour
    {
        [SerializeField] private Player m_Player;
        [SerializeField] private TextMeshProUGUI m_UI_Txt_PlayerName;

        private Action<string> m_OnSetPlayerNameAction;

        private void Awake()
        {
            m_OnSetPlayerNameAction = _name => { m_UI_Txt_PlayerName.text = _name; };
            m_Player.onSetPlayerName_OnClient += m_OnSetPlayerNameAction;
        }

        private void OnDestroy()
        {
            if (m_OnSetPlayerNameAction != null)
            {
                m_Player.onSetPlayerName_OnClient -= m_OnSetPlayerNameAction;
                m_OnSetPlayerNameAction = null;
            }
        }
    }
}