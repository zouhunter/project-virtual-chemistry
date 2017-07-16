using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

[System.Serializable]
public class HoverOutLine: MouseEvent
{
    public Renderer singleRenderer;
    public Renderer[] renderers;
    public Color color = Color.red;
    public Shader m_HighLight;

    private Shader local;
    private Shader[] locals;

    [Range(0, 0.03f)]
    public float scale;

    [SerializeField]
    private bool enable;
    public bool Enable { get { return enable; } set { enable = value; } }

    //第一次打开时，将UI创建图标
    public void Start()
    {
        local = singleRenderer.material.shader;
        if (renderers.Length > 0)
        {
            locals = new Shader[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                locals[i] = renderers[i].material.shader;
            }
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
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Cursor.visible)
        {
            SetOutLineShader(singleRenderer);

            if (renderers.Length > 0)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    SetOutLineShader(renderers[i]);
                }
            }
        }
    }
    public void OnMouseExit()
    {
        SetOldShader(singleRenderer, local);

        if (renderers.Length > 0)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                SetOldShader(renderers[i], locals[i]);
            }
        }
    }

    void SetOldShader(Renderer render, Shader old)
    {
        if(render) render.material.shader = old;
    }
    void SetOutLineShader(Renderer render)
    {
        if (render)
        {
            render.material.shader = m_HighLight;
            render.material.SetColor("_OutlineColor", color);
            render.material.SetFloat("_Outline", scale);
        }
    }
}
