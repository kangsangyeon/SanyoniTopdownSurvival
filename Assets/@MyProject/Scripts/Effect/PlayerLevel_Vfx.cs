using System;
using MyProject.Event;
using UnityEngine;

namespace MyProject
{
    public class PlayerLevel_Vfx : MonoBehaviour
    {
        [SerializeField] private Player m_Player;
        [SerializeField] private GameObject m_Prefab_LevelUpVfx;
        [SerializeField] private Transform m_FootPoint;

        private bool m_ReservedLevelUpVfx;

        private Action<ILevelable_CurrentExperienceOrLevelChanged_EventParam> m_OnCurrentExperienceOrLevelChangedAction;
        private Action m_OnRespawnAction;

        private void Awake()
        {
            m_OnCurrentExperienceOrLevelChangedAction = _param =>
            {
                if (_param.becomeNewLevel)
                {
                    if (m_Player.health.health == 0)
                    {
                        m_ReservedLevelUpVfx = true;
                    }
                    else
                    {
                        var _fx = GameObject.Instantiate(
                            m_Prefab_LevelUpVfx,
                            m_FootPoint);
                        _fx.transform.localPosition = Vector3.zero;
                        _fx.transform.localRotation = Quaternion.identity;

                        Destroy(_fx, 4.0f);
                    }
                }
            };
            m_Player.GetComponent<ILevelable>().onCurrentExperienceOrLevelChanged +=
                m_OnCurrentExperienceOrLevelChangedAction;

            m_OnRespawnAction = () =>
            {
                if (m_ReservedLevelUpVfx)
                {
                    var _fx = GameObject.Instantiate(
                        m_Prefab_LevelUpVfx,
                        m_FootPoint);
                    _fx.transform.localPosition = Vector3.zero;
                    _fx.transform.localRotation = Quaternion.identity;

                    Destroy(_fx, 4.0f);
                }
            };
            m_Player.onRespawn_OnClient += m_OnRespawnAction;
        }

        private void OnDestroy()
        {
            if (m_OnCurrentExperienceOrLevelChangedAction != null)
            {
                m_Player.GetComponent<ILevelable>().onCurrentExperienceOrLevelChanged -=
                    m_OnCurrentExperienceOrLevelChangedAction;
                m_OnCurrentExperienceOrLevelChangedAction = null;
            }

            if (m_OnRespawnAction != null)
            {
                m_Player.onRespawn_OnClient -= m_OnRespawnAction;
                m_OnRespawnAction = null;
            }
        }
    }
}