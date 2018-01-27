using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.EventSystems;
[System.Serializable]
public class PointerColliderEvent : UnityEvent<GameObject>
{
    protected override MethodInfo FindMethod_Impl(string name, object targetObj)
    {
        return base.FindMethod_Impl(name, targetObj);
    }
}

public class MousePointer : MonoBehaviour
{
    [System.Serializable]
    public struct MouseTexure
    {
        public LayerMask layerMask;
        public Texture2D texture;
    }
    public float distence = 10;
    public Vector2 pos = new Vector2(10, 10);
    public Texture2D hideTexture;
    public Texture2D defultTexture;
    public List<MouseTexure> textureList = new List<MouseTexure>();
    public PointerColliderEvent onPointerObject;
    public PointerColliderEvent onLostPointerObject;

    private GameObject lastObj;
    private Ray ray;
    private RaycastHit hit;
    private int totalmask;
    private Texture2D currTexture;
    //private float timer;
    void Awake()
    {
        for (int i = 0; i < textureList.Count; i++){
            totalmask += textureList[i].layerMask;
        }
        ChangeToNewTexture(defultTexture);
    }

    void Update()
    {
        if (Camera.main)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject() &&
                Physics.Raycast(ray, out hit, distence, totalmask, QueryTriggerInteraction.Collide))
            {
                currTexture = textureList.Find((x) => (x.layerMask & 1<<hit.collider.gameObject.layer) == 1 << hit.collider.gameObject.layer).texture;
                if (lastObj != hit.collider.gameObject)
                {
                    ChangeToNewTexture(currTexture);
                    onPointerObject.Invoke(hit.collider.gameObject);
                    lastObj = hit.collider.gameObject;
                    //timer = 0;
                }
            }
            else
            {
                if (lastObj != null)
                {
                    lastObj = null;
                    onLostPointerObject.Invoke(lastObj);
                    ChangeToNewTexture(defultTexture);
                }
            }
        }
    }
    void OnGUI()
    {
        if (!Cursor.visible)
        {
            var mousePos = Input.mousePosition;
            if (hideTexture != null)
            {
                GUI.DrawTexture(new Rect(mousePos.x, Screen.height - mousePos.y, 20, 20), hideTexture);
            }
        }
    }

    void ChangeToNewTexture(Texture2D texture)
    {
        Cursor.SetCursor(texture, pos, CursorMode.ForceSoftware);
    }
}

