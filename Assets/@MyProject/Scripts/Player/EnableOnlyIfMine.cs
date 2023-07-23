using UnityEngine;

namespace MyProject
{
    public class EnableOnlyIfMine : MonoBehaviour
    {
        [SerializeField] private Player m_Player;

        private void Start()
        {
            if (m_Player.onStartClientCalled)
            {
                if (m_Player.IsOwner)
                {
                    // 이 게임 오브젝트가 내 캐릭터에 부착되어 있는 게임 오브젝트라면 이 게임 오브젝트를 사용합니다.
                    gameObject.SetActive(true);
                    return;
                }
            }
            else
            {
                m_Player.onStartClient += () =>
                {
                    if (m_Player.IsOwner)
                    {
                        // 이 게임 오브젝트가 내 캐릭터에 부착되어 있는 게임 오브젝트라면 이 게임 오브젝트를 사용합니다.
                        gameObject.SetActive(true);
                        return;
                    }
                };
            }

            // 이 게임 오브젝트가 내 캐릭터에 부착된 오브젝트인지 아닌지 아직 모르기 때문에 우선 비활성화합니다.
            // 위에 부착한 이벤트에 의해, 내 캐릭터에 부착된 오브젝트라면 다시 활성화될 것입니다.
            gameObject.SetActive(false);
        }
    }
}