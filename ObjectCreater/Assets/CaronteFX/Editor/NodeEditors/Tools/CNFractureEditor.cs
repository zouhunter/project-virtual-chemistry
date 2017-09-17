using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;

namespace CaronteFX
{
  public class CNFractureEditor : CNMonoFieldEditor
  {
    //GUIContent chopModeContent_          = new GUIContent("Chop mode", "Add description");
    GUIContent globalPattenContent_      = new GUIContent("Global pattern", "Add description");
    GUIContent nDesiredPiecesContent_    = new GUIContent("Rough number of pieces", "Add description");
    GUIContent seedContent_              = new GUIContent("Seed", "Add description");
    GUIContent interiorMaterialContent_  = new GUIContent("Interior faces material", "Add description");

    GUIContent geometryContent_          = new GUIContent("Steering geometry", "Add description");
    GUIContent gridResolutionContent_    = new GUIContent("Steering resolution", "Add description");
    GUIContent focusModeContent_         = new GUIContent("Highest concentration", "Add description");  
    GUIContent densityRateContent_       = new GUIContent("Lowest/highest concentration rate", "Add description");
    GUIContent transitionLengthContent_  = new GUIContent("Transition length", "Add description");
    GUIContent doExtrusionEffectContent_ = new GUIContent("Extrusion effect", "Add description");
    GUIContent doCoordinateContent_      = new GUIContent("Coordinate", "Add description");

    GUIContent referenceSystemContent_     = new GUIContent("Reference system", "Add description");
    GUIContent referenceSystemAxisContent_ = new GUIContent("Front direction", "Add description");
    GUIContent raysNumberContent_          = new GUIContent("Rays number", "Add description");
    GUIContent raysRateRandContent_        = new GUIContent("Rays angle randomness", "Add description");

    GUIContent ringsIntRadiusContent_           = new GUIContent("Annulus internal radius", "Add description");
    GUIContent ringsIntTransitionLengthContent_ = new GUIContent("Internal transition length", "Add description");
    GUIContent ringsIntTransitionDecayContent_  = new GUIContent("Internal transition decay", "Add description");
    
    GUIContent ringsExtRadiusContent_           = new GUIContent("Annulus external radius", "Add description");
    GUIContent ringsExtTransitionLengthContent_ = new GUIContent("External transition length", "");
    GUIContent ringsExtTransitionDecayContent_  = new GUIContent("External transition decay", "");
    
    GUIContent ringsNumberInsideAnnulusContent_ = new GUIContent("Rings number inside annulus", "");
    GUIContent ringsRateRandContent_            = new GUIContent("Rings radius randomness", "");
    GUIContent createCentralPieceContent        = new GUIContent("Create central piece","");

    GUIContent noiseRateContent_ = new GUIContent("Noise", "noiseRateContent");
    GUIContent twistRateContent_ = new GUIContent("Twist", "Add description");

    GUIContent hideParentObjectsAutoCt_ = new GUIContent("Auto hide original objects", "Add description");

    GUIContent cropGeometryCt_ = new GUIContent("Crop geometry", "Add description");
    GUIContent cropModeCt_ = new GUIContent("Crop mode", "Add description");

    ushort currentPiece_;

    public static Texture icon_uniform_;
    public static Texture icon_geometry_;
    public static Texture icon_radial_;

    public override Texture TexIcon 
    { 
      get
      { 
        switch (Data.ChopMode)
        {
        case CNFracture.CHOP_MODE.VORONOI_UNIFORM:
          return icon_uniform_;

        case CNFracture.CHOP_MODE.VORONOI_BY_GEOMETRY:
          return icon_geometry_;

        case CNFracture.CHOP_MODE.VORONOI_RADIAL:
          return icon_radial_;
        
        default:
          return null;
        }
      } 
    }

    protected GUIContent[] tabNames_  = new GUIContent[] { new GUIContent("Parameters"), new GUIContent("Stats") };
    protected GUIContent[] cropModes_ = new GUIContent[] { new GUIContent("INSIDE"), new GUIContent("INSIDE + BOUNDARY"), new GUIContent("OUTSIDE"), new GUIContent("OUTSIDE + BOUNDARY") };
   
    int   tabIndex_;
    int   cropModeIdx_;
    float pushRate_;
    float pushMultiplier_;

    new CNFracture Data { get; set; }

    public CNFractureEditor( CNFracture data, CommandNodeEditorState state )
      : base(data, state)
    {
      Data = (CNFracture)data;
    }

    public void CheckUpdate(out int[] unityIdsAdded, out int[] unityIdsRemoved)
    {
      unityIdsAdded   = null;
      unityIdsRemoved = null;
      Data.NeedsUpdate = false;                
    }

    protected override void LoadState()
    {
      base.LoadState();

      pushRate_       = Data.PushRate;
      pushMultiplier_ = Data.PushMultiplier;
    }

    public override void ValidateState()
    {
      base.ValidateState();

      if ( (pushRate_ != Data.PushRate || pushMultiplier_ != Data.PushMultiplier) )
      {
        SeparatePieces();

        pushRate_       = Data.PushRate;
        pushMultiplier_ = Data.PushMultiplier;
      }  
    }


    public void Chop()
    {
      GameObject[] goToChop = FieldController.GetUnityGameObjects();
      int numObjects = goToChop.Length;

      string errorMessage = string.Empty;

      if ( numObjects == 0 )
      {
        errorMessage = "Objects field must contain at least one object with geometry";
      }

      if (Data.CropGeometry != null && !Data.CropGeometry.HasMesh() )
      {
        errorMessage = "Crop geometry GameObject must contain a mesh";
      }

      if (Data.ChopMode == CNFracture.CHOP_MODE.VORONOI_BY_GEOMETRY)
      {
        if (Data.ChopGeometry == null )
        {
          errorMessage = "Specifying a steering geometry is mandatory";
        }
        else if ( !Data.ChopGeometry.HasMesh() )
        {
          errorMessage = "Steering geometry GameObject must contain a mesh";
        }
      }

      if (Data.ChopMode == CNFracture.CHOP_MODE.VORONOI_RADIAL)
      {
        if (Data.ReferenceSystem == null)
        {
          errorMessage = "Specifying the reference system is mandatory";
        } 
      }

      if ( errorMessage != string.Empty )
      {
        EditorUtility.DisplayDialog("CaronteFX", errorMessage, "Ok");
        return;
      }

      Undo.RecordObject(Data, "Chop - " + Data.Name);

      List<GameObject>  listParentGO           = new List<GameObject>();
      List<Mesh>        listParentMesh_un      = new List<Mesh>();
      List<MeshComplex> listParentMesh_car     = new List<MeshComplex>();
      List<Matrix4x4>   listMatrixModelToWorld = new List<Matrix4x4>();

      for (int i = 0; i < numObjects; i++)
      {
        GameObject go = goToChop[i];
        Mesh un_mesh  = go.GetMesh();

        if (un_mesh != null)
        {
          MeshComplex mc = new MeshComplex();
          mc.Set( un_mesh );

          listParentMesh_car    .Add( mc );
          listParentMesh_un     .Add( un_mesh );
          listParentGO          .Add( go );
          listMatrixModelToWorld.Add( go.transform.localToWorldMatrix );
        }
      }

      Bounds globalBounds = CREditorUtils.GetGlobalBoundsWorld( listParentGO );

      ChopRequest cr = new ChopRequest();

      cr.doKeepUVCoords_          = true;
      cr.doKeepVertexNormals_     = true;
      cr.doParentIndexTriangInfo_ = true;
      cr.arrMeshToChop_           = listParentMesh_car.ToArray();
      cr.arrMatrixModelToWorld_   = listMatrixModelToWorld.ToArray();
      
      cr.doGlobalPattern_         = Data.DoGlobalPattern;
      cr.seed_                    = (uint)Data.Seed;

      bool chopModeUniform  = false;
      bool chopModeGeometry = false;
      bool chopModeRadial   = false;

      cr.pProgressFunction_ = null;

      switch (Data.ChopMode)
      {
        case CNFracture.CHOP_MODE.VORONOI_UNIFORM:
          cr.chopMode_ = CaronteSharp.CP_CHOP_MODE.CP_CHOP_MODE_VORONOI_UNIFORM;
          chopModeUniform  = ChopModeUniform( cr );
          break;

        case CNFracture.CHOP_MODE.VORONOI_BY_GEOMETRY:
          cr.chopMode_ = CaronteSharp.CP_CHOP_MODE.CP_CHOP_MODE_VORONOI_BY_GEOMETRY;
          chopModeGeometry = ChopModeGeometry( cr );
          break;

        case CNFracture.CHOP_MODE.VORONOI_RADIAL:
          cr.chopMode_ = CaronteSharp.CP_CHOP_MODE.CP_CHOP_MODE_VORONOI_RADIAL;
          chopModeRadial   = ChopModeRadial( cr );
          break;

      }

      if ( !chopModeUniform && !chopModeGeometry && !chopModeRadial )
      {
        return;
      }

      WeldRequest wr = GetWeldRequest();

      CaronteSharp.MeshComplex[]     arrMeshPieceCaronte;
      CaronteSharp.MeshParentInfo[]  arrMeshParentInfo;
      Vector3[]   arrMeshPosition;
      ArrayIndex  arrInsideOutsideIdx;

      EditorUtility.DisplayProgressBar( Data.Name, "Chopping...", 1.0f);
      CaronteSharp.Tools.FractureMeshesV2( cr, wr, out arrMeshPieceCaronte, out arrMeshParentInfo, out arrMeshPosition, out arrInsideOutsideIdx );


      bool thereIsOutput = arrMeshPieceCaronte.Length > 0;
 
      if (cnManager.IsFreeVersion() && !thereIsOutput )
      {
        EditorUtility.DisplayDialog("CaronteFX - Free version", "CaronteFX Free version can only fracture the meshes included in the example scenes and the unity primitives (cube, plane, sphere, etc.)", "Ok");
      }

      if (thereIsOutput)
      {
        List<GameObject> listChoppedParentGO;
        CreateListChoppedParentGO( listParentGO, out listChoppedParentGO );

        EditorUtility.DisplayProgressBar( Data.Name, "Post processing...", 1.0f);
        UnityEngine.Mesh[]   arrMeshPieceUnity;
        Tuple2<int,int>[]    arrSubmeshRange;
        int[]                arrMeshComplexIdx;

        CreateMeshPiecesUnity( listChoppedParentGO, arrMeshPieceCaronte, arrMeshParentInfo, arrMeshPosition, 
                                out arrMeshPieceUnity, out arrSubmeshRange, out arrMeshComplexIdx );

        Transform oldParent = DestroyOldObjects();

        Undo.RecordObject(Data, "Chop - " + Data.Name);
        CreateNewObjects( globalBounds.center, listParentGO, listChoppedParentGO, arrMeshParentInfo, 
                          arrMeshPieceUnity, arrSubmeshRange, arrMeshComplexIdx, arrMeshPosition, arrInsideOutsideIdx.arrIdx_ );

      
        if (oldParent != null)
        {
          Data.GameObjectChoppedRoot.transform.parent = oldParent;
        }

        SeparatePieces();

        CalculateStatistics();

        EditorUtility.ClearProgressBar();

        Undo.SetCurrentGroupName("Chop - " + Data.Name);
        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        EditorUtility.SetDirty(Data);
      }
    }

    private void CalculateStatistics()
    {
      GameObject[] goToChop = FieldController.GetUnityGameObjects();

      int nInputObjects = 0;
      int inputVertices = 0;
      int inputIndices = 0;

      int nOutputPieces = 0;
      int outputVertices = 0;
      int outputIndices = 0;

      for (int i = 0; i < goToChop.Length; i++)
      {
        GameObject go = goToChop[i];
        Mesh mesh = go.GetMesh();

        if ( mesh != null )
        {
          nInputObjects++;
          inputVertices += mesh.vertexCount;
          inputIndices+= mesh.triangles.Length;
        }
      }

      Data.InputObjects    = nInputObjects;
      Data.InputVertices   = inputVertices;
      Data.InputTriangles  = inputIndices / 3;

      GameObject[] arrGOChopped = Data.ArrChoppedGameObject;
      for (int i = 0; i < arrGOChopped.Length; i++)
      {
        GameObject go = arrGOChopped[i];
        Mesh mesh = go.GetMesh();

        if ( mesh != null )
        {
          nOutputPieces++;
          outputVertices += mesh.vertexCount;
          outputIndices+= mesh.triangles.Length;
        }
      }

      Data.OutputPieces = nOutputPieces;
      Data.OutputVertices  = outputVertices;
      Data.OutputTriangles = outputIndices / 3;

    }

    private void CreateListChoppedParentGO( List<GameObject> listParentGO, out List<GameObject> listChoppedParentGO )
    {
      listChoppedParentGO = new List<GameObject>();

      // Create ParentGO Pieces
      for ( int i = 0; i < listParentGO.Count; i++)
      {
        GameObject parentGO       = listParentGO[i];
        GameObject chopParentGO   = parentGO.CreateDummy(parentGO.name + "_chopped");
        listChoppedParentGO.Add(chopParentGO);
      }

    }

    private Transform DestroyOldObjects()
    {
      if ( Data.GameObjectChoppedRoot == null )
      {
        return null;
      }

      Transform oldParent = Data.GameObjectChoppedRoot.transform.parent;

      Undo.DestroyObjectImmediate(Data.GameObjectChoppedRoot);

      return oldParent;
    }

    private WeldRequest GetWeldRequest()
    {
      WeldRequest wr = new WeldRequest();

      GameObject go = Data.CropGeometry;
      if (go != null)
      {
        Mesh mesh = go.GetMesh();

        if (mesh != null)
        {
          UnityEngine.Mesh transformedMesh;

          Matrix4x4 m_Local_to_World  = go.transform.localToWorldMatrix;
          CRGeometryUtils.CreateMeshTransformed(mesh, m_Local_to_World, out transformedMesh);

          MeshSimple cropGeometry = new MeshSimple();
          cropGeometry.Set( transformedMesh );
          Object.DestroyImmediate( transformedMesh );

          wr.meshCropGeometry_ = cropGeometry;
        }      
      }

      wr.cropMode_                     = (CROP_MODE)Data.CropMode;
      wr.weldAllRemainingsTogether_    = Data.WeldInOnePiece;
      wr.includeBoundary_              = Data.FrontierPieces;
      wr.isAbleToClassifyDisconnected_ = true;

      return wr;
    }

    public void ChangeInteriorMaterial(Material interiorMaterial)
    {
      if (Data.ArrChoppedGameObject != null && Data.ArrInteriorSubmeshIdx != null)
      {
        int nGameObjects = Data.ArrChoppedGameObject.Length;

        for (int i = 0; i < nGameObjects; i++)
        {
          GameObject go = Data.ArrChoppedGameObject[i];
          if ( go != null )
          {
            Renderer   renderer  = go.GetComponent<Renderer>();
            Material[] materials = renderer.sharedMaterials;

            int subMeshIdx = Data.ArrInteriorSubmeshIdx[i];
            if (subMeshIdx != -1)
            {
              materials[subMeshIdx] = interiorMaterial;  
              renderer.sharedMaterials = materials;
            }
          }
        }
      }
    }

    private void CreateNewObjects( Vector3 position, List<GameObject> listParentGO, List<GameObject> listChoppedParentGO, CaronteSharp.MeshParentInfo[] arrMeshParentInfo, 
                                   UnityEngine.Mesh[] arrMeshPieceUnity, Tuple2<int,int>[] arrSubmeshRange, int[] arrMeshComplexIdx,  Vector3[] arrMeshPosition, uint[] arrOutsideInsideIdx  )
    {  
      List< GameObject > listChoppedGO = new List<GameObject>();

      List< Tuple2<GameObject, GameObject> > listOutsideInsideGOParent = new List< Tuple2<GameObject, GameObject> >();
      bool hasBeenSplitted = arrOutsideInsideIdx != null;

      // Create InsideOutSidePieces
      for ( int i = 0; i < listParentGO.Count; i++)
      {
        GameObject parentGO     = listParentGO[i];
        GameObject chopParentGO = listChoppedParentGO[i];

        if ( hasBeenSplitted )
        {
          GameObject outsideGO = parentGO.CreateDummy(parentGO.name + "_outside");
          GameObject insideGO  = parentGO.CreateDummy(parentGO.name + "_inside");

          outsideGO.transform.parent = chopParentGO.transform;
          insideGO.transform.parent  = chopParentGO.transform;

          listOutsideInsideGOParent.Add( Tuple2.New( outsideGO, insideGO ) );
        }
        
        if ( Data.HideParentObjectAuto )
        {
          Undo.RecordObject( parentGO, "Change activate state - " + parentGO.name );
          parentGO.SetActive(false);
          EditorUtility.SetDirty( parentGO );
        }
      }

      //Create Chopped Pieces
      Dictionary<GameObject, int> dictChoppedGOInteriorSubmeshIdx = new Dictionary<GameObject, int>();

      int nMeshPieces = arrMeshPieceUnity.Length;
      for (int i = 0; i < nMeshPieces; i++)
      {
        UnityEngine.Mesh meshPiece    = arrMeshPieceUnity[i];
        Tuple2<int, int> submeshRange = arrSubmeshRange[i];
        int meshComplexIdx            = arrMeshComplexIdx[i];
        Vector3 goPosition            = arrMeshPosition[i];

        MeshParentInfo parentInfo     = arrMeshParentInfo[meshComplexIdx];
        int parentIdx                 = (int)parentInfo.parentMeshIdx_;

        GameObject parentGO        = listParentGO[parentIdx];
        GameObject chopParentGO    = listChoppedParentGO[parentIdx];
        
        int interiorPieceSubmeshIdx;
        GameObject goPiece = CRGeometryUtils.CreatePiece( i, meshPiece, parentInfo, goPosition, submeshRange, 
                                                          Data.InteriorMaterial, parentGO, chopParentGO, out interiorPieceSubmeshIdx );
        
        if (hasBeenSplitted)
        {
          Tuple2<GameObject, GameObject> tupleOutsideInside = listOutsideInsideGOParent[parentIdx];

          GameObject outsideGO = tupleOutsideInside.First;
          GameObject insideGO  = tupleOutsideInside.Second;

          uint outsideInsideIdx = arrOutsideInsideIdx[meshComplexIdx];
          if( outsideInsideIdx == 0 )
          {
            goPiece.transform.parent = outsideGO.transform;
            goPiece.name = parentGO.name + "_out_" + i;
          }
          else if( outsideInsideIdx == 1 )
          {
            goPiece.transform.parent = insideGO.transform;
            goPiece.name = parentGO.name + "_in_" + i;
          }
        }

        listChoppedGO                  .Add(goPiece);
        dictChoppedGOInteriorSubmeshIdx.Add(goPiece, interiorPieceSubmeshIdx);
      }

      

      GameObject chopRoot = new GameObject( Data.Name + "_output" );
      chopRoot.transform.position = position;
      Undo.RegisterCreatedObjectUndo(chopRoot, "Created " + Data.Name + "_output");

      Data.GameObjectChoppedRoot = chopRoot;

      for (int i = 0; i < listChoppedParentGO.Count; i++)
      {
        GameObject chopParentGO = listChoppedParentGO[i];

        chopParentGO.transform.parent = Data.GameObjectChoppedRoot.transform;
        chopParentGO.transform.SetAsFirstSibling();
      }

      Selection.activeGameObject = Data.GameObjectChoppedRoot ;

      SaveNewChopInfo(dictChoppedGOInteriorSubmeshIdx);
    }

    private void CreateMeshPiecesUnity( List<GameObject> listChoppedParentGO, CaronteSharp.MeshComplex[] arrMeshPieceCaronte, CaronteSharp.MeshParentInfo[] arrMeshParentInfo, Vector3[] arrMeshPosition,
                                        out UnityEngine.Mesh[] arrMeshPieceUnity, out Tuple2<int,int>[] arrSubmeshRange, out int[] arrMeshComplexIndex )
    {
      if (arrMeshPieceCaronte.Length == 0)
        arrMeshPieceUnity = new UnityEngine.Mesh[0];

      CRGeometryUtils.CreateMeshesFromCaronte( arrMeshPieceCaronte, out arrMeshPieceUnity, out arrSubmeshRange, out arrMeshComplexIndex );
    }

    private bool ChopModeUniform( ChopRequest cr )
    {
      if ( Data.NDesiredPieces == 0 )
      {
        return false;
      }
      cr.nDesiredPieces_ = (uint)Data.NDesiredPieces;
      cr.doExtrusionEffect_ = Data.DoExtrusionEffect;
      cr.doCoordinate_      = Data.DoCoordinate;

      return true;
    }

    private bool ChopModeGeometry( ChopRequest cr )
    {
      if ( (Data.ChopGeometry == null) || !Data.ChopGeometry.HasMesh() || Data.NDesiredPieces <= 0 )
      {
        return false;
      }

      Matrix4x4 m_Local_to_World  = Data.ChopGeometry.transform.localToWorldMatrix;
      UnityEngine.Mesh un_chopGeometry = Data.ChopGeometry.GetMesh();
      
      UnityEngine.Mesh transformedMesh;
      CRGeometryUtils.CreateMeshTransformed( un_chopGeometry, m_Local_to_World, out transformedMesh );

      CaronteSharp.MeshSimple car_chopGeometry = new CaronteSharp.MeshSimple();
      car_chopGeometry.Set(transformedMesh);
      Object.DestroyImmediate(transformedMesh);

      cr.nDesiredPieces_    = (uint)Data.NDesiredPieces;  
      cr.meshFocusGeometry_ = car_chopGeometry;
      cr.focusMode_         = (PSBG_FOCUS_MODE)Data.FocusMode;
      cr.gridResolution_    = (uint)Data.GridResolution;
      cr.densityRate_       = Data.DensityRate;
      cr.transitionLength_  = Data.TransitionLength;
      cr.doExtrusionEffect_ = Data.DoExtrusionEffect;
      cr.doCoordinate_      = Data.DoCoordinate;

      return true;
    }

    private bool ChopModeRadial( ChopRequest cr )
    {
      if (Data.ReferenceSystem == null)
      {
        Debug.Log("Radial mode requires specifying a reference system.");
        return false;      
      }

      Transform tr = Data.ReferenceSystem.transform;
      Matrix4x4 m_LOCAL_to_WORLD = tr.localToWorldMatrix;

      cr.focusSystem_center_ = Data.ReferenceSystem.position;

      switch (Data.ReferenceSystemAxis)
      {
        case CNFracture.AxisDir.x:
          cr.focusSystem_axisDir_ = m_LOCAL_to_WORLD.MultiplyVector(Data.ReferenceSystem.right);
          break;
        case CNFracture.AxisDir.y:
          cr.focusSystem_axisDir_ = m_LOCAL_to_WORLD.MultiplyVector(Data.ReferenceSystem.up);
          break;
        case CNFracture.AxisDir.z:
          cr.focusSystem_axisDir_ = m_LOCAL_to_WORLD.MultiplyVector(Data.ReferenceSystem.forward);
          break;
      }

      cr.rays_number_   = (uint)Data.RaysNumber;
      cr.rays_rateRand_ = Data.RaysRateRand;

      cr.rings_numberInsideAnnulus_ = (uint)Data.RingsNumberInsideAnnulus;
      cr.rings_intRadius_           = Data.RingsIntRadius;
      cr.rings_extRadius_           = Data.RingsExtRadius;
      cr.rings_intTransitionLength_ = Data.RingsIntTransitionLength;
      cr.rings_extTransitionLength_ = Data.RingsExtTransitionLength;
      cr.rings_intTransitionDecay_  = Data.RingsIntTransitionDecay;
      cr.rings_extTransitionDecay_  = Data.RingsExtTransitionDecay;;
      
      cr.rings_rateRand_ = Data.RingsRateRand;
      cr.doCentralPiece_ = Data.DoCentralPiece;

      cr.noiseRate_    = Data.NoiseRate;
      cr.twistRate_    = Data.TwistRate;
      cr.doCoordinate_ = Data.DoCoordinate;

      return true;   
    }

    private void SaveNewChopInfo(Dictionary<GameObject, int> dictChoppedGOInteriorSubmeshIdx)
    {     
      List<GameObject> listChoppedGameObject = new List<GameObject>();

      GameObject[] arrChoppedGO = CREditorUtils.GetAllChildObjectsWithGeometry(Data.GameObjectChoppedRoot, true);
      listChoppedGameObject.AddRange(arrChoppedGO);

      
      int nTotalPieces = listChoppedGameObject.Count;

      Data.ArrChoppedGameObject = listChoppedGameObject.ToArray();

      Data.ArrInteriorSubmeshIdx           = new int[nTotalPieces];
      Data.ArrGameObject_Bounds_Chopped    = new Bounds[nTotalPieces];
      Data.ArrGameObject_Chopped_Positions = new Vector3[nTotalPieces];

      for (int i = 0; i < nTotalPieces; i++)
      {
        GameObject go = listChoppedGameObject[i];

        int interiorSubMeshIdx = dictChoppedGOInteriorSubmeshIdx[go];

        Data.ArrInteriorSubmeshIdx[i]           = interiorSubMeshIdx;
        Data.ArrGameObject_Bounds_Chopped[i]    = go.GetWorldBounds();
        Data.ArrGameObject_Chopped_Positions[i] = go.transform.localPosition;
      }

      EditorUtility.SetDirty( Data );
    }

    public void SeparatePieces()
    {
      int numPieces = Data.ArrGameObject_Bounds_Chopped.Length;
      Box[]  arrBox = new Box[numPieces];
      for (int i = 0; i < numPieces; i++)
      {
        Bounds bounds = Data.ArrGameObject_Bounds_Chopped[i];
        arrBox[i] = new Box(bounds.min, bounds.max);
      }
      Vector3[] arrDeltaPush;
      CaronteSharp.Tools.CalculateDeltasToPushAwayPieces(arrBox, Data.PushMultiplier, out arrDeltaPush);
      
      for (int i = 0; i < numPieces; i++)
      {
        GameObject go = Data.ArrChoppedGameObject[i];
        if (go != null)
        {
          Transform tr  = go.transform;
          tr.localPosition = Data.ArrGameObject_Chopped_Positions[i] + (tr.worldToLocalMatrix.MultiplyVector(arrDeltaPush[i]) * Data.PushRate);
        }
      }
    }

    private void DrawCropGeometry()
    {
      EditorGUI.BeginChangeCheck();
      var value = (GameObject) EditorGUILayout.ObjectField(cropGeometryCt_, Data.CropGeometry, typeof(GameObject), true );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change crop geometry - " + Data.Name);
        Data.CropGeometry = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void LoadGUICropMode()
    {
      if ( Data.CropMode == CNFracture.CROP_MODE.INSIDE )
      {
        if ( !Data.FrontierPieces )
        {
          cropModeIdx_ = 0;
        }
        else
        {
          cropModeIdx_ = 1;
        }
      }
      else if ( Data.CropMode == CNFracture.CROP_MODE.OUTSIDE )
      {
        if (!Data.FrontierPieces)
        {
          cropModeIdx_ = 2;
        }
        else
        {
          cropModeIdx_ = 3;
        }
      }
    }

    private void DrawCropMode()
    {
      LoadGUICropMode();
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Popup( cropModeCt_, cropModeIdx_, cropModes_ );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change crop mode - " + Data.Name);
        cropModeIdx_ = value;
        SetCropMode();
        EditorUtility.SetDirty( Data );
      }
    }

    private void DrawWeldInOne()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle("Weld all remainings together", Data.WeldInOnePiece);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change weld all remainings together - " + Data.Name);
        Data.WeldInOnePiece = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawPushRate()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider("Push Rate", Data.PushRate, 0.0f, 1.0f);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change push rate - " + Data.Name);
        Data.PushRate = value;
        EditorUtility.SetDirty(Data);
      } 
    }

    private void DrawPushMultiplier()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Push Multiplier", Data.PushMultiplier);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change push multiplier - " + Data.Name);
        Data.PushMultiplier = value;
        EditorUtility.SetDirty(Data);
      }
    }

    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);
      
      RenderTitle(isEditable, false, false);

      EditorGUI.BeginDisabledGroup(!isEditable);
      RenderFieldObjects( "Objects", FieldController, true, true, CNFieldWindow.Type.normal );

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUILayout.BeginHorizontal();
      if ( GUILayout.Button("Chop", GUILayout.Height(30f)) )
      {
        Chop();
      }

      if ( GUILayout.Button("Save assets..", GUILayout.Height(30f), GUILayout.Width(100f) ) )
      {
        SaveChopResult();
      }

      EditorGUILayout.EndHorizontal();

      EditorGUI.EndDisabledGroup();
      CRGUIUtils.DrawSeparator();
      CRGUIUtils.Splitter();

      tabIndex_ = GUILayout.SelectionGrid(tabIndex_, tabNames_, 2);
      CRGUIUtils.Splitter();

      scroller_ = EditorGUILayout.BeginScrollView(scroller_);

      if (tabIndex_ == 1)
      {
        EditorGUILayout.LabelField("Last chop input objects "    + Data.InputObjects );
        EditorGUILayout.LabelField("Last chop input vertices: "  + Data.InputVertices );
        EditorGUILayout.LabelField("Last chop input triangles: " + Data.InputTriangles );

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Last chop output pieces: "    + Data.OutputPieces );
        EditorGUILayout.LabelField("Last chop output vertices: "  + Data.OutputVertices );
        EditorGUILayout.LabelField("Last chop output triangles: " + Data.OutputTriangles );
      }
      else
      {
        EditorGUI.BeginDisabledGroup(!isEditable);

        float originalLabelwidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 200f;

        EditorGUILayout.Space();

        switch (Data.ChopMode)
        {
          case CNFracture.CHOP_MODE.VORONOI_UNIFORM:
            DrawGUIUniform();
            break;
          case CNFracture.CHOP_MODE.VORONOI_BY_GEOMETRY: 
            DrawGUIGeometry();
            break;
          case CNFracture.CHOP_MODE.VORONOI_RADIAL:
            DrawGUIRadial();
            break;
        }

        EditorGUILayout.Space();
        CRGUIUtils.Splitter();
        EditorGUILayout.Space();

        DrawCropGeometry();
        DrawCropMode();
        DrawWeldInOne();

        CRGUIUtils.Splitter();

        GUIStyle centerLabel = new GUIStyle(EditorStyles.largeLabel);
        centerLabel.alignment = TextAnchor.MiddleCenter;
        centerLabel.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Output", centerLabel);

        EditorGUI.BeginDisabledGroup(Data.GameObjectChoppedRoot == null);
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.Space();

        if ( GUILayout.Button("Select pieces") )
        {
          Selection.activeGameObject = Data.GameObjectChoppedRoot;
        }

        DrawPushRate();
        DrawPushMultiplier();
        ValidateState();

        EditorGUI.EndDisabledGroup();

        EditorGUIUtility.labelWidth = originalLabelwidth;

        EditorGUI.EndDisabledGroup();
      }

      EditorGUILayout.EndScrollView();
      GUILayout.EndArea();  
    }
 

    void SetCropMode()
    {
      if ( cropModeIdx_ == 0 )
      {
        Data.CropMode       = CNFracture.CROP_MODE.INSIDE;
        Data.FrontierPieces = false;
      }
      else if (cropModeIdx_ == 1)
      {
        Data.CropMode       = CNFracture.CROP_MODE.INSIDE;
        Data.FrontierPieces = true;
      }
      else if ( cropModeIdx_ == 2 )
      {
        Data.CropMode       = CNFracture.CROP_MODE.OUTSIDE;
        Data.FrontierPieces = false;
      }
      else if ( cropModeIdx_ == 3 )
      {
        Data.CropMode       = CNFracture.CROP_MODE.OUTSIDE;
        Data.FrontierPieces = true;
      }
    }

    private bool HasUnsavedChopReferences()
    {
      if (Data.GameObjectChoppedRoot == null)
      {
        return false;
      }

      if ( CREditorUtils.IsAnyUnsavedMeshInHierarchy(Data.GameObjectChoppedRoot) )
      {
        return true;
      }
      else
      {
        return false;
      }    
    }

    private void SaveChopResult()
    {
      if (!HasUnsavedChopReferences())
      {
        EditorUtility.DisplayDialog("CaronteFX - Info", "There is not any mesh to save in assets.", "ok" );
        return;
      }

      CREditorUtils.SaveAnyUnsavedMeshInHierarchy(Data.GameObjectChoppedRoot, false);
    }

    private void DrawGlobalPattern()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle(globalPattenContent_, Data.DoGlobalPattern);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change global pattern" + Data.Name);
        Data.DoGlobalPattern = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawNDesiredPieces()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField(nDesiredPiecesContent_, Data.NDesiredPieces);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change rough number of pieces - " + Data.Name);
        Data.NDesiredPieces = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawSeed()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField(seedContent_, Data.Seed);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change seed - " + Data.Name);
        Data.Seed = Mathf.Clamp(value, 0, int.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawInteriorMaterial()
    {
      EditorGUI.BeginChangeCheck();
      var value = (Material) EditorGUILayout.ObjectField(interiorMaterialContent_, Data.InteriorMaterial, typeof(Material), true );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change interior material - " + Data.Name);
        ChangeInteriorMaterial( value );
        Data.InteriorMaterial = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawDoExtrussionEffect()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle(doExtrusionEffectContent_, Data.DoExtrusionEffect);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change do extrussion effect - " + Data.Name);
        Data.DoExtrusionEffect = value;
        EditorUtility.SetDirty(Data);
      }
    }


    private void DrawDoCoordinate()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle(doCoordinateContent_, Data.DoCoordinate);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change do coordinate - " + Data.Name);
        Data.DoCoordinate = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawHideParentObjectAuto()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle( hideParentObjectsAutoCt_, Data.HideParentObjectAuto );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Auto hide original objects - " + Data.Name);
        Data.HideParentObjectAuto = value;
        EditorUtility.SetDirty(Data);
      }
    }

    void DrawGUIUniform()
    {
      DrawGlobalPattern(); 
      DrawNDesiredPieces();
      DrawSeed();
      DrawInteriorMaterial();
      EditorGUILayout.Space();
      DrawDoExtrussionEffect();
      DrawDoCoordinate();
      EditorGUILayout.Space();
      DrawHideParentObjectAuto();
    }

    private void DrawChopGeometry()
    {
      EditorGUI.BeginChangeCheck();
      var value = (GameObject) EditorGUILayout.ObjectField(geometryContent_, Data.ChopGeometry, typeof(GameObject), true );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change steering geometry - " + Data.Name);
        Data.ChopGeometry = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawGridResolution()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField(gridResolutionContent_, Data.GridResolution );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change grid resolution - " + Data.Name);
        Data.GridResolution = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawFocusMode()
    {
      EditorGUI.BeginChangeCheck();
      var value = (CNFracture.FOCUS_MODE) EditorGUILayout.EnumPopup(focusModeContent_, Data.FocusMode );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change focus mode - " + Data.Name);
        Data.FocusMode = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawDensityRate()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(densityRateContent_, Data.DensityRate );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change density rate - " + Data.Name);
        Data.DensityRate = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawTransitionLength()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(transitionLengthContent_, Data.TransitionLength);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change transition length - " + Data.Name);
        Data.TransitionLength = value;
        EditorUtility.SetDirty(Data);
      }
    }

    void DrawGUIGeometry()
    {
      DrawGUIUniform();
      EditorGUILayout.Space();
      CRGUIUtils.Splitter();
      EditorGUILayout.Space();
      EditorGUILayout.Space();
      DrawChopGeometry();
      DrawGridResolution();
      DrawFocusMode();
      DrawDensityRate();
      DrawTransitionLength();
    }

    private void DrawReferenceSystem()
    {
      EditorGUI.BeginChangeCheck();
      var value = (Transform) EditorGUILayout.ObjectField(referenceSystemContent_, Data.ReferenceSystem, typeof(Transform), true );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change reference system - " + Data.Name);
        Data.ReferenceSystem = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawReferenceSystemAxis()
    {
      EditorGUI.BeginChangeCheck();
      var value = (CNFracture.AxisDir) EditorGUILayout.EnumPopup(referenceSystemAxisContent_, Data.ReferenceSystemAxis);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change reference system axis - " + Data.Name);
        Data.ReferenceSystemAxis = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRaysNumber()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField(raysNumberContent_, Data.RaysNumber);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change rays number - " + Data.Name);
        Data.RaysNumber = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRaysRateRand()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(raysRateRandContent_, Data.RaysRateRand);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change rays rate rand. - " + Data.Name);
        Data.RaysRateRand = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawIntRadius()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(ringsIntRadiusContent_, Data.RingsIntRadius);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change annulus internal radius - " + Data.Name);
        Data.RingsIntRadius = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRingsIntTransitionLength()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(ringsIntTransitionLengthContent_, Data.RingsIntTransitionLength);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change internal transition length - " + Data.Name);
        Data.RingsIntTransitionLength = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRingsIntTransitionDecay()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(ringsIntTransitionDecayContent_, Data.RingsIntTransitionDecay);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change internal transition decay - " + Data.Name);
        Data.RingsIntTransitionDecay = value;
        EditorUtility.SetDirty(Data);
      }
    }


    private void DrawRingsExtRadius()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(ringsExtRadiusContent_, Data.RingsExtRadius);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change annulus external radius - " + Data.Name);
        Data.RingsExtRadius = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRingsExtTransitionLength()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(ringsExtTransitionLengthContent_, Data.RingsExtTransitionLength);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change annulus external transition length - " + Data.Name);
        Data.RingsExtTransitionLength = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRingsExtTransitionDecay()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(ringsExtTransitionDecayContent_, Data.RingsExtTransitionDecay);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change annulus external transition decay - " + Data.Name);
        Data.RingsExtTransitionDecay = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRingsNumberInsideAnnulus()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField(ringsNumberInsideAnnulusContent_, Data.RingsNumberInsideAnnulus);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change number of rings inside annulus - " + Data.Name);
        Data.RingsNumberInsideAnnulus = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRingsRateRand()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(ringsRateRandContent_, Data.RingsRateRand);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change rings radius randomness - " + Data.Name);
        Data.RingsRateRand = value;
        EditorUtility.SetDirty(Data);
      }
    }


    private void DrawDoCentralPiece()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle(createCentralPieceContent, Data.DoCentralPiece);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change create centeral piece - " + Data.Name);
        Data.DoCentralPiece = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawNoiseRate()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(noiseRateContent_, Data.NoiseRate);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change noise rate - " + Data.Name);
        Data.NoiseRate = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawTwistRate()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(twistRateContent_, Data.TwistRate);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change twist rate - " + Data.Name);
        Data.TwistRate = value;
        EditorUtility.SetDirty(Data);
      }
    }

    void DrawGUIRadial()
    {
      DrawGlobalPattern();
      DrawSeed();
      DrawInteriorMaterial();
      EditorGUILayout.Space();
      DrawDoExtrussionEffect();
      DrawDoCoordinate();
      EditorGUILayout.Space();
      DrawHideParentObjectAuto();

      EditorGUILayout.Space();
      CRGUIUtils.Splitter();
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      DrawReferenceSystem();
      DrawReferenceSystemAxis();
      
      EditorGUILayout.Space();
      DrawRaysNumber(); 
      DrawRaysRateRand();

      EditorGUILayout.Space();
      CRGUIUtils.Splitter();
      EditorGUILayout.Space();
      EditorGUILayout.Space();
      DrawIntRadius();
      DrawRingsIntTransitionLength();
      DrawRingsIntTransitionDecay();
      
      EditorGUILayout.Space();
      DrawRingsExtRadius();
      DrawRingsExtTransitionLength();
      DrawRingsExtTransitionDecay();

      EditorGUILayout.Space();
      CRGUIUtils.Splitter();
      EditorGUILayout.Space();
      EditorGUILayout.Space();
      DrawRingsNumberInsideAnnulus();
      DrawRingsRateRand();
      DrawDoCentralPiece();
      EditorGUILayout.Space();
      DrawNoiseRate();
      DrawTwistRate();
    }




  } //class CNFractureView
} // namespace Caronte

