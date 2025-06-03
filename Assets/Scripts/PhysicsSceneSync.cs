using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts
{
    public class PhysicsSceneSync : NetworkBehaviour
    {
        public bool _synchronizePhysics = false;
        public bool _synchronizePhysics2D = false;

        private static HashSet<int> _synchronizedScenes = new();

        void Start()
        {
            int sceneHandle = gameObject.scene.handle;

            if (_synchronizedScenes.Contains(sceneHandle))
                return;

            if (_synchronizePhysics || _synchronizePhysics2D)
            {
                _synchronizedScenes.Add(sceneHandle);
                InstanceFinder.TimeManager.OnPrePhysicsSimulation += TimeManager_OnPrePhysicsSimulation;
            }
        }

        void OnDestroy()
        {
            if (InstanceFinder.TimeManager == null)
                return;

            UnregisterPhysicsSimulation();
        }

        public override void OnStopNetwork()
        {
            UnregisterPhysicsSimulation();
        }

        private void TimeManager_OnPrePhysicsSimulation(float delta)
        {
            if (_synchronizePhysics)
                gameObject.scene.GetPhysicsScene().Simulate(delta);

            if (_synchronizePhysics2D)
                gameObject.scene.GetPhysicsScene2D().Simulate(delta);
        }

        private void UnregisterPhysicsSimulation()
        {
            if (_synchronizePhysics || _synchronizePhysics2D)
            {
                _synchronizePhysics = false;
                _synchronizePhysics2D = false;
                _synchronizedScenes.Remove(gameObject.scene.handle);
                InstanceFinder.TimeManager.OnPrePhysicsSimulation -= TimeManager_OnPrePhysicsSimulation;
            }
        }
    }
}