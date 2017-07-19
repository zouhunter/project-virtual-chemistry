using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
public class Progress : MonoBehaviour
{
    public GameObject Panel;
    public GameObject slider;
    public Transform image;

    public float animTime;

    void OnEnable()
    {
        GameManager.onSceneLoad += OnSceneLoad;
    }
    void Start(){
        Panel.SetActive(false);
    }
    public void OnSceneLoad(string s)
    {
        slider.SetActive(true);

        Panel.SetActive(true);
        image.DORotate(Vector3.forward * 360, 10, RotateMode.LocalAxisAdd).SetLoops(-1);
    }
    void OnDestroy()
    {
        if(GameManager.Instance)
        GameManager.onSceneLoad -= OnSceneLoad;
    }

}
