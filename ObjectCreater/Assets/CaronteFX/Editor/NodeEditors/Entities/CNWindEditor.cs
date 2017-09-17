using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CaronteFX
{
  public class CNWindEditor : CNEntityEditor
  {
    public static Texture icon_;
    public override Texture TexIcon { get{ return icon_; } }
 
    new CNWind Data { get; set; }
    public CNWindEditor( CNWind data, CommandNodeEditorState state )
      : base( data, state )
    {
      Data = (CNWind)data;
    }

    //----------------------------------------------------------------------------------
    public override void CreateEntitySpec()
    {
      eManager.CreateWind( Data );
    }
    //----------------------------------------------------------------------------------
    public override void ApplyEntitySpec()
    {
      GameObject[] arrGameObjectBody = FieldController.GetUnityGameObjects();

      eManager.RecreateWind( Data, arrGameObjectBody );
    }
    //----------------------------------------------------------------------------------
    public void AddGameObjectsToBodies( UnityEngine.Object[] objects, bool recalculateFields )
    {
      FieldController.AddGameObjects( objects, recalculateFields );
    }
    //----------------------------------------------------------------------------------
    private void DrawFluidDensity()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Fluid density", Data.FluidDensity );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change fluid density - " + Data.Name);
        Data.FluidDensity = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    private void DrawVelocity()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Vector3Field("Velocity", Data.Velocity );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change velocity - " + Data.Name);
        Data.Velocity = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    private void DrawSpeedDeltaMax()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Speed variation", Data.SpeedDeltaMax);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change speed variation" + Data.Name);
        Data.SpeedDeltaMax = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    private void DrawAngleDeltaMax()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Angle variation", Data.AngleDeltaMax);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change angle variation" + Data.Name);
        Data.AngleDeltaMax = value;
        EditorUtility.SetDirty(Data);
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
    private void DrawHighFrequencyAM()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Amplitude rate", Data.HighFrequency_am);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change amplitude rate" + Data.Name);
        Data.HighFrequency_am = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    private void DrawHighFrequencySP()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Speed up", Data.HighFrequency_sp);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change amplitude speed up - " + Data.Name);
        Data.HighFrequency_sp = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);

      RenderTitle(isEditable, true, false);

      EditorGUI.BeginDisabledGroup(!isEditable);

      RenderFieldObjects( "Bodies", FieldController,  true, true, CNFieldWindow.Type.extended );
      
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      CRGUIUtils.DrawSeparator();
      CRGUIUtils.Splitter();

      scroller_ = EditorGUILayout.BeginScrollView(scroller_);
      
      bool currentMode = EditorGUIUtility.wideMode;
      EditorGUIUtility.wideMode = true;

      float originalLabelWidth = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = 180f;

      EditorGUILayout.Space();
      EditorGUILayout.Space();
      DrawTimer();
      EditorGUILayout.Space();

      DrawFluidDensity();
      EditorGUILayout.Space();
      DrawVelocity();
      EditorGUILayout.Space();
 
      DrawSpeedDeltaMax();
      DrawAngleDeltaMax();

      EditorGUILayout.Space();

      CRGUIUtils.Splitter();
      EditorGUILayout.Space();
      Data.NoiseFoldout = EditorGUILayout.Foldout(Data.NoiseFoldout, "Noise");
      if (Data.NoiseFoldout)
      {
        GUILayout.Space( 10f );
 
        DrawPeriodTime();
        DrawPeriodSpace();
      }
      CRGUIUtils.Splitter();
      EditorGUILayout.Space();
      Data.HfFoldout = EditorGUILayout.Foldout(Data.HfFoldout, "High frequency");
      if (Data.HfFoldout)
      {
        GUILayout.Space( 10f );
        DrawHighFrequencyAM();
        DrawHighFrequencySP();
      }
      CRGUIUtils.Splitter();
      EditorGUIUtility.labelWidth = originalLabelWidth;
      EditorGUIUtility.wideMode = currentMode;

      EditorGUILayout.EndScrollView();

      GUILayout.EndArea();
    }
  }
}