using System;
using FishNet;
using UnityEngine;

namespace MyProject
{
    public class GunWeapon_Sound : MonoBehaviour
    {
        [SerializeField] private Player m_Player;
        [SerializeField] private AudioSource m_AudioSource;

        [SerializeField] private AudioClip m_FireClip;
        [SerializeField] private AudioClip m_ReloadStartClip;
        [SerializeField] private AudioClip m_ReloadFinishClip;

        private Action<IWeapon_OnAttack_EventParam> m_OnAttackAction;
        private Action m_OnReloadStartAction;

        private void InitializeGunWeaponEvents(IGunWeapon _gunWeapon)
        {
            m_OnAttackAction = (_param) =>
                m_AudioSource.PlayOneShot(m_FireClip);
            _gunWeapon.onAttack += m_OnAttackAction;

            m_OnReloadStartAction = () =>
            {
                if (m_AudioSource.isPlaying)
                    m_AudioSource.Stop();

                m_AudioSource.clip = m_ReloadStartClip;
                m_AudioSource.Play();

                float _waitForPlayReloadFinish = _gunWeapon.reloadDuration - m_ReloadFinishClip.length;
                this.Invoke(() =>
                {
                    if (m_AudioSource.isPlaying)
                        m_AudioSource.Stop();

                    m_AudioSource.clip = m_ReloadFinishClip;
                    m_AudioSource.Play();
                }, _waitForPlayReloadFinish);
            };
            _gunWeapon.onReloadStart += m_OnReloadStartAction;
        }

        private void UninitializeGunWeaponEvents(IGunWeapon _gunWeapon)
        {
            _gunWeapon.onAttack -= m_OnAttackAction;
            m_OnAttackAction = null;

            _gunWeapon.onReloadStart -= m_OnReloadStartAction;
            m_OnReloadStartAction = null;
        }

        private void Start()
        {
            if (m_Player.weapon is IGunWeapon _gunWeapon)
                InitializeGunWeaponEvents(_gunWeapon);

            m_Player.onWeaponChanged_OnServer += (_prevWeaponId) =>
            {
                if (_prevWeaponId.HasValue)
                {
                    IWeapon _prevWeapon = InstanceFinder.ClientManager.Objects.Spawned[_prevWeaponId.Value] as IWeapon;

                    if (_prevWeapon is IGunWeapon _prevGunWeapon)
                        UninitializeGunWeaponEvents(_prevGunWeapon);
                }

                if (m_Player.weapon is IGunWeapon _gunWeapon)
                    InitializeGunWeaponEvents(_gunWeapon);
            };
        }
    }
}