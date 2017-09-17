using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;

namespace CaronteFX
{
  public class CNExplosionEditorState : CommandNodeEditorState
  {
    public Transform explosion_Transform_;
    public int       resolution_ ;
    public float     wave_front_speed_ ;
    public float     range_;
    public float     decay_;
    public float     momentum_;
    public float     timer_;
    public float     objects_limit_speed_;
    public bool      asymmetry_;
    public int       asymmetry_random_seed_;
    public int       asymmetry_bump_number_;
    public float     asymmetry_additional_speed_ratio_;
  }

  public class CNExplosionEditor : CNEntityEditor
  {
    public static Texture icon_;
    public override Texture TexIcon{ get{ return icon_; } }

    new CNExplosionEditorState state_;

    new CNExplosion Data { get; set; }

    public CNExplosionEditor(CNExplosion data, CNExplosionEditorState state)
      : base( data, state )
    {
      Data = (CNExplosion)data;
      state_ = state;
    }

    protected override void LoadState()
    {
      base.LoadState();

      state_.explosion_Transform_              = Data.Explosion_Transform;
      state_.resolution_                       = Data.Resolution;
      state_.wave_front_speed_                 = Data.Wave_front_speed;
      state_.range_                            = Data.Range;
      state_.decay_                            = Data.Decay;
      state_.momentum_                         = Data.Momentum;
      state_.timer_                            = Data.Timer;
      state_.objects_limit_speed_              = Data.Objects_limit_speed;
      state_.asymmetry_                        = Data.Asymmetry;
      state_.asymmetry_random_seed_            = Data.Asymmetry_random_seed;
      state_.asymmetry_bump_number_            = Data.Asymmetry_bump_number;
      state_.asymmetry_additional_speed_ratio_ = Data.Asymmetry_additional_speed_ratio;
    }

    public override void ValidateState()
    {
      base.ValidateState();

      if ( state_.explosion_Transform_              != Data.Explosion_Transform || 
           state_.resolution_                       != Data.Resolution ||
           state_.wave_front_speed_                 != Data.Wave_front_speed ||
           state_.range_                            != Data.Range ||
           state_.decay_                            != Data.Decay ||
           state_.momentum_                         != Data.Momentum ||
           state_.timer_                            != Data.Timer ||
           state_.objects_limit_speed_              != Data.Objects_limit_speed ||
           state_.asymmetry_                        != Data.Asymmetry ||
           state_.asymmetry_random_seed_            != Data.Asymmetry_random_seed ||
           state_.asymmetry_bump_number_            != Data.Asymmetry_bump_number ||
           state_.asymmetry_additional_speed_ratio_ != Data.Asymmetry_additional_speed_ratio )
      {

        if (!IsExcluded)
        {
          ApplyEntitySpec();
        }  
      }
    }

    public override void SceneSelection()
    {
      FieldController.SceneSelectionTopMost();
    }

    public override void FreeResources()
    {
      FieldController.DestroyField();
      DestroyExplosion();
    }
    
    public override void CreateEntitySpec()
    {
      eManager.CreateExplosion( Data );
      LoadState();
    }

    public override void ApplyEntitySpec()
    {
      GameObject[] arrGameObject = FieldController.GetUnityGameObjects();
      eManager.RecreateExplosion( Data, arrGameObject );
      LoadState();
    }

    public void DestroyExplosion()
    {
      eManager.DestroyEntity( Data );
    }

    private void DrawExplosionTransform()
    {
      EditorGUI.BeginChangeCheck();
      var value = (Transform) EditorGUILayout.ObjectField("Explosion transform", Data.Explosion_Transform, typeof(Transform), true );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change explosion transform - " + Data.Name);
        Data.Explosion_Transform = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawResolution()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntSlider("Resolution", Data.Resolution, 1, 3);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change resolution - " + Data.Name);
        Data.Resolution = value;
        Data.RenderStepSize = 1 + (Data.Resolution * Data.Resolution - Data.Resolution);
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawWavefronteSpeed()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Wavefront speed", Data.Wave_front_speed );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change wavefront speed - " + Data.Name);
        Data.Wave_front_speed = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRange()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Range", Data.Range );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change range - " + Data.Name);
        Data.Range = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawDecay()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Decay", Data.Decay );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change decay - " + Data.Name);
        Data.Decay = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawMomentum()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Momentum", Data.Momentum );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change momentum - " + Data.Name);
        Data.Momentum = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawObjectsLimitSpeed()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Objects Limit speed", Data.Objects_limit_speed );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change objects limit speed - " + Data.Name);
        Data.Objects_limit_speed = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawAsymmetry()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle("Asymmetry", Data.Asymmetry );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change asymmetry - " + Data.Name);
        Data.Asymmetry = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawAsymmetryRandomSeed()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField("Random seed", Data.Asymmetry_random_seed );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change random seed - " + Data.Name);
        Data.Asymmetry_random_seed = Mathf.Clamp(value, 0, int.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawAsymmetryBumpNumber()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField("Number of bumps", Data.Asymmetry_bump_number );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change number of bumps - " + Data.Name);
        Data.Asymmetry_bump_number = Mathf.Clamp(value, 0, int.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawAsymmetryAdditionalSpeedRatio()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Additional speed ratio", Data.Asymmetry_additional_speed_ratio);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change additional speed ratio - " + Data.Name);
        Data.Asymmetry_additional_speed_ratio = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }

    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);
      RenderTitle(isEditable, true, true, true);

      EditorGUI.BeginDisabledGroup( !isEditable );
      RenderFieldObjects("Bodies", FieldController, true, true, CNFieldWindow.Type.extended);
      EditorGUI.EndDisabledGroup();

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      CRGUIUtils.DrawSeparator();
      CRGUIUtils.Splitter();
    
      scroller_ = EditorGUILayout.BeginScrollView(scroller_);

      bool currentMode = EditorGUIUtility.wideMode;
      EditorGUIUtility.wideMode = true;

      EditorGUI.BeginDisabledGroup( !isEditable );
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      DrawTimer();  

      EditorGUILayout.Space();
      EditorGUI.BeginChangeCheck();
      DrawExplosionTransform();
      EditorGUILayout.Space();

      DrawResolution();
      DrawWavefronteSpeed();
      DrawRange();
      DrawDecay();
      DrawMomentum();
      
      EditorGUILayout.Space();
      DrawObjectsLimitSpeed();

      EditorGUILayout.Space();
      DrawAsymmetry();
      CRGUIUtils.Splitter();
      EditorGUILayout.Space();
      EditorGUI.BeginDisabledGroup(!Data.Asymmetry);
      DrawAsymmetryRandomSeed();
      DrawAsymmetryBumpNumber();
      DrawAsymmetryAdditionalSpeedRatio();
      EditorGUI.EndDisabledGroup();

      if (EditorGUI.EndChangeCheck())
      {
        ApplyEntity();
        EditorUtility.SetDirty(Data);
      }
      
      EditorGUI.EndDisabledGroup();

      EditorGUIUtility.wideMode = currentMode;

      EditorGUILayout.Space();
      EditorGUILayout.EndScrollView();
      GUILayout.EndArea();
    }
     
  }
}

