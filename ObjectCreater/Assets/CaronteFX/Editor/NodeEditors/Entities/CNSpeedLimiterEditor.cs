using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;

namespace CaronteFX
{
  public class CNSpeedLimiterEditor : CNEntityEditor
  {
    public static Texture icon_;

    public override Texture TexIcon { get{ return icon_; } }

    new CNSpeedLimiter Data { get; set; }

    public CNSpeedLimiterEditor(CNSpeedLimiter data, CommandNodeEditorState state)
      : base( data, state )
    {
      Data = data;
    }

    public override void SceneSelection()
    {
      FieldController.SceneSelectionTopMost();
      FieldController.IsBodyField = true;
    }

    public override void FreeResources()
    {
      FieldController.DestroyField();
      eManager.DestroyEntity( Data );
    }

    public override void CreateEntitySpec()
    {
      eManager.CreateSpeedLimiter(Data);
    }

    public override void ApplyEntitySpec()
    { 
      GameObject[] arrGameObject = FieldController.GetUnityGameObjects();
      eManager.RecreateSpeedLimiter(Data, arrGameObject);
    }

    public void DrawSpeedLimit()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Speed Limit", Data.SpeedLimit );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change speed limit - " + Data.Name);
        Data.SpeedLimit = value;
        EditorUtility.SetDirty( Data );
      }
    }

    public void DrawFallingSpeedLimit()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Falling Speed Limit", Data.FallingSpeedLimit );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change falling speed limit - " + Data.Name);
        Data.FallingSpeedLimit = value;
        EditorUtility.SetDirty( Data );
      }
    }


    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);
      
      RenderTitle(isEditable, true, false);

      EditorGUI.BeginDisabledGroup( !isEditable );
      RenderFieldObjects("Bodies", FieldController, true, true, CNFieldWindow.Type.extended);

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      CRGUIUtils.DrawSeparator();
      CRGUIUtils.Splitter();

      EditorGUILayout.Space();
      EditorGUILayout.Space();
      bool currentMode = EditorGUIUtility.wideMode;
      EditorGUIUtility.wideMode = true;
      DrawTimer();
      EditorGUILayout.Space();
      DrawSpeedLimit();
      DrawFallingSpeedLimit();
      EditorGUIUtility.wideMode = currentMode;
      
      EditorGUILayout.Space();
      EditorGUI.EndDisabledGroup();
      GUILayout.EndArea();
    }
  }
}
