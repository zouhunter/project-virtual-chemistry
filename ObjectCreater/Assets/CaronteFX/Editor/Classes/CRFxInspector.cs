using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CaronteFX
{
  [CustomEditor(typeof(Caronte_Fx))]
  public class Caronte_Fx_Inspector : Editor
  {

    public override void OnInspectorGUI()
    {
      Caronte_Fx fxData = target as Caronte_Fx;
      
      EditorGUILayout.Space();
      EditorGUILayout.Space();
      GUILayout.BeginHorizontal();  
      GUILayout.FlexibleSpace();
      if (GUILayout.Button("Open in CaronteFX Editor", GUILayout.Height(30f) ) )
      { 
        CRManagerEditor window = (CRManagerEditor)CRManagerEditor.Init();
        window.Controller.SetFxDataActive(fxData);
      }
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();
      EditorGUILayout.Space();
      EditorGUILayout.Space();
    }
  }
}