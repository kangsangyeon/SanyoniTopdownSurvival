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

    [System.Serializable]
    public struct Player_OnKill_EventParam
    {
        public PlayerInfo target;
    }

    [System.Serializable]
    public struct Player_OnDead_EventParam
    {
        // TODO
        // object
    }

    // [System.Serializable]
    // public struct Player_OnAbilityAdded_EventParam
    // {
    //     public PlayerInfo player;
    //     public string abilityId;
    // }
    //
    // [System.Serializable]
    // public struct Player_OnAbilityRemoved_EventParam
    // {
    //     public PlayerInfo player;
    //     public string abilityId;
    // }

    [System.Serializable]
    public struct Player_RequestAddAbilityParam
    {
        public string abilityId;
    }

    [System.Serializable]
    public struct Player_ObserversRpc_AddAbility_EventParam
    {
        public PlayerInfo player;
        public string abilityId;
    }

    [System.Serializable]
    public struct Player_ObserversRpc_RemoveAbility_EventParam
    {
        public PlayerInfo player;
        public string abilityId;
    }
}