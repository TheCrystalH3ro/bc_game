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
        public int instanceId;

        public SceneChangeData(NetworkObject player, Vector3 position, string scene, string previousScene, int instanceId) : this()
        {
            this.player = player;
            this.position = position;
            this.scene = scene;
            this.previousScene = previousScene;
            this.instanceId = instanceId;
        }
    }
}