using FishNet.Object.Prediction;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public struct MovementReconcileData : IReconcileData
    {
        public Vector2 position;
        private uint _tick;

        public MovementReconcileData(Vector2 pos) : this()
        {
            position = pos;
        }

        public void Dispose() {}
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }
}