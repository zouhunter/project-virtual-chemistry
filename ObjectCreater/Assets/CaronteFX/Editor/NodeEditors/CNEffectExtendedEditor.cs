using UnityEngine;
using UnityEditor;
using System.Collections;


namespace CaronteFX
{
  public class CNEffectExtendedEditor : CNEffectEditor
  {  
    protected enum tabType
    {
      General = 0,
      Advanced = 1,
    }

    private string[] tabNames = new string[] { "General", "Advanced" };
    private int tabIndex = 0;

    public CNEffectExtendedEditor( CNGroup data, CommandNodeEditorState state )
      : base( data, state )
    {

    }

    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);
      
      RenderTitle(isEditable);

      EditorGUILayout.Space();
      float originalLabelwidth = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = 100f;

      EditorGUI.BeginDisabledGroup( !isEditable );
      EditorGUI.BeginChangeCheck();
      selectedScopeIdx_ = EditorGUILayout.Popup("Effect Scope", selectedScopeIdx_, scopeStrings);
      if (EditorGUI.EndChangeCheck())
      {
        ChangeScope( (CNGroup.CARONTEFX_SCOPE)selectedScopeIdx_ );
      }

      EditorGUI.EndDisabledGroup();
      EditorGUILayout.Space();
      EditorGUILayout.Space();
      if ( GUILayout.Button(new GUIContent("Select scope GameObjects"), GUILayout.Height(30f)) )
      {
        SceneSelection();
      }
      EditorGUIUtility.labelWidth = originalLabelwidth;

      CRGUIUtils.DrawSeparator();
      CRGUIUtils.Splitter();
      DrawEffectGUIWindow(isEditable);

      GUILayout.EndArea();
    }

    private void DrawTotalTime()
    {  
      GUILayout.Space(width_indent);
      EditorGUILayout.LabelField("Total simulation time (secs)");
      EditorGUILayout.EndHorizontal();
      EditorGUILayout.BeginHorizontal();
      GUILayout.Space(width_indent);

      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(effectData_.totalTime_);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(fxData_, "Change time - " + Data.Name);
        effectData_.totalTime_ = Mathf.Clamp( value, 0f, 3600f);
        EditorUtility.SetDirty( fxData_ );
      }
    }

    private void DrawTotalTimeReset()
    {
      if ( GUILayout.Button( "reset", EditorStyles.miniButton, GUILayout.Width(50f) ) )
      {
        Undo.RecordObject(fxData_, "Reset time - " + Data.Name);
        effectData_.totalTime_ = 10f;
        EditorUtility.SetDirty( fxData_ );
      }
    }

    private void DrawQuality()
    {
      EditorGUI.BeginDisabledGroup(effectData_.byUserDeltaTime_);
      EditorGUILayout.Space();
      EditorGUILayout.Space();
      EditorGUILayout.BeginHorizontal();
      GUILayout.Space(width_indent);
      EditorGUILayout.LabelField("Quality", GUILayout.Width(80f));
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider(effectData_.quality_, 1f, 100f);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(fxData_, "Change quality - " + Data.Name);
        effectData_.quality_ = value;
        EditorUtility.SetDirty( fxData_ );
      }
      EditorGUILayout.EndHorizontal();
      EditorGUI.EndDisabledGroup();
    }

    private void DrawAntiJittering()
    {
      EditorGUILayout.BeginHorizontal();
      GUILayout.Space(width_indent);
      EditorGUILayout.LabelField("Anti-Jittering", GUILayout.Width(80f));
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider(effectData_.antiJittering_, 1, 100);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(fxData_, "Change antijittering - " + Data.Name);
        effectData_.antiJittering_ = value;
        EditorUtility.SetDirty( fxData_ );
      }
      EditorGUILayout.EndHorizontal();
    }

    private void DrawFrameRate()
    {
      EditorGUILayout.BeginHorizontal();
      GUILayout.Space(width_indent);
      EditorGUILayout.LabelField("Frames per second (fps)");
      EditorGUILayout.EndHorizontal();

      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField(effectData_.frameRate_);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(fxData_, "Change frame rate - " + Data.Name);
        effectData_.frameRate_ = Mathf.Clamp(value, 0, 2000);
        EditorUtility.SetDirty( fxData_ );
      }

      EditorGUILayout.BeginHorizontal();
      GUILayout.Space(width_indent);
      EditorGUILayout.EndHorizontal();
    }

    private void DrawByUserDetalTime()
    {
      EditorGUILayout.BeginHorizontal();
      GUILayout.Space(width_indent);
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.ToggleLeft( "User defined delta time", effectData_.byUserDeltaTime_ );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(fxData_, "Change user defined delta time - " + Data.Name);
        effectData_.byUserDeltaTime_ = value;
        if (value)
        {
          effectData_.byUserCharacteristicObjectProperties_ = false;
        }
        EditorUtility.SetDirty( fxData_ );
      }
      EditorGUILayout.EndHorizontal();
    }

    private void DrawDeltaTime()
    {
      EditorGUI.BeginDisabledGroup( !effectData_.byUserDeltaTime_ );
      EditorGUILayout.BeginHorizontal();
      GUILayout.Space(width_indent);
      EditorGUILayout.LabelField( "Delta Time (secs)" );
      EditorGUI.EndDisabledGroup();
      GUILayout.Space(20f);

      EditorGUILayout.LabelField("Last delta time");
      EditorGUILayout.EndHorizontal();
      EditorGUI.BeginDisabledGroup(!effectData_.byUserDeltaTime_);
      EditorGUILayout.BeginHorizontal();
      GUILayout.Space(width_indent);
      EditorGUI.BeginChangeCheck();
      var deltaTime = CRGUIExtension.FloatTextField(effectData_.deltaTime_, 0.000001f, 10f, -1f, "undefined");
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(fxData_, "Change delta time - " + Data.Name);
        effectData_.deltaTime_ = deltaTime;
        EditorUtility.SetDirty( fxData_ );
      }
      EditorGUI.EndDisabledGroup();
      GUILayout.Space(20f);
      EditorGUILayout.LabelField( (effectData_.calculatedDeltaTime_ < 0f) ? "not calculated yet" : effectData_.calculatedDeltaTime_.ToString(), EditorStyles.textField);
      EditorGUILayout.EndHorizontal();
    }

    private void DrawByUserCharacteristicObjectProperties()
    {
      EditorGUILayout.BeginHorizontal();
      GUILayout.Space(width_indent);

      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.ToggleLeft("User defined characteristic distances", effectData_.byUserCharacteristicObjectProperties_);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(fxData_, "Change by user properties - " + Data.Name);
        effectData_.byUserCharacteristicObjectProperties_ = value;
        if (value)
        {
          effectData_.byUserDeltaTime_ = false;
        }
      }
      EditorGUILayout.EndHorizontal();
    }

    private void DrawThickness()
    {
      EditorGUI.BeginDisabledGroup(!effectData_.byUserCharacteristicObjectProperties_);
      EditorGUILayout.BeginHorizontal();
      GUILayout.Space(width_indent);
      EditorGUILayout.LabelField("Thickness");
      EditorGUI.EndDisabledGroup();
      GUILayout.Space(20f);
      EditorGUILayout.LabelField("Last thickness");
      EditorGUILayout.EndHorizontal();
      EditorGUI.BeginDisabledGroup(!effectData_.byUserCharacteristicObjectProperties_);
      EditorGUILayout.BeginHorizontal();
      GUILayout.Space(width_indent);
      EditorGUI.BeginChangeCheck();
      var thickness = CRGUIExtension.FloatTextField(effectData_.thickness_, 0f, 10f, -1f, "undefined");
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject( fxData_, "Change thickness - " + Data.Name);
        effectData_.thickness_ = thickness;
      }
      EditorGUI.EndDisabledGroup();
      GUILayout.Space(20f);
      EditorGUILayout.LabelField((effectData_.calculatedThickness_ < 0f) ? "not calculated yet" : effectData_.calculatedThickness_.ToString(), EditorStyles.textField);
      EditorGUILayout.EndHorizontal();
    }

    private void DrawLength()
    { 
      EditorGUI.BeginDisabledGroup(!effectData_.byUserCharacteristicObjectProperties_);
      EditorGUILayout.BeginHorizontal();
      GUILayout.Space(width_indent);
      EditorGUILayout.LabelField("Length");
      EditorGUI.EndDisabledGroup();
      GUILayout.Space(20f);
      EditorGUILayout.LabelField("Last Length");
      EditorGUILayout.EndHorizontal();

      EditorGUI.BeginDisabledGroup(!effectData_.byUserCharacteristicObjectProperties_);
      EditorGUILayout.BeginHorizontal();
      GUILayout.Space(width_indent);
      EditorGUI.BeginChangeCheck();
      var length = CRGUIExtension.FloatTextField(effectData_.length_, 0f, 10f, -1f, "undefined");
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject( fxData_, "Change length - " + Data.Name);
        effectData_.length_ = length;
      }
      EditorGUI.EndDisabledGroup();
      GUILayout.Space(20f);
      EditorGUILayout.LabelField((effectData_.calculatedLength_ < 0f) ? "not calculated yet" : effectData_.calculatedLength_.ToString(), EditorStyles.textField);
      EditorGUILayout.EndHorizontal();
    }

    private void DrawEffectGUIWindow(bool isEditable)
    {
      tabIndex = GUILayout.SelectionGrid( tabIndex, tabNames, 2 );
      CRGUIUtils.Splitter();

      scroller_ = EditorGUILayout.BeginScrollView( scroller_ );
      
      EditorGUI.BeginDisabledGroup( !isEditable );
     
      EditorGUILayout.Space();

      #region General
      if (tabIndex == (int)tabType.General)
      {
        //TIME
        EditorGUILayout.BeginHorizontal();
        DrawTotalTime();
        DrawTotalTimeReset();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        //QUALITY
        DrawQuality();

        GUILayout.Space(20f);
        //ANTI-JITTERING
        DrawAntiJittering();

        GUILayout.Space(20f);
        //FRAMERATE
        DrawFrameRate();
      }
      #endregion

      #region Advanced
      else if (tabIndex == (int)tabType.Advanced)
      {
        EditorGUILayout.Space();
        DrawByUserDetalTime();
        GUILayout.Space(10f);
        DrawDeltaTime();
        GUILayout.Space(20f);
        DrawByUserCharacteristicObjectProperties();
        GUILayout.Space(10f);
        DrawThickness();
        DrawLength();
      }

      EditorGUILayout.Space();
      #endregion

      EditorGUILayout.Space();

      EditorGUI.EndDisabledGroup();

      EditorGUILayout.EndScrollView();
    }


  }
}

