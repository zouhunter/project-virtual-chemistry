using UnityEngine;
using System.Collections;
using WorldActionSystem;
using UnityEngine.Events;
using System.Collections.Generic;

public class ColorWorpChargeObjAnim : ChargeObjBinding
{
    [SerializeField]
    private float colorTime = 2f;
    [SerializeField]
    private Renderer render;
    private Color currentColor;
    private Queue<Color> targetColor = new Queue<Color>();
    private string lastType;
    //氯化铁
    private static Color fecl2Color = new Color(0.8f, 0.89f, 0.02f);
    //硫酸亚铁
    private static Color feso4Color = new Color(0.02f, 0.89f, 0.97f);
    //氢氧化钠 + 氯化铁
    private static Color feoh3Color = new Color(1, 0.06f, 0);
    //氢氧化钠 + 硫酸亚铁
    private static Color feohso4Color1 = new Color(1, 0.04f, 0);
    private static Color feohso4Color2 = new Color(0.25f, 0.5f, 0.4f);

    private string[] info1 = new string[] { "氢氧化钠 + 氯化铁", "FeCl3+3NaOH  = Fe(OH)3+3NaCl" };
    private string[] info2 = new string[] { "氢氧化钠 + 硫酸亚铁" , "FeSO4+2NaOH  =  Fe(OH)2↓+Na2SO4 \n4Fe(OH)2+O2+2H2O = 4Fe(OH)3" };
    protected override void OnCharge(Vector3 center, ChargeData data, UnityAction onComplete)
    {
        base.OnCharge(center, data, onComplete);
        if (data.type == "氯化铁")
        {
            SetColor(fecl2Color);
        }
        else if (data.type == "硫酸亚铁")
        {
            SetColor(feso4Color);
        }
        else if (data.type == "氢氧化钠")
        {
            if (lastType == "氯化铁")
            {
                targetColor.Enqueue(feoh3Color);
                StartCoroutine(ColorChange(info1));
            }
            else if (lastType == "硫酸亚铁")
            {
                targetColor.Enqueue(feohso4Color1);
                targetColor.Enqueue(feohso4Color2);
                StartCoroutine(ColorChange(info2));
            }
        }
        lastType = data.type;
    }
    IEnumerator ColorChange(string[] info)
    {
        Debug.Log("ColorChange");
        yield return new WaitForSeconds(animTime);
        var onStep = colorTime / targetColor.Count;
        while (targetColor.Count > 0)
        {
            var target = targetColor.Dequeue();
            Debug.Log(onStep);
            for (float i = 0; i < onStep; i += Time.deltaTime)
            {
                var newColor = Color.Lerp(currentColor, target, i / onStep);
                SetColor(newColor);
                Facade.SendNotification("progress", i / onStep);
                yield return null;
            }
            currentColor = target;
        }

        ShowInfo(info);
    }

    private void ShowInfo(string[] info)
    {
        PresentationData data = new PresentationData();
        data.title = info[0];
        data.infomation = info[1];

        BundleUISystem.UIGroup.Open("PresentationPanel", data);
    }

    private void SetColor(Color color)
    {
        render.material.SetColor("_EmissionColor", color);
    }
}
