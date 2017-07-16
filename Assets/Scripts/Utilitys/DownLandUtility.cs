using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public static class DownLandUtility {
    /// <summary>
    /// 下载图片集合
    /// </summary>
    public static void DownLandSprites(string DirName,UnityAction<List<Sprite>> LoadSprite)
    {
        string path_ = string.Format("file:///{0}/{1}/",Application.streamingAssetsPath,DirName);
        GameManager.Instance.StartCoroutine(LoadPics(path_, LoadSprite));
    }
    static IEnumerator LoadPics(string path_, UnityAction<List<Sprite>> LoadSprite)
    {
        WWW www;
        string fileName;
        List<Sprite> sprites = new List<Sprite>();
        for (int i = 0; i < 5; i++)
        {
            fileName = path_ + i + ".png";
            www = new WWW(fileName);
            yield return www;
            if (www.error == null)
            {
                Sprite sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), Vector2.one * 0.5f);
                sprites.Add(sprite);
            }
        }
        LoadSprite(sprites);
    }
}

