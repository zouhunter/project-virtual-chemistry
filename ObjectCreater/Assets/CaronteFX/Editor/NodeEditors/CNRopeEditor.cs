using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CaronteFX
{
  public class CNRopeEditorState : CNBodyEditorState
  {
    public int   sides_;
    public float stretch_;
    public float bend_;
    public float torsion_;
    public float dampingPerSecond_CM_;
  }

  public class CNRopeEditor : CNBodyEditor
  {   
    public static Texture icon_;
    public override Texture TexIcon 
    { 
      get{ return icon_; } 
    }

    new CNRopeEditorState state_;
    new CNRope Data { get; set; }
    //-----------------------------------------------------------------------------------
    public CNRopeEditor( CNRope data, CNRopeEditorState state )
      : base(data, state)
    {
      Data   = (CNRope)data;
      state_ = state;
    }
    //-----------------------------------------------------------------------------------
    protected override void LoadState()
    {
      base.LoadState();

      state_.sides_                = Data.Sides;
      state_.stretch_              = Data.Stretch;
      state_.bend_                 = Data.Bend;
      state_.torsion_              = Data.Torsion;
      state_.dampingPerSecond_CM_  = Data.DampingPerSecond_CM;
    }
    //-----------------------------------------------------------------------------------
    public override void ValidateState()
    {
      base.ValidateState();

      ValidateSides();
      ValidateStretchTorsionBend();
      ValidateDampingPerSecondCM();
    }
    //-----------------------------------------------------------------------------------
    protected override void ValidateVelocity()
    {
      if ( state_.velocityStart_ != Data.VelocityStart )
      {
        eManager.SetVelocity( Data );
        Debug.Log("Changed linear velocity");
        state_.velocityStart_ = Data.VelocityStart;
      }
    }
    //-----------------------------------------------------------------------------------
    protected override void ValidateOmega()
    {
      if ( state_.omegaStart_inDegSeg_ != Data.OmegaStart_inDegSeg )
      {
        eManager.SetVelocity( Data );
        Debug.Log("Changed angular velocity");
        state_.omegaStart_inDegSeg_ = Data.OmegaStart_inDegSeg;
      }
    }
    //-----------------------------------------------------------------------------------
    private void ValidateSides()
    {
      if ( state_.sides_ != Data.Sides )
      {
        RecreateBodies();
        Debug.Log("Changed sides");  
        state_.sides_ = Data.Sides;
      }
    }
    //-----------------------------------------------------------------------------------
    private void  ValidateStretchTorsionBend()
    {
      if ( state_.stretch_ != Data.Stretch ||
           state_.bend_    != Data.Bend    || 
           state_.torsion_ != Data.Torsion   )
      {
        eManager.SetStretchTorsionBend( Data );
        Debug.Log("Changed Stretch, Torsion, Bend");

        state_.stretch_ = Data.Stretch;
        state_.bend_    = Data.Bend;
        state_.torsion_ = Data.Torsion;
      }
    }

    //-----------------------------------------------------------------------------------
    private void ValidateDampingPerSecondCM()
    {
      if (state_.dampingPerSecond_CM_ != Data.DampingPerSecond_CM )
      { 
        eManager.SetInternalDamping( Data );
        Debug.Log("Changed internal damping");

        state_.dampingPerSecond_CM_ = Data.DampingPerSecond_CM;
      }
    }
    //-----------------------------------------------------------------------------------
    public override void CreateBodies(GameObject[] arrGameObject)
    {
      CreateBodies(arrGameObject, "Caronte FX - Rope creation", "Creating " + Data.Name + " ropes. ");
    }
    //-----------------------------------------------------------------------------------
    public override void DestroyBodies(GameObject[] arrGameObject)
    {
      DestroyBodies(arrGameObject, "CaronteFX - Rope destruction", "Destroying " + Data.Name + "ropes. ");
    }
    //-----------------------------------------------------------------------------------
    public override void DestroyBodies(int[] arrInstanceId)
    {
      DestroyBodies(arrInstanceId, "Caronte FX - Rope destruction", "Destroying " + Data.Name + " ropes. ");
    }
    //-----------------------------------------------------------------------------------
    protected override void ActionCreateBody( GameObject go )
    {
      eManager.CreateBody(Data, go);
    }
    //-----------------------------------------------------------------------------------
    protected override void ActionDestroyBody( GameObject go )
    {
      eManager.DestroyBody(Data, go);
    }
    //-----------------------------------------------------------------------------------
    protected override void ActionDestroyBody( int instanceId )
    {
      eManager.DestroyBody(Data, instanceId);
    }
    //-----------------------------------------------------------------------------------
    protected override void ActionCheckBodyForChanges( GameObject go, bool recreateIfInvalid )
    {
	    eManager.CheckBodyForChanges(Data, go, recreateIfInvalid);
    }
    //-----------------------------------------------------------------------------------
    protected override void DrawLinearVelocity()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Vector3Field( new GUIContent("Initial linear velocity"), Data.VelocityStart );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change linear velocity - " + Data.Name);
        Data.VelocityStart = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected override void DrawAngularVelocity()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Vector3Field( new GUIContent("Initial angular velocity"), Data.OmegaStart_inDegSeg );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change angular velocity - " + Data.Name);
        Data.OmegaStart_inDegSeg = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawDoAutocollide()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle( new GUIContent("Auto collide"), Data.AutoCollide );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change auto collide - " + Data.Name);
        Data.AutoCollide = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawSides()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField( new GUIContent("Sides"), Data.Sides );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change resolution - " + Data.Name);
        Data.Sides = Mathf.Clamp(value, 4, 32);
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawStretch()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( new GUIContent("Stretch"), Data.Stretch );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change stretch - " + Data.Name);
        Data.Stretch = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawBend()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( new GUIContent("Bend"), Data.Bend );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change bend - " + Data.Name);
        Data.Bend = Mathf.Clamp(value, 0f, 5f);
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawTorsion()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( new GUIContent("Torsion"), Data.Torsion );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change torsion - " + Data.Name);
        Data.Torsion = Mathf.Clamp(value, 0f, 5f);
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawDampingPerSecondCM()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( new GUIContent("Internal damping"), Data.DampingPerSecond_CM);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change internal damping - " + Data.Name);
        Data.DampingPerSecond_CM = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected override void RenderFieldsBody(bool isEditable)
    {
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      CRGUIUtils.DrawSeparator();
      CRGUIUtils.Splitter();

      scroller_ = EditorGUILayout.BeginScrollView(scroller_);
      
      EditorGUILayout.Space();
      float originalLabelWidth    = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = 180f;

      EditorGUI.BeginDisabledGroup(!isEditable);
      {
        DrawDoCollide();
        DrawDoAutocollide();
        EditorGUILayout.Space();

        DrawGUIMassOptions();

        GUILayout.Space(simple_space);
        DrawSides();
        GUILayout.Space(simple_space);

        DrawStretch();
        DrawBend();
        DrawTorsion();

        GUILayout.Space(simple_space);
        DrawDampingPerSecondCM();
        GUILayout.Space(simple_space);
        CRGUIUtils.Splitter();
        GUILayout.Space(simple_space);

        DrawRestitution();
        DrawFrictionKinetic();
     
        GUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(Data.FromKinetic);
        DrawFrictionStatic();
        EditorGUI.EndDisabledGroup();
        DrawFrictionStaticFromKinetic();
        GUILayout.EndHorizontal();

        GUILayout.Space(simple_space);
        DrawDampingPerSecondWorld();
        GUILayout.Space(simple_space);

        bool currentMode = EditorGUIUtility.wideMode;
        EditorGUIUtility.wideMode = true;
        DrawLinearVelocity();
        DrawAngularVelocity();
        EditorGUIUtility.wideMode = currentMode;
      }
      EditorGUI.EndDisabledGroup();

      EditorGUILayout.Space();
      CRGUIUtils.Splitter();
      EditorGUILayout.Space();

      EditorGUI.BeginDisabledGroup(!isEditable);
      {
        DrawExplosionOpacity();
        DrawExplosionResponsiveness();
      }
      EditorGUI.EndDisabledGroup();

      EditorGUIUtility.labelWidth = originalLabelWidth;

      EditorGUILayout.Space();     
      EditorGUILayout.EndScrollView();
    }
    //-----------------------------------------------------------------------------------
  }
}
