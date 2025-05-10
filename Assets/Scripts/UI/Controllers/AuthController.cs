using System.Collections;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Modules;
using Assets.Scripts.Modules.Validation.Providers;
using Assets.Scripts.Responses;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI.Controllers
{
    public class AuthController : MonoBehaviour
    {
        [SerializeField] private string apiUrl;

        [SerializeField] private MainMenuController mainMenuController;

        [SerializeField] private int maxNameLength;
        [SerializeField] private int maxEmailLength;
        [SerializeField] private int maxPasswordLength;

        [SerializeField] private TMP_InputField loginField;
        [SerializeField] private TMP_InputField passwordField;

        [SerializeField] private TMP_InputField registerNameField;
        [SerializeField] private TMP_InputField registerEmailField;
        [SerializeField] private TMP_InputField registerPasswordField;
        [SerializeField] private TMP_InputField registerPasswordConfirmationField;

        void Awake()
        {
            IValidationProvider validationProvider = new AuthValidationProvider(maxNameLength, maxEmailLength, maxPasswordLength);
            AuthModule.Initialize(apiUrl, ValidationModule.Singleton.Validate, validationProvider);
        }

        public void LoginClick()
        {
            Login();
        }

        public void RegisterClick()
        {
            Register();
        }

        public void Login()
        {
            string username = loginField.text;
            string password = passwordField.text;

            StartCoroutine(AuthModule.Singleton.Login(username, password, OnLoginSuccess, OnLoginFail));
        }

        public void Register()
        {
            string username = registerNameField.text;
            string email = registerEmailField.text;
            string password = registerPasswordField.text;
            string passwordConfirmation = registerPasswordConfirmationField.text;

            StartCoroutine(AuthModule.Singleton.Register(username, email, password, passwordConfirmation, OnRegisterSuccess, OnRegisterFail));
        }

        private void OnLoginSuccess(string jwtToken)
        {
            PlayerPrefs.SetString("authToken", jwtToken);

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            SceneManager.LoadScene("CharacterSelect", LoadSceneMode.Single);
        }

        private void OnLoginFail(string errorMessage)
        {
            Debug.LogError("Error while trying to login: " + errorMessage);
            mainMenuController.ShowMessageBox(errorMessage);
        }

        private void OnRegisterSuccess()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            mainMenuController.ShowMessageBox("Your account has been successfully created. Get started by logging in.");
            mainMenuController.OpenLoginMenu();
        }

        private void OnRegisterFail(ApiErrors errors)
        {
            string errorMessage = "";

            foreach (var error in errors.errors)
            {     
                Debug.Log(error.field);
                errorMessage += error.message + "\n";
            }

            mainMenuController.ShowMessageBox(errorMessage);
        }
    }
}
