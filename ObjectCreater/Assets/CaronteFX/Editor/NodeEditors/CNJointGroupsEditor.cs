using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CaronteSharp;


namespace CaronteFX
{
  public class CNJointGroupsEditor : CommandNodeEditor
  {
    public static Texture icon_area_;
    public static Texture icon_vertices_;
    public static Texture icon_leaves_;
    public static Texture icon_locators_;

    public override Texture TexIcon
    {
      get
      { 
        switch (Data.CreationMode)
        {
        case CNJointGroups.CreationModeEnum.ByContact:
          return icon_area_;

        case CNJointGroups.CreationModeEnum.ByMatchingVertices:
          return icon_vertices_;

        case CNJointGroups.CreationModeEnum.ByStem:
          return icon_leaves_;
        
        default:
          return icon_locators_;
        }
      } 
    }

    protected CNFieldController FieldControllerA { get; set; }
    protected CNFieldController FieldControllerB { get; set; }
    protected CNFieldController FieldControllerC { get; set; }

    public enum LocatorsModeEnum
    {   
      Positions, 
      Vertexes,  
      BoxCenters,
      None
    }

    LocatorsModeEnum locatorsMode_;
    string[] locatorsModeNames_ = new string[] { "At locators positions", "At locators vertexes", "At locators box centers" };

    public LocatorsModeEnum LocatorsMode
    {
      set
      {
        locatorsMode_ = value;

        switch (locatorsMode_)
        {
          case LocatorsModeEnum.Positions:
            Data.CreationMode = CNJointGroups.CreationModeEnum.AtLocatorsPositions;
          break;

          case LocatorsModeEnum.Vertexes:
            Data.CreationMode = CNJointGroups.CreationModeEnum.AtLocatorsVertexes;
          break;

          case LocatorsModeEnum.BoxCenters:
            Data.CreationMode = CNJointGroups.CreationModeEnum.AtLocatorsBBoxCenters;
          break;
        }
      }
    }

    public bool MaximumForce
    {
      get
      {
        return (Data.ForceMaxMode == CNJointGroups.ForceMaxModeEnum.Unlimited);
      }

      set 
      { 
        if (value)
        {
          Data.ForceMaxMode = CNJointGroups.ForceMaxModeEnum.Unlimited;
        }
        else
        {
          Data.ForceMaxMode = CNJointGroups.ForceMaxModeEnum.ConstantLimit;
        }         
      }
    }

    protected new CNJointGroups Data { get; set; }

    //-----------------------------------------------------------------------------------
    public CNJointGroupsEditor( CNJointGroups data, CommandNodeEditorState state )
      : base( data, state )
    {
      Data = (CNJointGroups)data;
    }
    //-----------------------------------------------------------------------------------
    public override void Init()
    {
      base.Init();

      CNField.AllowedTypes allowedTypes =   CNField.AllowedTypes.Geometry 
                                          | CNField.AllowedTypes.BodyNode;

      FieldControllerA = new CNFieldController( Data, Data.ObjectsA, eManager, goManager );
      FieldControllerA.SetFieldType( allowedTypes );
      FieldControllerA.SetCalculatesDiff(true);
      FieldControllerA.IsBodyField = true;

      FieldControllerB = new CNFieldController( Data, Data.ObjectsB, eManager, goManager );
      FieldControllerB.SetFieldType( allowedTypes );
      FieldControllerB.SetCalculatesDiff(true);
      FieldControllerB.IsBodyField = true;

      FieldControllerC = new CNFieldController( Data, Data.LocatorsC, eManager, goManager );
      FieldControllerC.SetFieldType( CNField.AllowedTypes.Locator | CNField.AllowedTypes.Geometry );
      FieldControllerC.SetCalculatesDiff(true);
    }
    //-----------------------------------------------------------------------------------
    protected void SetLocatorsOption()
    {
      switch (Data.CreationMode)
      {
        case CNJointGroups.CreationModeEnum.AtLocatorsPositions:
          locatorsMode_ = LocatorsModeEnum.Positions;
          break;

        case CNJointGroups.CreationModeEnum.AtLocatorsVertexes:
          locatorsMode_ = LocatorsModeEnum.Vertexes;
          break;

        case CNJointGroups.CreationModeEnum.AtLocatorsBBoxCenters:
          locatorsMode_ = LocatorsModeEnum.BoxCenters;
          break;

        default:
          locatorsMode_ = LocatorsModeEnum.None;
          break;
      }
    }
    //-----------------------------------------------------------------------------------
    public void CreateEntities()
    {
      if ( !Data.IsNodeExcludedInHierarchy ) 
      {
        GameObject[] arrGameObjectA;
        GameObject[] arrGameObjectB;
        Vector3[]    arrLocatorsC;

        GetFieldGameObjects( FieldControllerA, out arrGameObjectA );
        GetFieldGameObjects( FieldControllerB, out arrGameObjectB );

        bool fieldAIsReallyEmpty = FieldControllerA.HasNoReferences();
        bool fieldBIsReallyEmpty = FieldControllerB.HasNoReferences();

        GetFieldLocators( FieldControllerC, out arrLocatorsC );

        eManager.CreateMultiJoint( Data, arrGameObjectA, arrGameObjectB, arrLocatorsC, fieldAIsReallyEmpty, fieldBIsReallyEmpty );
        cnManager.SceneSelection();

        LoadState();
      }
    }
    //-----------------------------------------------------------------------------------
    public virtual void DestroyEntities()
    {
      GameObject[] arrGameObjectA;
      GameObject[] arrGameObjectB;

      GetFieldGameObjects( FieldControllerA, out arrGameObjectA );
      GetFieldGameObjects( FieldControllerB, out arrGameObjectB );

      eManager.DestroyMultiJoint( Data, arrGameObjectA, arrGameObjectB );
    }
    //-----------------------------------------------------------------------------------
    public virtual void RecreateEntities()
    {
      DestroyEntities();
      CreateEntities();
    }
    //-----------------------------------------------------------------------------------
    public void EditEntites()
    {
      eManager.EditMultiJoint( Data );
    }
    //-----------------------------------------------------------------------------------
    protected void GetFieldGameObjects( CNFieldController fieldController, out GameObject[] arrGameObject )
    {
      arrGameObject = fieldController.GetUnityGameObjects();
    }
    //-----------------------------------------------------------------------------------
    private void GetFieldLocators( CNFieldController fieldController, out Vector3[] arrLocations )
    {
      GameObject[] gameObjects = fieldController.GetUnityGameObjects();

      List<Vector3> listLocatorPosition = new List<Vector3>();
      switch ( Data.CreationMode )
      {
        case CNJointGroups.CreationModeEnum.AtLocatorsPositions:
          for (int i = 0; i < gameObjects.Length; i++)
          {
            Transform tr = gameObjects[i].transform;
            if ( tr.childCount == 0 )
            {
              listLocatorPosition.Add(tr.position);
            }
          }
          break;

        case CNJointGroups.CreationModeEnum.AtLocatorsBBoxCenters:
          for (int i = 0; i < gameObjects.Length; ++i)
          {
            Renderer renderer = gameObjects[i].GetComponent<Renderer>();
            if ( renderer != null )
            {
              Bounds bbox = renderer.bounds;
              listLocatorPosition.Add(bbox.center);
            }
          }
          break;

        case CNJointGroups.CreationModeEnum.AtLocatorsVertexes:
          for (int i = 0; i < gameObjects.Length; ++i)
          {
            GameObject go = gameObjects[i];
            MeshFilter meshFilter = go.GetComponent<MeshFilter>();

            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
              UnityEngine.Mesh mesh = meshFilter.sharedMesh;
              UnityEngine.Mesh meshTransformed;

              CRGeometryUtils.CreateMeshTransformed( mesh, go.transform.localToWorldMatrix, out meshTransformed );
              Vector3[] meshVertices = meshTransformed.vertices;
              for (int j = 0; j < meshVertices.Length; ++j)
              {
                listLocatorPosition.Add(meshVertices[j]);
              }   

              UnityEngine.Object.DestroyImmediate( meshTransformed );
            }
          }
          break;

        default:
          break;
      }

      arrLocations = listLocatorPosition.ToArray();
    }
    //-----------------------------------------------------------------------------------
    public override void SetActivityState()
    {
      base.SetActivityState();
      eManager.SetActivity( Data );
    }
    //-----------------------------------------------------------------------------------
    public override void SetVisibilityState()
    {
      base.SetVisibilityState();
      eManager.SetVisibility( Data );
    }
    //-----------------------------------------------------------------------------------
    public override void SetExcludedState()
    {
      base.SetExcludedState();
      if (IsExcluded)
      {
        DestroyEntities();
      }
      else
      {
        CreateEntities();
      }
      EditorUtility.ClearProgressBar();
    }
    //-----------------------------------------------------------------------------------
    public void AddGameObjectsToA( UnityEngine.Object[] draggedObjects, bool recalculateFields )
    {
      FieldControllerA.AddGameObjects( draggedObjects, recalculateFields );
    }
    //-----------------------------------------------------------------------------------
    public void AddGameObjectsToB( UnityEngine.Object[] draggedObjects, bool recalculateFields )
    {
      FieldControllerB.AddGameObjects( draggedObjects, recalculateFields );
    }
    //-----------------------------------------------------------------------------------
    public void AddGameObjectsToC( UnityEngine.Object[] draggedObjects, bool recalculateFields )
    {
      FieldControllerC.AddGameObjects( draggedObjects, recalculateFields );
    }
    //-----------------------------------------------------------------------------------  
    public bool IsLocatorsModeActive()
    {
      return ( Data.CreationMode == CNJointGroups.CreationModeEnum.AtLocatorsBBoxCenters ||
               Data.CreationMode == CNJointGroups.CreationModeEnum.AtLocatorsPositions ||
               Data.CreationMode == CNJointGroups.CreationModeEnum.AtLocatorsVertexes );
    }
    //-----------------------------------------------------------------------------------
    public override void FreeResources()
    {
      FieldControllerA.DestroyField();
      FieldControllerB.DestroyField();
      FieldControllerC.DestroyField();
      DestroyEntities();
    }
    //-----------------------------------------------------------------------------------
    public override void LoadInfo()
    {
      FieldControllerA.RestoreFieldInfo();
      FieldControllerB.RestoreFieldInfo();
      FieldControllerC.RestoreFieldInfo();
    }
    //-----------------------------------------------------------------------------------
    public override void StoreInfo()
    {
      FieldControllerA.StoreFieldInfo();
      FieldControllerB.StoreFieldInfo();
      FieldControllerC.StoreFieldInfo();
    }
    //-----------------------------------------------------------------------------------
    public override void BuildListItems()
    {
      FieldControllerA.BuildListItems();
      FieldControllerB.BuildListItems();
      FieldControllerC.BuildListItems();
    }
    //-----------------------------------------------------------------------------------
    public override void SetScopeId(uint scopeId)
    {
      FieldControllerA.SetScopeId( scopeId );
      FieldControllerB.SetScopeId( scopeId );
      FieldControllerC.SetScopeId( scopeId );
    }
    //-----------------------------------------------------------------------------------
    public override bool RemoveNodeFromFields(CommandNode node)
    {
      Undo.RecordObject(Data, "CaronteFX - Remove node from fields");

      bool removedNodeA = Data.ObjectsA.RemoveNode(node);
      bool removedNodeB = Data.ObjectsB.RemoveNode(node);
      bool removedNodeC = Data.LocatorsC.RemoveNode(node);

      bool removed = removedNodeA || removedNodeB || removedNodeC;
      if (removed)
      {
        Data.NeedsUpdate = true;
      }

      return removed;
    }
    //-----------------------------------------------------------------------------------
    public void CheckUpdate()
    {
      bool creationModeWithLocators = ( Data.CreationMode == CNJointGroups.CreationModeEnum.AtLocatorsBBoxCenters ||
                                        Data.CreationMode == CNJointGroups.CreationModeEnum.AtLocatorsPositions ||
                                        Data.CreationMode == CNJointGroups.CreationModeEnum.AtLocatorsVertexes);

      bool updateNeeded = Data.NeedsUpdate || (   FieldControllerA.IsUpdateNeeded() ||
                                                  FieldControllerB.IsUpdateNeeded() ||
                                                 ( FieldControllerC.IsUpdateNeeded() && creationModeWithLocators) );

      if (updateNeeded)
      {
        DestroyEntities();
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawForceMax()
    {
      string name;
      if (Data.CreationMode == CNJointGroups.CreationModeEnum.ByContact)
      {
        name = "Max force (N/m2)";
      }
      else
      {
        name = "Max force (N)";
      }
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( name, Data.ForceMax );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + name + " - " + Data.Name);
        Data.ForceMax = Mathf.Clamp(value, 0, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawMaximumForce()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.ToggleLeft("Maximum", MaximumForce, GUILayout.Width(80f) );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change maximum force - " + Data.Name);
        MaximumForce = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawForceMaxRand()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider( "Max force rand.", Data.ForceMaxRand, 0.0f, 1.0f); 
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change max force rand. - " + Data.Name);
        Data.ForceMaxRand = Mathf.Clamp(value, 0, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawForceRange()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Force range (m)", Data.ForceRange );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change force range - " + Data.Name);
        Data.ForceRange = Mathf.Clamp(value, 0, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawForceProfile()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.CurveField("Force profile", Data.ForceProfile, Color.green, new Rect( 0f, 0f, 1f, 1f) );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change force profile - " + Data.Name);
        Data.ForceProfile = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawBreakIfDistanceExceeded()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle( Data.BreakIfDistExcedeed );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change break if distance exceeded - " + Data.Name);
        Data.BreakIfDistExcedeed = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawDistanceForBreak()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Break distance (m)", Data.DistanceForBreak);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change break distance - " + Data.Name);
        Data.DistanceForBreak = Mathf.Clamp( value, 0f, float.MaxValue );
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawDistanceForBreakRand()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider("Break distance random", Data.DistanceForBreakRand, 0.0f, 1.0f);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change break distance random - " + Data.Name);
        Data.DistanceForBreakRand = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawBreakAllIfLeftFewUnbroken()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle("Break all in pair if few unbroken", Data.BreakAllIfLeftFewUnbroken);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change break all in pair if few unbroken - " + Data.Name);
        Data.BreakAllIfLeftFewUnbroken = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawUnbrokenNumberForBreakAll()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField("Unbroken number to break all", Data.UnbrokenNumberForBreakAll);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change unbroken number to break all - " + Data.Name);
        Data.UnbrokenNumberForBreakAll = Mathf.Clamp(value, 0, int.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawBreakIfHinge()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle("Break if hinge", Data.BreakIfHinge);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change break if hinge - " + Data.Name);
        Data.BreakIfHinge = value;
        EditorUtility.SetDirty(Data);
      }
    }    
    //-----------------------------------------------------------------------------------
    protected void DrawEnableCollisionIfBreak()
    {
     EditorGUI.BeginChangeCheck(); 
     var value = EditorGUILayout.Toggle("Enable collisions if break", Data.EnableCollisionIfBreak);
     if (EditorGUI.EndChangeCheck())
     {
       Undo.RecordObject(Data, "Change enable collisions if break - " + Data.Name);
       Data.EnableCollisionIfBreak = value;
       EditorUtility.SetDirty(Data);
     }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawPlasticity()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle( Data.Plasticity);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change plasticity - " + Data.Name);
        Data.Plasticity = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawDistanceForPlasticity()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Plasticity distance (m)", Data.DistanceForPlasticity);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change plasticity distance - " + Data.Name);
        Data.DistanceForPlasticity = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawDistanceForPlasticityRand()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider("Plasticity distance random", Data.DistanceForPlasticityRand, 0f, 1f);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change plasticity distance rangom - " + Data.Name);
        Data.DistanceForPlasticityRand = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawPlasticityRateAcquired()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Plasticity acquired", Data.PlasticityRateAcquired);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change plasticity acquired - " + Data.Name);
        Data.PlasticityRateAcquired = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    public override void RenderGUI( Rect area, bool isEditable )
    {
      GUILayout.BeginArea( area );
      
      RenderTitle(isEditable, true, true, true);
        
      EditorGUI.BeginDisabledGroup(!isEditable);
      if (Data.CreationMode != CNJointGroups.CreationModeEnum.ByStem)
      {
        RenderFieldObjects( "ObjectsA", FieldControllerA, true, true, CNFieldWindow.Type.extended );
        RenderFieldObjects( "ObjectsB", FieldControllerB, true, true, CNFieldWindow.Type.extended );
      }
      else
      {
        RenderFieldObjects( "Leaves", FieldControllerA, true, true, CNFieldWindow.Type.extended );
        RenderFieldObjects( "Trunk", FieldControllerB, true, true, CNFieldWindow.Type.extended );
      }
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      if (GUILayout.Button(Data.NeedsUpdate ? "Create/Recreate(*)" : "Create/Recreate", GUILayout.Height(30f)))
      {
        RecreateEntities();
      }
      EditorGUI.EndDisabledGroup();

      CRGUIUtils.DrawSeparator();
      CRGUIUtils.Splitter();

      scroller_ = EditorGUILayout.BeginScrollView(scroller_);

      float originalLabelwidth = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = 200f;

      EditorGUI.BeginDisabledGroup(!isEditable);
      {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUI.BeginChangeCheck();
        RenderCreationParams( Data.CreationMode );
        if( EditorGUI.EndChangeCheck() && eManager.IsMultiJointCreated(Data) )
        {
          DestroyEntities();
        }

        EditorGUILayout.Space();

        CRGUIUtils.Splitter();    
        EditorGUILayout.Space();

        //FORCES
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(MaximumForce);
        DrawForceMax();
        EditorGUI.EndDisabledGroup();
        DrawMaximumForce();   
        EditorGUILayout.EndHorizontal();
        EditorGUI.BeginDisabledGroup(MaximumForce);
        DrawForceMaxRand();
        DrawForceRange();
        DrawForceProfile();
        EditorGUI.EndDisabledGroup();
      }
      EditorGUI.EndDisabledGroup();

      CRGUIUtils.Splitter();
      EditorGUILayout.Space();

      //BREAK
      EditorGUILayout.BeginHorizontal();
      Data.BreakFoldout = EditorGUILayout.Foldout(Data.BreakFoldout, "Break if distance exceeded" );

      EditorGUI.BeginDisabledGroup(!isEditable);
      {
        GUILayout.Space(145f);
        DrawBreakIfDistanceExceeded();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        {
          if (Data.BreakFoldout)
          {
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(!Data.BreakIfDistExcedeed);
            DrawDistanceForBreak();
            DrawDistanceForBreakRand();
            EditorGUI.EndDisabledGroup();
          }
          CRGUIUtils.Splitter();
          EditorGUILayout.Space();
          DrawBreakAllIfLeftFewUnbroken();
          EditorGUI.BeginDisabledGroup(!Data.BreakAllIfLeftFewUnbroken);
          DrawUnbrokenNumberForBreakAll();
          EditorGUI.EndDisabledGroup();
          DrawBreakIfHinge();
          DrawEnableCollisionIfBreak();
        }
      }
      EditorGUI.EndDisabledGroup();

      CRGUIUtils.Splitter();
      EditorGUILayout.Space();

      //PLASTICITY
      EditorGUILayout.BeginHorizontal();
      Data.PlasticityFoldout = EditorGUILayout.Foldout(Data.PlasticityFoldout, "Plasticity");

      EditorGUI.BeginDisabledGroup(!isEditable);
      {
        GUILayout.Space(145f);
        DrawPlasticity();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (Data.PlasticityFoldout)
        {
          EditorGUILayout.Space();
          
          EditorGUI.BeginDisabledGroup(!Data.Plasticity);
          DrawDistanceForPlasticity();
          DrawDistanceForPlasticityRand();
          DrawPlasticityRateAcquired();
          EditorGUI.EndDisabledGroup();
        }
      }  
      EditorGUI.EndDisabledGroup();
      CRGUIUtils.Splitter();

      EditorGUIUtility.labelWidth = originalLabelwidth;
      EditorGUILayout.EndScrollView();
      GUILayout.EndArea();
    } // RenderGUI
    //-----------------------------------------------------------------------------------
    protected void DrawContactDistanceSearch()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Distance search (m)", Data.ContactDistanceSearch);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change contact distance search - " + Data.Name);
        Data.ContactDistanceSearch = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawContactAreaMin()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Area min.", Data.ContactAreaMin);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change contact area min. - " + Data.Name);
        Data.ContactAreaMin = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawContactAngleMaxInDegrees()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Angle max.", Data.ContactAngleMaxInDegrees);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change angle max. - " + Data.Name );
        Data.ContactAngleMaxInDegrees = Mathf.Clamp(value, 0f, 360f);
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawContactNumberMax()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField("Number max. per pair", Data.ContactNumberMax);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change number max. per pair - " + Data.Name );
        Data.ContactNumberMax = Mathf.Clamp(value, 0, int.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawMatchingDistanceSearch()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Distance search", Data.MatchingDistanceSearch);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change distance search - " + Data.Name);
        Data.MatchingDistanceSearch = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawLocatorsMode()
    {
      EditorGUI.BeginChangeCheck();
      var value = (LocatorsModeEnum) EditorGUILayout.Popup( "Creation Mode", (int)locatorsMode_, locatorsModeNames_);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change creation mode - " + Data.Name);
        LocatorsMode = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawLimitNumberOfActiveJoints()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle("Limit number of processed joints", Data.LimitNumberOfActiveJoints);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change limit number of processed joints - " + Data.Name);
        Data.LimitNumberOfActiveJoints = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawActiveJointsMaxInABPair()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField("Max processed joints per pair", Data.ActiveJointsMaxInABPair);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change max processed joints per pair - " + Data.Name);
        Data.ActiveJointsMaxInABPair = Mathf.Clamp(value, 0, int.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawDisableCollisionsByPairs()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle("Disable collisions by pairs", Data.DisableCollisionsByPairs);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change disable collisions by pairs - " + Data.Name);
        Data.DisableCollisionsByPairs = value;
        if (value)
        {
          Data.DisableAllCollisionsOfAsWithBs = false;
        }
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawDisableAllCollisionsOfAsWithBs()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle("Disable all collisions A-B", Data.DisableAllCollisionsOfAsWithBs);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change disable all collisions A-B - " + Data.Name);
        Data.DisableAllCollisionsOfAsWithBs = value;
        if (value)
        {
          Data.DisableCollisionsByPairs = false;
        } 
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    private void RenderCreationParams( CNJointGroups.CreationModeEnum creationMode)
    {
      SetLocatorsOption();

      if ( Data.CreationMode == CNJointGroups.CreationModeEnum.ByContact )
      {
        DrawContactDistanceSearch();
        DrawContactAreaMin();
        DrawContactAngleMaxInDegrees();
        DrawContactNumberMax();
      }
      else if ( Data.CreationMode == CNJointGroups.CreationModeEnum.ByMatchingVertices )
      {
        DrawMatchingDistanceSearch();
      }
      else if ( locatorsMode_ != LocatorsModeEnum.None )
      {
        RenderFieldObjects("Locators", FieldControllerC, Data.IsCreateModeAtLocators, true, CNFieldWindow.Type.normal);
        GUILayout.Space(simple_space);
        DrawLocatorsMode();
      }

      GUILayout.Space(simple_space);
      DrawLimitNumberOfActiveJoints();
      EditorGUI.BeginDisabledGroup(!Data.LimitNumberOfActiveJoints);
      DrawActiveJointsMaxInABPair();
      EditorGUI.EndDisabledGroup();

      EditorGUILayout.Space();

      DrawDisableCollisionsByPairs();
      DrawDisableAllCollisionsOfAsWithBs();  
    }

  } //JointGroupsView

} // namespace Caronte...


