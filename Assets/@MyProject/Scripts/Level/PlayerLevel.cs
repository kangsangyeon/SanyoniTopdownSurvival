using System;
using FishNet.Object;
using MyProject.Event;
using UnityEngine;

namespace MyProject
{
    public class PlayerLevel : NetworkBehaviour, ILevelable
    {
        #region ILevelable

        private int m_CurrentLevel = 1;
        public int currentLevel => m_CurrentLevel;

        private int m_MaxLevel = 10;
        public int maxLevel => m_MaxLevel;

        private int m_CurrentExperience;
        public int currentExperience => m_CurrentExperience;

        private int m_RequiredExperienceToNextLevel;
        public int requiredExperienceToNextLevel => m_RequiredExperienceToNextLevel;

        public event Action<ILevelable_CurrentExperienceOrLevelChanged_EventParam> onCurrentExperienceOrLevelChanged;

        [Server]
        private void Server_OnCurrentExperienceOrLevelChanged(
            in ILevelable_CurrentExperienceOrLevelChanged_EventParam _param)
        {
            onCurrentExperienceOrLevelChanged?.Invoke(_param);
            ObserversRpc_OnCurrentExperienceOrLevelChanged(_param);
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnCurrentExperienceOrLevelChanged(
            ILevelable_CurrentExperienceOrLevelChanged_EventParam _param)
        {
            onCurrentExperienceOrLevelChanged?.Invoke(_param);
        }

        [Server]
        public void AddExperience(in ExperienceParam _param, out int _addedExperience)
        {
            if (_param.experience == 0)
            {
                _addedExperience = 0;
                return;
            }

            if (m_CurrentLevel > m_MaxLevel)
            {
                _addedExperience = 0;
                return;
            }

            m_CurrentExperience = m_CurrentExperience + _param.experience;
            _addedExperience = _param.experience;

            bool _becomeNewLevel = false;
            if (m_CurrentExperience >= m_RequiredExperienceToNextLevel)
            {
                _becomeNewLevel = true;
                ++m_CurrentLevel;
                Debug.Log(
                    $"player level up! become lv {m_CurrentLevel} (exp {m_CurrentExperience}/{m_RequiredExperienceToNextLevel})");

                if (m_CurrentLevel == m_MaxLevel)
                {
                    _addedExperience = m_RequiredExperienceToNextLevel;
                    m_RequiredExperienceToNextLevel = 0;
                }
                else
                {
                    // todo: required experience to next level을 다음 레벨까지의 경험치로 올바르게 초기화해주어야 합니다.
                    UpdateRequiredExperienceToNextLevel();
                }
            }

            Server_OnCurrentExperienceOrLevelChanged(new ILevelable_CurrentExperienceOrLevelChanged_EventParam()
            {
                currentExperience = m_CurrentExperience,
                currentLevel = m_CurrentLevel,
                addedExperience = _addedExperience,
                becomeNewLevel = _becomeNewLevel
            });

            Debug.Log($"PlayerLevel: player gain {_addedExperience}!");
        }

        #endregion

        private void UpdateRequiredExperienceToNextLevel()
        {
            m_RequiredExperienceToNextLevel = m_CurrentLevel * 50;
        }

        private void Start()
        {
            UpdateRequiredExperienceToNextLevel();
        }
    }
}