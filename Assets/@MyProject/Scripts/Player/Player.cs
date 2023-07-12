using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FishNet.Object;
using MyProject.Event;
using UnityEngine;

namespace MyProject
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private PlayerHealth m_Health;
        public PlayerHealth health => m_Health;

        [SerializeField] private PlayerMovement m_Movement;
        public PlayerMovement movement => m_Movement;

        [SerializeField] private Collider2D m_Collider;

        [SerializeField] private List<SpriteRenderer> m_SpriteRenderers;
        public ReadOnlyCollection<SpriteRenderer> spriteRenderers => m_SpriteRenderers.AsReadOnly();

        [SerializeField] private HealthBar m_HealthBar;
        [SerializeField] private UI_PlayerAmmo m_UI_PlayerAmmo;

        private IWeapon m_Weapon;

        public IWeapon weapon
        {
            get => m_Weapon;
            set
            {
                if (m_Weapon != value)
                {
                    m_Weapon = value;
                    Server_OnWeaponChanged();
                }
            }
        }

        private int m_KillCount = 0;
        public int killCount => m_KillCount;

        private int m_Power = 0;
        public int power => m_Power;

        #region Attack Property

        private AttackProperty m_AttackProperty = new AttackProperty();
        public AttackProperty attackProperty => m_AttackProperty;

        [SerializeField]
        private List<AttackPropertyModifier> m_AttackPropertyModifierList = new List<AttackPropertyModifier>();

        public IReadOnlyList<AttackPropertyModifier> attackPropertyModifierList => m_AttackPropertyModifierList;

        public void AddAttackPropertyModifier(AttackPropertyModifier _modifier)
        {
            m_AttackPropertyModifierList.Add(_modifier);
            RefreshAttackProperty();
        }

        public void RemoveAttackPropertyModifier(AttackPropertyModifier _modifier)
        {
            m_AttackPropertyModifierList.Remove(_modifier);
            RefreshAttackProperty();
        }

        private void RefreshAttackProperty()
        {
            AttackProperty _newAttackProperty = new AttackProperty();
            m_AttackPropertyModifierList.ForEach(m => m.Modify(m_AttackProperty));
        }

        #endregion

        #region Events

        public event System.Action<Player> onKill_OnServer; // param: target(=죽인 대상)
        public event System.Action<Player_OnKill_EventParam> onKill_OnClient;

        [Server]
        private void Server_OnKill(Player _target)
        {
            onKill_OnServer?.Invoke(_target);
            onKill_OnClient?.Invoke(new Player_OnKill_EventParam() { target = new PlayerInfo(_target) });
            ObserversRpc_OnKill(new Player_OnKill_EventParam() { target = new PlayerInfo(_target) });
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnKill(Player_OnKill_EventParam _param)
        {
            onKill_OnClient?.Invoke(_param);
        }

        public event System.Action<object> onDead_OnServer; // param: source(=죽은 원인)
        public event System.Action<Player_OnDead_EventParam> onDead_OnClient;

        [Server]
        private void Server_OnDead(object _source)
        {
            onDead_OnServer?.Invoke(_source);
            onDead_OnClient?.Invoke(new Player_OnDead_EventParam() { });
            ObserversRpc_OnDead(new Player_OnDead_EventParam() { });
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnDead(Player_OnDead_EventParam _param)
        {
            onDead_OnClient?.Invoke(_param);
        }

        public event System.Action onPowerChanged_OnServer;
        public event System.Action onPowerChanged_OnClient;

        [Server]
        private void Server_OnPowerChanged()
        {
            onPowerChanged_OnServer?.Invoke();
            onPowerChanged_OnClient?.Invoke();
            ObserversRpc_OnPowerChanged();
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnPowerChanged()
        {
            onPowerChanged_OnClient?.Invoke();
        }

        public event System.Action onRespawn_OnServer;
        public event System.Action onRespawn_OnClient;

        [Server]
        private void Server_OnRespawn()
        {
            onRespawn_OnServer?.Invoke();
            onRespawn_OnClient?.Invoke();
            ObserversRpc_OnRespawn();
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnRespawn()
        {
            onRespawn_OnClient?.Invoke();
        }

        public event System.Action onWeaponChanged_OnServer;
        public event System.Action onWeaponChanged_OnClient;

        [Server]
        private void Server_OnWeaponChanged()
        {
            onWeaponChanged_OnServer?.Invoke();
            onWeaponChanged_OnClient?.Invoke();
            ObserversRpc_OnWeaponChanged();
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnWeaponChanged()
        {
            onWeaponChanged_OnClient?.Invoke();
        }

        #endregion

        [Server]
        public void Server_Respawn(Vector2 _position)
        {
            movement.Teleport(base.Owner, _position);
            Server_OnRespawn();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Scene_Game.Instance.TargetRpc_JoinGame(base.Owner, new GameJoinedEventParam()
            {
                playerInfoList = Scene_Game.Instance.playerInfoDict.Values.ToList()
            });
            Scene_Game.Instance.Server_AddPlayer(this);

            health.onHealthIsZero_OnServer += () =>
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

                        _killer.Server_OnKill(this);
                    }
                }

                // 플레이어로부터 죽은 것이 아닐 때 실행됩니다.
                _healthModifier = health.damageList.Last();

                Server_OnDead(_healthModifier.source);
            };

            onRespawn_OnServer += () =>
            {
                health.ApplyModifier(new HealthModifier()
                    { magnitude = health.MaxHealth, source = this, time = Time.time }); // source: respawn
            };
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            Scene_Game.Instance.Server_RemovePlayer(this);
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            weapon = GetComponentInChildren<IWeapon>();

            m_AttackPropertyModifierList.ForEach(AddAttackPropertyModifier);

            if (base.Owner.IsLocalClient == false)
            {
                m_UI_PlayerAmmo.enabled = false;
            }

            health.onHealthIsZero_OnSync += () =>
            {
                m_Collider.enabled = false;
                foreach (SpriteRenderer _renderer in m_SpriteRenderers)
                    _renderer.enabled = false;
                if (m_HealthBar)
                    m_HealthBar.enabled = false;
            };

            onRespawn_OnClient += () =>
            {
                m_Collider.enabled = true;
                foreach (SpriteRenderer _renderer in m_SpriteRenderers)
                    _renderer.enabled = true;
                if (m_HealthBar)
                    m_HealthBar.enabled = true;
            };
        }
    }
}