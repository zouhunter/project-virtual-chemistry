using UnityEngine;

public class InstanceUtility 
{
    public static GameObject NormalInstance(GameObject prefab, Transform parent = null)
    {
        Debug.Assert(prefab != null);
        GameObject go = ObjectManager.Instance.GetPoolObject(prefab,parent,true) as GameObject;
        if (parent != null)
            go.transform.SetParent(parent, true);
        return go;
    }
    public static GameObject MouseBehavierInstance(GameObject prefab, Transform parent = null)
    {
        Debug.Assert(prefab != null);
        GameObject go = ObjectManager.Instance.GetPoolObject(prefab, parent, true,false,false);
        if (parent != null)
            go.transform.SetParent(parent, false);
        go.GetComponent<RecordingPrefab>().GetMouseBehavior();
        return go;
    }
}
