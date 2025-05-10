using Assets.Scripts.Modules;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    void Start()
    {
        if(PlayerPrefs.GetInt("isServer") == 1) {
            return;
        }

        ConnectionModule.Singleton.JoinServer();
    }
}