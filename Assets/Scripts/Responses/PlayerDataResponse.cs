using System;
using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Responses
{
    [Serializable]
    public class PositionData
    {
        public float x;
        public float y;
        public float z;

        public PositionData(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public PositionData(Vector3 position)
        {
            this.x = position.x;
            this.y = position.y;
            this.z = position.z;
        }
    }

    [Serializable]
    public class WorldData
    {
        public string scene;
        public PositionData position;

        public WorldData(string sceneName, PositionData pos)
        {
            scene = sceneName;
            position = pos;
        }
    }

    [Serializable]
    public class PlayerDataResponse : BaseResponse
    {
        public uint id;
        public ushort level;
        public float experience;
        public WorldData worldData;

        public PlayerDataResponse(uint id, ushort level, float experience, WorldData worldData)
        {
            this.id = id;
            this.level = level;
            this.experience = experience;
            this.worldData = worldData;
        }
    }
}
