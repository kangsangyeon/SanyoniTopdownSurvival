using UnityEngine;

namespace MyProject
{
    public class OfflineGameplayDependencies : MonoBehaviour
    {
        private static OfflineGameplayDependencies s_Instance;

        [SerializeField] private SpawnManager m_SpawnManager;
        public static SpawnManager spawnManager => s_Instance.m_SpawnManager;

        [SerializeField] private ObjectPoolManager m_ObjectPoolManager;
        public static ObjectPoolManager objectPoolManager => s_Instance.m_ObjectPoolManager;

        [SerializeField] private AbilityDatabase m_AbilityDatabase;
        public static AbilityDatabase abilityDatabase => s_Instance.m_AbilityDatabase;

        [SerializeField] private Scene_Game m_GameScene;
        public static Scene_Game gameScene => s_Instance.m_GameScene;

        // [SerializeField] private GameplayCanvases m_GameplayCanvases;
        // public static GameplayCanvases gameplayCanvases => s_Instance.m_GameplayCanvases;
        //
        // [SerializeField] private AudioManager m_AudioManager;
        // public static AudioManager audioManager => s_Instance.m_AudioManager;

        private void Awake()
        {
            s_Instance = this;
            m_AbilityDatabase.Initialize();
        }

        private void OnDestroy()
        {
            s_Instance = null;
            m_AbilityDatabase.Uninitialize();
        }
    }
}