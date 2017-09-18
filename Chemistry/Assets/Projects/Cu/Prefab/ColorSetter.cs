using UnityEngine;
using System.Collections;

public class ColorSetter : MonoBehaviour {
    public Color color;

    private const string param = "Color";
    private Material mat;
    private void Awake()
    {
        mat = GetComponent<Renderer>().material;
    }
    public void ChangeColor()
    {
        mat.SetColor("_ReflectColor", color);
    }
}
