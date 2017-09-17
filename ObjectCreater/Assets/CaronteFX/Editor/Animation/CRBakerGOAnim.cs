#define CR_UNITY_3_PLUS
#define CR_UNITY_4_PLUS
#define CR_UNITY_5_PLUS

#if UNITY_2_6
#define CR_UNITY_2_X
#undef CR_UNITY_3_PLUS
#undef CR_UNITY_4_PLUS
#undef CR_UNITY_5_PLUS

#elif UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
  #define CR_UNITY_3_X
  #undef CR_UNITY_4_PLUS
  #undef CR_UNITY_5_PLUS

#elif UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
#define CR_UNITY_4_X
#undef CR_UNITY_5_PLUS

#elif UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9
  #define CR_UNITY_5_X
#endif


using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CaronteSharp;

namespace CaronteFX
{
  /// <summary>
  /// Used to bake Caronte Simulations
  /// </summary>
  public class CRBakerGOAnim
  { 
    //-----------------------------------------------------------------------------------
    //--------------------------------DATA MEMBERS---------------------------------------
    //-----------------------------------------------------------------------------------
    const int  binaryVersion_ = 6;
    
    CNManager          manager_;
    CREntityManager    entityManager_;
    CRPlayer           player_;

    CRMeshCombiner     skinnedMeshCombiner_;
    CRMeshCombiner     frameMeshCombiner_;

    UnityEngine.Object meshesPrefab_;
    String             assetsPath_;
    String             fxName_;
    
    Dictionary<uint, string>        idBodyToRelativePath_;
    Dictionary<uint, GameObject>    idBodyToBakedGO_;
    Dictionary<uint, CRGOKeyframe>  idBodyToGOKeyframe_;
    Dictionary<uint, Vector2>       idBodyToVisibilityInterval_;

    Dictionary<GameObject, GameObject> originalGOToBakedGO_;

    List<uint>       listIdBodyTmp_;
    List<GameObject> listGOTmp_;

    HashSet<uint> setBoneBodies_;
    HashSet<uint> setVisibleBodies_;

    MeshComplex meshComplexForUpdate_ = new MeshComplex();

    int   frame_;
    int   bakingFrame_;

    int   frameCount_;

    int   frameStart_ = -1;
    int   frameEnd_   = -1;

    float visibilityShift_ = 0;

    public string animationName_;
    public string bakeObjectName_;

    public bool sbCompression_ = true;
    public bool sbTangents_    = false;
    public bool bakeEvents_    = true;

    public bool bakeAllNodes_         = true;
    public bool combineMeshesInFrame_ = false;
 
    public List<CNBody> listBodyNode_ = new List<CNBody>();
    public BitArray bitArrNeedsBaking_;
    public BitArray bitArrNeedsCollapsing_;

    GameObject  rootGameObject_;
    //-----------------------------------------------------------------------------------
    public int FrameStart
    {
      get
      {
        return frameStart_;
      }
      set
      {
        frameStart_ = Mathf.Clamp(value, 0, player_.MaxFrames);
      }
    }

    public int FrameEnd
    {
      get
      {
        return frameEnd_;
      }
      set
      {
        frameEnd_ = Mathf.Clamp(value, 0, player_.MaxFrames);
      }
    }

    public int MaxFrames
    {
      get
      {
        return ( player_.MaxFrames );
      }
    }

    public float FrameTime
    {
      get
      {
        return ( player_.FrameTime ); 
      }
    }

    public int FPS
    {
      get
      {
        return ( player_.FPS );
      }
    }

    private UnityEngine.Object MeshesPrefab
    {
      get
      {
        if (meshesPrefab_ == null)
        {
          string meshesPrefabPath = AssetDatabase.GenerateUniqueAssetPath(assetsPath_ + "/" + fxName_ +  "_meshes.prefab");
          meshesPrefab_ = PrefabUtility.CreateEmptyPrefab(meshesPrefabPath);
        }
        return meshesPrefab_;
      }
    }
    //----------------------------------------------------------------------------------
    public CRBakerGOAnim( CNManager manager, CREntityManager entityManager, CRPlayer player )
    {
      manager_       = manager;
      entityManager_ = entityManager;
      player_        = player;

      skinnedMeshCombiner_ = new CRMeshCombiner(true);
      frameMeshCombiner_   = new CRMeshCombiner(false);

      meshesPrefab_ = null;
      assetsPath_   = string.Empty;

      fxName_ = string.Empty;

      idBodyToRelativePath_       = new Dictionary<uint, string>();
      idBodyToBakedGO_            = new Dictionary<uint, GameObject>();
      idBodyToGOKeyframe_         = new Dictionary<uint, CRGOKeyframe>();
      idBodyToVisibilityInterval_ = new Dictionary<uint, Vector2>();

      listIdBodyTmp_ = new List<uint>();
      listGOTmp_     = new List<GameObject>();

      setBoneBodies_    = new HashSet<uint>();
      setVisibleBodies_ = new HashSet<uint>();
    }
    //----------------------------------------------------------------------------------
    public void BuildBakerInitData()
    {
      animationName_   = "default_take";
      bakeObjectName_  = manager_.FxData.name + "_baked";

      frameStart_ = 0;
      frameEnd_   = player_.MaxFrames;

      BuildListNodesForBaking();
    }
    //----------------------------------------------------------------------------------
    private void BuildListNodesForBaking()
    {
      manager_.GetListBodyNodesForBake( listBodyNode_ );

      int nBodyNodes = listBodyNode_.Count;

      bitArrNeedsBaking_     = new BitArray(nBodyNodes, true);
      bitArrNeedsCollapsing_ = new BitArray(nBodyNodes, true);

      for (int i = 0; i < nBodyNodes; i++ )
      {
        CNBody bodyNode = listBodyNode_[i];
        int nBodies = entityManager_.GetNumberOfBodiesFromBodyNode(bodyNode);

        bool isAnimated = bodyNode is CNAnimatedbody;

        bitArrNeedsCollapsing_[i] = (nBodies > 5) && !isAnimated;
      }  
    }
    //----------------------------------------------------------------------------------
    private List< Tuple2<CNBody, bool> > GetListBodyNodeToBake()
    {
      List< Tuple2<CNBody, bool> > listBodyNodesToBake = new List< Tuple2<CNBody, bool> >();
      int nBodies = listBodyNode_.Count;

      for (int i = 0; i < nBodies; i++)
      {
        CNBody bodyNode = listBodyNode_[i];

        bool needsBake  = bitArrNeedsBaking_[i];
        bool needsCollapsing = bitArrNeedsCollapsing_[i];

        if (needsBake)
        {
          if (bodyNode is CNRigidbody)
          {
            listBodyNodesToBake.Add( Tuple2.New(bodyNode, needsCollapsing) );
          }
          else
          {
            listBodyNodesToBake.Add( Tuple2.New(bodyNode, false) );
          }      
        }
      }

      return listBodyNodesToBake;
    }
    //----------------------------------------------------------------------------------
    public void ClearData()
    {
      skinnedMeshCombiner_.Clear();
      frameMeshCombiner_  .Clear();

      meshesPrefab_ = null;
      assetsPath_   = string.Empty;
      fxName_       = string.Empty;

      idBodyToRelativePath_      .Clear();
      idBodyToBakedGO_           .Clear();
      idBodyToGOKeyframe_        .Clear();
      idBodyToVisibilityInterval_.Clear();

      listIdBodyTmp_  .Clear();
      listGOTmp_      .Clear();

      setBoneBodies_   .Clear();
      setVisibleBodies_.Clear();
    }
    //----------------------------------------------------------------------------------
    public void BakeSimulationAsAnim()
    {
      string fxName = manager_.FxData.name;
      frameCount_      = Mathf.Abs( frameEnd_ - frameStart_ ) + 1;
      visibilityShift_ = FrameTime * frameStart_;

      List< Tuple2<CNBody, bool> > listBodyNodeToBake = GetListBodyNodeToBake();

      string folder;
      int pathIndex;
      bool assetsPath = DisplaySaveFolderDialog("Animation assets folder...", out folder, out pathIndex );
      if (!assetsPath)
      {
        return;
      }
     
      InitBake(folder, pathIndex, fxName);
      CreateRootGameObject(bakeObjectName_);
      CheckBodiesVisibility();   
      CreateBakedObjects(listBodyNodeToBake);   
      BakeAnimBinaryFile();
      SetStartData();
      FinishBake();   
    }
    //----------------------------------------------------------------------------------
    public void BakeCurrentFrame()
    {
      string fxName = manager_.FxData.name + "_frame_" + player_.Frame;
      List< Tuple2<CNBody, bool> > listBodyNodeToBake = GetListBodyNodeToBake();

      string folder;
      int pathIndex;
      bool assetsPath = DisplaySaveFolderDialog("Frame assets folder...", out folder, out pathIndex);
      if (!assetsPath)
      {
        return;
      }
     
      InitBake(folder, pathIndex, fxName);
      CreateRootGameObject(bakeObjectName_ + "_frame_" + player_.Frame);
      CreateFrameBakedObjects(listBodyNodeToBake);     
      FinishBake();
    }
    //----------------------------------------------------------------------------------
    private void InitBake(string folderPath, int pathIdx, string fxName)
    {
      ClearData();
      assetsPath_ = folderPath.Substring(pathIdx);
      fxName_ = fxName;
      SimulationManager.SetBroadcastMode( UN_BROADCAST_MODE.BAKING );  
      SimulationManager.PauseHotOn();
    }
    //----------------------------------------------------------------------------------
    private void FinishBake()
    {      
      SimulationManager.PauseOn();
      SimulationManager.SetBroadcastMode( UN_BROADCAST_MODE.SIMULATING );
      ClearData();
      EditorGUIUtility.PingObject(rootGameObject_);
      Selection.activeGameObject = rootGameObject_;
    }      
    //----------------------------------------------------------------------------------
    private bool DisplaySaveFolderDialog(string saveText, out string folderPath, out int pathIndex  )
    {
      folderPath = EditorUtility.SaveFolderPanel(saveText, "Assets/", "");
      if (folderPath == string.Empty)
      {
        pathIndex = -1;
        return false;
      }

      pathIndex = folderPath.IndexOf("Assets");
      while (folderPath.Length == 0 || pathIndex == -1)
      {
        bool ok = EditorUtility.DisplayDialog("CaronteFX - Invalid path", "Target folder must be inside project Assets. Please, select a proper folder.", "Ok", "Cancel");
        if ( ok )
        {
          folderPath = EditorUtility.SaveFolderPanel(saveText, "Assets/", "");
          pathIndex  = folderPath.IndexOf("Assets");
        }
        else
        {
          return false;
        }   
      }
      return true;
    }
    //----------------------------------------------------------------------------------
    private void CreateRootGameObject(string bakeObjectName)
    {
      string uniqueName = CREditorUtils.GetUniqueGameObjectName(bakeObjectName);
      rootGameObject_   = new GameObject(uniqueName);
    }
    //----------------------------------------------------------------------------------
    private void CreateBakedObjects(List< Tuple2<CNBody, bool> > listBodyNodeToBake)
    {
      listIdBodyTmp_.Clear();
      int nBodyNodes = listBodyNodeToBake.Count;

      for (int i = 0; i < nBodyNodes; i++)
      {
        Tuple2<CNBody, bool> tupleBodyNodeNeedsCollapsing_ = listBodyNodeToBake[i]; 

        CNBody bodyNode      = tupleBodyNodeNeedsCollapsing_.First;
        bool needsCollapsing = tupleBodyNodeNeedsCollapsing_.Second;

        entityManager_.BuildListBodyIdFromBodyNode( bodyNode, listIdBodyTmp_ );

        if (listIdBodyTmp_.Count > 0)
        {
          string bodyNodeName = i + ": " + bodyNode.Name;

          GameObject nodeGO = new GameObject(bodyNodeName);
          nodeGO.transform.parent = rootGameObject_.transform;

          if (!needsCollapsing)
          {
            CreateBakedGameObjects(bodyNodeName, bodyNode, listIdBodyTmp_, nodeGO);
          }
          else
          {
            CreateSkinnedGameObjects(bodyNodeName, bodyNode, listIdBodyTmp_, nodeGO);
          }
        }
      }

      InitKeyframeData();
    }
    //----------------------------------------------------------------------------------
    private void CreateFrameBakedObjects(List< Tuple2<CNBody, bool> > listBodyNodeToBake)
    {
      listIdBodyTmp_.Clear();
      int nBodyNodes = listBodyNodeToBake.Count;

      List<uint> listIdBodyTotal = new List<uint>();

      for (int i = 0; i < nBodyNodes; i++)
      {
        Tuple2<CNBody, bool> tupleBodyNodeNeedsCollapsing_ = listBodyNodeToBake[i]; 

        CNBody bodyNode = tupleBodyNodeNeedsCollapsing_.First;
        entityManager_.BuildListBodyIdFromBodyNode( bodyNode, listIdBodyTmp_ );
        listIdBodyTotal.AddRange(listIdBodyTmp_);
      }

      Matrix4x4 m_WORLD_to_LOCALCOMBINED = CalculateWorldToLocalMatrixSimulating(listIdBodyTotal);
      Vector3 position = -( m_WORLD_to_LOCALCOMBINED.GetColumn(3) );

      frameMeshCombiner_.SetWorldToLocalClearingInfo( m_WORLD_to_LOCALCOMBINED );

      for (int i = 0; i < nBodyNodes; i++)
      {
        Tuple2<CNBody, bool> tupleBodyNodeNeedsCollapsing_ = listBodyNodeToBake[i]; 

        CNBody bodyNode = tupleBodyNodeNeedsCollapsing_.First;

        entityManager_.BuildListBodyIdFromBodyNode( bodyNode, listIdBodyTmp_ );

        if (listIdBodyTmp_.Count > 0)
        {
          if (combineMeshesInFrame_)
          {
            CreateFrameBakedGameObjectsCombined(listIdBodyTmp_, position);
          }
          else
          {
            string bodyNodeName = bodyNode.Name;
            GameObject nodeGO = new GameObject("snapshot_" + player_.Frame + "_" + bodyNodeName);

            nodeGO.transform.parent = rootGameObject_.transform;
            CreateFrameBakedGameObjects(listIdBodyTmp_, nodeGO);
          }        
        }
      }

      if ( frameMeshCombiner_.CanGenerateMesh() )
      {
        GenerateCombinedGameObject(position);
      }

      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh(); 
    }
    //----------------------------------------------------------------------------------
    private void CreateBakedGameObjects(string bodyNodeName, CNBody bodyNode, List<uint> listBodyId, GameObject nodeGO)
    {
      foreach( var idBody in listBodyId )
      { 
        GameObject originalGO = entityManager_.GetGOFromIdBody(idBody);

        if (!setVisibleBodies_.Contains(idBody) || originalGO == null ) 
        {
          continue;
        }

        GameObject bakedGO = (GameObject)UnityEngine.Object.Instantiate(originalGO);
        SetBakedGameObject( bakedGO, originalGO, nodeGO, idBody, listGOTmp_ );

        if ( bakedGO.HasMesh() )
        {
          bool isRope = entityManager_.IsRope(idBody);
          if ( isRope )
          {
            Tuple2<Mesh, Vector3> ropeInit = entityManager_.GetRopeInit(idBody);
            Vector3 center = ropeInit.Second;
            CREditorUtils.SetMesh(bakedGO, ropeInit.First);

            bakedGO.transform.localPosition = center;
            bakedGO.transform.localRotation = Quaternion.identity;
            bakedGO.transform.localScale    = Vector3.one;

            AssetDatabase.AddObjectToAsset(ropeInit.First, MeshesPrefab);
          }
          else
          {
            Mesh mesh = bakedGO.GetMesh();
            bool isInProject = AssetDatabase.Contains( mesh.GetInstanceID() );
            if (!isInProject)
            {
              Mesh newMesh = UnityEngine.Object.Instantiate(mesh);
              newMesh.name = mesh.name;
              CREditorUtils.SetMesh(bakedGO, newMesh);
              AssetDatabase.AddObjectToAsset(newMesh, MeshesPrefab);
            }
          }

        }

        string relativePath = bodyNodeName + "/" + bakedGO.name;
        idBodyToRelativePath_.Add(idBody, relativePath);
        idBodyToBakedGO_     .Add(idBody, bakedGO);
      }
    }
    //----------------------------------------------------------------------------------
    private void CreateSkinnedGameObjects( string bodyNodeName, CNBody bodyNode, List<uint> listBodyId, GameObject nodeGO )
    { 
      Matrix4x4 m_WORLD_to_LOCALCOMBINED = CalculateWorldToLocalMatrix(listBodyId);
      skinnedMeshCombiner_.SetWorldToLocalClearingInfo( m_WORLD_to_LOCALCOMBINED );

      GameObject bonesGO = new GameObject(nodeGO.name + "_bones"); 
      bonesGO.transform.parent = nodeGO.transform;

      int skinnedIdx = 0;

      foreach( var idBody in listBodyId )
      {                 
        GameObject originalGO = entityManager_.GetGOFromIdBody(idBody);

        if (!setVisibleBodies_.Contains(idBody) || originalGO == null ) 
        {
          continue;
        }

        GameObject bakedBone  = new GameObject();
        SetBakedBoneGameObject( bakedBone, originalGO, nodeGO, idBody );

        bakedBone.transform.parent = bonesGO.transform;

        if (!skinnedMeshCombiner_.CanAddGameObject( originalGO ) )
        {
          GenerateSkinGameObject(nodeGO, skinnedIdx);
          skinnedIdx++;
        }
        skinnedMeshCombiner_.AddGameObject( originalGO, bakedBone );
        string relativePath = bodyNodeName + "/" + bonesGO.name + "/" + bakedBone.name;

        idBodyToRelativePath_.Add(idBody, relativePath);
        idBodyToBakedGO_     .Add(idBody, bakedBone);

        setBoneBodies_.Add(idBody);
      }

      if( skinnedMeshCombiner_.CanGenerateMesh() )
      {
        GenerateSkinGameObject(nodeGO, skinnedIdx );
      } 
    }
    //----------------------------------------------------------------------------------
    private void GenerateSkinGameObject(GameObject nodeGO, int skinnedIdx)
    {
      Material[] arrMaterial;
      Transform[] arrBone;

      Mesh skinnedMesh = skinnedMeshCombiner_.GenerateMesh(out arrMaterial, out arrBone);
      skinnedMesh.name = nodeGO.name + "_" + skinnedIdx;

      GameObject go = new GameObject(nodeGO.name + "_skinned_" + skinnedIdx); 
      go.transform.parent = nodeGO.transform;

      SkinnedMeshRenderer smr = go.AddComponent<SkinnedMeshRenderer>();

      smr.bones               = arrBone;
      smr.sharedMesh          = skinnedMesh;   
      smr.sharedMaterials     = arrMaterial;
      smr.updateWhenOffscreen = true;

      Bounds worldBounds    = smr.bounds;
      go.transform.position = worldBounds.center;

      Mesh newMesh = UnityEngine.Object.Instantiate(skinnedMesh);
      newMesh.name = skinnedMesh.name;

      CREditorUtils.SetMesh(go, newMesh);
      AssetDatabase.AddObjectToAsset(newMesh, MeshesPrefab);
      
      skinnedMeshCombiner_.Clear();
    }
    //----------------------------------------------------------------------------------
    private void CreateFrameBakedGameObjects(List<uint> listBodyId, GameObject nodeGO)
    {
      foreach( var idBody in listBodyId )
      { 
        Transform simulatingTr = entityManager_.GetBodyTransformRef(idBody);

        GameObject simulatingGO = simulatingTr.gameObject;
        if (simulatingGO.activeSelf)
        {
          GameObject bakedGO  = (GameObject)UnityEngine.Object.Instantiate(simulatingGO);

          bakedGO.name = "snapshot_" + player_.Frame + "_" + simulatingGO.name;
          bakedGO.hideFlags = HideFlags.None;
          Caronte_Fx_Body fxBody = bakedGO.GetComponent<Caronte_Fx_Body>();
          if (fxBody != null)
          {
            UnityEngine.Object.DestroyImmediate(fxBody);
          }

          bakedGO.transform.parent = nodeGO.transform;
          if ( bakedGO.HasMesh() )
          {
            Mesh mesh = bakedGO.GetMesh();
            bool isInProject = AssetDatabase.Contains( mesh.GetInstanceID() );
            
            if ( entityManager_.IsRope(idBody) )
            {
              Tuple2<Mesh, MeshUpdater> meshUpdater = entityManager_.GetBodyMeshRenderUpdaterRef(idBody);
              Mesh newMesh = UnityEngine.Object.Instantiate(meshUpdater.First);
              CREditorUtils.SetMesh(bakedGO, newMesh);
              AssetDatabase.AddObjectToAsset(newMesh, MeshesPrefab);
            }
            else if (!isInProject)
            {
              Mesh newMesh = UnityEngine.Object.Instantiate(mesh);
              newMesh.name = mesh.name;
              newMesh.hideFlags = HideFlags.None;
              CREditorUtils.SetMesh(bakedGO, newMesh);
              AssetDatabase.AddObjectToAsset(newMesh, MeshesPrefab);
            }
          }
        }

      }
    }
    //----------------------------------------------------------------------------------
    private void CreateFrameBakedGameObjectsCombined(List<uint> listBodyId, Vector3 position)
    {
      foreach( var idBody in listBodyId )
      { 
        Transform simulatingTr  = entityManager_.GetBodyTransformRef(idBody);
        GameObject simulatingGO = simulatingTr.gameObject;

        if (simulatingGO.activeSelf)
        {
          if (!frameMeshCombiner_.CanAddGameObject( simulatingGO ) )
          {
            GenerateCombinedGameObject(position);
          }

          frameMeshCombiner_.AddGameObject( simulatingGO );
        }
      }
    }
    //----------------------------------------------------------------------------------
    private void GenerateCombinedGameObject(Vector3 position)
    {
      Material[] arrMaterial;;

      Mesh mesh = frameMeshCombiner_.GenerateMesh(out arrMaterial);
      mesh.name = fxName_;

      GameObject go = new GameObject(fxName_ + "_combined"); 
      go.transform.parent = rootGameObject_.transform;

      MeshFilter mf = go.AddComponent<MeshFilter>();
      mf.sharedMesh = mesh;

      MeshRenderer mr = go.AddComponent<MeshRenderer>();  
      mr.sharedMaterials = arrMaterial;
 
      go.transform.position = position;

      AssetDatabase.AddObjectToAsset(mesh, MeshesPrefab);
      
      frameMeshCombiner_.Clear();
    }
    //----------------------------------------------------------------------------------
    private Matrix4x4 CalculateWorldToLocalMatrix(List<uint> listBodyId)
    {
      Bounds box_WORLD = new Bounds();
      int nBodies = listBodyId.Count;

      for( int a = 0; a < nBodies; a++ )
      {        
        uint idBody = listBodyId[a];
        GameObject originalGO = entityManager_.GetGOFromIdBody(idBody);

        if ( originalGO == null )
        {
          continue;
        }

        Mesh mesh = originalGO.GetMesh();

        if ( mesh == null )
        {
          continue;
        }

        Renderer rd = originalGO.GetComponent<Renderer>();
        if ( rd == null )
        {
          continue;
        }

        Matrix4x4 m_MODEL_to_WORLD = originalGO.transform.localToWorldMatrix;
        Bounds bounds_world = new Bounds();
        CRGeometryUtils.CreateBoundsTransformed( mesh.bounds, m_MODEL_to_WORLD, out bounds_world );

        if (a == 0)
        {
          box_WORLD = bounds_world;
        }
        else
        {
          box_WORLD.Encapsulate( bounds_world );
        }
      }

      Matrix4x4 matrix_WORLD_to_LOCAL = new Matrix4x4();
      matrix_WORLD_to_LOCAL.SetTRS( -box_WORLD.center, Quaternion.identity, Vector3.one );

      return matrix_WORLD_to_LOCAL;
    }
    //----------------------------------------------------------------------------------
    private Matrix4x4 CalculateWorldToLocalMatrixSimulating(List<uint> listBodyId)
    {
      Bounds box_WORLD = new Bounds();
      int nBodies = listBodyId.Count;

      for( int a = 0; a < nBodies; a++ )
      {        
        uint idBody = listBodyId[a];
        Transform simulatingTr = entityManager_.GetBodyTransformRef(idBody);
        if ( simulatingTr == null )
        {
          continue;
        }

        GameObject simulatingGO = simulatingTr.gameObject;
        Mesh mesh = simulatingGO.GetMesh();

        if ( mesh == null )
        {
          continue;
        }

        Renderer rd = simulatingGO.GetComponent<Renderer>();
        if ( rd == null )
        {
          continue;
        }

        Bounds bounds_world = rd.bounds;

        if (a == 0)
        {
          box_WORLD = bounds_world;
        }
        else
        {
          box_WORLD.Encapsulate( bounds_world );
        }
      }

      Matrix4x4 matrix_WORLD_to_LOCAL = new Matrix4x4();
      matrix_WORLD_to_LOCAL.SetTRS( -box_WORLD.center, Quaternion.identity, Vector3.one );

      return matrix_WORLD_to_LOCAL;
    }
    //----------------------------------------------------------------------------------
    private void InitKeyframeData()
    {
      SimulationManager.SetReplayingFrame( (uint)frameStart_, false );
      bool read = false;
      do
      {
        System.Threading.Thread.Sleep( 1 );
        read = SimulationManager.ReadSimulationBufferUniqueUnsafe( BroadcastStartDelegate, InitKeyFrame );
      } while (!read);
    }
    //----------------------------------------------------------------------------------
    private void PositionInFrame(int frame)
    {
      SimulationManager.SetReplayingFrame( (uint)frame, false );        
      bool read = false;
      do
      {
        System.Threading.Thread.Sleep( 1 );
        read = SimulationManager.ReadSimulationBufferUniqueUnsafe( BroadcastStartDelegate, BakeBodyKeyFrame );
      } while (!read);
    }
    //----------------------------------------------------------------------------------
    private void CheckVisibility(int frame)
    {
      SimulationManager.SetReplayingFrame( (uint)frame, false );
      bool read = false;
      do
      {
        System.Threading.Thread.Sleep( 1 );
        read = SimulationManager.ReadSimulationBufferUniqueUnsafe( BroadcastStartDelegate, CheckBodyVisibility );
      } while (!read);
    }
    //----------------------------------------------------------------------------------
    private void BroadcastStartDelegate( bool doDisplayInvisibleBodies )
    {

    }
    //----------------------------------------------------------------------------------
    private void SetBakedGameObject( GameObject bakedGO, GameObject originalGO, GameObject nodeGO, uint idBody, List<GameObject> listGameObjectToDestroy )
    {
      listGameObjectToDestroy.Clear();
      entityManager_.GetRemoveBodyChildrenList( originalGO, bakedGO, listGameObjectToDestroy );
      foreach( GameObject go in listGameObjectToDestroy )
      {
        UnityEngine.Object.DestroyImmediate(go);
      }
      listGameObjectToDestroy.Clear();

      bakedGO.name = idBody + ": " + originalGO.name;

      bakedGO.transform.parent        = originalGO.transform.parent;
      bakedGO.transform.localPosition = originalGO.transform.localPosition;
      bakedGO.transform.localRotation = originalGO.transform.localRotation;
      bakedGO.transform.localScale    = originalGO.transform.localScale;

      bakedGO.transform.parent = nodeGO.transform;

      CREditorUtils.ReplaceSkinnedMeshRenderer( bakedGO );

      Transform simTransform = entityManager_.GetBodyTransformRef(idBody);
      bakedGO.SetActive( simTransform.gameObject.activeInHierarchy );

      Caronte_Fx_Body cfxBody = bakedGO.GetComponent<Caronte_Fx_Body>();
      if (cfxBody != null)
      {
        UnityEngine.Object.DestroyImmediate( cfxBody );
      }
    }
    //----------------------------------------------------------------------------------
    private void SetBakedBoneGameObject( GameObject bakedGO, GameObject originalGO, GameObject nodeGO, uint idBody )
    {
      bakedGO.name = idBody + ": " + originalGO.name;

      bakedGO.transform.parent = originalGO.transform.parent;

      bakedGO.transform.localPosition = originalGO.transform.localPosition;
      bakedGO.transform.localRotation = originalGO.transform.localRotation;
      bakedGO.transform.localScale    = originalGO.transform.localScale;

      bakedGO.transform.parent     = nodeGO.transform;
      bakedGO.transform.localScale = Vector3.one;
    }
    //----------------------------------------------------------------------------------
    private void CheckBodiesVisibility()
    {
      int updateFramesDelta = Mathf.Max( frameCount_ / 5, 5);

      for (bakingFrame_ = frameStart_; bakingFrame_ <= frameEnd_; bakingFrame_++ )
      {
        frame_ = bakingFrame_ - frameStart_; 
   
        if ( (frame_ % updateFramesDelta) == 0 )
        {
          float progress = (float) frame_ / (float) frameCount_;
          string progressInfo = "Frame " + frame_ + " of " + frameCount_ + "."; 
          EditorUtility.DisplayProgressBar("CaronteFx - Checking visibility. ", progressInfo, progress );
        }

        CheckVisibility(bakingFrame_);
      }
    }
    //----------------------------------------------------------------------------------
    private void BakeAnimBinaryFile()
    { 
      int nGameObjects = idBodyToGOKeyframe_.Count;

      MemoryStream ms = new MemoryStream();
      if ( ms != null )
      {
        BinaryWriter bw = new BinaryWriter(ms);
        if (bw != null)
        {
          bw.Write(binaryVersion_);
          bw.Write(sbCompression_);
          bw.Write(sbTangents_);
          bw.Write(frameCount_);
          bw.Write(FPS);
          bw.Write(nGameObjects);

          Dictionary<uint, int> idBodyToIdGameObjectInFile = new Dictionary<uint, int>();
          List<CRGOKeyframe> listGOKeyframe = new List<CRGOKeyframe>();

          foreach (var element in idBodyToGOKeyframe_)
          {
            uint idBody = element.Key;
            CRGOKeyframe goKeyframe = element.Value;

            idBodyToIdGameObjectInFile.Add(idBody, listGOKeyframe.Count);

            listGOKeyframe.Add(goKeyframe);

            string pathRelativeTo       = idBodyToRelativePath_[idBody];
            int vertexCount             = goKeyframe.VertexCount;
            Vector2 visibleTimeInterval = idBodyToVisibilityInterval_[idBody];

            bw.Write(pathRelativeTo);
            bw.Write(vertexCount);
            bw.Write(visibleTimeInterval.x);
            bw.Write(visibleTimeInterval.y);
          }

          Dictionary<uint, CNContactEmitter> tableIdToContactEmitter = entityManager_.GetTableIdToContactEmitter();
          bw.Write( tableIdToContactEmitter.Count );

          Dictionary<uint, int> idContactEmitterToIdEmitterInFile = new Dictionary<uint, int>();
          List<CNContactEmitter> listContactEmitter = new List<CNContactEmitter>();

          foreach (var pair in tableIdToContactEmitter)
          {
            uint idContactEmitter   = pair.Key;
            CNContactEmitter ceNode = pair.Value;

            idContactEmitterToIdEmitterInFile.Add(idContactEmitter, listContactEmitter.Count);
            listContactEmitter.Add(ceNode);
            string contactEmitterName = ceNode.Name;

            bw.Write(contactEmitterName);
          }

          long[] fOffsets = new long[frameCount_];
          long fOffsetsP = ms.Position;
          for (int i = 0; i < frameCount_; i++)
          {
            long val = 0;
            bw.Write(val);
          }

          int updateFramesDelta = Mathf.Max( frameCount_ / 5, 5);

          for (bakingFrame_ = frameStart_; bakingFrame_ <= frameEnd_; bakingFrame_++ )
          {
            int frame = bakingFrame_ - frameStart_; 

            if( (frame % updateFramesDelta) == 0 )
            {        
              float progress = (float)frame / (float)frameCount_;
              string progressInfo = "Baking to " + rootGameObject_.name + ". " + "Frame " + frame + " of " + frameCount_ + "."; 
              EditorUtility.DisplayProgressBar("CaronteFx - Baking animation Clip (BinaryFile)", progressInfo, progress);
            }

            PositionInFrame( bakingFrame_ );

            fOffsets[frame] = ms.Position;
            for (int i = 0; i < nGameObjects; i++)
            {
              CRGOKeyframe goKeyframe = listGOKeyframe[i];

              BROADCASTFLAG flags = goKeyframe.GetFrameFlags();  
              bw.Write( (byte)flags );
              
              bool isVisible = ( flags & BROADCASTFLAG.VISIBLE ) == BROADCASTFLAG.VISIBLE;
              bool isGhost   = ( flags & BROADCASTFLAG.GHOST )   == BROADCASTFLAG.GHOST;

              if (isVisible || isGhost)
              {
                Vector3 position = goKeyframe.GetPositionInFrame();

                bw.Write(position.x);
                bw.Write(position.y);
                bw.Write(position.z);

                Quaternion rotation = goKeyframe.GetRotationInFrame();

                bw.Write(rotation.x);
                bw.Write(rotation.y);
                bw.Write(rotation.z);
                bw.Write(rotation.w);

                Bounds? nullableBounds = goKeyframe.GetBoundsInFrame();
                if (nullableBounds != null)
                {
                  WriteMesh(nullableBounds, goKeyframe, ms, bw);
                }
              }
            }
            
            WriteFrameContactEvents(idBodyToIdGameObjectInFile, idContactEmitterToIdEmitterInFile, ms, bw);
          }

          ms.Position = fOffsetsP;
          for (int i = 0; i < frameCount_; i++)
          {
            bw.Write(fOffsets[i]);
          }

          EditorUtility.ClearProgressBar();

          CreateAssetAndAnimationComponent(ms);

          bw.Close();
          ms.Close();
        }
      }   
    }
    //----------------------------------------------------------------------------------
    private void WriteMesh( Bounds? nullableBounds, CRGOKeyframe goKeyframe, MemoryStream ms, BinaryWriter bw )
    {
      Bounds bounds = (Bounds)nullableBounds;
                  
      Vector3 boundsMin = bounds.min;
      Vector3 boundsMax = bounds.max;

      bw.Write(boundsMin.x);
      bw.Write(boundsMin.y);
      bw.Write(boundsMin.z);

      bw.Write(boundsMax.x);
      bw.Write(boundsMax.y);
      bw.Write(boundsMax.z);

      Vector3 boundsSize = bounds.size;

      List<Vector3> listVertexPos = goKeyframe.GetVertexesPosInFrame();
      List<Vector3> listVertexNor = goKeyframe.GetVertexesNorInFrame();

      if (sbCompression_)
      {
        for (int j = 0; j < listVertexPos.Count; j++)
        {
          Vector3 vertexPos = listVertexPos[j];

          short cpos = (short)(((vertexPos.x - boundsMin.x) / boundsSize.x) * 65535.0f);
          bw.Write(cpos);

          cpos = (short)(((vertexPos.y - boundsMin.y) / boundsSize.y) * 65535.0f);
          bw.Write(cpos);

          cpos = (short)(((vertexPos.z - boundsMin.z) / boundsSize.z) * 65535.0f);
          bw.Write(cpos);
        }

        for (int j = 0; j < listVertexNor.Count; j++)
        {
          Vector3 vertexNor = listVertexNor[j];

          sbyte cnor = (sbyte)(vertexNor.x * 127.0f);
          bw.Write(cnor);

          cnor = (sbyte)(vertexNor.y * 127.0f);
          bw.Write(cnor);

          cnor = (sbyte)(vertexNor.z * 127.0f);
          bw.Write(cnor);
        }

        if (sbTangents_)
        {
          List<Vector4> listVertexTan = goKeyframe.GetVertexesTanInFrame();

          for (int j = 0; j < listVertexNor.Count; j++)
          {
            Vector4 vertexTan = listVertexTan[j];

            sbyte ctan = (sbyte)(vertexTan.x * 127.0f);
            bw.Write(ctan);
            ctan = (sbyte)(vertexTan.y * 127.0f);
            bw.Write(ctan);
            ctan = (sbyte)(vertexTan.z * 127.0f);
            bw.Write(ctan);
            ctan = (sbyte)(vertexTan.w * 127.0f);
            bw.Write(ctan);
          }
        }
      }
      else
      {
        for (int j = 0; j < listVertexPos.Count; j++)
        {
          Vector3 vertexPos = listVertexPos[j];

          bw.Write(vertexPos.x);
          bw.Write(vertexPos.y);
          bw.Write(vertexPos.z);
        }

        for (int j = 0; j < listVertexNor.Count; j++)
        {
          Vector3 vertexNor = listVertexNor[j];

          bw.Write(vertexNor.x);
          bw.Write(vertexNor.y);
          bw.Write(vertexNor.z);
        }

        if (sbTangents_)
        {
          List<Vector4> listVertexTan = goKeyframe.GetVertexesTanInFrame();

          for (int j = 0; j < listVertexNor.Count; j++)
          {
            Vector4 vertexTan = listVertexTan[j];

            bw.Write(vertexTan.x);
            bw.Write(vertexTan.y);
            bw.Write(vertexTan.z);
            bw.Write(vertexTan.w);
          }
        }
      }
    }
    //----------------------------------------------------------------------------------
    private void WriteFrameContactEvents(Dictionary<uint, int> idBodyToIdGameObjectInFile, Dictionary<uint, int> idContactEmitterToIdEmitterInFile, MemoryStream ms, BinaryWriter bw)
    {
      int nFrameEvents = 0;
      long nFrameEventsPos = ms.Position;
      bw.Write(nFrameEvents);
      
      if (bakeEvents_)
      {
        List<CaronteSharp.ContactEventInfo> listFrameEventInfo = SimulationManager.listContactEventInfo_;

        foreach( CaronteSharp.ContactEventInfo evInfo in listFrameEventInfo )
        {
          uint idBodyA = evInfo.a_bodyId_;
          uint idBodyB = evInfo.b_bodyId_;
            
          if ( idBodyToIdGameObjectInFile.ContainsKey(idBodyA) &&
                idBodyToIdGameObjectInFile.ContainsKey(idBodyB) )
          {
            uint idEntity = evInfo.idEntity_;
            int idEmitterInFile = idContactEmitterToIdEmitterInFile[idEntity];
            bw.Write(idEmitterInFile);
            int idGameObjectInFileA = idBodyToIdGameObjectInFile[idBodyA];
            int idGameObjectInFileB = idBodyToIdGameObjectInFile[idBodyB];
            bw.Write(idGameObjectInFileA);
            bw.Write(idGameObjectInFileB);
          }
          else
          {
            continue;
          }
          bw.Write(evInfo.position_.x);
          bw.Write(evInfo.position_.y);
          bw.Write(evInfo.position_.z);

          bw.Write(evInfo.a_v_.x);
          bw.Write(evInfo.a_v_.y);
          bw.Write(evInfo.a_v_.z);

          bw.Write(evInfo.b_v_.x);
          bw.Write(evInfo.b_v_.y);
          bw.Write(evInfo.b_v_.z);

          bw.Write(evInfo.relativeSpeed_N_);
          bw.Write(evInfo.relativeSpeed_T_);

          bw.Write(evInfo.relativeP_N_);
          bw.Write(evInfo.relativeP_T_);

          nFrameEvents++;
        }
      }

      long currentPos = ms.Position;
      ms.Position = nFrameEventsPos;
      bw.Write(nFrameEvents);
      ms.Position = currentPos;
    }
    //----------------------------------------------------------------------------------
    private void CreateAssetAndAnimationComponent( MemoryStream ms )
    {
      //string cacheFilePath = AssetDatabase.GenerateUniqueAssetPath( folderPath + "/" + animationName_ + "_tmp" + ".cra" );
      CRAnimationAsset animationAsset = CRAnimationAsset.CreateInstance<CRAnimationAsset>();
      animationAsset.bytes = ms.ToArray();

      string assetFilePath = AssetDatabase.GenerateUniqueAssetPath( assetsPath_ + "/" + animationName_ + ".asset");
      AssetDatabase.CreateAsset( animationAsset, assetFilePath );

      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh(); 

      CRAnimation crAnimation = rootGameObject_.GetComponent<CRAnimation>();
      if ( crAnimation == null )
      {
        crAnimation = rootGameObject_.AddComponent<CRAnimation>();
      }

      crAnimation.activeAnimation = animationAsset;
      crAnimation.listAnimations.Add(animationAsset);
    }
    //----------------------------------------------------------------------------------
    public void InitKeyFrame(BD_TYPE type, BodyInfo bodyInfo)
    {
      uint idBody = bodyInfo.idBody_;
      if  ( idBodyToBakedGO_.ContainsKey(idBody) )
      {     
        GameObject go = idBodyToBakedGO_[idBody];
        
        int nVertexCount = 0;

        if ( type == BD_TYPE.BODYMESH_ANIMATED_BY_VERTEX ||
             type == BD_TYPE.SOFTBODY || 
             type == BD_TYPE.CLOTH )
        {
          Mesh mesh = go.GetMesh();
          nVertexCount = mesh.vertexCount;
        }

        Vector2 visibleTimeInterval = entityManager_.GetVisibleTimeInterval( idBody );
        visibleTimeInterval.x -= visibilityShift_;
        visibleTimeInterval.y -= visibilityShift_;

        idBodyToVisibilityInterval_.Add( idBody, visibleTimeInterval );
        idBodyToGOKeyframe_.Add( idBody, new CRGOKeyframe(nVertexCount) );
      }
    }
    //----------------------------------------------------------------------------------
    public void BakeBodyKeyFrame( BD_TYPE type, BodyInfo bodyInfo )
    {
      uint idBody = bodyInfo.idBody_;
      if ( idBodyToGOKeyframe_.ContainsKey(idBody) )
      {
        CRGOKeyframe goKeyframe = idBodyToGOKeyframe_[idBody];

        switch (type)
        {
          case BD_TYPE.RIGIDBODY:
          case BD_TYPE.BODYMESH_ANIMATED_BY_MATRIX:
            {
              RigidBodyInfo rbInfo = bodyInfo as RigidBodyInfo;
              goKeyframe.SetBodyKeyframe( rbInfo.broadcastFlag_, rbInfo.position_, rbInfo.orientation_ );
              break;
            }

          case BD_TYPE.BODYMESH_ANIMATED_BY_VERTEX:
            {
              BodyMeshInfo bodymeshInfo = bodyInfo as BodyMeshInfo;
              bool isVisibleorGhost = goKeyframe.SetBodyKeyframe( bodymeshInfo.broadcastFlag_, bodymeshInfo.position_, bodymeshInfo.orientation_ );
              
              if (isVisibleorGhost)
              {
                Bounds bounds;
                Vector3[] arrNormal;
                Vector4[] arrTangent;
                CalculateMeshData( bodymeshInfo.idBody_, bodymeshInfo.arrVertices_, out arrNormal, out arrTangent, out bounds );
                goKeyframe.SetVertexKeyframe( bodymeshInfo.arrVertices_, arrNormal, arrTangent, bounds );
              }
              break;
            }

          case BD_TYPE.SOFTBODY:
          case BD_TYPE.CLOTH:
            {
              SoftBodyInfo sbInfo = (SoftBodyInfo)bodyInfo;
              bool isVisibleorGhost = goKeyframe.SetBodyKeyframe( sbInfo.broadcastFlag_, sbInfo.center_, Quaternion.identity );
              
              if (isVisibleorGhost)
              {
                Bounds bounds;
                Vector3[] arrNormal;
                Vector4[] arrTangent;
                CalculateMeshData( sbInfo.idBody_, sbInfo.arrVerticesRender_, out arrNormal, out arrTangent, out bounds );    
                goKeyframe.SetVertexKeyframe( sbInfo.arrVerticesRender_, arrNormal, arrTangent, bounds );
              }    
              break;
            }
        }
      }
    }
    //----------------------------------------------------------------------------------
    private void CheckBodyVisibility(BD_TYPE type, BodyInfo bodyInfo)
    {
      uint idBody = bodyInfo.idBody_;
      bool isVisible = ( bodyInfo.broadcastFlag_ & BROADCASTFLAG.VISIBLE ) == BROADCASTFLAG.VISIBLE;
      bool isGhost   = ( bodyInfo.broadcastFlag_ & BROADCASTFLAG.GHOST ) == BROADCASTFLAG.GHOST;

      if (isVisible || isGhost)
      {
        setVisibleBodies_.Add(idBody);
      }
    }
    //----------------------------------------------------------------------------------
    private void SetStartData()
    {
      PositionInFrame(frameStart_);

      foreach( var pair in idBodyToGOKeyframe_ )
      {
        uint idBody = pair.Key;
        CRGOKeyframe goKeyframe = pair.Value;

        GameObject go = idBodyToBakedGO_[idBody];
        BROADCASTFLAG broadcastFlag = goKeyframe.GetFrameFlags();

        if ( setBoneBodies_.Contains(idBody) )
        {
          bool isVisible = (broadcastFlag & BROADCASTFLAG.VISIBLE) == BROADCASTFLAG.VISIBLE;
          if (!isVisible)
          {
            go.transform.localScale = Vector3.zero;
          }
        }
        else
        {
          bool isVisible = ( broadcastFlag & BROADCASTFLAG.VISIBLE ) == BROADCASTFLAG.VISIBLE;
          go.SetActive( isVisible );
        }
      }
    }
    //----------------------------------------------------------------------------------
    private void CalculateMeshData( uint idBody, Vector3[] arrVertices, out Vector3[] arrNormal, out Vector4[] arrTangent, out Bounds bounds )
    {
      Tuple2<UnityEngine.Mesh, MeshUpdater> meshData = entityManager_.GetBodyMeshRenderUpdaterRef(idBody);

      UnityEngine.Mesh mesh   = meshData.First;
      MeshUpdater meshUpdater = meshData.Second;

      mesh.vertices = arrVertices;
      meshComplexForUpdate_.SetForUpdate( mesh );

      CaronteSharp.Tools.UpdateVertexNormalsAndTangents( meshUpdater, meshComplexForUpdate_ );
      
      mesh.normals  = meshComplexForUpdate_.arrNormal_;
      mesh.tangents = meshComplexForUpdate_.arrTan_;

      mesh.RecalculateBounds();

      arrNormal   = meshComplexForUpdate_.arrNormal_;
      arrTangent  = meshComplexForUpdate_.arrTan_;
      bounds      = mesh.bounds;
    }
  }
}