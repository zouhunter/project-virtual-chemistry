using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class SelectDrawer : MonoBehaviour
{
    public Material green;
    public Material red;
    private Vector3 startPos;
    private bool needDraw;
   
    public void OnPostRender()
    {
        if (needDraw)
        {
            DrawQuater();
        }
    }

    public void DrawQuater()
    {
        Vector3 endPos = Input.mousePosition;
        Vector3[] linePoints = new Vector3[4];
        linePoints[0] = startPos;
        linePoints[1] = startPos + Vector3.up * (endPos.y - startPos.y);
        linePoints[2] = endPos;
        linePoints[3] = startPos + Vector3.right * (endPos.x - startPos.x);

        GL.PushMatrix();
        green.SetPass(0);

        //坐标转化
        GL.LoadPixelMatrix();
        GL.Begin(GL.LINES);

        for (int i = 0; i < linePoints.Length; ++i)
        {
            if (i + 1 == linePoints.Length)
            {
                GL.Vertex(linePoints[i]);
                GL.Vertex(linePoints[0]);
            }
            else
            {
                GL.Vertex(linePoints[i]);
                GL.Vertex(linePoints[i + 1]);
            }
        }
        GL.Vertex(linePoints[3]);
        GL.Vertex(linePoints[0]);

        GL.End();
        GL.PopMatrix();
    }
   
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            needDraw = true;
            startPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            needDraw = false;
        }
        if (EventSystem.current.IsPointerOverGameObject())
        {
            needDraw = false;
        }
    }
}

