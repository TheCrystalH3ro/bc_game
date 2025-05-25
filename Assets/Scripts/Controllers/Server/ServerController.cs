using System;
using System.Collections;
using Assets.Scripts.Modules;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Controllers.Server
{
    public class ServerController : MonoBehaviour
    {
        void Start()
        {
            string[] cmdArgs = Environment.GetCommandLineArgs();

            if(Array.Exists(cmdArgs, element => element == "--server"))
                RunServer();
        }

        private void RunServer()
        {
            PlayerPrefs.SetInt("isServer", 1);
            SceneManager.LoadScene(SceneModule.MAIN_SCENE_NAME, LoadSceneMode.Single);
        }

        public void StartServerButtonClick()
        {
            RunServer();
        }
    }
}
