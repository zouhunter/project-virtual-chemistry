using UnityEngine;
using System.Collections;

public class PresentSender : MonoBehaviour {
    public string title;
    public string text;
    public bool tipOnly;
    public void Send()
    {
        if(tipOnly)
        {
            SceneMain.Current.InvokeEvents<string>(AppConfig.EventKey.TIP, text);
        }
        else
        {
            PresentationData data = PresentationData.Allocate(title, text, text);
            BundleUISystem.UIGroup.Open<PresentationPanel>(data);
        }
      
    }

}
