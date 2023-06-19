using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MyProject
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerHealth m_Health;
        public PlayerHealth health => m_Health;

        [SerializeField] private Collider2D m_Collider;

        [SerializeField] private List<SpriteRenderer> m_SpriteRenderers;
        public ReadOnlyCollection<SpriteRenderer> spriteRenderers => m_SpriteRenderers.AsReadOnly();

        [SerializeField] private HealthBar m_HealthBar;

        private IWeapon m_Weapon;

        public IWeapon weapon
        {
            get => m_Weapon;
            set
            {
                if (m_Weapon != value)
                {
                    m_Weapon = value;
                    onWeaponChanged.Invoke();
                }
            }
        }

        private int m_KillCount = 0;
        public int killCount => m_KillCount;

        private int m_Power = 0;
        public int power => m_Power;

        public UnityEvent<Player> onKill = new UnityEvent<Player>(); // param: target(=죽인 대상)
        public UnityEvent<object> onDead = new UnityEvent<object>(); // param: source(=죽은 원인)
        public UnityEvent onPowerChanged = new UnityEvent();
        public UnityEvent onRespawn = new UnityEvent();
        public UnityEvent onWeaponChanged = new UnityEvent();

        private void Start()
        {
            weapon = GetComponentInChildren<IWeapon>();

            health.onHealthIsZero.AddListener(() =>
            {
                m_Collider.enabled = false;
                foreach (SpriteRenderer _renderer in m_SpriteRenderers)
                    _renderer.enabled = false;
                if (m_HealthBar)
                    m_HealthBar.enabled = false;
            });

            health.onHealthIsZero.AddListener(() =>
            {
                HealthModifier _healthModifier;

                // 죽은 플레이어의 가장 최근 데미지 요인들부터 순회하여
                // 가장 마지막으로 자신에게 피해를 입힌 플레이어를 얻습니다.
                _healthModifier =
                    health.damageList
                        .Reverse().FirstOrDefault(m =>
                            m.source is IWeapon _weapon
                            && _weapon.owner is Player);

                if (_healthModifier != null)
                {
                    if (Time.time - 10 > _healthModifier.time)
                    {
                        // 가장 마지막으로 플레이어로부터 받은 피해가 조금 오래 된 경우,
                        // 플레이어로부터 죽었다고 판정하지 않습니다.
                    }
                    else
                    {
                        // 플레이어로 인해 죽은 경우 실행됩니다.
                        var _weapon = _healthModifier.source as IWeapon;
                        var _killer = _weapon.owner as Player;

                        ++_killer.m_KillCount;
                        _killer.onKill.Invoke(this);
                    }
                }

                // 플레이어로부터 죽은 것이 아닐 때 실행됩니다.
                _healthModifier = health.damageList.Last();

                onDead.Invoke(_healthModifier.source);
            });

            onRespawn.AddListener(() =>
            {
                m_Collider.enabled = true;
                foreach (SpriteRenderer _renderer in m_SpriteRenderers)
                    _renderer.enabled = true;
                if (m_HealthBar)
                    m_HealthBar.enabled = true;

                health.ApplyModifier(new HealthModifier()
                    { magnitude = health.MaxHealth, source = this, time = Time.time }); // source: respawn
            });
        }
    }
}