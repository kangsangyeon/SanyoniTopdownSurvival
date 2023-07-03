using System.Collections.Generic;
using System.Numerics;

namespace MyProject.Event
{
    [System.Serializable]
    public struct PlayerInfo
    {
        public int connectionId;
        public string name;
        public Vector2 position;
        public int killCount;
        public int power;

        public PlayerInfo(Player _player)
        {
            connectionId = _player.OwnerId;
            name = _player.gameObject.name;
            position = new Vector2(_player.transform.position.x, _player.transform.position.y);
            killCount = _player.killCount;
            power = _player.power;
        }
    }

    [System.Serializable]
    public struct GameJoinedEventParam
    {
        public List<PlayerInfo> playerInfoList;
    }

    [System.Serializable]
    public struct PlayerAddedEventParam
    {
        public PlayerInfo player;
    }

    [System.Serializable]
    public struct PlayerRemovedEventParam
    {
        public PlayerInfo player;
    }

    [System.Serializable]
    public struct PlayerKillEventParam
    {
        public PlayerInfo killer;
        public PlayerInfo target;
    }
}