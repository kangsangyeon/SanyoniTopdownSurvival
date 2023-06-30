using System.Collections;
using System.Collections.Generic;
using FishNet;
using UnityEngine;

namespace MyProject
{
    public class SceneDebug_Game : MonoBehaviour
    {
        [SerializeField] private Scene_Game m_Scene;

        void Start()
        {
            if (InstanceFinder.IsServer)
            {
                m_Scene.onPlayerKill_OnServer.AddListener((_killer, _target) =>
                    Debug.Log($"player killed: {_killer.gameObject.name} kill {_target.gameObject.name}."));
                m_Scene.onPlayerAdded_OnServer.AddListener(_player =>
                    Debug.Log($"player added: {_player.gameObject.name}"));
            }
        }
    }
}