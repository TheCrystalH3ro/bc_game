using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class CharacterInfo : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text nameDisplay;
        [SerializeField] private TMPro.TMP_Text levelDisplay;
        [SerializeField] private Image avatar;

        public void ShowCharacterInfo(String name, Sprite sprite, ushort level) {

            nameDisplay.text = name;
            avatar.sprite = sprite;
            levelDisplay.text = string.Format("Level {0}", level);

            gameObject.SetActive(true);
        }

        public void CloseCharacterInfo() {
            gameObject.SetActive(false);
        }
    }
}
