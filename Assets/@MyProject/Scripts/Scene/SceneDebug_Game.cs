using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyProject
{
    public class SceneDebug_Game : MonoBehaviour
    {
        [SerializeField] private Scene_Game m_Scene;

        void Start()
        {
            m_Scene.onPlayerKill.AddListener((_killer, _target) =>
            {
                Debug.Log($"player killed: {_killer.gameObject.name} kill {_target.gameObject.name}.");
            });
            m_Scene.onPlayerAdded.AddListener(_player => { Debug.Log($"player added: {_player.gameObject.name}"); });
        }
    }
}