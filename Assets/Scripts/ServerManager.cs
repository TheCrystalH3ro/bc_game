using Assets.Scripts.Enums;
using Assets.Scripts.Modules;
using FishNet;
using FishNet.Connection;
using FishNet.Transporting;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    [SerializeField] private GameObject gameServerControllerPrefab;

    void Start()
    {
        if (PlayerPrefs.GetInt("isServer") == 1)
        {
            InstanceFinder.NetworkManager.ServerManager.OnServerConnectionState += HandleServerStarted;
            ConnectionModule.Singleton.HostServer();

            EnumSeederModule.Singleton.RegisterEnum(typeof(ItemType), "item/type");
            EnumSeederModule.Singleton.RegisterEnum(typeof(PlayerClass), "character/type");
            // EnumSeederModule.Singleton.RegisterEnum(typeof(EnemyType), "enemy");
        }
    }

    private void HandleServerStarted(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState != LocalConnectionState.Started)
            return;

        var instance = Instantiate(gameServerControllerPrefab);
        InstanceFinder.ServerManager.Spawn(instance);
    }
}