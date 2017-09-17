using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using BundleUISystem;

public class StartLoadingPanel : UIPanelTemp
{
    public GameObject Panel;
    public Transform image;

    public float animTime;
    public override void HandleData(object data)
    {
        base.HandleData(data);
        image.DORotate(Vector3.forward * 360, 10, RotateMode.LocalAxisAdd).SetLoops(-1);
    }
}
