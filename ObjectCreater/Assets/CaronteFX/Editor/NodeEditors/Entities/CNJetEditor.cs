using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;

namespace CaronteFX
{
  public class CNJetEditor : CNEntityEditor
  {
    public static Texture icon_;

    public override Texture TexIcon { get{ return icon_; } }

    protected CNFieldController FieldControllerLocators { get; set; }

    new CNJet Data { get; set; }

    public CNJetEditor(CNJet data, CommandNodeEditorState state)
      : base( data, state )
    {
      Data = data;
    }

   //-----------------------------------------------------------------------------------
    public override void Init()
    {
      base.Init();

      CNField.AllowedTypes allowedTypes =   CNField.AllowedTypes.Geometry 
                                          | CNField.AllowedTypes.BodyNode;

      FieldController.SetFieldType(allowedTypes);
      FieldController.IsBodyField = true;

      FieldControllerLocators = new CNFieldController( Data, Data.Locators, eManager, goManager );
      FieldControllerLocators.SetFieldType( CNField.AllowedTypes.Locator | CNField.AllowedTypes.Geometry );
      FieldControllerLocators.SetCalculatesDiff(true);
    }
    //----------------------------------------------------------------------------------
    public override void LoadInfo()
    {
      base.LoadInfo();
      FieldControllerLocators.RestoreFieldInfo();
    }
    //----------------------------------------------------------------------------------
    public override void StoreInfo()
    {
      base.StoreInfo();
      FieldControllerLocators.StoreFieldInfo();
    }
    //----------------------------------------------------------------------------------
    public override void BuildListItems()
    {
      base.BuildListItems();
      FieldControllerLocators.BuildListItems();
    }
    //----------------------------------------------------------------------------------
    public override void SetScopeId(uint scopeId)
    {
      base.SetScopeId(scopeId);
      FieldControllerLocators.SetScopeId(scopeId);
    }
    //-----------------------------------------------------------------------------------
    public override void FreeResources()
    {
      FieldControllerLocators.DestroyField();
      base.FreeResources();    
    }
    //----------------------------------------------------------------------------------
    public override void CreateEntitySpec()
    {
      eManager.CreateJet( Data );
    }
    //----------------------------------------------------------------------------------
    public override void ApplyEntitySpec()
    { 
      GameObject[] arrGameObject = FieldController.GetUnityGameObjects();
      GameObject[] arrLocators   = FieldControllerLocators.GetUnityGameObjects();
      eManager.RecreateJet( Data, arrGameObject, arrLocators );
    }
    //----------------------------------------------------------------------------------
    public void DrawForce()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Vector3Field("Force", Data.Force );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change force - " + Data.Name);
        Data.Force = value;
        EditorUtility.SetDirty( Data );
      }
    }

    //----------------------------------------------------------------------------------
    public void DrawSpeedLimit()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Speed limit", Data.SpeedLimit );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change speed limit - " + Data.Name);
        Data.SpeedLimit = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty( Data );
      }
    }
    //----------------------------------------------------------------------------------
    public void DrawForceDeltaMax()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Force delta max.", Data.ForceDeltaMax );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change force delta max. - " + Data.Name);
        Data.ForceDeltaMax = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty( Data );
      }
    }
    //----------------------------------------------------------------------------------
    public void DrawAngleDeltaMax()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Angle delta max.", Data.AngleDeltaMax );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change angle delta max. - " + Data.Name);
        Data.AngleDeltaMax = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty( Data );
      }
    }
    //----------------------------------------------------------------------------------
    private void DrawPeriodTime()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Temporal period", Data.PeriodTime);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change temporal period" + Data.Name);
        Data.PeriodTime = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    private void DrawPeriodSpace()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Spatial period", Data.PeriodSpace);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change spatial period" + Data.Name);
        Data.PeriodSpace = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);
      
      RenderTitle(isEditable, true, false);

      EditorGUI.BeginDisabledGroup( !isEditable );
      RenderFieldObjects("Bodies",   FieldController, true, true, CNFieldWindow.Type.extended);
      RenderFieldObjects("Locators", FieldControllerLocators, true, true, CNFieldWindow.Type.normal);

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
      DrawForce();
      DrawSpeedLimit();
      EditorGUILayout.Space();
      DrawForceDeltaMax();
      DrawAngleDeltaMax();
      EditorGUILayout.Space();
      DrawPeriodTime();
      DrawPeriodSpace();
      EditorGUIUtility.wideMode = currentMode;
      
      EditorGUILayout.Space();
      EditorGUI.EndDisabledGroup();
      GUILayout.EndArea();
    }
  }
}