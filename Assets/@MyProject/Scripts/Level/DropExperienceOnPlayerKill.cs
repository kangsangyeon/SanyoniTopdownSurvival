using System;
using FishNet;
using UnityEngine;

namespace MyProject
{
    public class DropExperienceOnPlayerKill : MonoBehaviour, IDropExperience
    {
        [SerializeField] private Player m_Player;

        private Action<Player> m_OnKillAction;

        #region IDropExperience

        [SerializeField] private int m_ExperienceAmount = 50;
        public int experienceAmount => m_ExperienceAmount;

        #endregion

        private void OnEnable()
        {
            m_OnKillAction = _target =>
            {
                m_Player.GetComponent<PlayerLevel>().AddExperience(new ExperienceParam()
                {
                    tick = InstanceFinder.TimeManager.Tick,
                    source = this,
                    sourceNetworkObjectId = null,
                    experience = experienceAmount
                }, out int _addedExperience);
            };
            m_Player.onKill_OnServer += m_OnKillAction;
        }

        private void OnDisable()
        {
            m_Player.onKill_OnServer -= m_OnKillAction;
            m_OnKillAction = null;
        }
    }
}