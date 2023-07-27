using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
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

        [SerializeField] private UI_HealthBar m_HealthBar;

        [SerializeField] private Transform m_FootPoint;
        public Transform footPoint => m_FootPoint;

        [SyncVar(WritePermissions = WritePermission.ClientUnsynchronized,
            OnChange = nameof(SyncVar_OnChangePlayerName))]
        private string m_PlayerName;

        public string playerName => m_PlayerName;

        [SyncVar(WritePermissions = WritePermission.ServerOnly)]
        private int? m_WeaponNetworkObjectId;

        private IWeapon m_Weapon;

        public IWeapon weapon
        {
            get => m_Weapon;
            set
            {
                if (m_Weapon != value)
                {
                    IWeapon _prevWeapon = m_Weapon;
                    m_Weapon = value;
                    m_WeaponNetworkObjectId =
                        (m_Weapon as NetworkBehaviour)?.ObjectId;
                    Server_OnWeaponChanged(_prevWeapon, value);
                }
            }
        }

        private int m_KillCount = 0;
        public int killCount => m_KillCount;

        private int m_Power = 0;
        public int power => m_Power;

        private DamageParam m_LastDamage;
        public DamageParam lastDamage => m_LastDamage;

        #region NetworkBehaviour Events

        public event System.Action onStartServer;
        public event System.Action onStopServer;
        public event System.Action onStartClient;
        public event System.Action onStopClient;
        public event System.Action onInitializedOnClient; // server only

        private bool m_InitializedOnClient; // server only

        [ServerRpc]
        private void ServerRpc_InitializedOnClient()
        {
            m_InitializedOnClient = true;
            onInitializedOnClient?.Invoke();
        }

        #endregion

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

        public event System.Action<AbilityDefinition> onAbilityAdded_OnClient;
        public event System.Action<AbilityDefinition> onAbilityRemoved_OnClient;
        public event System.Action onAbilityPropertyRefreshed_OnClient;

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
            onAbilityAdded_OnClient?.Invoke(_definition);

            ObserversRpc_AddAbility(new Player_ObserversRpc_AddAbility_EventParam()
                { player = new PlayerInfo(this), abilityId = _definition.abilityId });
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
            onAbilityAdded_OnClient?.Invoke(_abilityDefinition);
        }

        [Server]
        public void Server_RemoveAbility(AbilityDefinition _definition)
        {
            m_AbilityList.Remove(_definition);
            foreach (var m in _definition.abilityPropertyModifierDefinitionList)
                m_AbilityPropertyModifierList.Remove(m);
            RefreshAbilityProperty();
            onAbilityRemoved_OnClient?.Invoke(_definition);

            ObserversRpc_RemoveAbility(new Player_ObserversRpc_RemoveAbility_EventParam()
                { player = new PlayerInfo(this), abilityId = _definition.abilityId });
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
            onAbilityRemoved_OnClient?.Invoke(_abilityDefinition);
        }

        #endregion

        private void RefreshAbilityProperty()
        {
            AbilityProperty _newAbilityProperty = new AbilityProperty();
            m_AbilityPropertyModifierList.ForEach(m => ApplyAbilityPropertyModifier(_newAbilityProperty, m));
            m_AbilityProperty = _newAbilityProperty;

            onAbilityPropertyRefreshed_OnClient?.Invoke();
        }

        private void ApplyAbilityPropertyModifier(
            AbilityProperty _abilityProperty,
            AbilityPropertyModifierDefinition _modifierDefinition)
        {
            /* character */

            _abilityProperty.maxHealthAddition +=
                _modifierDefinition.maxHealthAddition;

            _abilityProperty.moveSpeedAddition +=
                _modifierDefinition.moveSpeedAddition;

            /* gun attack */

            _abilityProperty.reloadDurationAddition +=
                _modifierDefinition.reloadDurationAddition;

            _abilityProperty.fireDelayAddition +=
                _modifierDefinition.fireDelayAddition;

            _abilityProperty.maxMagazineAddition +=
                _modifierDefinition.maxMagazineAddition;

            _abilityProperty.projectileSpeedAddition +=
                _modifierDefinition.projectileSpeedAddition;

            _abilityProperty.projectileDamageAddition +=
                _modifierDefinition.projectileDamageAddition;

            _abilityProperty.projectileSizeAddition +=
                _modifierDefinition.projectileSizeAddition;

            _abilityProperty.projectileCountPerShotAddition +=
                _modifierDefinition.projectileCountPerShotAddition;

            /* melee attack */

            _abilityProperty.meleeAttackDelayAddition +=
                _modifierDefinition.meleeAttackDelayAddition;

            _abilityProperty.meleeAttackIntervalAddition +=
                _modifierDefinition.meleeAttackIntervalAddition;

            _abilityProperty.meleeAttackDamageMagnitudeAddition +=
                _modifierDefinition.meleeAttackDamageMagnitudeAddition;

            _abilityProperty.meleeAttackSizeAddition +=
                _modifierDefinition.meleeAttackSizeAddition;

            /* sword */

            _abilityProperty.swordProjectileRequiredStackAddition +=
                _modifierDefinition.swordProjectileRequiredStackAddition;
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

        public event System.Action<IWeapon, IWeapon> onWeaponChanged_OnServer; // param: <prevWeapon, newWeapon>
        public event System.Action<IWeapon, IWeapon> onWeaponChanged_OnClient; // param: <prevWeapon, newWeapon>

        [Server]
        private void Server_OnWeaponChanged(IWeapon _prevWeapon, IWeapon _newWeapon)
        {
            onWeaponChanged_OnServer?.Invoke(_prevWeapon, _newWeapon);
            onWeaponChanged_OnClient?.Invoke(_prevWeapon, _newWeapon);

            int? _prevWeaponNetworkObjectId = null;
            if (_prevWeapon != null)
                _prevWeaponNetworkObjectId = (_prevWeapon as NetworkBehaviour).ObjectId;

            int? _newWeaponNetworkObjectId = null;
            if (_newWeapon != null)
                _newWeaponNetworkObjectId = (_newWeapon as NetworkBehaviour).ObjectId;

            ObserversRpc_OnWeaponChanged(_prevWeaponNetworkObjectId, _newWeaponNetworkObjectId);
        }

        [ObserversRpc(ExcludeServer = true)]
        private void ObserversRpc_OnWeaponChanged(int? _prevWeaponNetworkObjectId, int? _newWeaponNetworkObjectId)
        {
            IWeapon _prevWeapon = null;
            IWeapon _newWeapon = null;

            if (_prevWeaponNetworkObjectId.HasValue)
            {
                if (base.ClientManager.Objects.Spawned.ContainsKey(_prevWeaponNetworkObjectId.Value))
                    _prevWeapon = base.ClientManager.Objects.Spawned[_prevWeaponNetworkObjectId.Value]
                        .GetComponent<IWeapon>();
            }

            if (_newWeaponNetworkObjectId.HasValue)
            {
                if (base.ClientManager.Objects.Spawned.ContainsKey(_newWeaponNetworkObjectId.Value))
                    _newWeapon = base.ClientManager.Objects.Spawned[_newWeaponNetworkObjectId.Value]
                        .GetComponent<IWeapon>();
            }

            m_Weapon = _newWeapon;
            onWeaponChanged_OnClient?.Invoke(_prevWeapon, _newWeapon);
        }

        public event System.Action<string> onSetPlayerName_OnClient;

        [Client]
        private void Client_RequestSetPlayerName(string _name)
        {
            m_PlayerName = _name;
            gameObject.name = m_PlayerName;
            onSetPlayerName_OnClient?.Invoke(_name);
            ServerRpc_SetPlayerName(_name);
        }

        [ServerRpc]
        private void ServerRpc_SetPlayerName(string _name)
        {
            m_PlayerName = _name;
            gameObject.name = m_PlayerName;
            onSetPlayerName_OnClient?.Invoke(_name);

            // m_PlayerName은 SyncVar이기 때문에 자동으로 전파됩니다.
        }

        // [ObserversRpc(ExcludeServer = true, ExcludeOwner = true)]
        // private void ObserversRpc_SetPlayerName(string _name)
        // {
        //     m_PlayerName = _name;
        //     gameObject.name = m_PlayerName;
        //     onSetPlayerName_OnClient?.Invoke(_name);
        // }

        private void SyncVar_OnChangePlayerName(string _prevName, string _newName, bool _asServer)
        {
            gameObject.name = _newName;
            onSetPlayerName_OnClient?.Invoke(_newName);
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

            onInitializedOnClient += () =>
            {
                // 클라이언트가 연결되었을 때 무기를 잡고 모두에게 전파합니다.
                // weapon property는 서버에서 기록하면, 클라이언트에게 자동으로 전파됩니다.
                weapon = GetComponentInChildren<IWeapon>();
            };

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
                    { magnitude = health.maxHealth, source = this, time = Time.time }); // source: respawn
            };

            onStartServer?.Invoke();
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            Scene_Game.Instance.Server_RemovePlayer(this);

            onStopServer?.Invoke();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (m_WeaponNetworkObjectId.HasValue)
            {
                // 서버에서 전파받은 무기 id가 있다면,
                // 무기를 초기화합니다.
                // 이는 이미 무기를 가진 플레이어가 있는 세션에 다른 플레이어가 들어왔을 때
                // 그의 클라이언트에서 무기를 가진 플레이어가 스폰될 때 실행됩니다.

                IWeapon _weapon = null;
                if (base.ClientManager.Objects.Spawned.ContainsKey(m_WeaponNetworkObjectId.Value))
                    _weapon = base.ClientManager.Objects.Spawned[m_WeaponNetworkObjectId.Value]
                        .GetComponent<IWeapon>();

                if (_weapon != null)
                {
                    m_Weapon = _weapon;
                    onWeaponChanged_OnClient?.Invoke(null, _weapon);
                }
            }

            onStartClient?.Invoke();

            if (base.IsOwner)
            {
                Client_RequestSetPlayerName(
                    OfflineGameplayDependencies.ui_InputField_PlayerName.text);
                OfflineGameplayDependencies.ui_InputField_PlayerName.gameObject.SetActive(false);

                ServerRpc_InitializedOnClient();
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            onStopClient?.Invoke();
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            if (base.Owner.IsLocalClient)
            {
                OfflineGameplayDependencies.gameScene.myPlayer = this;
            }

            health.onHealthIsZero_OnClient += () =>
            {
                if (m_HealthBar)
                    m_HealthBar.enabled = false;
            };

            onRespawn_OnClient += () =>
            {
                if (m_HealthBar)
                    m_HealthBar.enabled = true;
            };
        }

        private void Update()
        {
            if (base.IsOwner)
            {
                if (weapon != null)
                {
                    if (Input.GetKey(KeyCode.Mouse0))
                    {
                        m_Weapon.QueueAttack();
                    }

                    if (Input.GetKeyDown(KeyCode.R) && m_Weapon is IGunWeapon _gunWeapon)
                    {
                        _gunWeapon.QueueReload();
                    }
                }
            }
        }
    }
}