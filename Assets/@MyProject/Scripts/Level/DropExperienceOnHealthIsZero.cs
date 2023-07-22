using System;
using System.Linq;
using FishNet;
using UnityEngine;

namespace MyProject
{
    public class DropExperienceOnHealthIsZero : MonoBehaviour, IDropExperience
    {
        private EntityHealth m_Health;
        private Action m_OnHealthIsZeroAction;

        #region IDropExperience

        [SerializeField] private int m_ExperienceAmount = 10;
        public int experienceAmount => m_ExperienceAmount;

        #endregion

        private void OnEnable()
        {
            if (InstanceFinder.IsServer == false)
            {
                return;
            }

            m_Health = GetComponent<EntityHealth>();

            m_OnHealthIsZeroAction = () =>
            {
                ILevelable _levelable = null;

                var _lastHealthModifier = m_Health.damageList.Last();
                if (_lastHealthModifier.source != null)
                {
                    _levelable =
                        (_lastHealthModifier.source as MonoBehaviour).GetComponent<ILevelable>();
                }

                if (_levelable == null)
                {
                    if (_lastHealthModifier.sourceOwnerObject != null)
                    {
                        _levelable =
                            (_lastHealthModifier.sourceOwnerObject as MonoBehaviour).GetComponent<ILevelable>();
                    }
                }

                if (_levelable != null)
                {
                    _levelable.AddExperience(new ExperienceParam()
                    {
                        tick = InstanceFinder.TimeManager.Tick,
                        source = this,
                        experience = experienceAmount
                    }, out int _addedExperience);
                }
            };
            m_Health.onHealthIsZero_OnServer += m_OnHealthIsZeroAction;
        }

        private void OnDisable()
        {
            if (InstanceFinder.IsServer == false)
            {
                return;
            }

            m_Health.onHealthIsZero_OnServer -= m_OnHealthIsZeroAction;
            m_OnHealthIsZeroAction = null;
        }
    }
}