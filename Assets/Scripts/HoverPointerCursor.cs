using System.Collections;
using UnityEngine;

public class HoverPointerCursor : MonoBehaviour
{
    [SerializeField] private Texture2D pointerCursor;

    private bool isHovered = false;

    void OnMouseEnter()
    {
        isHovered = true;
        Cursor.SetCursor(pointerCursor, Vector2.zero, CursorMode.Auto);
    }

    void OnMouseExit()
    {
        isHovered = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void SetPointerCursor(Texture2D cursor)
    {
        pointerCursor = cursor;

        if (isHovered)
        {
            Cursor.SetCursor(pointerCursor, Vector2.zero, CursorMode.Auto);
        }
    }
}
