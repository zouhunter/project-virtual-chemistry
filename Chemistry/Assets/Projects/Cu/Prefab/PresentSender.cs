using UnityEngine;
using System.Collections;

public class PresentSender : MonoBehaviour {
    public string title;
    public string text;
    public void Send()
    {
        PresentationData data = PresentationData.Allocate(title, text, text);
        BundleUISystem.UIGroup.Open<PresentationPanel>(data);
    }

}
