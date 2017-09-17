using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;

namespace CaronteFX
{
  public class CNGravityEditor : CNEntityEditor
  {
    public static Texture icon_;

    public override Texture TexIcon { get{ return icon_; } }

    new CNGravity Data { get; set; }

    public CNGravityEditor(CNGravity data, CommandNodeEditorState state)
      : base( data, state )
    {
      Data = (CNGravity)data;
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
      eManager.CreateGravity( Data );
    }

    public override void ApplyEntitySpec()
    { 
      GameObject[] arrGameObject = FieldController.GetUnityGameObjects();
      eManager.RecreateGravity( Data, arrGameObject );
    }

    public void DrawGravity()
    {
      EditorGUI.BeginChangeCheck();
      var gravity = EditorGUILayout.Vector3Field("Acceleration", Data.Gravity );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change gravity - " + Data.Name);
        Data.Gravity = gravity;
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
      DrawGravity();
      EditorGUIUtility.wideMode = currentMode;
      
      EditorGUILayout.Space();
      EditorGUI.EndDisabledGroup();
      GUILayout.EndArea();
    }
  }
}

