using UnityEngine;

namespace MyProject
{
    public class ReplayParticleOnEnable : MonoBehaviour
    {
        private ParticleSystem m_Particle;

        private void Awake()
        {
            m_Particle = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            m_Particle.Stop();
            m_Particle.Simulate(0, true, true);
            m_Particle.Play(true);
        }
    }
}