using UnityEngine;

namespace MyProject
{
    public class Sound_GunWeapon : MonoBehaviour
    {
        [SerializeField] private Player m_Player;
        [SerializeField] private AudioSource m_AudioSource;

        [SerializeField] private AudioClip m_FireClip;
        [SerializeField] private AudioClip m_ReloadStartClip;
        [SerializeField] private AudioClip m_ReloadFinishClip;

        private void InitializeGunWeaponEvents(IGunWeapon _gunWeapon)
        {
            _gunWeapon.onAttack += (_param) =>
                m_AudioSource.PlayOneShot(m_FireClip);

            _gunWeapon.onReloadStart += () =>
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
        }

        private void Start()
        {
            if (m_Player.weapon is IGunWeapon _gunWeapon)
                InitializeGunWeaponEvents(_gunWeapon);
            m_Player.onWeaponChanged_OnServer += () =>
            {
                if (m_Player.weapon is IGunWeapon _gunWeapon)
                    InitializeGunWeaponEvents(_gunWeapon);
            };
        }
    }
}