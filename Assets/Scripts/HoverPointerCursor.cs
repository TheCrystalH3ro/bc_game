using System.Collections;
using UnityEngine;

public class HoverPointerCursor : MonoBehaviour
{
    [SerializeField] private Texture2D pointerCursor;

    void OnMouseEnter()
    {
        Cursor.SetCursor(pointerCursor, Vector2.zero, CursorMode.Auto);
    }

    void OnMouseExit()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void SetPointerCursor(Texture2D cursor) {
        pointerCursor =  cursor;
    }
}
