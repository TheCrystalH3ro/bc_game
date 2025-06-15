using System.Collections;
using Assets.Scripts.Controllers;
using Assets.Scripts.Modules;
using Assets.Scripts.UI.Controllers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI
{
    public class PauseMenu : MonoBehaviour
    {

        public void TogglePauseMenu() {

            if(gameObject.activeSelf) {
                CloseMenu();
                return;
            }

            gameObject.SetActive(true);
        }

        public void CloseMenu() {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            gameObject.SetActive(false);
        }

        public void ReturnToCharacterSelect()
        {
            HUDController.Singleton.ShowLoadingScreen();
            GameController.Singleton.Disconnect();
        }

        public void Quit()
        {
            ConnectionModule.Singleton.Disconnect();
            Application.Quit();
        }
    }
}
