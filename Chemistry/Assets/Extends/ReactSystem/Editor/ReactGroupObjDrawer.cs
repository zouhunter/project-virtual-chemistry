using UnityEngine;
using System.Collections;
using UnityEditor;
namespace ReactSystem
{
    [CustomEditor(typeof(ReactGroupObj))]
    public class ExperimentDataObjectDrawer : Editor
    {
        ReactGroupObj experimentData;
        private void OnEnable()
        {
            experimentData = (ReactGroupObj)target;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Record"))
            {
                SaveStartUpGroup();
            }
            if (GUILayout.Button("Show"))
            {
                ReConnectStart();
            }
        }
        void ReConnectStart()
        {
            GameObject parent = new GameObject(experimentData.expName);
            GameObject child = null;
            for (int i = 0; i < experimentData.elements.Count; i++)
            {
                child = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(experimentData.elements[i].element);
                child.transform.SetParent(parent.transform, false);
                child.transform.position = experimentData.elements[i].position;
                child.transform.rotation = experimentData.elements[i].rotation;
            }
        }
        void SaveStartUpGroup()
        {
            if (Selection.activeGameObject == null) return;
            var objectsParent = Selection.activeTransform;
            experimentData.elements.Clear();
            GameObject pfb;
            GameObject item;
            RunTimeElemet element;
            for (int i = 0; i < objectsParent.childCount; i++)// (Transform item in objectsParent)
            {
                item = objectsParent.GetChild(i).gameObject;
                //pfb = PrefabUtility.GetPrefabObject(item) as GameObject;
                pfb = PrefabUtility.GetPrefabParent(item) as GameObject;
                if (pfb == null)
                {
                    EditorUtility.DisplayDialog("警告", item.name + "对象未制作预制体", "确认");
                    break;
                }
                element = new RunTimeElemet();
                element.name = pfb.name;
                element.element = pfb;
                element.id = i;
                element.position = item.transform.position;
                element.rotation = item.transform.rotation;
                experimentData.elements.Add(element);
            }
        }
    }

}