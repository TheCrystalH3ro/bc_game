using System.Numerics;
using Assets.Scripts.Responses;
using Unity.VisualScripting;

namespace Assets.Scripts.Interfaces
{
    public interface IPlayerData
    {
        public ushort GetLevel();
        public float GetExperience();
        public WorldData GetWorldData();
    }
}