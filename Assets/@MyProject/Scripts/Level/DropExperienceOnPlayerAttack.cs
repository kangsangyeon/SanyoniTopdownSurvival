using System;
using FishNet;
using UnityEngine;

namespace MyProject
{
    public class DropExperienceOnPlayerAttack : MonoBehaviour, IDropExperience
    {
        [SerializeField] private Player m_Player;

        private Action<IWeapon_OnAttack_EventParam> m_OnKillAction;

        #region IDropExperience

        [SerializeField] private int m_ExperienceAmount = 1;
        public int experienceAmount => m_ExperienceAmount;

        #endregion

        private void InitializeWeaponEvents(IWeapon _weapon)
        {
            m_OnKillAction = _param =>
            {
                m_Player.GetComponent<PlayerLevel>().AddExperience(new ExperienceParam()
                {
                    tick = InstanceFinder.TimeManager.Tick,
                    source = this,
                    sourceNetworkObjectId = null,
                    experience = experienceAmount
                }, out int _addedExperience);
            };
            _weapon.onAttack += m_OnKillAction;
        }

        private void UninitializeWeaponEvents(IWeapon _weapon)
        {
            _weapon.onAttack -= m_OnKillAction;
            m_OnKillAction = null;
        }

        private void OnEnable()
        {
            if (m_Player.weapon != null)
                InitializeWeaponEvents(m_Player.weapon);

            m_Player.onWeaponChanged_OnServer += _prevWeaponObjectId =>
            {
                if (_prevWeaponObjectId.HasValue)
                {
                    IWeapon _prevWeapon =
                        InstanceFinder.ClientManager.Objects.Spawned[_prevWeaponObjectId.Value] as IWeapon;
                    if (_prevWeapon != null)
                        UninitializeWeaponEvents(_prevWeapon);
                }

                if (m_Player.weapon != null)
                    InitializeWeaponEvents(m_Player.weapon);
            };
        }

        private void OnDisable()
        {
            if (m_Player.weapon != null)
                UninitializeWeaponEvents(m_Player.weapon);
        }
    }
}