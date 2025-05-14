using FishNet.Object.Prediction;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public struct MovementReplicateData : IReplicateData
    {
        public Vector2 direction;

        private uint _tick;

        public void Dispose() {}
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }
}