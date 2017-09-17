using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public static class WebUtility {
    public const string type_file = "file:///";
    /// <summary>
    /// 异步加载图片
    /// </summary>
    /// <param name="relPath"></param>
    /// <param name="sprite"></param>
    /// <returns></returns>
    public static IEnumerator LoadSprite(string urltype,string path,UnityEngine.UI.Image image)
    {
        string m_path = urltype + path;
        WWW www = new WWW(m_path);///暂时不考虑更新问题
        yield return www;

        if (www.error == null)
        {
            image.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), Vector2.one * 0.5f);
        }
        else 
        {
            //Debug.LogError(www.error);
        }
    }
}

