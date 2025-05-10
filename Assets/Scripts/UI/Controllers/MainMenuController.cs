using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI.Controllers
{
    public class MainMenuController : MonoBehaviour
    {

        [SerializeField] private GameObject loginScreen;
        [SerializeField] private GameObject registerScreen;

        [SerializeField] private MessageBox messageBox;

        public void OpenRegisterMenu() {
            loginScreen.SetActive(false);
            registerScreen.SetActive(true);
        }

        public void OpenLoginMenu() {
            registerScreen.SetActive(false);
            loginScreen.SetActive(true);
        }

        public void ShowMessageBox(string message) {
            messageBox.DisplayMessage(message);
        }
    
        public void Quit() {
            Application.Quit();
        }
    }
}
