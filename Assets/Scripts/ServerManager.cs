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
        }
    }
}