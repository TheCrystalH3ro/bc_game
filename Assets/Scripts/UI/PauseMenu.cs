using System.Collections;
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

        public void ReturnToCharacterSelect() {
            GameObject playerData = GameObject.FindGameObjectWithTag("PlayerData");
            Destroy(playerData);

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            SceneManager.LoadScene("CharacterSelect", LoadSceneMode.Single);
        }

        public void Quit() {
            Application.Quit();
        }
    }
}
