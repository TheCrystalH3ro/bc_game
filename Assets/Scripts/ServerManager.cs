using Assets.Scripts.Modules;
using FishNet;
using FishNet.Connection;
using UnityEngine;

public class ServerManager : MonoBehaviour 
{
    public GameObject gameControllerPrefab;

    void Start()
    {
        if(PlayerPrefs.GetInt("isServer") == 1)
        {
            ConnectionModule.Singleton.HostServer();

            InstanceFinder.SceneManager.OnClientLoadedStartScenes += HandleClientLoaded;
            return;
        }
    }

    private void HandleClientLoaded(NetworkConnection client, bool asServer)
    {
        if (!asServer)
        {
            return;
        }

        GameObject gameController = Instantiate(gameControllerPrefab);
        InstanceFinder.ServerManager.Spawn(gameController, client);
    }
}