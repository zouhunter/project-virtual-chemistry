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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using CaronteSharp;

using Object = UnityEngine.Object;


namespace CaronteFX
{
  /// <summary>
  /// <para>Main class for the plugin management</para>
  /// </summary>
  public class CNManager
  {
    /**********************************************************************************************/
    /* Public types and data                                                                      */
    /**********************************************************************************************/

    #region Public types and data

    public Caronte_Fx FxData
    {
      get { return fxData_; }
      set
      {
        fxData_ = value;
        if (fxData_ != null)
        {
          if (fxData_.effect == null)
          {
            fxData_.e_addEffect();
            EditorUtility.SetDirty(fxData_);
          }
          Init();
        }
      }
    }

    public int  CurrentFxIdx
    {
      get { return currentFxIdx_; }
      set { currentFxIdx_ = value; }
    }

    public CREffectData EffectData
    {
      get { return FxData.effect; }
    }

    public CNGroup RootNode
    {
      get { return EffectData.rootNode_; }
    }

    public CNGroup SubeffectsNode
    {
      get { return EffectData.subeffectsNode_;}
    }
    
    public bool BlockEdition
    {
      get { return Hierarchy.BlockEdition; }
    }

    public bool IsInited
    {
      get { return isInited_; }
    }

    public static bool IsInitedStatic
    {
      get { return (instance_ != null && instance_.isInited_);}
    }

    public CREntityManager EntityManager
    {
      get { return entityManager_; }
    }

    public CRGOManager GOManager
    {
      get { return goManager_; }
    }

    public CNHierarchy Hierarchy
    {
      get { return hierarchy_; }     
    }

    public CRBakerGOAnim SimulationBaker
    {
      get { return simulationBaker_; }
    }

    public  CRPlayer Player
    {
      get { return simulationPlayer_; }
    }

    #endregion // Public types and data

    /**********************************************************************************************/
    /* Private fields                                                                             */
    /**********************************************************************************************/

    #region Private fields

    private CRManagerEditor        managerEditor_;
    private CRListBox              nodesListBox_;
    private CREntityManager        entityManager_; 
    private CRGOManager            goManager_;
    private CNHierarchy            hierarchy_;
    private CRSimulationDisplayer  simulationDisplayer_;
    private CRPlayer               simulationPlayer_;
    private CRBakerGOAnim          simulationBaker_;
    
    private Caronte_Fx       fxData_;
    private List<Caronte_Fx> listFxData_;
    private int              currentFxIdx_;              

    private bool     isInited_;
    private int      selectedButton_ ;
    private string[] sceneGUIButtonNames_ = new string[5] { "Bodies", "Joints", "Explosions", "Events", "Cust. Colliders" };

    private double elapsedTime_;
    private double lastUpdateTime_;

    private const double timeForUpdate_ = 1.5;

    private bool hierarchyChangeRequested_;
    private bool undoRedoChecksRequested_;
    private bool forbiddenUndoWhileSimulatingOrReplaying_;

    #endregion // Private fields

    /**********************************************************************************************/
    /* Static init                                                                                */
    /**********************************************************************************************/

    #region Static initialization

    private static CNManager instance_ = null;
    public  static CNManager Instance
    {
      get
      {
        if (instance_ == null)
        {
          DLLEnviromentInit();
          if (IntPtr.Size == 4)
          {
            CRDebug.Log("Windows - x86 Editor");
          }
          else
          {
            CRDebug.Log("Windows - x64 Editor");
          }
          
          bool dllUpToDate = CaronteSharp.Caronte.IsDllUpToDate();   

          if (dllUpToDate)
          {
            CRDebug.Log("Dll version up to date.");
            instance_ = new CNManager();
          }
          else
          {
            CRDebug.Log("Dll version not up to date.");
          }       
        }
        return instance_;
      }
    }


    private static void DLLEnviromentInit()
    {
       var currentPath = Environment.GetEnvironmentVariable("PATH",
                                                            EnvironmentVariableTarget.Process);
#if UNITY_EDITOR_32
      var dllPath = Application.dataPath
        + Path.DirectorySeparatorChar + "CaronteFX"
        + Path.DirectorySeparatorChar + "Editor"
        + Path.DirectorySeparatorChar + "Plugins"
        + Path.DirectorySeparatorChar + "x86";
#elif UNITY_EDITOR_64
    var dllPath = Application.dataPath
        + Path.DirectorySeparatorChar + "CaronteFX"
        + Path.DirectorySeparatorChar + "Editor"
        + Path.DirectorySeparatorChar + "Plugins"
        + Path.DirectorySeparatorChar + "x86_64";

#endif
      if (currentPath != null && currentPath.Contains(dllPath) == false)
      {
        Environment.SetEnvironmentVariable("PATH", currentPath + Path.PathSeparator
                                            + dllPath, EnvironmentVariableTarget.Process);
      }
    }
    #endregion

    //-----------------------------------------------------------------------------------
    private CNManager()
    {
      CaronteSharp.Caronte.StarterEditor( Mathf.Max(1, SystemInfo.processorCount - 1) );

      managerEditor_        = CRManagerEditor.Instance;
      entityManager_        = new CREntityManager();
      goManager_            = new CRGOManager();
      hierarchy_            = new CNHierarchy(this, managerEditor_, entityManager_, goManager_);
      simulationDisplayer_  = new CRSimulationDisplayer(entityManager_);
      simulationPlayer_     = new CRPlayer(this, entityManager_);
      simulationBaker_      = new CRBakerGOAnim(this, entityManager_, simulationPlayer_);
      listFxData_           = new List<Caronte_Fx>();

      selectedButton_           = 0;
      hierarchyChangeRequested_ = false;
      undoRedoChecksRequested_  = false;
      isInited_                 = false;

      AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
      AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
    }
    //-----------------------------------------------------------------------------------
    public void Init()
    {
      if (isInited_)
      {
        Deinit();
      }

      SetDLLEditingState();

      InitFX();

      EditorUtility.SetDirty(fxData_);

      managerEditor_ = CRManagerEditor.Instance;

      hierarchy_.SetManagerEditor(managerEditor_);
      hierarchy_.Init();

      nodesListBox_ = new CRListBox(Hierarchy, "managerLB", true);
      managerEditor_.NodesListBox = nodesListBox_;

      simulationDisplayer_.Init(FxData);
  
      EditorApplication.update    += CaronteEditorUpdate;
      EditorApplication.update    += ShowCaronteLog;

      EditorApplication.delayCall += CreateBodies;
      EditorApplication.delayCall += CreateJoints;
      EditorApplication.delayCall += CreateServos;
      EditorApplication.delayCall += CreateEntities;
      EditorApplication.delayCall += ValidateEditorsState;
      
      SceneView.onSceneGUIDelegate             += OnSceneGUI;
      EditorApplication.playmodeStateChanged   += ChangeState;
      EditorApplication.hierarchyWindowChanged += CustomHierarchyChange; 
      Undo.undoRedoPerformed                   += OnUndoRedo;

      hierarchyChangeRequested_ = true;

      lastUpdateTime_           = 0;
      elapsedTime_              = 0;

      ShowInvisibleBodies(FxData.ShowInvisibles);

      isInited_ = true;
    }
    //-----------------------------------------------------------------------------------
    public void Deinit()
    { 
      if ( !isInited_ )
      {
        return;
      }

      CloseWindows();

      SceneView.onSceneGUIDelegate             -= OnSceneGUI;
      EditorApplication.playmodeStateChanged   -= ChangeState;
      EditorApplication.hierarchyWindowChanged -= CustomHierarchyChange;
      Undo.undoRedoPerformed                   -= OnUndoRedo;

      EditorApplication.delayCall -= CreateJoints;
      EditorApplication.delayCall -= CreateBodies;
      EditorApplication.delayCall -= CreateServos;
      EditorApplication.delayCall -= CreateEntities;
      EditorApplication.delayCall -= ValidateEditorsState;

      EditorApplication.update    -= ShowCaronteLog;
      EditorApplication.update    -= CaronteEditorUpdate;

      hierarchy_.Deinit();
      simulationPlayer_.Deinit();

      isInited_ = false;

      entityManager_.DestroyBodiesTmpGameObjects();
      entityManager_.Clear();
      goManager_    .Clear();

      if (FxData != null)
      {
        FxData.ClearBodyMeshes();
        FxData.ClearJointMeshes();
        FxData.ClearSphereMeshes();
      }

      ResetCaronte();
      SceneView.RepaintAll();
    }
    //-----------------------------------------------------------------------------------
    private void InitFX()
    {
      FxData.gameObject.SetActive( true );
      FxData.e_purgeIsolatedNodes();
      FxData.SetStateEditing();

      UpdateFxDataVersionIfNeeded();
    }
    //-----------------------------------------------------------------------------------
    private void UpdateFxDataVersionIfNeeded()
    {
      if (FxData.DataVersion < 1)
      {
        GameObject dataHolder = FxData.GetDataGameObject();
        CNBody[] arrBodyNode = dataHolder.GetComponents<CNBody>();
        
        foreach(CNBody bodyNode in arrBodyNode)
        {
          bodyNode.OmegaStart_inDegSeg = bodyNode.OmegaStart_inRadSeg * Mathf.Rad2Deg;
          EditorUtility.SetDirty(bodyNode);
        }

        FxData.DataVersion = 1;
        EditorUtility.SetDirty(FxData);
      }
    }
    //-----------------------------------------------------------------------------------
    public bool IsFreeVersion()
    {
      return Caronte.IsFreeVersion();
    }
    //-----------------------------------------------------------------------------------
    private void SetDLLEditingState()
    {
      if ( !SimulationManager.IsEditing() )
      {
        SimulationManager.EditingBegin();
      }

      if ( SimulationManager.IsSimulating() || SimulationManager.IsReplaying() )
      {
        SimulationManager.ResetSimulation();
      }

      SimulationManager.SetBroadcastMode(UN_BROADCAST_MODE.EDITING);
    }
    //-----------------------------------------------------------------------------------
    public void OnUndoRedo()
    {
      if ( IsInited )
      {
        undoRedoChecksRequested_ = true;
      }
    }
    //-----------------------------------------------------------------------------------
    private void CloseWindows()
    {
      CRPlayerWindow      .CloseIfOpen();
      CNFieldWindow       .CloseIfOpen();
      CRBakeSimulationMenu.CloseIfOpen();
      CRBakeFrameMenu     .CloseIfOpen();
      CRIncludeMenu       .CloseIfOpen();
    }
    //-----------------------------------------------------------------------------------
    private void CaronteDebugCallback(string debugString)
    {
      CRDebug.Log(debugString);
    }
    //-----------------------------------------------------------------------------------
    public void CustomHierarchyChange()
    {
      if (IsInited)
      {
        if ( FxData == null )
        {
          //something happened to the current fxData
          Deinit();
          FxData = null;
        }
        else if ( SimulationManager.IsEditing() &&
                  !EditorApplication.isPlayingOrWillChangePlaymode && !AnimationMode.InAnimationMode() )
        {
          FxData.UpdateRootNodeName();
          hierarchyChangeRequested_ = true;
        }
      }
    }
    //-----------------------------------------------------------------------------------
    private void DoHierarchyChange()
    {
      if ( SimulationManager.IsEditing() && hierarchyChangeRequested_ && fxData_ != null )
      {
        goManager_.HierarchyChange();
        if (IsInited)
        {
          Hierarchy.RecalculateFieldsAutomatic();
        }
        CRManagerEditor.RepaintIfOpen();
        hierarchyChangeRequested_ = false;
      }
    }
    //-----------------------------------------------------------------------------------
    private void DoUndoRedoChecks()
    {
      if (undoRedoChecksRequested_)
      {
        bool isSimulatingOrReplaying = simulationPlayer_.IsSimulating || simulationPlayer_.IsReplaying;

        if ( isSimulatingOrReplaying && fxData_.IsEditingState() )
        {
          Undo.PerformRedo();
          EditorUtility.DisplayDialog("CaronteFX - Info", "To undo more actions first go back to Edit mode.", "Ok");
        }
        else if ( SimulationManager.IsEditing() )
        {
          Hierarchy.CheckHierarchyUndoRedo();
        }

        undoRedoChecksRequested_ = false;
      }
    }
    //-----------------------------------------------------------------------------------
    private void ChangeState()
    {
      if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying)
      {
        Deinit();
      }
    }
    //-----------------------------------------------------------------------------------
    private void CaronteEditorUpdate()
    {
      if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
      {
        Deinit();
      }

      if ( isInited_ )
      {
        if ( CRManagerEditor.IsFocused || CNFieldWindow.IsFocused )
        {
          DoHierarchyChange();
        }

        DoUndoRedoChecks();

        elapsedTime_   += EditorApplication.timeSinceStartup - lastUpdateTime_;
        lastUpdateTime_ = EditorApplication.timeSinceStartup;

        if ( elapsedTime_ > timeForUpdate_ )
        {
          if ( SimulationManager.IsEditing() )
          {
            CheckBodiesForChanges(false);

            simulationDisplayer_.UpdateListsBodyGORequested    = true;
            simulationDisplayer_.UpdateClothCollidersRequested = true;
          }      
          elapsedTime_ = 0;

          simulationPlayer_.CycleStatus();      
          managerEditor_.Repaint();
        }

        simulationDisplayer_.Update();
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetManagerEditor(CRManagerEditor managerEditor)
    {
      managerEditor_ = managerEditor;
      if (nodesListBox_ != null )
      {
        managerEditor_.NodesListBox = nodesListBox_;
        Hierarchy.SetManagerEditor(managerEditor);
      }
    }
    //-----------------------------------------------------------------------------------
    public IView GetFocusedNodeView()
    {
      return Hierarchy.GetFocusedNodeView();
    }
    //-----------------------------------------------------------------------------------
    void OnDomainUnload(object sender, EventArgs e) 
    {
      AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload; 
      Deinit();
      CaronteSharp.Caronte.FinisherEditor();
    }
    //-----------------------------------------------------------------------------------
    public CommandNode GetNodeAtListIdx(int listIdx)
    {
      return (Hierarchy.GetNodeAtListIdx(listIdx));
    }
    //-----------------------------------------------------------------------------------
    public CommandNode GetSelectedNode()
    {
      return (Hierarchy.FocusedNode);
    }
    //-----------------------------------------------------------------------------------
    private void ResetCaronte()
    {
      CaronteSharp.Caronte.FinisherEditor();
      CaronteSharp.Caronte.StarterEditor( Mathf.Max(1, SystemInfo.processorCount - 1) );
    }
    //-----------------------------------------------------------------------------------
    public static void ShowCaronteLog()
    {
      string[] errors;
      SimulationManager.GetLastErrors(out errors);
      if (errors.Length > 0)
      {
        CRDebug.Log("Caronte Log: ");
        for (int i = 0; i < errors.Length; ++i)
        {
          CRDebug.Log(errors[i]);
        }
      }
    }
    //----------------------------------------------------------------------------------
    public void ShowInvisibleBodies(bool active)
    {
      SimulationManager.SetBroadcastInvisibleBodies(active);
    }
    //----------------------------------------------------------------------------------
    private void CreateBodies()
    {
      List<CNBodyEditor> listBodyEditor = Hierarchy.ListBodyEditor;
      foreach(CNBodyEditor bodyEditor in listBodyEditor)
      {
        bodyEditor.CreateBodies();
      }
      EditorUtility.ClearProgressBar();

      hierarchy_.UpdateFieldLists();
    }
    //----------------------------------------------------------------------------------
    private void DestroyBodies()
    {
      List<CNBodyEditor> listBodyEditor = Hierarchy.ListBodyEditor;
      foreach(CNBodyEditor bodyEditor in listBodyEditor)
      {
        bodyEditor.DestroyBodies();
      }
      EditorUtility.ClearProgressBar();
    }
    //----------------------------------------------------------------------------------
    private void CreateJoints()
    {
      List<CNJointGroupsEditor> listMultiJointEditor = Hierarchy.ListMultiJointEditor;
      foreach (CNJointGroupsEditor multijointEditor in listMultiJointEditor)
      {
        multijointEditor.CreateEntities();
      }
    }
    //----------------------------------------------------------------------------------
    private void DestroyJoints()
    {
      List<CNJointGroupsEditor> listMultiJointEditor = Hierarchy.ListMultiJointEditor;
      foreach (CNJointGroupsEditor multijointEditor in listMultiJointEditor)
      {
        multijointEditor.DestroyEntities();
      }
    }
    //----------------------------------------------------------------------------------
    private void RecreateJoints()
    {
      List<CNJointGroupsEditor> listMultiJointEditor = Hierarchy.ListMultiJointEditor;
      foreach (CNJointGroupsEditor multijointEditor in listMultiJointEditor)
      {
        CNRigidGlueEditor rgEditor = multijointEditor as CNRigidGlueEditor;
        if (rgEditor != null)
        {
          rgEditor.RecreateEntitiesAsServos();
        }
        else
        {
          multijointEditor.RecreateEntities();
        }   
      }
    }
    //----------------------------------------------------------------------------------
    private void CreateServos()
    {
      List<CNServosEditor> listServosEditor = Hierarchy.ListServosEditor;
      foreach (CNServosEditor servosEditor in listServosEditor)
      {
        servosEditor.CreateEntities();
      }
    }
    //----------------------------------------------------------------------------------
    private void DestroyServos()
    {
      List<CNServosEditor> listServosEditor = Hierarchy.ListServosEditor;
      foreach (CNServosEditor servosEditor in listServosEditor)
      {
        servosEditor.DestroyEntities();
      }
    }
    //----------------------------------------------------------------------------------
    private void RecreateServos()
    {
      List<CNServosEditor> listServosEditor = Hierarchy.ListServosEditor;
      foreach (CNServosEditor svEditor in listServosEditor)
      {
        svEditor.RecreateEntities();
      }
    }
    //----------------------------------------------------------------------------------
    private void RecreateRopes()
    {
      List<CNBodyEditor> listBodyEditor = Hierarchy.ListBodyEditor;
      foreach (CNBodyEditor bodyEditor in listBodyEditor)
      {
        CNRopeEditor rpEditor = bodyEditor as CNRopeEditor;
        if (rpEditor != null)
        {
          rpEditor.RecreateBodies();
        }
      }
    }
    //----------------------------------------------------------------------------------
    private void CheckBodiesForChanges(bool recreateIfInvalid)
    {
      List<CNBodyEditor> listBodyEditor = Hierarchy.ListBodyEditor;
      int nBodyNodes = listBodyEditor.Count;

      for (int i = 0; i < nBodyNodes; i++)
      {
        CNBodyEditor bodyEditor = listBodyEditor[i];

        if (recreateIfInvalid)
        {
          string displayString = "Checking body nodes..." + (i + 1) + " of " + nBodyNodes + ".";
          float progress = (float)i / (float)nBodyNodes;
          EditorUtility.DisplayProgressBar("CaronteFX - Checking bodies for updated geometry.", displayString, progress);
        }

        bodyEditor.CheckBodiesForChanges(recreateIfInvalid);
      }
      EditorUtility.ClearProgressBar();
    }
    //----------------------------------------------------------------------------------
    private void SetBodiesState()
    {
      List<CNBodyEditor> listBodyEditor = Hierarchy.ListBodyEditor;

      foreach ( CNBodyEditor bodyEditor in listBodyEditor )
      {
        bodyEditor.SetCollisionState();
        bodyEditor.SetVisibilityState();
        bodyEditor.SetActivityIfDisabled();
      }
    }
    //----------------------------------------------------------------------------------
    public void ValidateEditorsState()
    {
      if ( SimulationManager.IsEditing() )
      {
        Hierarchy.ValidateEditors();
      }
    }
    //----------------------------------------------------------------------------------
    private void CreateEntities()
    {
      List<CNEntityEditor> listEntityEditor = Hierarchy.ListEntityEditor;

      foreach (CNEntityEditor entityEditor in listEntityEditor)
      {
        entityEditor.CreateEntity();  
      }
    }
    //----------------------------------------------------------------------------------
    private void ApplyEntities()
    {
      List<CNEntityEditor> listEditorNoTrigger = new List<CNEntityEditor>();
      List<CNEntityEditor> listEditorTrigger   = new List<CNEntityEditor>();

      foreach (CNEntityEditor entityEditor in Hierarchy.ListEntityEditor)
      {
        CNTriggerEditor triggerEditor = entityEditor as CNTriggerEditor;
        if ( triggerEditor == null )
        {
          listEditorNoTrigger.Add( entityEditor );
        }
        else
        {
          listEditorTrigger.Add( entityEditor );
        }
      }

      foreach (CNEntityEditor entityEditor in listEditorNoTrigger)
      {
        entityEditor.ApplyEntity();
      }

      foreach (CNEntityEditor entityEditor in listEditorTrigger)
      {
        entityEditor.ApplyEntity();
      }
    }
    //----------------------------------------------------------------------------------
    private void AddAnimations()
    {
      foreach (CNAnimatedbodyEditor animNodeEditor in Hierarchy.ListAnimatedBodyEditor)
      {
        if ( !animNodeEditor.IsExcluded && animNodeEditor.IsEnabled)
        {
          Player.AddAnimation(animNodeEditor);
        }
      }
    }
    //----------------------------------------------------------------------------------
    private void DoPreSimulationOperations()
    {
      CheckBodiesForChanges(true);
      ValidateEditorsState();

      SimulationManager.ForceReadjust();

      RecreateJoints();
      RecreateServos();

      entityManager_.SaveStateOfBodies();

      ApplyEntities();
      SetBodiesState();
  
      Undo.RecordObject(fxData_, "CaronteFX - Start simulating");
      FxData.SetStateSimulating();
      EditorUtility.SetDirty(fxData_);
    }
    //----------------------------------------------------------------------------------
    private SimulationParams GetSimulationParams()
    {
      SimulationParams params_ = new SimulationParams();

      params_.nThreads_ = (uint) SystemInfo.processorCount - 1;

      if (EffectData.quality_ < 50 )
      {
        float oldMax = 50;
        float oldMin = 1;

        float newMax = 50;
        float newMin = 20;

        float q = EffectData.quality_;

        float oldRange = (oldMax - oldMin);  
        float newRange = (newMax - newMin);  
        float newQ = (( (q - oldMin) * newRange) / oldRange) + newMin; 

        params_.qualityRq_0_100_  = newQ;
      }
      else
      {
        params_.qualityRq_0_100_  = EffectData.quality_;
      }
      
      params_.jitterZapperRq_0_100_ = EffectData.antiJittering_;

      params_.totalTime_ = EffectData.totalTime_;
      params_.fps_       = (uint) EffectData.frameRate_;

      params_.isUnTimeStepFixed_ = false;
      params_.unTimeStepFixed_   = Time.fixedDeltaTime;

      params_.isNormalized_deltaTime_ = (!EffectData.byUserDeltaTime_) || 
                                        (EffectData.deltaTime_ == -1);

      params_.deltaTime_              = EffectData.deltaTime_;

      params_.isByUser_distCharacteristic_ = (EffectData.byUserCharacteristicObjectProperties_)  && 
                                             (EffectData.thickness_ != -1) &&
                                             (EffectData.length_    != -1);

      params_.byUser_distCharacteristicThickness_ = EffectData.thickness_;
      params_.byUser_distCharacteristicLength_    = EffectData.length_;

      return params_;
    }
    //----------------------------------------------------------------------------------
    public void StartSimulating()
    {
      DoPreSimulationOperations();
      SimulationStart();
    }
    //----------------------------------------------------------------------------------
    private void SimulationStart()
    {
      if ( SimulationManager.IsEditing() && entityManager_.NumberOfBodies > 0 )
      { 
        SimulationParams simParams = GetSimulationParams();

        AddAnimations();
        entityManager_.CreateBodiesTmpGameObjects();
    
        Debug.Log("[CaronteFX] Starting simulation with " + simParams.nThreads_ + " threads");
        UN_SimulationProperties un_simProperties = Player.SimulatingBeginFirst( simParams );
        Debug.Log("[CaronteFX] Delta time: " + un_simProperties.deltaTime_);

        EffectData.SetLastUsedProperties( un_simProperties.deltaTime_, 2 * un_simProperties.distThick_, 2 * un_simProperties.distNarrow_ );
        EditorUtility.SetDirty( FxData );

        Selection.activeGameObject = null;

        CRPlayerWindow.ShowWindow( simulationPlayer_ );
        Hierarchy.BlockEdition = true;

        EditorApplication.delayCall += CNFieldWindow.CloseIfOpen;    
      }

      if ( entityManager_.NumberOfBodies == 0 )
      {
        EditorUtility.DisplayDialog("CaronteFX - Info", "In order to Simulate, you must first define some object bodies.", "ok" );
      }
    }
    //----------------------------------------------------------------------------------
    private bool CheckForFreeVersionLimitations(SimulationParams simParams)
    {
      bool wasAnyLimitationsSurpased = false;

      UN_SimulationStatistics statistics = simulationDisplayer_.Statistics;
      if ( statistics.nRigids_ > 20)
      {
        EditorUtility.DisplayDialog("CaronteFX - Info", "CaronteFX free version can simulate a maximum of 20 rigid bodies.", "ok" );
        wasAnyLimitationsSurpased = true;
      }
      else if (statistics.nBodyMesh_ > 3)
      {
        EditorUtility.DisplayDialog("CaronteFX - Info", "CaronteFX free version can simulate a maximum of 3 animated or irresponsive bodies.", "ok" );
        wasAnyLimitationsSurpased = true;
      }
      else if (statistics.nSoftbodies_ > 3)
      {
        EditorUtility.DisplayDialog("CaronteFX - Info", "CaronteFX free version can simulate a maximum of 3 soft bodies.", "ok" );
        wasAnyLimitationsSurpased = true;
      }
      else if (statistics.nCloth_ > 1)
      {
        EditorUtility.DisplayDialog("CaronteFX - Info", "CaronteFX free version can simulate a maximum of 1 cloth body.", "ok" );
        wasAnyLimitationsSurpased = true;
      }
      else if (simParams.totalTime_ > 5.0)
      {
        EditorUtility.DisplayDialog("CaronteFX - Info", "CaronteFX free version can simulate a maximum of 5 seconds.", "ok" );
        wasAnyLimitationsSurpased = true;
      }

      return wasAnyLimitationsSurpased;
    }
    //----------------------------------------------------------------------------------
    public void ShowPlayer()
    {
      CRPlayerWindow.ShowWindow( simulationPlayer_ );
    }
    //----------------------------------------------------------------------------------
    public void ResetSimulation()
    {
      if (IsInited)
      {     
        Deinit();
        LoadEffectsArray();
        Init();
      }
    }
    //----------------------------------------------------------------------------------
    public void PrepareToRestartSimulation()
    {
      entityManager_.DestroyBodiesTmpGameObjects();
      entityManager_.LoadStateOfBodies();

      DestroyJoints();
      DestroyServos();
      CreateJoints();
      CreateServos();

      SimulationManager.PrepareToRestartSimulation();

      CustomHierarchyChange();
      Hierarchy.BlockEdition = false;
      fxData_.SetStateEditing();
    }
    //----------------------------------------------------------------------------------
    public void SceneSelection()
    {
      Hierarchy.SceneSelection();
    }
    //----------------------------------------------------------------------------------
    public void GetListBodyNodesForBake( List<CNBody> listBodyNode )
    {
      listBodyNode.Clear();

      foreach( CommandNodeEditor nodeEditor in Hierarchy.ListCommandNodeEditor )
      {
        CNBodyEditor  bodyEditor   = nodeEditor as CNBodyEditor;
        if ( bodyEditor != null && !bodyEditor.IsExcluded )
        {
          listBodyNode.Add( (CNBody)nodeEditor.Data );
        }
      }
    }
    //----------------------------------------------------------------------------------
    public bool IsEffectIncluded(Caronte_Fx fx)
    {
      return Hierarchy.IsEffectIncluded(fx);
    }
    //----------------------------------------------------------------------------------
    public void SetEnableStateSelection(bool enabled)
    {
      Hierarchy.SetEnableStateSelection(enabled);
    }
    //----------------------------------------------------------------------------------
    public void DuplicateSelection()
    {
      Hierarchy.DuplicateSelection();
    }
    //----------------------------------------------------------------------------------
    public void ContextClickExternal()
    {
      Hierarchy.ContextClick();
    }
    //----------------------------------------------------------------------------------
    public void DoDeferredActions()
    {
      Hierarchy.RemoveNodesDefeerred();
    }
    //----------------------------------------------------------------------------------
    public void RefreshEffectsArray()
    {    
      Caronte_Fx[] arrFxData = CREditorUtils.GetAllSceneComponentsOfType<Caronte_Fx>();
      Array.Sort(arrFxData, delegate(Caronte_Fx fx1, Caronte_Fx fx2)
      {
        return fx1.gameObject.name.CompareTo(fx2.gameObject.name);
      });

      int indexOfCurrent = -1;
      for (int i = 0; i < arrFxData.Length; i++)
      {
        if ( arrFxData[i] == FxData )
        {
          indexOfCurrent = i;
        }
      }

      if (indexOfCurrent != -1)
      {
        CurrentFxIdx = indexOfCurrent;
      }
  
      listFxData_.Clear();
      listFxData_.AddRange(arrFxData);
    }
    //----------------------------------------------------------------------------------
    public void LoadEffectsArray()
    {
      Caronte_Fx[] arrFxData = CREditorUtils.GetAllSceneComponentsOfType<Caronte_Fx>();
      int arrFxData_size = arrFxData.Length;

      Array.Sort(arrFxData, delegate(Caronte_Fx fx1, Caronte_Fx fx2)
      {
        return fx1.gameObject.name.CompareTo(fx2.gameObject.name);
      });

      bool foundActive = false;
      for (int i = 0; i < arrFxData_size; i++)
      {
        Caronte_Fx data = arrFxData[i];
        if (data.enabled && data.ActiveInEditor)
        {
          foundActive   = true;
          FxData        = data;
          currentFxIdx_ = i;
          break;
        }
      }

      if (!foundActive && arrFxData_size > 0)
      {
        Caronte_Fx data = arrFxData[0];
        FxData = data;
        FxData.ActiveInEditor = true;
      }

      listFxData_.Clear();
      listFxData_.AddRange(arrFxData);
    }
    //----------------------------------------------------------------------------------
    private void DeactivateEffects()
    {
      int listFxData_size = listFxData_.Count;
      for (int i = 0; i < listFxData_size; i++)
      {
        Caronte_Fx data = listFxData_[i];
        data.ActiveInEditor = false;
      }
    }
    //----------------------------------------------------------------------------------
    public void CreateNewFx()
    {
      // create 
      DeactivateEffects();

      GameObject go = new GameObject("CaronteFx_0");
      Undo.RegisterCreatedObjectUndo (go, "Created CaronteFX GameObject");

      go.tag = "EditorOnly";
      Caronte_Fx data = go.AddComponent<Caronte_Fx>();

      MakeFxNameUnique(data);

      data.ActiveInEditor = true;

      EditorUtility.SetDirty(data);
      EditorGUIUtility.PingObject(data);
      Selection.activeGameObject = data.gameObject;

      LoadEffectsArray();
    }
    //----------------------------------------------------------------------------------
    public void ChangeToFx(int fxIdx)
    {
      DeactivateEffects();

      Deinit();

      Caronte_Fx currentData = listFxData_[fxIdx];
      currentData.ActiveInEditor = false;
      EditorUtility.SetDirty(currentData);
        
      currentFxIdx_ = fxIdx;

      Caronte_Fx data = listFxData_[fxIdx];
      data.ActiveInEditor = true;

      EditorUtility.SetDirty(data);
      EditorGUIUtility.PingObject(listFxData_[fxIdx]);
      Selection.activeGameObject = listFxData_[fxIdx].gameObject;

      LoadEffectsArray();
    }
    //----------------------------------------------------------------------------------
    public void SetFxDataActive(Caronte_Fx fxDataToActivate)
    {
      if (fxDataToActivate == FxData)
      {
        return;
      }

      Caronte_Fx[] arrFxData = GameObject.FindObjectsOfType<Caronte_Fx>();

      int arrFxData_size = arrFxData.Length;
      for (int i = 0; i < arrFxData_size; i++)
      {
        Caronte_Fx _fxData = arrFxData[i];
        _fxData.ActiveInEditor = false;
        if (_fxData == fxDataToActivate)
        {
          fxDataToActivate.ActiveInEditor = true;
          FxData = fxDataToActivate;
          currentFxIdx_ = i;
        }
      }
    }
    //----------------------------------------------------------------------------------
    public void MakeFxNameUnique(Caronte_Fx __fxData)
    {
      bool loop = false;
      int count = 0;
      GameObject __go = __fxData.gameObject;
      do
      {
        if (loop) loop = false;
        foreach (Caronte_Fx _fxData in listFxData_)
        {
          if (_fxData != null)
          {
            GameObject _go = _fxData.gameObject;
            if (_fxData != __fxData && _go.name == __go.name)
            {
              int index = __go.name.IndexOf("_");
              __go.name = __go.name.Substring(0, index);
              count++;
              __go.name += "_" + count;
              loop = true;
              break;
            }
          }
        }
      } while (loop);
    }
    //----------------------------------------------------------------------------------
    public string[] GetFxNames(out string longestName)
    {
      Caronte_Fx[] arrFxData = listFxData_.ToArray();
      int arrFxData_size = arrFxData.Length;
      List<string> names = new List<String>(arrFxData_size + 2);
      longestName = string.Empty;
      for (int i = 0; i < arrFxData_size; i++)
      {
        if (arrFxData[i] != null)
        {
          string effectName = arrFxData[i].name;
          string auxName = effectName;
          int id = 2;
          while (names.Contains(auxName))
          {
            auxName = effectName + " (" + id + ")";
            id++;
          }
          names.Add(auxName);
          if (auxName.Length > longestName.Length)
          {
            longestName = auxName;
          }
        }
      }
      longestName += "........";

      return names.ToArray();
    }
    //----------------------------------------------------------------------------------
    public void GetBodiesData(List<CRBodyData> listBodyData)
    {
      listBodyData.Clear();

      Transform[] selection = Selection.GetTransforms(SelectionMode.ExcludePrefab);

      foreach (Transform tr in selection)
      {
        GameObject go = tr.gameObject;
        Caronte_Fx_Body fxBody = go.GetComponent<Caronte_Fx_Body>();
        CRBodyData bodyData;
        if (fxBody != null)
        {
          GetBodyData( go, out bodyData );
          listBodyData.Add(bodyData);
        }    
      }

      if (listBodyData.Count == 0)
      {
        listBodyData.Add( new CRBodyData() );
      }
    }
    //-----------------------------------------------------------------------------------
    public void GetBodyData(GameObject gameObject, out CRBodyData bodyData)
    {
      bodyData = new CRBodyData();

      uint idBody = uint.MaxValue;
      if (simulationPlayer_.IsEditing)
      {
        idBody = entityManager_.GetIdBodyFromGo( gameObject );
      }
      else
      {
        idBody = entityManager_.GetIdBodyFromGOForSimulatingOrReplaying( gameObject );
      }
   
      if (idBody != uint.MaxValue)
      {
        List<CommandNode> listNode = entityManager_.GetListNodeReferences(idBody);

        BodyType bodyType  = entityManager_.GetBodyType(idBody);
        bodyData.idBody_   = idBody;
        bodyData.bodyType_ = bodyType;
        bodyData.listNode_ = listNode;
      }
    }
    //----------------------------------------------------------------------------------
    public void AddCaronteFxGameObjects(List<GameObject> listCaronteFxGameObject)
    {
      Hierarchy.AddCaronteFxGameObjects(listCaronteFxGameObject);
    }
    //----------------------------------------------------------------------------------
    public void CreateNodeUnique<T>(string name)
      where T : CommandNode
    {
      Hierarchy.CreateNodeUnique<T>(name);
    }
    //----------------------------------------------------------------------------------
    public void CreateIrresponsiveBodiesNode()
    {
      Hierarchy.CreateIrresponsiveBodiesNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateRopeBodiesNode()
    {
      Hierarchy.CreateRopeBodiesNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateMultiJointNodeArea()
    {
      Hierarchy.CreateMultiJointAreaNode(); 
    }
    //----------------------------------------------------------------------------------
    public void CreateMultiJointNodeVertices()
    {
      Hierarchy.CreateMultiJointVerticesNode(); 
    }
    //----------------------------------------------------------------------------------
    public void CreateMultiJointNodeLeaves()
    {
      Hierarchy.CreateMultiJointLeavesNode(); 
    }
    //----------------------------------------------------------------------------------
    public void CreateMultiJointNodeLocators()
    {
      Hierarchy.CreateMultiJointLocatorsNode(); 
    }
    //----------------------------------------------------------------------------------
    public void CreateRigidglueNode()
    {
      Hierarchy.CreateRigidGlueNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateServosLinearNode()
    {
      Hierarchy.CreateServosLinearNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateServosAngularNode()
    {
      Hierarchy.CreateServosAngularNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateMotorsLinearNode()
    {
      Hierarchy.CreateMotorsLinearNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateMotorsAngularNode()
    {
      Hierarchy.CreateMotorsAngularNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateFracturerNodeUniform()
    {
      Hierarchy.CreateFracturerUniformNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateFracturerNodeGeometry()
    {
      Hierarchy.CreateFracturerGeometryNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateFracturerNodeRadial()
    {
      Hierarchy.CreateFracturerRadialNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateExplosionNode()
    {
      Hierarchy.CreateExplosionNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateClothBodiesNode()
    {
      Hierarchy.CreateClothBodiesNode();
    }
    //----------------------------------------------------------------------------------
    public CommandNodeEditor GetNodeEditor(CommandNode node)
    {
      return Hierarchy.GetNodeEditor(node);
    }
    //----------------------------------------------------------------------------------
    private void OnSceneGUI(SceneView sceneView)
    {
      if (FxData != null && isInited_)
      {
        if (FxData.DrawExplosions)
        {
          simulationDisplayer_.RenderExplosions( Mathf.Log(FxData.ExplosionsOpacity) / 10f );
        }

        if (FxData.DrawContactEvents)
        {
          simulationDisplayer_.RenderContactEvents( Mathf.Log(FxData.ContactEventSize) / 10f );
        }
    
        if (FxData.ShowOverlay)
        {
          DrawOverlay(sceneView);
        }
      }
    }
    //----------------------------------------------------------------------------------
    public void DrawOverlay(SceneView sceneView)
    {
      if (CRManagerEditor.IsOpen)
      {
        Handles.BeginGUI();

        simulationDisplayer_.RenderStatistics();

        Rect svRect = sceneView.position;
        Rect windowRect = new Rect(svRect.size.x - 215f, svRect.size.y - 150f, 210f, 145f);

        string title = "CaronteFX";
        windowRect = GUILayout.Window(0, windowRect, DoSceneWindow, title);

        Handles.EndGUI();
      }
    }
    //-----------------------------------------------------------------------------------
    private void SetSelectionGridButtonNames()
    {

      if (FxData.DrawBodyBoxes ||
          FxData.DrawClothColliders ||
          FxData.ShowInvisibles )
      {
        sceneGUIButtonNames_[0] = "*Bodies*";
      }
      else
      {
        sceneGUIButtonNames_[0] = "Bodies";
      }

      if (FxData.DrawJoints)
      {
        sceneGUIButtonNames_[1] = "*Joints*";
      }
      else
      {
        sceneGUIButtonNames_[1] = "Joints";
      }

      if (FxData.DrawExplosions)
      {
        sceneGUIButtonNames_[2] = "*Explosions*";
      }
      else
      {
        sceneGUIButtonNames_[2] = "Explosions";
      }

      if (FxData.DrawContactEvents)
      {
        sceneGUIButtonNames_[3] = "*Events*";
      }
      else
      {
        sceneGUIButtonNames_[3] = "Events";
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawBodiesSection()
    {
      EditorGUILayout.BeginHorizontal(); 
      EditorGUI.BeginChangeCheck();
      FxData.DrawBodyBoxes = GUILayout.Toggle(FxData.DrawBodyBoxes, "Draw boxes");
      if (EditorGUI.EndChangeCheck())
      {
        simulationDisplayer_.UpdateListsBodyGORequested = true;
        EditorUtility.SetDirty(FxData);
      }
      EditorGUI.BeginChangeCheck();
      FxData.DrawClothColliders = GUILayout.Toggle( FxData.DrawClothColliders, "Cloth balls");
      if (EditorGUI.EndChangeCheck())
      {
        simulationDisplayer_.UpdateClothCollidersRequested = true;
        EditorUtility.SetDirty(FxData);
      }
      EditorGUILayout.EndHorizontal();
      GUILayout.Space(2f);
      EditorGUI.BeginChangeCheck();
      Color current = GUI.contentColor;
      if (FxData.ShowInvisibles)
      {
        GUI.contentColor = Color.red;
      }
      FxData.ShowInvisibles = GUILayout.Toggle(FxData.ShowInvisibles, "Draw invisibles");
      GUI.contentColor = current;
      if (EditorGUI.EndChangeCheck())
      {
        ShowInvisibleBodies(FxData.ShowInvisibles);
        simulationDisplayer_.UpdateListsBodyGORequested = true;
        EditorUtility.SetDirty(FxData);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawJointsSection()
    {
      EditorGUI.BeginChangeCheck();
      EditorGUILayout.BeginHorizontal();  
      FxData.DrawJoints = GUILayout.Toggle(FxData.DrawJoints, "Draw joints");
      EditorGUI.BeginDisabledGroup(!FxData.DrawJoints);
      FxData.DrawOnlySelected = GUILayout.Toggle(FxData.DrawOnlySelected, "Only selected");
      EditorGUILayout.EndHorizontal();
      if (EditorGUI.EndChangeCheck())
      {
        Hierarchy.SceneSelection();
        simulationDisplayer_.UpdateListJointsRequested = true;
        EditorUtility.SetDirty(FxData);
      }

      EditorGUI.BeginChangeCheck();
      GUILayout.BeginHorizontal();
      GUILayout.Label( "Render size", GUILayout.MaxWidth(100) );
      FxData.JointsSize = GUILayout.HorizontalSlider(FxData.JointsSize, 1.05f, 10f);
      GUILayout.EndHorizontal();
      EditorGUI.EndDisabledGroup();
      if (EditorGUI.EndChangeCheck())
      {
        simulationDisplayer_.UpdateListJointsRequested = true;
        EditorUtility.SetDirty(FxData);
        }
    }
    //-----------------------------------------------------------------------------------
    private void DrawExplosionsSection()
    {
      EditorGUI.BeginChangeCheck();
      FxData.DrawExplosions = GUILayout.Toggle(FxData.DrawExplosions, "Draw explosions");
      EditorGUI.BeginDisabledGroup(!FxData.DrawExplosions);
      GUILayout.BeginHorizontal();
      GUILayout.Label("Render opacity", GUILayout.MaxWidth(100));
      FxData.ExplosionsOpacity = GUILayout.HorizontalSlider(FxData.ExplosionsOpacity, 1.5f, 10f);
      GUILayout.EndHorizontal();
      EditorGUI.EndDisabledGroup();
      if (EditorGUI.EndChangeCheck())
      {
        EditorUtility.SetDirty(FxData);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawEventsSection()
    {
      EditorGUI.BeginChangeCheck();
      FxData.DrawContactEvents = GUILayout.Toggle(FxData.DrawContactEvents, "Draw contacts");
      EditorGUI.BeginDisabledGroup(!FxData.DrawContactEvents);
      GUILayout.BeginHorizontal();
      GUILayout.Label("Render size", GUILayout.MaxWidth(100));
      FxData.ContactEventSize = GUILayout.HorizontalSlider(FxData.ContactEventSize, 1.5f, 10f);
      GUILayout.EndHorizontal();
      EditorGUI.EndDisabledGroup();
      if (EditorGUI.EndChangeCheck())
      {
        EditorUtility.SetDirty(FxData);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawVisibilitySection()
    {
      List<Renderer> listBodyRenderer;
      List<MonoBehaviour> listBodyCollider;
      entityManager_.GetListBodyRendererAndCollider(out listBodyRenderer, out listBodyCollider);

      CRGUIUtils.DrawToggleMixedRenderers("Draw render meshes", listBodyRenderer, 150f);
      CRGUIUtils.DrawToggleMixedMonoBehaviours("Draw collider meshes", listBodyCollider, 190f);
    }
    //-----------------------------------------------------------------------------------
    private void DoSceneWindow(int windowID)
    {
      SetSelectionGridButtonNames();

      selectedButton_ = GUILayout.SelectionGrid(selectedButton_, sceneGUIButtonNames_, 2, GUILayout.Height(55f) );
      CRGUIUtils.Splitter(Color.gray);
 
      if ( selectedButton_ == 0)
      {
        DrawBodiesSection();
      }
      else if(selectedButton_ == 1)
      {
        DrawJointsSection();
      }
      else if (selectedButton_ == 2)
      {
        DrawExplosionsSection();
      }
      else if (selectedButton_ == 3)
      {
        DrawEventsSection();
      }
      else if (selectedButton_ == 4)
      {
        DrawVisibilitySection();
      }
      
      if ( SimulationManager.IsEditing() )
      {
        string editorString = CRManagerEditor.IsOpen ? "Focus Editor" : "Open Editor";
        if ( GUILayout.Button(editorString) )
        {
          CRManagerEditor.Init();
        }
      }
      else
      {
        string playerString = CRPlayerWindow.IsOpen ? "Focus Player" : "Open Player";
        if ( GUILayout.Button(playerString) )
        {
          CRPlayerWindow.ShowWindow( Player );
        }
      } 
    }
    //-----------------------------------------------------------------------------------
    public void BuildBakerData()
    {
      simulationBaker_.BuildBakerInitData();
    }
    //-----------------------------------------------------------------------------------
  } 
}
