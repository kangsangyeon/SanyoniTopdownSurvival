using System;
using UnityEngine;

namespace MyProject
{
    public class ApplyPlayerAbility_ToWeapon : MonoBehaviour
    {
        [SerializeField] private Player m_Player;

        private Action m_OnAbilityPropertyRefreshedAction;

        private void InitializeMeleeWeapon(IMeleeWeapon _meleeWeapon)
        {
            m_OnAbilityPropertyRefreshedAction = () =>
            {
                if (_meleeWeapon.meleeAttackRange is BoxCollider _boxCollider)
                {
                    Vector3 _newSize = _boxCollider.size;
                    _newSize.x += m_Player.abilityProperty.meleeAttackSizeAddition;
                    _newSize.z += m_Player.abilityProperty.meleeAttackSizeAddition;
                    _boxCollider.size = _newSize;
                }
                else if (_meleeWeapon.meleeAttackRange is SphereCollider _sphereCollider)
                {
                    _sphereCollider.radius += m_Player.abilityProperty.meleeAttackSizeAddition;
                }
            };
            m_Player.onAbilityPropertyRefreshed_OnClient += m_OnAbilityPropertyRefreshedAction;
        }

        private void UninitializeMeleeWeapon(IMeleeWeapon _meleeWeapon)
        {
            if (m_OnAbilityPropertyRefreshedAction != null)
            {
                if (_meleeWeapon.meleeAttackRange is BoxCollider _boxCollider)
                {
                    Vector3 _newSize = _boxCollider.size;
                    _newSize.x -= m_Player.abilityProperty.meleeAttackSizeAddition;
                    _newSize.z -= m_Player.abilityProperty.meleeAttackSizeAddition;
                    _boxCollider.size = _newSize;
                }
                else if (_meleeWeapon.meleeAttackRange is SphereCollider _sphereCollider)
                {
                    _sphereCollider.radius -= m_Player.abilityProperty.meleeAttackSizeAddition;
                }

                m_Player.onAbilityPropertyRefreshed_OnClient -= m_OnAbilityPropertyRefreshedAction;
                m_OnAbilityPropertyRefreshedAction = null;
            }
        }

        private void Awake()
        {
            if (m_Player.weapon != null)
            {
                if (m_Player.weapon is IMeleeWeapon _meleeWeapon)
                    InitializeMeleeWeapon(_meleeWeapon);
            }

            m_Player.onWeaponChanged_OnClient += (_prevWeapon, _nextWeapon) =>
            {
                if (_prevWeapon is IMeleeWeapon _prevMeleeWeapon)
                    UninitializeMeleeWeapon(_prevMeleeWeapon);

                if (_nextWeapon is IMeleeWeapon _meleeWeapon)
                    InitializeMeleeWeapon(_meleeWeapon);
            };
        }
    }
}