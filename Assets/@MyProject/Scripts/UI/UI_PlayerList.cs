using System.Collections.Generic;
using MyProject;
using MyProject.Event;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_PlayerList : MonoBehaviour
{
    [SerializeField] private Scene_Game m_GameScene;
    [SerializeField] private UIDocument m_Document;

    private VisualElement m_Parent;
    private readonly Dictionary<int, UI_PlayerElement> m_PlayerUIDict = new Dictionary<int, UI_PlayerElement>();

    private System.Action<PlayerAddedEventParam> m_OnPlayerAdded_OnClient;
    private System.Action<PlayerRemovedEventParam> m_OnPlayerRemoved_OnClient;
    private System.Action<PlayerKillEventParam> m_OnPlayerKill_OnClient;
    private System.Action m_OnPlayerRankRefreshed;

    public void Initialize()
    {
        m_Parent = m_Document.rootVisualElement.Q("player-list");

        m_OnPlayerAdded_OnClient = _param => TryInitializePlayerUI(_param.player.connectionId);
        m_GameScene.onPlayerAdded_OnClient += m_OnPlayerAdded_OnClient;
        
        m_OnPlayerRemoved_OnClient = _param => RemovePlayerUI(_param.player.connectionId);
        m_GameScene.onPlayerRemoved_OnClient += m_OnPlayerRemoved_OnClient;
        
        m_OnPlayerKill_OnClient = param => RefreshPlayerUI(param.killer.connectionId);
        m_GameScene.onPlayerKill_OnClient += m_OnPlayerKill_OnClient;
        
        m_OnPlayerRankRefreshed = () => RefreshPlayerRankUI();
        m_GameScene.onPlayerRankRefreshed += m_OnPlayerRankRefreshed;
    }

    public void Uninitialize()
    {
        m_GameScene.onPlayerAdded_OnClient -= m_OnPlayerAdded_OnClient;
        m_OnPlayerAdded_OnClient = null;
        
        m_GameScene.onPlayerRemoved_OnClient -= m_OnPlayerRemoved_OnClient;
        m_OnPlayerRemoved_OnClient = null;
        
        m_GameScene.onPlayerKill_OnClient -= m_OnPlayerKill_OnClient;
        m_OnPlayerKill_OnClient = null;

        m_GameScene.onPlayerRankRefreshed -= m_OnPlayerRankRefreshed;
        m_OnPlayerRankRefreshed = null;

        RemoveAllPlayerUI();
    }

    private void RemovePlayerUI(int _connectionId)
    {
        m_PlayerUIDict[_connectionId].RemoveFromHierarchy();
        m_PlayerUIDict.Remove(_connectionId);
    }

    private void RemoveAllPlayerUI()
    {
        foreach (var _playerElem in m_PlayerUIDict.Values)
            _playerElem.RemoveFromHierarchy();

        m_PlayerUIDict.Clear();
    }

    private UI_PlayerElement GetPlayerElemUI(int _connectionId)
    {
        if (m_PlayerUIDict.ContainsKey(_connectionId) == false)
            InitializePlayerUI(_connectionId);

        return m_PlayerUIDict[_connectionId];
    }

    private void TryInitializePlayerUI(int _connectionId)
    {
        if (m_PlayerUIDict.ContainsKey(_connectionId))
            return;

        InitializePlayerUI(_connectionId);
    }

    private void InitializePlayerUI(int _connectionId)
    {
        UI_PlayerElement _playerElement = new UI_PlayerElement()
            { name = $"player-elem [connectionId: {_connectionId}]" };
        m_Parent.hierarchy.Add(_playerElement);
        m_PlayerUIDict.Add(_connectionId, _playerElement);

        // _player.onPowerChanged.AddListener(() => RefreshPlayerUI(_player));
        // _player.onKill.AddListener(target => RefreshPlayerUI(_player));

        RefreshPlayerUI(_connectionId);
    }

    private void RefreshPlayerUI(int _connectionId)
    {
        UI_PlayerElement _playerElement = m_PlayerUIDict[_connectionId];

        Label _playerRankLabel = _playerElement.Q<Label>("player-rank");
        Label _playerNameLabel = _playerElement.Q<Label>("player-name");
        Label _playerKillLabel = _playerElement.Q<Label>("player-kill");
        Label _playerPowerLabel = _playerElement.Q<Label>("player-power");

        _playerRankLabel.text =
            m_GameScene.playerRankDict.ContainsKey(_connectionId)
                ? $"# {m_GameScene.playerRankDict[_connectionId]}"
                : $"# ";
        _playerNameLabel.text = m_GameScene.playerInfoDict[_connectionId].name;
        _playerKillLabel.text = m_GameScene.playerInfoDict[_connectionId].killCount.ToString();
        _playerPowerLabel.text = m_GameScene.playerInfoDict[_connectionId].power.ToString();
    }

    private void RefreshPlayerRankUI()
    {
        foreach (var _connectionId in m_GameScene.playerInfoDict.Keys)
        {
            UI_PlayerElement _playerElement = GetPlayerElemUI(_connectionId);
            Label _playerRankLabel = _playerElement.Q<Label>("player-rank");
            _playerRankLabel.text = $"# {m_GameScene.playerRankDict[_connectionId]}";
        }
    }
}