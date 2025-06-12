using System;
using Assets.Scripts.Enums;
using Assets.Scripts.Requests;
using Assets.Scripts.Responses;
using UnityEngine;

namespace Assets.Scripts.Requests
{
    [Serializable]
    public class PlayerDataRequest : BaseRequest
    {
        public uint id;
        public ushort level;
        public float experience;
        public WorldData worldData;

        public PlayerDataRequest(uint id, ushort level, float experience, WorldData worldData)
        {
            this.id = id;
            this.level = level;
            this.experience = experience;
            this.worldData = worldData;
        }
    }
}
