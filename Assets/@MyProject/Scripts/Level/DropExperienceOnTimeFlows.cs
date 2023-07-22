using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Transporting;
using UnityEngine;

namespace MyProject
{
    public class DropExperienceOnTimeFlows : MonoBehaviour, IDropExperience
    {
        [SerializeField] private float m_TimeInterval = 1.0f;

        private Coroutine m_Coroutine;
        private List<ILevelable> m_PlayerLevelableList;

        private Action<ServerConnectionStateArgs> m_OnServerConnectionStateAction;
        private Action<Player> m_OnPlayerAddedAction;
        private Action<Player> m_OnPlayerRemovedAction;

        #region IDropExperience

        [SerializeField] private int m_ExperienceAmount = 1;
        public int experienceAmount => m_ExperienceAmount;

        #endregion

        private IEnumerator IncreaseScoreCoroutine()
        {
            while (true)
            {
                m_PlayerLevelableList.ForEach(p =>
                    p.AddExperience(new ExperienceParam()
                    {
                        tick = InstanceFinder.TimeManager.Tick,
                        source = this,
                        sourceNetworkObjectId = null,
                        experience = experienceAmount
                    }, out int _addedExperience)
                );
                yield return new WaitForSeconds(m_TimeInterval);
            }
        }

        private void InitializeTimeManagerEvents()
        {
            m_PlayerLevelableList =
                OfflineGameplayDependencies.gameScene.playerList.Select(p => p.GetComponent<ILevelable>()).ToList();
            m_OnPlayerAddedAction = _player => { m_PlayerLevelableList.Add(_player.GetComponent<ILevelable>()); };
            OfflineGameplayDependencies.gameScene.onPlayerAdded_OnServer += m_OnPlayerAddedAction;
            m_OnPlayerRemovedAction = _player => { m_PlayerLevelableList.Remove(_player.GetComponent<ILevelable>()); };
            OfflineGameplayDependencies.gameScene.onPlayerRemoved_OnServer += m_OnPlayerRemovedAction;

            m_Coroutine = StartCoroutine(IncreaseScoreCoroutine());
        }

        private void UninitializeTimeManagerEvents()
        {
            OfflineGameplayDependencies.gameScene.onPlayerAdded_OnServer -= m_OnPlayerAddedAction;
            m_OnPlayerAddedAction = null;

            OfflineGameplayDependencies.gameScene.onPlayerRemoved_OnServer -= m_OnPlayerRemovedAction;
            m_OnPlayerRemovedAction = null;

            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
                m_Coroutine = null;
            }
        }

        private void OnEnable()
        {
            m_OnServerConnectionStateAction = _args =>
            {
                if (_args.ConnectionState == LocalConnectionState.Started)
                    InitializeTimeManagerEvents();
                else if (_args.ConnectionState == LocalConnectionState.Stopping)
                    UninitializeTimeManagerEvents();
            };
            InstanceFinder.ServerManager.OnServerConnectionState += m_OnServerConnectionStateAction;
        }

        private void OnDisable()
        {
            InstanceFinder.ServerManager.OnServerConnectionState -= m_OnServerConnectionStateAction;
            m_OnServerConnectionStateAction = null;
        }
    }
}