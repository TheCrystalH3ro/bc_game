using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controllers;
using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using Assets.Scripts.Modules;
using Assets.Scripts.Responses;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI.Controllers
{
    public class CharacterSelectController : MonoBehaviour
    {  
        private List<PlayerCharacter> playerCharacters;

        [SerializeField] private Sprite knightSprite;
        [SerializeField] private Sprite wizardSprite;
        [SerializeField] private Sprite rogueSprite;

        [SerializeField] private GameObject characterSlotPrefab;

        [SerializeField] private GameObject characterCreationWindow;

        [SerializeField] private CharacterInfo characterInfoWindow;
        [SerializeField] private GameObject deleteConfirmation;

        private CharacterSlotController selectedCharacter;

        void Awake()
        {
            selectedCharacter = null;

            FetchCharacters();
        }

        private Sprite GetCharacterSprite(PlayerClass playerClass)
        {
            return playerClass switch
            {
                PlayerClass.Wizard => wizardSprite,
                PlayerClass.Rogue => rogueSprite,
                _ => knightSprite,
            };
        }

        public void DeselectCharacter()
        {
            selectedCharacter = null;
        }

        public void OpenCharacterSelect(CharacterSlotController characterSlot)
        {
            selectedCharacter = characterSlot;
            PlayerCharacter playerCharacter = characterSlot.GetPlayerCharacter();

            characterInfoWindow.ShowCharacterInfo(playerCharacter.GetName(), GetCharacterSprite(playerCharacter.GetPlayerClass()), playerCharacter.GetLevel());
        }

        public void CloseCharacterSelect()
        {
            selectedCharacter = null;
            characterInfoWindow.CloseCharacterInfo();
        }

        public void OpenCharacterCreation(CharacterSlotController characterSlot)
        {
            selectedCharacter = characterSlot;
            characterCreationWindow.SetActive(true);
        }

        public void OpenDeleteConfirmation()
        {
            deleteConfirmation.SetActive(true);
        }

        public void CancelDelete()
        {
            deleteConfirmation.SetActive(false);
        }

        public void DeleteCharacterButton()
        {
            DeleteCharacter();
        }

        private void FetchCharacters()
        {
            playerCharacters = new List<PlayerCharacter>();

            string jwtToken = PlayerPrefs.GetString("authToken");

            CharacterModule.Singleton.GetCharacters(jwtToken, OnFetchSuccess, OnFetchFail);
        }

        private void OnFetchSuccess(CharactersResponse characters)
        {
            foreach(var character in characters.characters) {
                playerCharacters.Add(new PlayerCharacter(character));
            }

            LoadCharacters();
        }

        private void OnFetchFail(string errorMessage)
        {
            Debug.LogError("Error while trying to login: " + errorMessage);
        }

        private void LoadCharacters()
        {
            GameObject[] characterSlots = GameObject.FindGameObjectsWithTag("CharacterSlot");
            characterSlots = characterSlots.OrderBy(go => go.transform.GetSiblingIndex()).ToArray();

            int index = 0;
            foreach(GameObject slot in characterSlots) {

                GameObject characterSlot = Instantiate(characterSlotPrefab);

                CharacterSlotController characterSlotController = characterSlot.GetComponent<CharacterSlotController>();

                characterSlotController.SetCharaterSelectController(this);

                if(index < playerCharacters.Count)
                {
                    PlayerCharacter playerCharacter = playerCharacters[index];
                    characterSlotController.SetPlayerCharacter(playerCharacter, GetCharacterSprite(playerCharacter.GetPlayerClass()));
                }

                characterSlot.transform.SetParent(slot.transform, false);

                index++;
            }
        }

        public void CreateCharacter(CharacterResponse character)
        {
            if(selectedCharacter == null) return;

            PlayerCharacter playerCharacter = new(character);
            playerCharacters.Add(playerCharacter);
            selectedCharacter.SetPlayerCharacter(playerCharacter, GetCharacterSprite(playerCharacter.GetPlayerClass()));
        }

        private void DeleteCharacter()
        {

            deleteConfirmation.SetActive(false);
            characterInfoWindow.CloseCharacterInfo();

            if(selectedCharacter == null || selectedCharacter.GetPlayerCharacter() == null) return;

            string jwtToken = PlayerPrefs.GetString("authToken");

            CharacterModule.Singleton.DeleteCharacter(selectedCharacter.GetPlayerCharacter().GetId(), jwtToken, OnDeleteSuccess, OnDeleteFail);
        }

        private void OnDeleteSuccess()
        {
            selectedCharacter.DeleteCharacter(GetCharacterSprite(PlayerClass.Knight));
            selectedCharacter = null;

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void OnDeleteFail(string errorMessage)
        {
            Debug.LogError("Error while trying to login: " + errorMessage);
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        public void StartGame()
        {
            if(selectedCharacter == null || selectedCharacter.GetPlayerCharacter() == null)
                return;

            PlayerCharacter playerCharacter = selectedCharacter.GetPlayerCharacter();

            PlayerPrefs.SetInt("CharacterId", (int) playerCharacter.GetId());
            PlayerPrefs.SetInt("isServer", 0);

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            SceneManager.LoadScene(SceneModule.MAIN_SCENE_NAME, LoadSceneMode.Single);
        }

        public void Logout()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
    }
}
