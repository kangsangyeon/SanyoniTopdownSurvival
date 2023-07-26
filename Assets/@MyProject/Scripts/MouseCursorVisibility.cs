using System;
using UnityEngine;

namespace MyProject
{
    public class MouseCursorVisibility : MonoBehaviour
    {
        private bool m_Playing = false;
        private Action<Player> m_OnSetMyPlayerAction;

        private void Awake()
        {
            Action<Player> m_OnSetMyPlayerAction = _player =>
            {
                if (_player != null)
                {
                    Cursor.visible = false;
                    m_Playing = true;
                }
                else
                {
                    Cursor.visible = true;
                    m_Playing = false;
                }
            };
            OfflineGameplayDependencies.gameScene.onSetMyPlayer_OnLocal += m_OnSetMyPlayerAction;
        }

        private void Update()
        {
            if (m_Playing == false)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.visible = !Cursor.visible;
            }
        }

        private void OnDestroy()
        {
            Cursor.visible = true;

            if (m_OnSetMyPlayerAction != null)
            {
                OfflineGameplayDependencies.gameScene.onSetMyPlayer_OnLocal -= m_OnSetMyPlayerAction;
                m_OnSetMyPlayerAction = null;
            }
        }
    }
}