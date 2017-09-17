using UnityEngine;
using System.Collections;

public class RectTransformData
{
    public Vector3 position;
    public Quaternion rotation;
    public Transform parent;
    public Vector2 pivot;
    public Vector2 sizeDelta;

    public RectTransformData(RectTransform transform)
    {
        Set(transform);
    }

    public void Set(RectTransform transform)
    {
        position = transform.position;
        rotation = transform.rotation;
        parent = transform.parent;
        pivot = transform.pivot;
        sizeDelta = transform.sizeDelta;
    }

    public void Get(RectTransform transform)
    {
        transform.position = position;
        transform.rotation = rotation;
        transform.SetParent(parent, true);
        transform.pivot = pivot;
        transform.sizeDelta = sizeDelta;
    }
}
