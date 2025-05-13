using System.Collections;
using Assets.Scripts.Models;
using Assets.Scripts.UI.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Controllers
{
    public class CharacterSlotController : MonoBehaviour
    {
        private PlayerCharacter character;
        private CharacterSelectController characterSelectController;

        [SerializeField] Image characterImage;

        public void SetPlayerCharacter(PlayerCharacter playerCharacter, Sprite sprite)
        {
            this.character = playerCharacter;

            characterImage.sprite = sprite;

            characterImage.color = Color.white;
        }

        public PlayerCharacter GetPlayerCharacter()
        {
            return this.character;
        }

        public void SetCharaterSelectController(CharacterSelectController controller)
        {
            this.characterSelectController = controller;
        }

        public void SelectCharacter()
        {
            if(characterSelectController == null) return;

            if(character == null)
            {
                characterSelectController.OpenCharacterCreation(this);
                return;
            }

            characterSelectController.OpenCharacterSelect(this);
        }

        public void DeleteCharacter(Sprite defaultSprite)
        {
            this.character = null;

            characterImage.sprite = defaultSprite;
            characterImage.color = new Color(0f, 0f, 0f, 206f / 255f);
        }
    }
}
