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
    private Image image;

    public float animTime;

    void OnEnable()
    {
        image = Panel.GetComponent<Image>();
        GameManager.onSceneLoad += OnSceneLoad;
    }
    void Start(){
        Panel.SetActive(false);
    }
    public void OnSceneLoad(string s)
    {
        slider.SetActive(true);

        Panel.SetActive(true);
        image.color = new Color(1, 1, 1, 1);
        image.DOFade(0, animTime).SetEase(Ease.Linear).OnComplete(() => { image.DORewind(); image.gameObject.SetActive(false); });
    }
    void OnDestroy()
    {
        if(GameManager.Instance)
        GameManager.onSceneLoad -= OnSceneLoad;
    }

}
