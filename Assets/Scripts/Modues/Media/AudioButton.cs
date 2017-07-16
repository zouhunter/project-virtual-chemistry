using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AudioButton : MonoBehaviour,IPointerDownHandler
{
    //可设置按键音乐
    public string audioClip = "btn02";

    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.Instance.Play(audioClip);
    }
}
