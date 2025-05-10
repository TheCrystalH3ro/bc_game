using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    public class TextHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Texture2D pointerTexture;
        [SerializeField] private CanvasGroup underline;

        private TMP_Text textComponent;

        private void Start() {
            textComponent = GetComponent<TMP_Text>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {

            if(underline != null) {
                LeanTween.alphaCanvas(underline, 1f, 0.25f);
            }

            Cursor.SetCursor(pointerTexture, Vector2.zero, CursorMode.Auto);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(underline != null) {
                LeanTween.alphaCanvas(underline, 0f, 0.25f);
            }

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}
