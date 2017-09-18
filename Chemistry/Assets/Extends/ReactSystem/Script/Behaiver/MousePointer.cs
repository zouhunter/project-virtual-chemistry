using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace ReactSystem
{
    public class MousePointer : MonoBehaviour
    {
        [System.Serializable]
        public struct MouseTexure
        {
            public LayerMask layerMask;
            public Texture2D texture;
        }
        [Range(0, 1)]
        public float updateTime;
        public Vector2 pos = new Vector2(10, 10);
        public Texture2D hideTexture;
        public Texture2D defultTexture;
        [Array]
        public List<MouseTexure> textureList = new List<MouseTexure>();

        private Ray ray;
        private RaycastHit hit;
        private int totalmask;
        private Texture2D currTexture;
        private Texture2D lastTexture;
        private float timer;
        void Start()
        {
            for (int i = 0; i < textureList.Count; i++)
            {
                totalmask += textureList[i].layerMask;
            }
            ChangeToNewTexture(defultTexture);
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (timer > 1 && Camera.main)
            {
                timer = 0f;
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 200, totalmask, QueryTriggerInteraction.Collide))
                {
                    currTexture = textureList.Find((x) => x.layerMask == LayerMask.GetMask(LayerMask.LayerToName(hit.collider.gameObject.layer))).texture;
                    if (currTexture != lastTexture)
                    {
                        ChangeToNewTexture(currTexture);
                        lastTexture = currTexture;
                    }
                }
                else
                {
                    lastTexture = null;
                    ChangeToNewTexture(defultTexture);
                }
            }
        }
        void OnGUI()
        {
            if (!Cursor.visible)
            {
                var mousePos = Input.mousePosition;
                GUI.DrawTexture(new Rect(mousePos.x, Screen.height - mousePos.y, 20, 20), hideTexture);
            }
        }

        void ChangeToNewTexture(Texture2D texture)
        {
            Cursor.SetCursor(texture, pos, CursorMode.ForceSoftware);
        }
    }

}