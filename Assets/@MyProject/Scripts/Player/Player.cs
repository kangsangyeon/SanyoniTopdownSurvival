using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using MyProject.Event;
using MyProject.Struct;
using UnityEngine;

namespace MyProject
{
    public class Player : NetworkBehaviour, IDamageableEntity
    {
        [SerializeField] private EntityHealth m_Health;
        public EntityHealth health => m_Health;

        [SerializeField] private PlayerMovement m_Movement;
        public PlayerMovement movement => m_Movement;

        [SerializeField] private Collider m_Collider;

        [SerializeField] private List<SpriteRenderer> m_SpriteRenderers;
        public ReadOnlyCollection<SpriteRenderer> spriteRenderers => m_SpriteRenderers.AsReadOnly();

        [SerializeField] private UI_HealthBar m_HealthBar;
        [SerializeField] private UI_PlayerAmmo m_UI_PlayerAmmo;

        private IWeapon m_Weapon;

        public IWeapon weapon
        {
            get => m_Weapon;
            set
            {
                if (m_Weapon != value)
                {
                    int? _prevWeaponId = null;
                    if (m_Weapon != null)
                        _prevWeaponId = (m_Weapon as NetworkBehaviour).ObjectId;

                    m_Weapon = value;
                    Server_OnWeaponChanged(_prevWeaponId);
                }
            }
        }

        private int m_KillCount = 0;
        public int killCount => m_KillCount;

        private int m_Power = 0;
        public int power => m_Power;

        private DamageParam m_LastDamage;
        public DamageParam lastDamage => m_LastDamage;

        #region IDamageableEntity

        public int maxTakableDamage => int.MaxValue;
        public bool useConstantDamage => false;
        public int constantDamage => 0;

        public void TakeDamage(in DamageParam _hitParam, out int _appliedDamage)
        {
            if (useConstantDamage)
                _hitParam.healthModifier.magnitude = constantDamage;
            else if (_hitParam.healthModifier.magnitude > maxTakableDamage)
                _hitParam.healthModifier.magnitude = maxTakableDamage;

            m_LastDamage = _hitParam;

            m_Health.ApplyModifier(_hitParam.healthModifier);
            _appliedDamage = _hitParam.healthModifier.magnitude;
        }

        #endregion

        #region Ability

        private AbilityProperty m_AbilityProperty = new AbilityProperty();
        public AbilityProperty abilityProperty => m_AbilityProperty;

        private List<AbilityDefinition> m_AbilityList = new List<AbilityDefinition>();
        public IReadOnlyList<AbilityDefinition> abilityList => m_AbilityList;

        private List<AbilityPropertyModifierDefinition> m_AbilityPropertyModifierList =
            new List<AbilityPropertyModifierDefinition>();

        public IReadOnlyList<AbilityPropertyModifierDefinition> abilityPropertyModifierList =>
            m_AbilityPropertyModifierList;

        #region Ability Events

        public event System.Action<Player_OnAbilityAdded_EventParam> onAbilityAdded_OnClient;
        public event System.Action<Player_OnAbilityRemoved_EventParam> onAbilityRemoved_OnClient;

        [Server]
        private void Server_OnAbilityAdded(Player _player, AbilityDefinition _abilityDefinition)
        {
            onAbilityAdded_OnClient?.Invoke(new Player_OnAbilityAdded_EventParam()
                { player = new PlayerInfo(_player), abilityId = _abilityDefinition.abilityId });
            ObserversRpc_OnAbilityAdded(new Player_OnAbilityAdded_EventParam()
                { player = new PlayerInfo(_player), abilityId = _abilityDefinition.abilityId });
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnAbilityAdded(Player_OnAbilityAdded_EventParam _param)
        {
            onAbilityAdded_OnClient?.Invoke(_param);
        }

        [Server]
        private void Server_OnAbilityRemoved(Player _player, AbilityDefinition _abilityDefinition)
        {
            onAbilityRemoved_OnClient?.Invoke(new Player_OnAbilityRemoved_EventParam()
                { player = new PlayerInfo(_player), abilityId = _abilityDefinition.abilityId });
            ObserversRpc_OnAbilityRemoved(new Player_OnAbilityRemoved_EventParam()
                { player = new PlayerInfo(_player), abilityId = _abilityDefinition.abilityId });
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnAbilityRemoved(Player_OnAbilityRemoved_EventParam _param)
        {
            onAbilityRemoved_OnClient?.Invoke(_param);
        }

        #endregion

        [ServerRpc]
        public void ServerRpc_RequestAddAbility(Player_RequestAddAbilityParam _param)
        {
            var _abilityDefinition = OfflineGameplayDependencies.abilityDatabase.GetAbility(_param.abilityId);
            Server_AddAbility(_abilityDefinition);
        }

        [Server]
        public void Server_AddAbility(AbilityDefinition _definition)
        {
            m_AbilityList.Add(_definition);
            foreach (var m in _definition.abilityPropertyModifierDefinitionList) m_AbilityPropertyModifierList.Add(m);
            RefreshAbilityProperty();

            ObserversRpc_AddAbility(new Player_ObserversRpc_AddAbility_EventParam()
                { player = new PlayerInfo(this), abilityId = _definition.abilityId });

            Server_OnAbilityAdded(this, _definition);
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_AddAbility(Player_ObserversRpc_AddAbility_EventParam _param)
        {
            AbilityDefinition _abilityDefinition =
                OfflineGameplayDependencies.abilityDatabase.GetAbility(_param.abilityId);

            m_AbilityList.Add(_abilityDefinition);
            foreach (var m in _abilityDefinition.abilityPropertyModifierDefinitionList)
                m_AbilityPropertyModifierList.Add(m);
            RefreshAbilityProperty();
        }

        [Server]
        public void Server_RemoveAbility(AbilityDefinition _definition)
        {
            m_AbilityList.Remove(_definition);
            foreach (var m in _definition.abilityPropertyModifierDefinitionList)
                m_AbilityPropertyModifierList.Remove(m);
            RefreshAbilityProperty();

            ObserversRpc_RemoveAbility(new Player_ObserversRpc_RemoveAbility_EventParam()
                { player = new PlayerInfo(this), abilityId = _definition.abilityId });

            Server_OnAbilityRemoved(this, _definition);
        }

        [ObserversRpc]
        private void ObserversRpc_RemoveAbility(Player_ObserversRpc_RemoveAbility_EventParam _param)
        {
            AbilityDefinition _abilityDefinition =
                OfflineGameplayDependencies.abilityDatabase.GetAbility(_param.abilityId);

            m_AbilityList.Remove(_abilityDefinition);
            foreach (var m in _abilityDefinition.abilityPropertyModifierDefinitionList)
                m_AbilityPropertyModifierList.Remove(m);
            RefreshAbilityProperty();
        }

        #endregion

        private void RefreshAbilityProperty()
        {
            AbilityProperty _newAbilityProperty = new AbilityProperty();
            m_AbilityPropertyModifierList.ForEach(m => ApplyAbilityPropertyModifier(_newAbilityProperty, m));
            m_AbilityProperty = _newAbilityProperty;
        }

        private void ApplyAbilityPropertyModifier(
            AbilityProperty _abilityProperty,
            AbilityPropertyModifierDefinition _modifierDefinition)
        {
            /* gun attack */

            _abilityProperty.reloadDurationMultiplier =
                _abilityProperty.reloadDurationMultiplier * _modifierDefinition.reloadDurationMultiplier;
            _abilityProperty.fireDelayMultiplier =
                _abilityProperty.fireDelayMultiplier * _modifierDefinition.fireDelayMultiplier;
            _abilityProperty.maxMagazineMultiplier =
                _abilityProperty.maxMagazineMultiplier * _modifierDefinition.maxMagazineMultiplier;
            _abilityProperty.projectileSpeedMultiplier =
                _abilityProperty.projectileSpeedMultiplier * _modifierDefinition.projectileSpeedMultiplier;
            _abilityProperty.projectileDamageMultiplier =
                _abilityProperty.projectileDamageMultiplier * _modifierDefinition.projectileDamageMultiplier;
            _abilityProperty.projectileSizeMultiplier =
                _abilityProperty.projectileSizeMultiplier * _modifierDefinition.projectileSizeMultiplier;
            _abilityProperty.projectileCountPerShot =
                _abilityProperty.projectileCountPerShot + _modifierDefinition.projectileCountPerShotAdditional;
            _abilityProperty.projectileShotAngleRange =
                _abilityProperty.projectileShotAngleRange + _modifierDefinition.projectileSpreadAngleMultiplier;

            /* melee attack */

            _abilityProperty.meleeAttackDelayMultiplier =
                _abilityProperty.meleeAttackDelayMultiplier * _modifierDefinition.meleeAttackDelayMultiplier;
            _abilityProperty.meleeAttackDamageMagnitudeMultiplier =
                _abilityProperty.meleeAttackDamageMagnitudeMultiplier
                * _modifierDefinition.meleeAttackDamageMagnitudeMultiplier;
        }

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
        public event System.Action<Player_OnDead_EventParam> onDead_OnOwnerClient;

        [Server]
        private void Server_OnDead(object _source)
        {
            onDead_OnServer?.Invoke(_source);
            onDead_OnClient?.Invoke(new Player_OnDead_EventParam() { });
            ObserversRpc_OnDead(new Player_OnDead_EventParam() { });
            TargetRpc_OnDead(this.Owner, new Player_OnDead_EventParam() { });
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnDead(Player_OnDead_EventParam _param)
        {
            onDead_OnClient?.Invoke(_param);
        }

        [TargetRpc]
        private void TargetRpc_OnDead(NetworkConnection _conn, Player_OnDead_EventParam _param)
        {
            onDead_OnOwnerClient?.Invoke(_param);
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

        public event System.Action<int?> onWeaponChanged_OnServer; // param: <prevWeaponObjectId>
        public event System.Action<int?> onWeaponChanged_OnClient; // param: <prevWeaponObjectId>

        [Server]
        private void Server_OnWeaponChanged(int? _prevWeaponId)
        {
            onWeaponChanged_OnServer?.Invoke(_prevWeaponId);
            onWeaponChanged_OnClient?.Invoke(_prevWeaponId);
            ObserversRpc_OnWeaponChanged(_prevWeaponId);
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnWeaponChanged(int? _prevWeaponId)
        {
            onWeaponChanged_OnClient?.Invoke(_prevWeaponId);
        }

        #endregion

        [Server]
        public void Server_Respawn(Vector3 _position)
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

            if (base.Owner.IsLocalClient)
            {
                OfflineGameplayDependencies.gameScene.myPlayer = this;
            }
            else
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