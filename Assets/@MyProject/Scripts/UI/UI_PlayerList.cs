using System.Collections.Generic;
using MyProject;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_PlayerList : MonoBehaviour
{
    [SerializeField] private Scene_Game m_GameScene;
    [SerializeField] private UIDocument m_Document;

    private VisualElement m_Parent;
    private readonly Dictionary<int, UI_PlayerElement> m_PlayerUIDict = new Dictionary<int, UI_PlayerElement>();

    public void Initialize()
    {
        m_Parent = m_Document.rootVisualElement.Q("player-list");

        m_GameScene.onPlayerAdded_OnClient.AddListener(_param =>
            TryInitializePlayerUI(_param.player.connectionId));

        m_GameScene.onPlayerRemoved_OnClient.AddListener(_param =>
            m_PlayerUIDict.Remove(_param.player.connectionId));

        m_GameScene.onPlayerKill_OnClient.AddListener(param => RefreshPlayerUI(param.killer.connectionId));
        m_GameScene.onPlayerRankRefreshed.AddListener(() => RefreshPlayerRankUI());
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
            if (m_PlayerUIDict.ContainsKey(_connectionId) == false)
                continue;

            UI_PlayerElement _playerElement = m_PlayerUIDict[_connectionId];
            Label _playerRankLabel = _playerElement.Q<Label>("player-rank");
            _playerRankLabel.text = $"# {m_GameScene.playerRankDict[_connectionId]}";
        }
    }
}