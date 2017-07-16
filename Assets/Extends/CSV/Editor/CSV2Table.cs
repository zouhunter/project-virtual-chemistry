using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class CSV2Table : EditorWindow
{
	TextAsset csv = null;
	string[][] arr = null;
    static string[] type = null;
	MonoScript script = null;
	bool foldout = true;

	[MenuItem("Window/CSV to Table")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(CSV2Table));
    }
    void OnEnable()
    {
        arr = null;
    }

    void OnGUI()
	{
		// CSV
		TextAsset newCsv = EditorGUILayout.ObjectField("CSV", csv, typeof(TextAsset), false) as TextAsset;
		if(newCsv != csv)
		{
			csv = newCsv;
			if(csv != null)
				arr = ParserCSV.Parse(csv.text);
			else
				arr = null;
		}

		// Script
		script = EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false) as MonoScript;

		// buttons
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Refresh") && csv != null)
        {
            arr = ParserCSV.Parse(csv.text);
            type = new string[arr[0].Length];
        }
		if(GUILayout.Button("Generate Code"))
		{
			string path = "";
			if(script != null)
			{
				path = AssetDatabase.GetAssetPath(script);
			}
			else
			{
				path = EditorUtility.SaveFilePanel("Save Script", "Assets", csv.name + "Table.cs", "cs");
            }
            if(!string.IsNullOrEmpty(path))
                script = CreateScript(csv, path);
        }
		EditorGUILayout.EndHorizontal();

		// columns
		if(arr != null)
		{
			foldout = EditorGUILayout.Foldout(foldout, "Columns");
			if(foldout)
			{
				EditorGUI.indentLevel++;
				if(csv != null && arr == null)
                {
					arr = ParserCSV.Parse(csv.text);
                    type = new string[arr[0].Length];
                }
				if(arr != null)
				{
                    if(type == null) type = new string[arr[0].Length];
                    for (int i = 0 ; i < arr[0].Length ; i++)
					{
                        EditorGUILayout.LabelField(arr[0][i]);
                        type[i] = EditorGUILayout.TextField(type[i]);
                        if (string.IsNullOrEmpty(type[i]))
                        {
                            type[i] = "string";
                        }
					}
				}
				EditorGUI.indentLevel--;
			}
		}
	}

	public static MonoScript CreateScript(TextAsset csv, string path)
	{
		if(csv == null || string.IsNullOrEmpty(csv.text))
			return null;

		string className = Path.GetFileNameWithoutExtension(path);
		string code = TableCodeGen.Generate(csv.text, type, className);
		
		File.WriteAllText(path, code);
		Debug.Log("Table script generated: " + path);
		
		AssetDatabase.Refresh();
		
		// absolute path to relative
		if (path.StartsWith(Application.dataPath))
		{
			path = "Assets" + path.Substring(Application.dataPath.Length);
		}
        
        return AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript)) as MonoScript;
	}

    void OnDisable()
    {
        arr = null;
        type = null;
    }
}
