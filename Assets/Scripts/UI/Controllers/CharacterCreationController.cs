using System.Collections;
using Assets.Scripts.Enums;
using Assets.Scripts.Modules;
using Assets.Scripts.Responses;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Controllers
{
    public class CharacterCreationController : MonoBehaviour
    {
        [SerializeField] private CharacterSelectController characterSelectController;

        [SerializeField] private TMP_InputField nameInput;

        [SerializeField] private Button knightButton;
        [SerializeField] private Button wizardButton;
        [SerializeField] private Button rogueButton;

        private PlayerClass selectedClass;
        private Button selectedButton;
        private ColorBlock originalColors;

        private void Start()
        {
            selectedClass = PlayerClass.Undefined;
            selectedButton = null;
            originalColors = knightButton.colors;
        }

        public void SelectKnight()
        {
            selectedClass = PlayerClass.Knight;
            PressButton(knightButton);
        }

        public void SelectWizard()
        {
            selectedClass = PlayerClass.Wizard;
            PressButton(wizardButton);
        }

        public void SelectRogue()
        {
            selectedClass = PlayerClass.Rogue;
            PressButton(rogueButton);
        }

        public void PressButton(Button button)
        {
            if(selectedButton != null) {
                selectedButton.colors = originalColors;
            }

            selectedButton = button;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            button.colors = colors;
        }

        public void CloseWindow()
        {
            gameObject.SetActive(false);
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            characterSelectController.DeselectCharacter();

            if(selectedButton != null) {
                selectedButton.colors = originalColors;
            }

            nameInput.text = "";
        }

        public void CreateCharacterButton()
        {
            CreateCharacter();
        }

        private void CreateCharacter()
        {
            if(selectedClass == PlayerClass.Undefined) {
                return;
            }

            string name = nameInput.text;

            if(string.IsNullOrEmpty(name)) {
                return;
            }

            string jwtToken = PlayerPrefs.GetString("authToken");

            CharacterModule.Singleton.CreateCharacter(name, (int) selectedClass, jwtToken, OnCreateSuccess, OnCreateFail);
        }

        public void OnCreateSuccess(CharacterResponse character)
        {
            characterSelectController.CreateCharacter(character);
            CloseWindow();
        }

        public void OnCreateFail(string errorMessage)
        {
            Debug.LogError("Error while trying to create character: " + errorMessage);
            CloseWindow();
        }
    }
}
