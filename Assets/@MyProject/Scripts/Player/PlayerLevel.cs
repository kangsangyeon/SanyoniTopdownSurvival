using System;
using System.Collections.Generic;
using FishNet.Object;
using MyProject.Event;
using UnityEngine;

namespace MyProject
{
    public class PlayerLevel : NetworkBehaviour, ILevelable
    {
        [SerializeField] private PlayerLevelPropertyDefinition m_Definition;

        #region ILevelable

        private int m_CurrentLevel = 1;
        public int currentLevel => m_CurrentLevel;

        public int maxLevel =>
            m_Definition.maxLevel;

        private int m_CurrentExperience;
        public int currentExperience => m_CurrentExperience;

        private int[] m_RequiredExperienceByLevel;

        public int requiredExperienceToNextLevel =>
            currentLevel >= maxLevel
                ? 0
                : m_RequiredExperienceByLevel[currentLevel + 1];

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

            if (m_CurrentLevel > maxLevel)
            {
                _addedExperience = 0;
                return;
            }

            m_CurrentExperience = m_CurrentExperience + _param.experience;
            _addedExperience = _param.experience;

            bool _becomeNewLevel = false;
            if (m_CurrentExperience >= requiredExperienceToNextLevel)
            {
                _becomeNewLevel = true;
                ++m_CurrentLevel;

                if (m_CurrentLevel == maxLevel)
                {
                    _addedExperience = requiredExperienceToNextLevel;
                }
            }

            Server_OnCurrentExperienceOrLevelChanged(new ILevelable_CurrentExperienceOrLevelChanged_EventParam()
            {
                currentExperience = m_CurrentExperience,
                currentLevel = m_CurrentLevel,
                addedExperience = _addedExperience,
                becomeNewLevel = _becomeNewLevel
            });
        }

        #endregion

        private void Start()
        {
            m_RequiredExperienceByLevel = new int[m_Definition.requiredExperienceToNextLevelGapArray.Length];
            int _prevValue = 0;
            for (int i = 2; i < m_Definition.requiredExperienceToNextLevelGapArray.Length; ++i)
            {
                m_RequiredExperienceByLevel[i] = _prevValue + m_Definition.requiredExperienceToNextLevelGapArray[i];
                _prevValue = m_RequiredExperienceByLevel[i];
            }
        }
    }
}