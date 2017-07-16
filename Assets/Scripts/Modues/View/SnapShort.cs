using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class SnapShort : MonoBehaviour {
#if UNITY_EDITOR
    [InspectorButton("Snap")]
    public int 截图;
    [Range(1,10)]
    public int superSize;
    public string picName;
    void Snap()
    {
        Application.CaptureScreenshot(string.Format("{0}/{1}", Application.dataPath, picName), superSize);
        UnityEditor.AssetDatabase.Refresh();
    }
#endif
}

