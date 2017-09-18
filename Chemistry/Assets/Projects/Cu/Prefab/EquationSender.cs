using UnityEngine;
using System.Collections;

public class EquationSender : MonoBehaviour {
    public string title = "化学反应";
    public void Send(string info)
    {
        PresentationData data = PresentationData.Allocate(title, info, info);
        BundleUISystem.UIGroup.Open<PresentationPanel>(data);
    }
}
