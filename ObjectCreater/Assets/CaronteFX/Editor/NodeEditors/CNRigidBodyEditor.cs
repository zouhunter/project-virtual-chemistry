using UnityEngine;
using UnityEditor;
using System;
using System.Collections;


namespace CaronteFX
{
  public class CNRigidbodyEditor : CNBodyEditor
  {
    public static Texture icon_responsive_;
    public static Texture icon_irresponsive_;

    public override Texture TexIcon 
    { 
      get
      { 
        if (Data.IsFiniteMass)
        {
          return icon_responsive_;
        }
        else
        {
         return icon_irresponsive_; 
        }
      }
    }

    new CNRigidbody Data { get; set; }

    //-----------------------------------------------------------------------------------
    public CNRigidbodyEditor( CNRigidbody data, CNBodyEditorState state )
      : base( data, state )
    {
      Data = (CNRigidbody)data;
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
    public void SetResponsiveness( bool responsive )
    {
      if (Data.IsFiniteMass != responsive )
      {
        Data.IsFiniteMass = responsive;

        if (!Data.IsNodeExcludedInHierarchy)
        {      
          GameObject[] arrGameObject = FieldController.GetUnityGameObjects();
          eManager.SetResponsiveness( Data, arrGameObject );   
        }

        EditorUtility.SetDirty( Data );
      }
    }
    //-----------------------------------------------------------------------------------
    public override void CreateBodies( GameObject[] arrGameObject )
    {
      CreateBodies(arrGameObject, "Caronte FX - Rigidbody creation", "Creating " + Data.Name + " rigidbodies. ");
    }
    //-----------------------------------------------------------------------------------
    public override void DestroyBodies(GameObject[] arrGameObject)
    {
      DestroyBodies(arrGameObject, "Caronte FX - Rigidbody destruction", "Destroying " + Data.Name + " rigidbodies. ");
    }
    //-----------------------------------------------------------------------------------
    public override void DestroyBodies(int[] arrInstanceId)
    {
      DestroyBodies(arrInstanceId, "Caronte FX - Rigidbody destruction", "Destroying " + Data.Name + " rigidbodies. ");
    }
    //-----------------------------------------------------------------------------------
    protected override void ActionCreateBody( GameObject go )
    {
      eManager.CreateBody(Data, go );
    }
    //-----------------------------------------------------------------------------------
    protected override void ActionDestroyBody( GameObject go)
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
      var value = EditorGUILayout.Vector3Field( Data.IsFiniteMass ? new GUIContent("Initial linear velocity") : new GUIContent("Linear velocity"), Data.VelocityStart );
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
      var value = EditorGUILayout.Vector3Field( Data.IsFiniteMass ? new GUIContent("Initial angular velocity") : new GUIContent("Angular velocity"), Data.OmegaStart_inDegSeg );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change angular velocity - " + Data.Name);
        Data.OmegaStart_inDegSeg = value;
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

      float originalLabelWidth = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = label_width;

      scroller_ = EditorGUILayout.BeginScrollView(scroller_);
      EditorGUI.BeginDisabledGroup(!isEditable);
      EditorGUILayout.Space();
      DrawDoCollide();
      EditorGUILayout.Space();

      EditorGUI.BeginDisabledGroup( !Data.IsFiniteMass );
      DrawGUIMassOptions();
      EditorGUI.EndDisabledGroup();
    
      GUILayout.Space(simple_space);

      DrawRestitution();
      DrawFrictionKinetic();

      GUILayout.BeginHorizontal();
      EditorGUI.BeginDisabledGroup(Data.FromKinetic);
      DrawFrictionStatic();
      EditorGUI.EndDisabledGroup();
      DrawFrictionStaticFromKinetic();    
      GUILayout.EndHorizontal();

      EditorGUI.BeginDisabledGroup( !Data.IsFiniteMass );
      
      GUILayout.Space(simple_space);
      DrawDampingPerSecondWorld();
      GUILayout.Space(simple_space);

      bool currentMode = EditorGUIUtility.wideMode;
      EditorGUIUtility.wideMode = true;

      EditorGUI.EndDisabledGroup();

      DrawLinearVelocity();
      DrawAngularVelocity();

      EditorGUILayout.Space();
      CRGUIUtils.Splitter();
      EditorGUILayout.Space();

      DrawExplosionOpacity();

      EditorGUI.BeginDisabledGroup(!Data.IsFiniteMass);
      DrawExplosionResponsiveness();
      EditorGUI.EndDisabledGroup();

      EditorGUIUtility.labelWidth = originalLabelWidth;
      EditorGUIUtility.wideMode = currentMode;
     
      EditorGUILayout.Space();
      EditorGUI.EndDisabledGroup();
       
      EditorGUILayout.EndScrollView();
    }
    //-----------------------------------------------------------------------------------

  }

}
