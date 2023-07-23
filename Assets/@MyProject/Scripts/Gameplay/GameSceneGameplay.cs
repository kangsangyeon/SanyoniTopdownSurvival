using System.Collections;
using FishNet;
using UnityEngine;

namespace MyProject
{
    public class GameSceneGameplay : MonoBehaviour
    {
        public event System.Action onGameReady;
        public event System.Action onGameStart;
        public event System.Action onGameEnd;

        private void StartGame()
        {
            StartCoroutine(GameCoroutine());
        }

        private IEnumerator GameCoroutine()
        {
            yield return null;

            // 카운트 다운을 시작합니다.


            // 게임을 시작합니다.

            // 게임 도중, 어느 한 플레이어가 먼저 5킬을 달성하거나
            // 시간이 다 된 경우 게임이 종료됩니다.

            // 게임을 끝냅니다.
        }

        private void Start()
        {
            if (InstanceFinder.IsServer)
            {
                // 최소 인원 2명이 모이면 호스트 플레이어가 게임 시작을 누를 수 있습니다.
                // 만약 플레이 도중 2명 이하가 된다면, 남은 플레이어가 승리합니다.
                // 게임 시작 도중에는 다른 게임을 시작할 수 없습니다. 이를 막습니다.
                // (예정) 게임 시작 전 플레이어들은 서로에게 데미지를 입히지 않고 자유롭게 공격도 하고 맵 오브젝트들을 부수며 뛰어놀 수 있습니다.

                OfflineGameplayDependencies.gameScene.onPlayerAdded_OnServer += _player => { };

                OfflineGameplayDependencies.gameScene.onPlayerRemoved_OnServer += _player => { };
            }
        }

        private void OnDestroy()
        {
            // TODO: 이벤트를 해제합니다.
        }
    }
}