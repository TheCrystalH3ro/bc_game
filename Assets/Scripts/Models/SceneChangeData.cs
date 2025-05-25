using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public struct SceneChangeData
    {
        public NetworkObject player;
        public Vector3 position;

        public SceneChangeData(NetworkObject player, Vector3 position) : this()
        {
            this.player = player;
            this.position = position;
        }
    }
}