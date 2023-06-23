using UnityEngine;

namespace MyProject
{
    public class OfflineGameplayDependencies : MonoBehaviour
    {
        private static OfflineGameplayDependencies s_Instance;

        [SerializeField] private SpawnManager m_SpawnManager;
        public static SpawnManager spawnManager => s_Instance.m_SpawnManager;

        // [SerializeField] private GameplayCanvases m_GameplayCanvases;
        // public static GameplayCanvases gameplayCanvases => s_Instance.m_GameplayCanvases;
        //
        // [SerializeField] private AudioManager m_AudioManager;
        // public static AudioManager audioManager => s_Instance.m_AudioManager;

        private void Awake()
        {
            s_Instance = this;
        }
    }
}