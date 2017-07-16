using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Text;
using System.Yaml;
using System.Yaml.Serialization;
using System;
using System.Reflection;

[System.Serializable]
public class SerializingToText
{
    public string className;
    public string fileName;

    public SerializingToText()
    {
        if (Selection.activeObject != null)
        {
            className = Selection.activeObject.name;
        }
    }
    public void GUI()
    {
        if (GUILayout.Button("序列化"))
        {
            CreateText();
        }
    }
    void CreateText()
    {
        if (className == null || className == "") return;
        if (fileName == null || fileName == "") fileName = className;
        string path = string.Format("{0}/{1}.txt",Application.streamingAssetsPath ,fileName);

        var serializer = new YamlSerializer();
      
        Type type = Assembly.LoadFile("E:/LocalProject/Unity3D/Chemistry/Chemistry/Library/ScriptAssemblies/Assembly-CSharp.dll").GetType(className);
        
        object handle = Activator.CreateInstance(type);

        serializer.SerializeToFile(path, handle);

        AssetDatabase.Refresh();
    }
}
