using FishNet;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Modules
{
    public class SpawnerModule : MonoBehaviour
    {
        public NetworkObject objectToSpawn;
        [SerializeField] private float xOffset = 0;
        [SerializeField] private float yOffset = 0;
        [SerializeField] private float zOffset = 0;

        void Start()
        {
            if (!InstanceFinder.IsServerStarted)
                return;

            SpawnObject();
        }

        public virtual NetworkObject SpawnObject()
        {
            var objectInstance = Instantiate(objectToSpawn);

            float xMin = gameObject.transform.position.x - xOffset;
            float xMax = gameObject.transform.position.x + xOffset;

            float yMin = gameObject.transform.position.y - yOffset;
            float yMax = gameObject.transform.position.y + yOffset;

            float zMin = gameObject.transform.position.z - zOffset;
            float zMax = gameObject.transform.position.z + zOffset;

            float x = Random.Range(xMin, xMax);
            float y = Random.Range(yMin, yMax);
            float z = Random.Range(zMin, zMax);

            objectInstance.transform.position = new Vector3(x, y, z);

            InstanceFinder.ServerManager.Spawn(objectInstance, null, gameObject.scene);

            return objectInstance;
        }
    }
}