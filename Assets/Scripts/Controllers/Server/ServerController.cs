using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Controllers.Server
{
    public class ServerController : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            string[] cmdArgs = System.Environment.GetCommandLineArgs();

            if(Array.Exists(cmdArgs, element => element == "--server")) {
                RunServer();
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void RunServer() {
            PlayerPrefs.SetInt("isServer", 1);
            SceneManager.LoadScene("Town", LoadSceneMode.Single);
        }

        public void StartServerButtonClick() {
            RunServer();
        }
    }
}
