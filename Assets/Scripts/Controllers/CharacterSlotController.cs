using System.Collections;
using Assets.Scripts.Models;
using Assets.Scripts.UI.Controllers;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class CharacterSlotController : MonoBehaviour
    {
        private PlayerCharacter character;
        private CharacterSelectController characterSelectController;

        public void SetPlayerCharacter(PlayerCharacter playerCharacter, Sprite sprite) {
            this.character = playerCharacter;

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;

            spriteRenderer.color = Color.white;
        }

        public PlayerCharacter GetPlayerCharacter() {
            return this.character;
        }

        public void SetCharaterSelectController(CharacterSelectController controller) {
            this.characterSelectController = controller;
        }

        public void OnMouseDown()
        {
            if(characterSelectController == null) {
                return;
            }

            if(character == null) {
                characterSelectController.OpenCharacterCreation(this);
                return;
            }

            characterSelectController.OpenCharacterSelect(this);
        }

        public void DeleteCharacter(Sprite defaultSprite) {
            this.character = null;

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = defaultSprite;
            spriteRenderer.color = new Color(0f, 0f, 0f, 206f / 255f);
        }
    }
}
