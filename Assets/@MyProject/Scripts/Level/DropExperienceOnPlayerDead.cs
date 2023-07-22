using System;
using FishNet;
using UnityEngine;

namespace MyProject
{
    public class DropExperienceOnPlayerDead : MonoBehaviour, IDropExperience
    {
        [SerializeField] private Player m_Player;

        private Action<object> m_OnDeadAction;

        #region IDropExperience

        [SerializeField] private int m_ExperienceAmount = 30;
        public int experienceAmount => m_ExperienceAmount;

        #endregion

        private void OnEnable()
        {
            m_OnDeadAction = _source =>
            {
                m_Player.GetComponent<PlayerLevel>().AddExperience(new ExperienceParam()
                {
                    tick = InstanceFinder.TimeManager.Tick,
                    source = this,
                    sourceNetworkObjectId = null,
                    experience = experienceAmount
                }, out int _addedExperience);
            };
            m_Player.onDead_OnServer += m_OnDeadAction;
        }

        private void OnDisable()
        {
            m_Player.onDead_OnServer -= m_OnDeadAction;
            m_OnDeadAction = null;
        }
    }
}