//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2012 Ian Deane
//----------------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class MB_EditorUtil
{
	static public void DrawSeparator ()
	{
		GUILayout.Space(12f);

		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = EditorGUIUtility.whiteTexture;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
			GUI.color = Color.white;
		}
	}
	
	static public Rect DrawHeader (string text)
	{
		GUILayout.Space(28f);
		Rect rect = GUILayoutUtility.GetLastRect();
		rect.yMin += 5f;
		rect.yMax -= 4f;
		rect.width = Screen.width;

		if (Event.current.type == EventType.Repaint)
		{
			GUI.color = Color.black;
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(0f, rect.yMin, Screen.width, 1f), EditorGUIUtility.whiteTexture);
			GUI.DrawTexture(new Rect(0f, rect.yMax - 1, Screen.width, 1f), EditorGUIUtility.whiteTexture);
			GUI.color = Color.white;
			GUI.Label(new Rect(rect.x + 4f, rect.y, rect.width - 4, rect.height), text, EditorStyles.boldLabel);
		}
		return rect;
	}

	public static void RegisterUndo(UnityEngine.Object o, string s){
#if UNITY_4_3 || UNITY_4_3_0
			Undo.RecordObject(o,s);
#else
			Undo.RecordObject(o, s);
#endif
	}

	public static void SetInspectorLabelWidth(float width){
#if UNITY_4_3 || UNITY_4_3_0
		EditorGUIUtility.labelWidth = width;
#else
		EditorGUIUtility.fieldWidth = width;
#endif
	}
}