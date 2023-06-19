using System.Collections.Generic;
using MyProject;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_PlayerList : MonoBehaviour
{
    [SerializeField] private Scene_Game m_GameScene;
    [SerializeField] private UIDocument m_Document;

    private VisualElement m_Parent;
    private readonly Dictionary<Player, UI_PlayerElement> m_PlayerUIDict = new Dictionary<Player, UI_PlayerElement>();

    private void Start()
    {
        m_Parent = m_Document.rootVisualElement.Q("player-list");

        m_GameScene.onPlayerAdded.AddListener(InitializePlayerUI);
        foreach (var _player in m_GameScene.playerList)
            InitializePlayerUI(_player);

        m_GameScene.onPlayerKill.AddListener((killer, target) => RefreshPlayerUI(killer));
        m_GameScene.onPlayerRankRefreshed.AddListener(() => RefreshPlayerRankUI());
    }

    private void InitializePlayerUI(Player _player)
    {
        UI_PlayerElement _playerElement = new UI_PlayerElement() { name = _player.gameObject.name };
        m_Parent.hierarchy.Add(_playerElement);
        m_PlayerUIDict.Add(_player, _playerElement);

        _player.onPowerChanged.AddListener(() => RefreshPlayerUI(_player));
        _player.onKill.AddListener(target => RefreshPlayerUI(_player));

        RefreshPlayerUI(_player);
    }

    private void RefreshPlayerUI(Player _player)
    {
        UI_PlayerElement _playerElement = m_PlayerUIDict[_player];

        Label _playerRankLabel = _playerElement.Q<Label>("player-rank");
        Label _playerNameLabel = _playerElement.Q<Label>("player-name");
        Label _playerKillLabel = _playerElement.Q<Label>("player-kill");
        Label _playerPowerLabel = _playerElement.Q<Label>("player-power");

        _playerRankLabel.text =
            m_GameScene.playerRankDict.ContainsKey(_player)
                ? $"# {m_GameScene.playerRankDict[_player]}"
                : $"# ";
        _playerNameLabel.text = _player.gameObject.name;
        _playerKillLabel.text = _player.killCount.ToString();
        _playerPowerLabel.text = _player.power.ToString();
    }

    private void RefreshPlayerRankUI()
    {
        foreach (var _player in m_GameScene.playerRankDict.Keys)
        {
            UI_PlayerElement _playerElement = m_PlayerUIDict[_player];
            Label _playerRankLabel = _playerElement.Q<Label>("player-rank");
            _playerRankLabel.text = $"# {m_GameScene.playerRankDict[_player]}";
        }
    }
}