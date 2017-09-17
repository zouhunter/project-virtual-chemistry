using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public static class CustomRectTransformUtility
{

    public static Rect GetScreenRect(this RectTransform rectTransform, Canvas canvas)
    {

        Vector3[] corners = new Vector3[4];
        Vector3[] screenCorners = new Vector3[2];

        rectTransform.GetWorldCorners(corners);

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
        {
            screenCorners[0] = UnityEngine.RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[1]);
            screenCorners[1] = UnityEngine.RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[3]);
        }
        else
        {
            screenCorners[0] = UnityEngine.RectTransformUtility.WorldToScreenPoint(null, corners[1]);
            screenCorners[1] = UnityEngine.RectTransformUtility.WorldToScreenPoint(null, corners[3]);
        }

        screenCorners[0].y = Screen.height - screenCorners[0].y;
        screenCorners[1].y = Screen.height - screenCorners[1].y;

        return new Rect(screenCorners[0], screenCorners[1] - screenCorners[0]);
    }

    public static Rect GetViewPointRect(this RectTransform rectTransform, Canvas canvas, Camera camera)
    {
        var screenRect = rectTransform.GetScreenRect(canvas);
        float x = screenRect.x/camera.pixelWidth;
        float y = screenRect.y/camera.pixelHeight;
        float w = screenRect.width/ camera.pixelWidth;
        float h = screenRect.height/ camera.pixelHeight;
        return new Rect(x, y, w, h);
    }

}
