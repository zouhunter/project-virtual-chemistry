using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
namespace WorldActionSystem
{
    public class LayerTool
    {
        [InitializeOnLoadMethod]
        static void AutoAddLayers()
        {
            var fields = typeof(Layers).GetFields(System.Reflection.BindingFlags.GetField|System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public);
            foreach (var item in fields)
            {
                var layer = item.GetValue(null) as string;

                if(Array.Find(InternalEditorUtility.layers,x=>x == layer) == null)
                {
                    Debug.Log("Add Layer:" + layer);
                    SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                    SerializedProperty it = tagManager.GetIterator();
                    while (it.NextVisible(true))
                    {
                        if (it.name == "layers")
                        {
                            for (int i = 0; i < it.arraySize; i++)
                            {
                                if (i == 3 || i == 6 || i == 7) continue;
                                SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
                                if (string.IsNullOrEmpty(dataPoint.stringValue))
                                {
                                    dataPoint.stringValue = layer;
                                    tagManager.ApplyModifiedProperties();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
          
        }

    }
}