using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
namespace FlowSystem
{
    public class GroupRecord : EditorWindow
    {
        [MenuItem("Window/GroupRecord")]
        static void RecordGroup()
        {
            EditorWindow window =  GetWindow<GroupRecord>("分组配制");
            window.position = new Rect(300, 300, 600, 400);
            window.Show();
        }

        private ExperimentDataObject experimentData;
        private ConfigGroupObject configData;
        private Transform objectsParent;
        private string _title;
        void OnGUI()
        {
            objectsParent = (Transform)EditorGUILayout.ObjectField(objectsParent, typeof(Transform), true, GUILayout.Width(100));
            using (var group = new EditorGUILayout.HorizontalScope())
            {
                experimentData = (ExperimentDataObject)EditorGUILayout.ObjectField(experimentData, typeof(ExperimentDataObject), false, GUILayout.Width(200));
                if (GUILayout.Button("保存初始位置"))
                {
                    SaveStartUpGroup();
                }
            }

            using (var group = new EditorGUILayout.HorizontalScope())
            {
                configData = (ConfigGroupObject)EditorGUILayout.ObjectField(configData, typeof(ConfigGroupObject), false, GUILayout.Width(200));
                _title = EditorGUILayout.TextField(_title);
                if (GUILayout.Button("新建一配制组合"))
                {
                    CreateNewGroup();
                }
            }
        }
        // This is called when the user clicks on the Create button.  
        void CreateNewGroup()
        {
            ConfigGroup group = new ConfigGroup(_title);
            configData.groupList.Add(group);

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
                group.elements.Add(element);
            }
            EditorUtility.SetDirty(configData);
        }

        // Allows you to provide an action when the user clicks on the   
        // other button.  
        void SaveStartUpGroup()
        {
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