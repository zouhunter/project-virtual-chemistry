using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace CaronteFX
{

  [Serializable]
  public class TupleIdMesh : Tuple2<uint, Mesh>
  {
    public TupleIdMesh( uint id, Mesh mesh )
      :  base(id, mesh)
    {

    }

  }

  public class Caronte_Fx : MonoBehaviour
  {
    [SerializeField]
    int dataVersion_ = 0;
    public int DataVersion
    {
      get { return dataVersion_; }
      set { dataVersion_ = value; }
    }
                
    [SerializeField]
    CREffectData effectData_;
    public CREffectData effect
    {
      get
      {
        return effectData_;
      }
      set
      {
        effectData_ = value;
      }
    }

    [SerializeField]
    private GameObject dataHolder_;

    [SerializeField]
    private bool active_ = true;
    public bool ActiveInEditor
    {
      get { return active_; }
      set { active_ = value; }
    }

    [SerializeField]
    private bool drawBodyBoxes_ = false;
    public bool DrawBodyBoxes
    {
      get { return drawBodyBoxes_; }
      set { drawBodyBoxes_ = value; }
    }

    [SerializeField]
    private bool drawClothColliders_ = true;
    public bool DrawClothColliders
    {
      get { return drawClothColliders_; }
      set { drawClothColliders_ = value; }
    }

    public List<Mesh> listMeshBodyBoxesEnabledVisible_  = new List<Mesh>();
    public List<Mesh> listMeshBodyBoxesDisabledVisible_ = new List<Mesh>(); 
    public List<Mesh> listMeshBodyBoxesEnabledHide_     = new List<Mesh>(); 
    public List<Mesh> listMeshBodyBoxesDisabledHide_    = new List<Mesh>(); 
    public List<Mesh> listMeshBodyBoxesSleeping_        = new List<Mesh>();

    public List<Mesh> listMeshClothSpheres_ = new List<Mesh>();

    public List<TupleIdMesh> listMeshJointBoxesNormalIn_      = new List<TupleIdMesh>();
    public List<TupleIdMesh> listMeshJointBoxesNormalOut_     = new List<TupleIdMesh>();
    public List<TupleIdMesh> listMeshJointBoxesDeformatedIn_  = new List<TupleIdMesh>();
    public List<TupleIdMesh> listMeshJointBoxesDeformatedOut_ = new List<TupleIdMesh>();
    public List<TupleIdMesh> listMeshJointBoxesBreakingIn_    = new List<TupleIdMesh>(); 
    public List<TupleIdMesh> listMeshJointBoxesBreakingOut_   = new List<TupleIdMesh>(); 
    public List<TupleIdMesh> listMeshJointBoxesBrokenIn_      = new List<TupleIdMesh>();
    public List<TupleIdMesh> listMeshJointBoxesBrokenOut_     = new List<TupleIdMesh>(); 

    [NonSerialized]
    public List<uint> listJointGroupsIdsSelected_ = new List<uint>();
    [NonSerialized]
    public List<uint> listRigidGlueIdsSelected_   = new List<uint>();

    [SerializeField]
    private bool drawJoints_ = true;
    public bool DrawJoints
    {
      get { return drawJoints_; }
      set { drawJoints_ = value; }
    }

    [SerializeField]
    private bool drawOnlySelected_ = false;
    public bool DrawOnlySelected
    {
      get { return drawOnlySelected_; }
      set { drawOnlySelected_ = value; }
    }

    [SerializeField]
    private float jointsSize_ = 1.5f;
    public  float JointsSize
    {
      get { return jointsSize_; }
      set { jointsSize_ = value; }
    }

    [SerializeField]
    private bool drawExplosions_ = true;
    public bool DrawExplosions
    {
      get { return drawExplosions_; }
      set { drawExplosions_ = value; }
    }

    [SerializeField]
    private float explosionsOpacity_ = 1.5f;
    public float ExplosionsOpacity
    {
      get { return explosionsOpacity_; }
      set { explosionsOpacity_ = value; }
    }

    [SerializeField]
    private bool drawContactEvents_ = true;
    public bool DrawContactEvents
    {
      get { return drawContactEvents_; }
      set { drawContactEvents_ = value; }
    }

    [SerializeField]
    private float contactEventSize_ = 5.0f;
    public  float ContactEventSize
    {
      get { return contactEventSize_; }
      set { contactEventSize_ = value; }
    }

    [SerializeField]
    private bool showStats_ = true;
    public bool ShowStats
    {
      get { return showStats_; }
      set { showStats_ = value; }
    }

    [SerializeField]
    private bool showOverlay_ = true;
    public bool ShowOverlay
    {
      get { return showOverlay_; }
      set { showOverlay_ = value; }
    }

    [SerializeField]
    private bool showInvisibles_ = false;
    public bool ShowInvisibles
    {
      get { return showInvisibles_; }
      set { showInvisibles_ = value; }
    }

    [SerializeField]
    private bool drawSleepingState_ = true;
    public bool DrawSleepingState
    {
      get { return drawSleepingState_; }
      set { drawSleepingState_ = value; }
    }

    [SerializeField]
    private CRSelectionData selectionData_ = new CRSelectionData();
    public CRSelectionData SelectionData
    {
      get { return selectionData_; }
      set { selectionData_ = value;}
    }

    private enum CaronteFXEditorState
    {
      EDITING,
      SIMULATING,
    }

    [SerializeField]
    private CaronteFXEditorState editorState_ = CaronteFXEditorState.EDITING;

    public void SetStateEditing()
    {
      editorState_ = CaronteFXEditorState.EDITING;
    }

    public void SetStateSimulating()
    {
      editorState_ = CaronteFXEditorState.SIMULATING;
    }

    public bool IsEditingState()
    {
      return editorState_ == CaronteFXEditorState.EDITING;
    }

    public GameObject GetDataGameObject()
    {
      return TargetGetDataHolder().gameObject;
    }

    public Transform TargetGetDataHolder()
    {
      if ( dataHolder_ == null )
      {
        foreach(Transform child in transform)
        {
          if(child.gameObject.name == "simData_")
          {
            dataHolder_ = child.gameObject;
            break;
          }
        }
      }
      if( !dataHolder_)
      {
        dataHolder_ = new GameObject("simdata_");
        dataHolder_.transform.parent = transform;
        dataHolder_.SetActive(false);
        dataHolder_.tag = "EditorOnly";
      }
      return dataHolder_.transform;
    }

    public void e_addEffect()
    {
      GameObject dataHolder = TargetGetDataHolder().gameObject;
      
      CREffectData e = new CREffectData();
      string name    = "Empty Effect";
      
      e.SetDefault();

      e.name_                  = name;
      e.rootNode_              = CRTreeNode.CreateInstance<CNGroup>(dataHolder);
      e.rootNode_.IsEffectRoot = true;
      e.rootNode_.Name         = "root";

      e.subeffectsNode_ = CRTreeNode.CreateInstance<CNGroup>(dataHolder, e.rootNode_);
      e.subeffectsNode_.Name = "Subeffects";
      e.subeffectsNode_.IsSubeffectsFolder = true;
      
      CNGravity gravityDaemon = CRTreeNode.CreateInstance<CNGravity>(dataHolder, e.rootNode_);
      gravityDaemon.Name = "Gravity_0";
 
      effect = e;
    }

    public void e_purgeIsolatedNodes()
    {
      GameObject dataHolder = TargetGetDataHolder().gameObject;

      CommandNode[] arrCommandNode = dataHolder.GetComponents<CommandNode>();
      foreach(CommandNode node in arrCommandNode)
      {
        if (node.Parent == null && !node.IsEffectRoot)
        {
          UnityEngine.Object.DestroyImmediate(node);
        }
      }
    }


    public CNGroup e_getRootNode()
    {
      return effectData_.rootNode_;
    }

    public CNGroup e_getSubEffectsNode()
    {
      return effectData_.subeffectsNode_;
    }

    public void e_clearFxData()
    {
      GameObject dataHolder = TargetGetDataHolder().gameObject;
      DestroyImmediate(dataHolder);
      TargetGetDataHolder();
    }

    public void UpdateRootNodeName()
    {
      e_getRootNode().Name = gameObject.name;
    }

    public void ClearBodyMeshes()
    {
      foreach( Mesh bodyMesh in listMeshBodyBoxesEnabledVisible_)
      {
        UnityEngine.Object.DestroyImmediate(bodyMesh);
      }
      listMeshBodyBoxesEnabledVisible_.Clear();

      foreach( Mesh bodyMesh in listMeshBodyBoxesDisabledVisible_)
      {
        UnityEngine.Object.DestroyImmediate(bodyMesh);
      }
      listMeshBodyBoxesDisabledVisible_.Clear();
 
      foreach( Mesh bodyMesh in listMeshBodyBoxesEnabledHide_)
      {
        UnityEngine.Object.DestroyImmediate(bodyMesh);
      }
      listMeshBodyBoxesEnabledHide_.Clear();

      foreach( Mesh bodyMesh in listMeshBodyBoxesDisabledHide_)
      {
        UnityEngine.Object.DestroyImmediate(bodyMesh);
      }
      listMeshBodyBoxesDisabledHide_.Clear();

      foreach( Mesh bodyMesh in listMeshBodyBoxesSleeping_ )
      {
        UnityEngine.Object.DestroyImmediate(bodyMesh);
      }
      listMeshBodyBoxesSleeping_.Clear();
    }

    public void ClearSphereMeshes()
    {
      foreach( Mesh clothMesh in listMeshClothSpheres_ )
      {
        UnityEngine.Object.DestroyImmediate(clothMesh);
      }
      listMeshClothSpheres_.Clear();
    }

    public void ClearJointMeshes()
    {
      foreach( Tuple2<uint,Mesh> idjointMesh in listMeshJointBoxesNormalIn_)
      {
        UnityEngine.Object.DestroyImmediate(idjointMesh.Second);
      }
      listMeshJointBoxesNormalIn_.Clear();

      foreach( Tuple2<uint,Mesh> idjointMesh in listMeshJointBoxesNormalOut_)
      {
        UnityEngine.Object.DestroyImmediate(idjointMesh.Second);
      }
      listMeshJointBoxesNormalOut_.Clear();

      foreach( Tuple2<uint,Mesh> idjointMesh in listMeshJointBoxesDeformatedIn_)
      {
        UnityEngine.Object.DestroyImmediate(idjointMesh.Second);
      }
      listMeshJointBoxesDeformatedIn_.Clear();

      foreach( Tuple2<uint,Mesh> idjointMesh in listMeshJointBoxesDeformatedOut_)
      {
        UnityEngine.Object.DestroyImmediate(idjointMesh.Second);
      }
      listMeshJointBoxesDeformatedOut_.Clear();
 
      foreach( Tuple2<uint,Mesh> idjointMesh in listMeshJointBoxesBreakingIn_)
      {
        UnityEngine.Object.DestroyImmediate(idjointMesh.Second);
      }
      listMeshJointBoxesBreakingIn_.Clear();

      foreach( Tuple2<uint,Mesh> idjointMesh in listMeshJointBoxesBreakingOut_)
      {
        UnityEngine.Object.DestroyImmediate(idjointMesh.Second);
      }
      listMeshJointBoxesBreakingOut_.Clear();

      foreach( Tuple2<uint,Mesh> idjointMesh in listMeshJointBoxesBrokenIn_)
      {
        UnityEngine.Object.DestroyImmediate(idjointMesh.Second);
      }
      listMeshJointBoxesBrokenIn_.Clear();

      foreach( Tuple2<uint,Mesh> idjointMesh in listMeshJointBoxesBrokenOut_)
      {
        UnityEngine.Object.DestroyImmediate(idjointMesh.Second);
      }
      listMeshJointBoxesBrokenOut_.Clear();
    }

    void OnValidate()
    {
      ClearBodyMeshes();
      ClearSphereMeshes();
      ClearJointMeshes();
    }

    void SetGizmosColor( Color colorStart )
    {
      Color color = colorStart;
      Gizmos.color = color;
    }

    void SetGizmosColor( Color colorStart, float alpha )
    {
      Color color = colorStart;
      color.a = alpha;
      Gizmos.color = color;
    }

    void OnDrawGizmos()
    {
      if (ActiveInEditor)
      {
        float alpha = 0.2f;
        Color currentColor = Gizmos.color;
        if (DrawBodyBoxes)
        {

          SetGizmosColor( Color.green, alpha );
          foreach( Mesh bodyBoxGroup in listMeshBodyBoxesEnabledVisible_)
          {
            Gizmos.DrawWireMesh( bodyBoxGroup );
          }

          SetGizmosColor( Color.blue, alpha );
          foreach( Mesh bodyBoxGroup in listMeshBodyBoxesDisabledVisible_)
          {
            Gizmos.DrawWireMesh( bodyBoxGroup );
          }

          if (ShowInvisibles)
          {
            SetGizmosColor( Color.yellow, alpha );
            foreach( Mesh bodyBoxGroup in listMeshBodyBoxesEnabledHide_)
            {
              Gizmos.DrawWireMesh( bodyBoxGroup);
            }

            SetGizmosColor( Color.red, alpha );
            foreach( Mesh bodyBoxGroup in listMeshBodyBoxesDisabledHide_)
            {
              Gizmos.DrawWireMesh( bodyBoxGroup );
            }
          }

          if (DrawSleepingState)
          {
            SetGizmosColor( Color.magenta, alpha );
            foreach( Mesh bodyBoxGroup in listMeshBodyBoxesSleeping_ )
            {
              Gizmos.DrawWireMesh( bodyBoxGroup );
            }
          }
        }

 
        if (DrawClothColliders)
        {
          SetGizmosColor( new Color(255f/255f,105f/255f,180/255f), alpha * 3.0f );
          foreach( Mesh clothCollider in listMeshClothSpheres_ )
          {
            Gizmos.DrawMesh( clothCollider );
          }
        }

        if (DrawJoints)
        {
          SetGizmosColor(Color.green);
          foreach( Tuple2<uint,Mesh> idJointMesh in listMeshJointBoxesNormalOut_)
          {
            uint id   = idJointMesh.First;

            if ( !drawOnlySelected_ || listJointGroupsIdsSelected_.Contains(id) || listRigidGlueIdsSelected_.Contains(id) )
            {
              Mesh mesh = idJointMesh.Second;
              Gizmos.DrawWireMesh( mesh );
            }
          }

          foreach( Tuple2<uint,Mesh> idJointMesh in listMeshJointBoxesNormalIn_)
          {
            uint id   = idJointMesh.First;

            if ( !drawOnlySelected_ || listJointGroupsIdsSelected_.Contains(id) || listRigidGlueIdsSelected_.Contains(id) )
            {
              Mesh mesh = idJointMesh.Second;
              Gizmos.DrawMesh( mesh );
            }
          }

          SetGizmosColor(Color.yellow);
          foreach( Tuple2<uint,Mesh> idJointMesh in listMeshJointBoxesDeformatedOut_)
          {
            uint id   = idJointMesh.First;

            if ( !drawOnlySelected_ || listJointGroupsIdsSelected_.Contains(id) || listRigidGlueIdsSelected_.Contains(id) )
            {
              Mesh mesh = idJointMesh.Second;
              Gizmos.DrawWireMesh( mesh );
            }
          }

          foreach( Tuple2<uint,Mesh> idJointMesh in listMeshJointBoxesDeformatedIn_)
          {
            uint id   = idJointMesh.First;

            if ( !drawOnlySelected_ || listJointGroupsIdsSelected_.Contains(id) || listRigidGlueIdsSelected_.Contains(id) )
            {
              Mesh mesh = idJointMesh.Second;
              Gizmos.DrawMesh( mesh );
            }
          }

          SetGizmosColor(Color.magenta);
          foreach( Tuple2<uint,Mesh> idJointMesh in listMeshJointBoxesBreakingOut_)
          {
            uint id   = idJointMesh.First;

            if ( !drawOnlySelected_ || listJointGroupsIdsSelected_.Contains(id) || listRigidGlueIdsSelected_.Contains(id) )
            {
              Mesh mesh = idJointMesh.Second;
              Gizmos.DrawWireMesh( mesh );
            }
          }

          foreach( Tuple2<uint,Mesh> idJointMesh in listMeshJointBoxesBreakingIn_)
          {
            uint id   = idJointMesh.First;

            if ( !drawOnlySelected_ || listJointGroupsIdsSelected_.Contains(id) || listRigidGlueIdsSelected_.Contains(id) )
            {
              Mesh mesh = idJointMesh.Second;
              Gizmos.DrawMesh( mesh );
            }
          }

          SetGizmosColor(Color.red);
          foreach( Tuple2<uint,Mesh> idJointMesh in listMeshJointBoxesBrokenOut_)
          {
            uint id   = idJointMesh.First;

            if ( !drawOnlySelected_ || listJointGroupsIdsSelected_.Contains(id) || listRigidGlueIdsSelected_.Contains(id) )
            {
              Mesh mesh = idJointMesh.Second;
              Gizmos.DrawWireMesh( mesh );
            }
          }

          foreach( Tuple2<uint,Mesh> idJointMesh in listMeshJointBoxesBrokenIn_)
          {
            uint id   = idJointMesh.First;

            if ( !drawOnlySelected_ || listJointGroupsIdsSelected_.Contains(id) || listRigidGlueIdsSelected_.Contains(id) )
            {
              Mesh mesh = idJointMesh.Second;
              Gizmos.DrawMesh( mesh );
            }
          }
        }

        SetGizmosColor(currentColor);
      }
    }

  } //class CRSimulationData
} //namespcae Caronte;