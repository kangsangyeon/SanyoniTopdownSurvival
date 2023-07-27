using System;
using UnityEngine;

namespace MyProject
{
    public class MouseCursor : MonoBehaviour
    {
        private bool m_Playing = false;
        private Action<Player> m_OnSetMyPlayerAction;

        private void SetCursorVisibleAndLockMode(bool _visibleAndUnlock)
        {
            if (_visibleAndUnlock)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }

        private void Awake()
        {
            Action<Player> m_OnSetMyPlayerAction = _player =>
            {
                if (_player != null)
                {
                    SetCursorVisibleAndLockMode(false);
                    m_Playing = true;
                }
                else
                {
                    SetCursorVisibleAndLockMode(true);
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
                SetCursorVisibleAndLockMode(!Cursor.visible);
            }
        }

        private void OnDestroy()
        {
            SetCursorVisibleAndLockMode(true);

            if (m_OnSetMyPlayerAction != null)
            {
                OfflineGameplayDependencies.gameScene.onSetMyPlayer_OnLocal -= m_OnSetMyPlayerAction;
                m_OnSetMyPlayerAction = null;
            }
        }
    }
}