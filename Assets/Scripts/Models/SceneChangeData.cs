using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Models
{
    public struct SceneChangeData
    {
        public NetworkObject player;
        public Vector3 position;
        public string scene;
        public string previousScene;

        public SceneChangeData(NetworkObject player, Vector3 position, string scene, string previousScene) : this()
        {
            this.player = player;
            this.position = position;
            this.scene = scene;
            this.previousScene = previousScene;
        }
    }
}