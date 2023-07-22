using FishNet.Component.Prediction;
using MyProject;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerCameraMovement : MonoBehaviour
{
    private Transform m_GraphicalTarget;
    [SerializeField] private Camera m_Camera;

    [SerializeField] private float m_PositionLerpSpeed = 50.0f;

    private void Start()
    {
        OfflineGameplayDependencies.gameScene.onSetMyPlayer_OnLocal += _player =>
        {
            if (_player.onStartClientCalled)
            {
                if (_player.IsOwner)
                {
                    // 이 카메라가 내 캐릭터에 부착되어 있는 카메라라면 이 카메라를 사용합니다.
                    gameObject.SetActive(true);
                    m_GraphicalTarget = _player.GetComponent<PredictedObject>().GetGraphicalObject();
                    return;
                }
            }
            else
            {
                _player.onStartClient += () =>
                {
                    if (_player.IsOwner)
                    {
                        // 이 카메라가 내 캐릭터에 부착되어 있는 카메라라면 이 카메라를 사용합니다.
                        gameObject.SetActive(true);
                        m_GraphicalTarget = _player.GetComponent<PredictedObject>().GetGraphicalObject();
                        _player.camera = m_Camera;

                        RenderPipelineManager.beginFrameRendering += (_context, _cameras) =>
                        {
                            transform.position =
                                Vector3.Lerp(
                                    transform.position, m_GraphicalTarget.position,
                                    m_PositionLerpSpeed * Time.deltaTime);
                        };
                        return;
                    }
                };
            }
        };

        // 이 카메라가 내 캐릭터에 부착된 카메라인지 아닌지 아직 모르기 때문에 우선 비활성화합니다.
        // 위에 부착한 이벤트에 의해, 내 캐릭터에 부착된 카메라라면 다시 활성화될 것입니다.
        gameObject.SetActive(false);
    }

    // private void LateUpdate()
    // {
    //     transform.position =
    //         Vector3.Lerp(
    //             transform.position, m_GraphicalTarget.position,
    //             m_PositionLerpSpeed * Time.deltaTime);
    // }
}