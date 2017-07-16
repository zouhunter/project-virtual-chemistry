using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
[System.Serializable]
public class HoverHandShow: MouseEvent
{
    public Texture2D handTexture;
    public Texture2D oldTexture;
    private Vector2 handPos = new Vector2(12, 12);

    [SerializeField]
    private bool enable;
    public bool Enable { get { return enable; } set { enable = value; } }

    public void Start()
    {
        if (Cursor.visible)
        {
            Cursor.SetCursor(oldTexture, handPos, CursorMode.ForceSoftware);
        }
    }

    public void OnMouseOver()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            OnMouseExit();
        }
    }
    public void OnMouseEnter()
    {
        if (Cursor.visible)
        {
            Cursor.SetCursor(handTexture, handPos, CursorMode.ForceSoftware);
        }
    }
    public void OnMouseExit()
    {
        if (Cursor.visible && !EventSystem.current.IsPointerOverGameObject())
        {
            Cursor.SetCursor(oldTexture, handPos, CursorMode.ForceSoftware);
        }
    }
}
