using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using CaronteSharp;
using Object = UnityEngine.Object;

using Dict_IdUnityToIdBody   = System.Collections.Generic.Dictionary< int, uint >;
using Dict_IdUnityToBodyInfo = System.Collections.Generic.Dictionary< int, CaronteFX.CRCreationData >;

using Tuple_GameObjectState = CaronteFX.Tuple2< UnityEngine.GameObject, bool >;
using Tuple_MeshMeshUpdater = CaronteFX.Tuple2< UnityEngine.Mesh, CaronteSharp.MeshUpdater >;

namespace CaronteFX
{
  public enum BodyType
  {
    Rigidbody,
    BodyMeshStatic,
    BodyMeshAnimatedByTransform,
    BodyMeshAnimatedByArrPos,
    Softbody,
    Clothbody,
    Ropebody,
    None
  }

  public struct CRBodyCount
  {
    int nRigidBodies_;
  }

  public class CREntityManager
  {
    //-----------------------------------------------------------------------------------
    //--------------------------------DATA MEMBERS---------------------------------------
    //-----------------------------------------------------------------------------------
    CRBiDictionary< uint, GameObject >           tableIdBodyToGO_ = new CRBiDictionary< uint, GameObject >();
    
    Dictionary< uint, BodyType >                 tableIdBodyToType_      = new Dictionary< uint, BodyType >();
    Dictionary< int, List<CommandNode> >         tableIdUnityToListNode_ = new Dictionary< int, List<CommandNode> >();

    Dictionary< CNBody, Dict_IdUnityToIdBody >   tableBodyNodeToDictIdBody_ = new Dictionary< CNBody, Dict_IdUnityToIdBody >();
    Dictionary< CNBody, Dict_IdUnityToBodyInfo > tableBodyNodeToDictInfo_   = new Dictionary< CNBody, Dict_IdUnityToBodyInfo >();

    Dictionary< CNJointGroups, uint >            tableMultiJointToIdJointGroups_ = new Dictionary< CNJointGroups, uint >();
    Dictionary< CNJointGroups, uint >            tableMultiJointToIdServos_      = new Dictionary< CNJointGroups, uint >();
    Dictionary< CNServos, uint>                  tableServosToId_                = new Dictionary< CNServos, uint >();
    Dictionary< CNEntity, uint >                 tableEntityNodeToId_            = new Dictionary< CNEntity, uint >();

    Dictionary< uint, CNBody >                   tableIdBodyToNode_        = new Dictionary< uint, CNBody >();
    Dictionary< uint, CNJointGroups >            tableIdMultiJointToNode_  = new Dictionary< uint, CNJointGroups >();
    Dictionary< uint, CommandNode >              tableIdServosToNode_      = new Dictionary< uint, CommandNode >();
    Dictionary< uint, CNEntity >                 tableIdEntityToNode_      = new Dictionary< uint, CNEntity >();
    //---------------------------------SIMULATION MEMBERS--------------------------------
    Dictionary< GameObject, uint > dictGOtoBodyIdSim_ = new Dictionary< GameObject,uint >();

    Dictionary< uint, UnityEngine.Transform > dictBodyTransformSim_    = new Dictionary< uint, UnityEngine.Transform >();
    Dictionary< uint, Tuple_MeshMeshUpdater > dictBodyMeshRenderSim_   = new Dictionary< uint, Tuple_MeshMeshUpdater >();
    Dictionary< uint, UnityEngine.Mesh >      dictBodyMeshColliderSim_ = new Dictionary< uint, UnityEngine.Mesh >(); 
 
    Dictionary< uint, Tuple2<Mesh, Vector3> >   dictBodyRopeInit_ = new Dictionary< uint, Tuple2<Mesh, Vector3> >();

    List<GameObject>            listAnimatorObjectsBuffering_ = new List<GameObject>();
    List<Tuple_GameObjectState> listGameObjectState_          = new List<Tuple_GameObjectState>();
    //---------------------------------AUX MEMBERS--------------------------------------------------
    Vector2 visibleTimeIntervalAux_  = new Vector2();

    List<uint>       listBodyIdAux_     = new List<uint>();
    List<GameObject> listGameObjectAux_ = new List<GameObject>();
    List<Transform>  listTransformAux_  = new List<Transform>();

    List<Renderer>      listBodyRenderer_ = new List<Renderer>();
    List<MonoBehaviour> listBodyCollider_ = new List<MonoBehaviour>();
    //-----------------------------------------------------------------------------------

    const uint INDEX_NOT_FOUND   = 0xFFFFFFFF;
    const uint INDEX_NOT_ALLOWED = 0x80000000;

    GameObject tmpSimulationGO_;
    GameObject tmpSimulationGO
    {
      get
      {
        if (tmpSimulationGO_ == null)
        {
          tmpSimulationGO_= new GameObject("tmp_CaronteFX_sim");
        }
        return tmpSimulationGO_;
      }
    }

    public int NumberOfBodies
    {
      get { return tableIdBodyToGO_.Count; }
    }

    //-----------------------------------------------------------------------------------
    public void Clear()
    {     
      tableIdBodyToGO_.Clear();
      
      dictGOtoBodyIdSim_      .Clear();
      dictBodyTransformSim_   .Clear();
      dictBodyMeshRenderSim_  .Clear();
      dictBodyMeshColliderSim_.Clear();
      
      tableIdUnityToListNode_.Clear();
      tableIdBodyToType_     .Clear();
      
      tableBodyNodeToDictIdBody_.Clear();
      tableBodyNodeToDictInfo_  .Clear();

      tableMultiJointToIdJointGroups_.Clear();
      tableMultiJointToIdServos_     .Clear();
      tableServosToId_               .Clear();
      tableEntityNodeToId_           .Clear();
      
      tableIdBodyToNode_      .Clear();
      tableIdMultiJointToNode_.Clear();
      tableIdServosToNode_    .Clear();
      tableIdEntityToNode_    .Clear();

      dictBodyRopeInit_.Clear();
      listAnimatorObjectsBuffering_.Clear();
      listGameObjectState_         .Clear();

      listBodyIdAux_.Clear();
    }
    //-----------------------------------------------------------------------------------
    public void UpdateListsBodyGameObject( List<GameObject> listBodyGOEnabledVisible, List<GameObject> listBodyGODisabledVisible,
                                           List<GameObject> listBodyGOEnabledHide,    List<GameObject> listBodyGODisabledHide    )
    {   
      listBodyGOEnabledVisible .Clear();
      listBodyGODisabledVisible.Clear();

      listBodyGOEnabledHide .Clear();
      listBodyGODisabledHide.Clear();

      foreach ( var pair in tableIdBodyToNode_ )
      {
        uint idBody     = pair.Key;
        CNBody bodyNode = pair.Value;

        GameObject go = tableIdBodyToGO_.GetByFirst(idBody);

        if (bodyNode.IsNodeVisibleInHierarchy)
        {
          if (bodyNode.IsNodeEnabledInHierarchy)
          {
            listBodyGOEnabledVisible.Add(go);
          }
          else
          {
            listBodyGODisabledVisible.Add(go);
          }
        }
        else
        {
          if (bodyNode.IsNodeEnabledInHierarchy)
          {
            listBodyGOEnabledHide.Add(go);
          }
          else
          {
            listBodyGODisabledHide.Add(go);
          }
        }
      }
    }
    //-----------------------------------------------------------------------------------
    public void UpdateListClothBodiesGameObjects( List< Tuple2<GameObject, float> > listClothBodyGORadius )
    {   
      listClothBodyGORadius.Clear();

      foreach ( var pair in tableIdBodyToNode_ )
      {
        uint idBody     = pair.Key;
        CNBody bodyNode = pair.Value;
      
        CNCloth clothNode = bodyNode as CNCloth;
        if (clothNode != null && !bodyNode.IsNodeExcludedInHierarchy)
        {
          GameObject go = tableIdBodyToGO_.GetByFirst(idBody);
          listClothBodyGORadius.Add( Tuple2.New( go, clothNode.Cloth_CollisionRadius) );
        }
      }
    }
    //-----------------------------------------------------------------------------------
    public void SaveStateOfBodies()
    {
      SimulationManager.SaveStateRamOfAllBodies();
    }
    //-----------------------------------------------------------------------------------
    public void LoadStateOfBodies()
    {
      SimulationManager.LoadStateRamOfAllBodies();
    }
    //-----------------------------------------------------------------------------------
    public void SaveStateOfJoints()
    {
      SimulationManager.SaveStateRamOfAllJointGroups();
    }
    //-----------------------------------------------------------------------------------
    public void LoadStateOfJoints()
    {
      SimulationManager.LoadStateRamOfAllJointGroups();
    }
    //-----------------------------------------------------------------------------------
    private Dict_IdUnityToBodyInfo GetNodeDictCRBodyInfo(CNBody bodyNode)
    {
      if ( !tableBodyNodeToDictInfo_.ContainsKey(bodyNode) )
      {
        tableBodyNodeToDictInfo_[bodyNode] = new Dict_IdUnityToBodyInfo();
      }
      return tableBodyNodeToDictInfo_[bodyNode];       
    }
    //-----------------------------------------------------------------------------------
    private Dict_IdUnityToIdBody GetNodeDictIdBody(CNBody bodyNode)
    {
      if ( !tableBodyNodeToDictIdBody_.ContainsKey(bodyNode) )
      {
        tableBodyNodeToDictIdBody_[bodyNode] = new Dict_IdUnityToIdBody();
      }
      return tableBodyNodeToDictIdBody_[bodyNode];       
    }
    //-----------------------------------------------------------------------------------
    public void BuildListBodyIdFromBodyNode(CNBody bodyNode, List<uint> listBodyId)
    {
      listBodyId.Clear();
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( bodyNode );
      listBodyId.AddRange( dictBodyInfo.Values );
    }
    //-----------------------------------------------------------------------------------
    public int GetNumberOfBodiesFromBodyNode(CNBody bodyNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( bodyNode );
      return (dictBodyInfo.Count);
    }
    //-----------------------------------------------------------------------------------
    public Vector2 GetVisibleTimeInterval(uint idBody)
    {
      BodyManager.GetVisibleTimeInterval( idBody, ref visibleTimeIntervalAux_ );
      return visibleTimeIntervalAux_;
    }
    //-----------------------------------------------------------------------------------
    //-----------------------BODIES------------------------------------------------------
    //-----------------------------------------------------------------------------------   
    public void SetActivity(CNBody bodyNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( bodyNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      if (bodyNode.IsNodeEnabledInHierarchy)
      {      
        foreach (uint idBody in listBodyIdAux_)
        {
          BodyManager.ConnectBody(idBody);
        }
      }
      else
      {
        foreach (uint idBody in listBodyIdAux_)
        {
          BodyManager.DisconnectBody(idBody, false);
        }
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetVisibility(CNBody bodyNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( bodyNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach(uint idBody in listBodyIdAux_)
      {
        BodyManager.SetVisibility(idBody, bodyNode.IsNodeVisibleInHierarchy);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetMass(CNBody bodyNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( bodyNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      float massClamped = Mathf.Clamp(bodyNode.Mass, 1e-5f, float.MaxValue);
      foreach (uint idBody in listBodyIdAux_)
      {
        BodyManager.SetMass(idBody, massClamped);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetDensity(CNBody bodyNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( bodyNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      float densityClamped = Mathf.Clamp(bodyNode.Density, 1e-2f, float.MaxValue);

      foreach(uint idBody in listBodyIdAux_)
      {
        BodyManager.SetDensity(idBody, densityClamped);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetRestitution(CNBody bodyNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( bodyNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach (uint idBody in listBodyIdAux_)
      {
        BodyManager.SetRestitution(idBody, bodyNode.Restitution_in01);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetFrictionKinetic(CNBody bodyNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( bodyNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach (uint idBody in listBodyIdAux_)
      {
        BodyManager.SetFrictionKinetic(idBody, bodyNode.FrictionKinetic_in01);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetFrictionStatic(CNBody bodyNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( bodyNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach (uint idBody in listBodyIdAux_)
      {
        BodyManager.SetFrictionStatic(idBody, bodyNode.FrictionStatic_in01);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetFrictionStaticFromKinetic(CNBody bodyNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( bodyNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach (uint idBody in listBodyIdAux_)
      {
        BodyManager.SetFrictionStaticFromKinetic(idBody);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetDampingPerSecondWorld(CNBody bodyNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( bodyNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach (uint idBody in listBodyIdAux_)
      {
        BodyManager.SetDampingPerSecond_WORLD(idBody, bodyNode.DampingPerSecond_WORLD);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetExplosionOpacity(CNBody bodyNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( bodyNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach (uint idBody in listBodyIdAux_)
      {
        BodyManager.SetExplosionOpacity(idBody, bodyNode.ExplosionOpacity);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetExplosionResponsiveness(CNBody bodyNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( bodyNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach (uint idBody in listBodyIdAux_)
      {
        BodyManager.SetExplosionResponsiveness(idBody, bodyNode.ExplosionResponsiveness);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetCollisionState(CNBody bodyNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( bodyNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);
  
      bool enableCollision = bodyNode.DoCollide;

      foreach (uint idBody in listBodyIdAux_)
      {
        BodyManager.EnableCollisionWithAll(idBody, enableCollision);
      }

      CNSoftbody sbNode = bodyNode as CNSoftbody;
      CNRope rpNode     = bodyNode as CNRope;
      CNCloth clNode    = bodyNode as CNCloth;

      if (sbNode != null )
      {
        foreach(uint idBody in listBodyIdAux_)
        {
          SoftbodyManager.Sb_enableAutocollision( idBody, sbNode.AutoCollide );
        }
      }
      else if ( rpNode != null )
      {
        foreach(uint idBody in listBodyIdAux_)
        {
          SoftbodyManager.Sb_enableAutocollision( idBody, rpNode.AutoCollide );
        }
      }
      else if (clNode != null )
      {
        foreach (uint idBody in listBodyIdAux_)
        {
          ClothManager.Cl_EnableAutocollision( idBody, clNode.Cloth_AutoCollide );
        }
      }
    }
    //-----------------------------------------------------------------------------------
    //-----------------------RIGID BODIES------------------------------------------------
    //-----------------------------------------------------------------------------------
    public void CreateBody(CNRigidbody rigidNode, GameObject go )
    { 
      bool hasMesh = AddBodyComponent(go);

      uint idBody       = INDEX_NOT_FOUND;
      Mesh meshRender   = null;
      Mesh meshCollider = null;
      bool createConvex = false;
      BodyType bodyType = BodyType.None;

      if (hasMesh)
      {
        GetRenderAndCollider(go, ref meshRender, ref meshCollider, out createConvex);

        bool isLVelocityZero = (rigidNode.VelocityStart == Vector3.zero);
        bool isWVeloctyZero  = (rigidNode.OmegaStart_inRadSeg == Vector3.zero);
        bool isVelocityZero =  isLVelocityZero && isWVeloctyZero;
        SetRigidType(rigidNode.IsFiniteMass, isVelocityZero, ref bodyType);

        if (meshRender != null)
        {
          RgInit rgInit = GetRgInit(rigidNode, go, meshRender, meshCollider);

          if (createConvex)
          {
            idBody = RigidbodyManager.CreateRigidbody_ConvexHull(rgInit, (ENUM_RIGID_MASS_TYPE)bodyType, true);
          }
          else
          {
            idBody = RigidbodyManager.CreateRigidbody(rgInit, (ENUM_RIGID_MASS_TYPE)bodyType, true);
          }

          if (idBody != INDEX_NOT_FOUND && idBody != INDEX_NOT_ALLOWED)
          {
            BodyManager.SetVisibility(idBody, rigidNode.IsNodeVisible);
          }
          else if (idBody == INDEX_NOT_FOUND)
          {
            CRDebug.LogWarning( go.name + " rigid body could not be created." );
          }
          else if ( idBody == INDEX_NOT_ALLOWED)
          {
            CRDebug.LogWarning( "Free version: creation of " + go.name + " rigid body is not allowed in the free version of CaronteFX." );
          }
        }
      }

      AddBody(go, idBody, bodyType, rigidNode, meshRender, meshCollider, createConvex);
    }
    //-----------------------------------------------------------------------------------
    public void DestroyBody(CNRigidbody rigidNode, GameObject go)
    {
      int instanceId = go.GetInstanceID();
      DestroyBody( rigidNode, instanceId ); 
    }
    //-----------------------------------------------------------------------------------
    public void DestroyBody(CNRigidbody rigidNode, int instanceId)
    {
      uint idBody = INDEX_NOT_FOUND;

      Dict_IdUnityToIdBody dictIdUnity = GetNodeDictIdBody(rigidNode);
      if ( dictIdUnity.ContainsKey(instanceId) )
      {
        idBody = dictIdUnity[instanceId];
        RigidbodyManager.DestroyRigid( idBody );
      }

      RemoveBody( instanceId, idBody, rigidNode );   
    }
    //-----------------------------------------------------------------------------------
    public void CheckBodyForChanges(CNRigidbody rigidNode, GameObject go, bool recreateIfInvalid)
    { 
      if (go != null)
      {
        int instanceId = go.GetInstanceID();

        Dict_IdUnityToIdBody dictIdBody = GetNodeDictIdBody(rigidNode);

        if ( dictIdBody.ContainsKey(instanceId) )
        {
          uint idBody = dictIdBody[instanceId];

          Dict_IdUnityToBodyInfo dictBodyInfo = GetNodeDictCRBodyInfo(rigidNode);
          CRCreationData crData = dictBodyInfo[instanceId];
          if ( crData.scale_ != go.transform.lossyScale )
          {
            if ( recreateIfInvalid )
            {
              DestroyBody( rigidNode, go );
              CreateBody( rigidNode, go );
            }

            CheckJointServosImplications( go );
          }
          else if ( recreateIfInvalid && !crData.AreFingerprintsValid() )
          {
            DestroyBody( rigidNode, go );
            CreateBody( rigidNode, go );

            CheckJointServosImplications( go );
          }
          else if ( crData.position_ != go.transform.position || crData.rotation_ != go.transform.rotation )
          {
            
            Matrix4x4 currentTr = go.transform.localToWorldMatrix;
            RigidbodyManager.Rg_locateByModel_WORLD( idBody, ref currentTr );
            
            crData.Update(go.transform.position, go.transform.rotation);
            CheckJointServosImplications( go );
          }
        }
      }
    }
    //-----------------------------------------------------------------------------------
    public RgInit GetRgInit(CNBody bodyNode, GameObject go, Mesh renderMesh, Mesh colliderMesh)
    {
      RgInit rgInit = new RgInit();
      rgInit.name_                    = go.name;

      MeshComplex meshRender_car = new MeshComplex();
      meshRender_car.Set(renderMesh);
      rgInit.meshRender_Model_ = meshRender_car;

      if (renderMesh != colliderMesh)
      {
        MeshComplex meshCollider_car = new MeshComplex();
        meshCollider_car.Set(colliderMesh);
        rgInit.meshCollider_Model_ = meshCollider_car;
      }
      else
      {
        rgInit.meshCollider_Model_ = meshRender_car;
      }

      rgInit.m_MODEL_to_WORLD_ = go.transform.localToWorldMatrix;

      float creationMass    = 0f;
      float creationDensity = 0f;

      if (bodyNode.Mass == -1f)
      {
        creationMass = -1f;
      }
      else
      {
        creationMass = Mathf.Clamp(bodyNode.Mass, 1e-5f, float.MaxValue);
      }

      if (bodyNode.Density == -1f)
      {
        creationDensity = -1f;
      }
      else
      {
        creationDensity = Mathf.Clamp(bodyNode.Density, 1e-2f, float.MaxValue);
      }

      rgInit.mass_                    = creationMass;
      rgInit.density_                 = creationDensity;
      rgInit.restitution_             = bodyNode.Restitution_in01;
      rgInit.frictionKinetic_         = bodyNode.FrictionKinetic_in01;

      if (bodyNode.FromKinetic)
      {
        rgInit.frictionStatic_          = -1;
      }
      else
      {
        rgInit.frictionStatic_          = bodyNode.FrictionStatic_in01;
      }

      rgInit.gravity_                 = bodyNode.Gravity;
      rgInit.dampingPerSecond_WORLD_  = bodyNode.DampingPerSecond_WORLD;
      rgInit.velocityStart_           = bodyNode.VelocityStart;
      rgInit.omegaStart_              = bodyNode.OmegaStart_inDegSeg * Mathf.Deg2Rad;
      rgInit.explosionOpacity_        = bodyNode.ExplosionOpacity;
      rgInit.explosionResponsiveness_ = bodyNode.ExplosionResponsiveness;

      return rgInit;
    }
    //-----------------------------------------------------------------------------------
    private void SetRigidType(bool isFiniteMass, bool isVelocityZero, ref BodyType bodyType)
    {
      if (isFiniteMass)
      {
        bodyType = BodyType.Rigidbody;
      }
      else
      {
        if (isVelocityZero)
        {
          bodyType = BodyType.BodyMeshStatic;
        }
        else
        {
          bodyType = BodyType.BodyMeshAnimatedByTransform;
        }
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetVelocity(CNRigidbody rigidNode)
    {
      Vector3 velocityStart = rigidNode.VelocityStart;
      Vector3 omegaStart    = rigidNode.OmegaStart_inDegSeg * Mathf.Deg2Rad;

      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( rigidNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach (uint idBody in listBodyIdAux_)
      {
        RigidbodyManager.Rg_velocitateByCm_WORLD(idBody, ref velocityStart, ref omegaStart);
      }

      if (rigidNode.IsFiniteMass)
      {
        if (velocityStart == Vector3.zero && omegaStart == Vector3.zero )
        {
          foreach (uint idBody in listBodyIdAux_)
          {
            SetBodyType(idBody, BodyType.BodyMeshStatic);
          }
        }
        else
        {
          foreach (uint idBody in listBodyIdAux_)
          {
            SetBodyType(idBody, BodyType.BodyMeshAnimatedByTransform);
          }
        }
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetResponsiveness(CNRigidbody rigidNode, GameObject[] arrGameObject)
    {
      bool responsive = rigidNode.IsFiniteMass;

      ENUM_RIGID_MASS_TYPE rigidMassType;
      BodyType bodyType;

      if (responsive)
      {
        rigidMassType = ENUM_RIGID_MASS_TYPE.FINITE_MASS;
        bodyType = BodyType.Rigidbody;
      }
      else
      {
        rigidMassType = ENUM_RIGID_MASS_TYPE.INFINITE_MASS_STATIC;
        bodyType = BodyType.BodyMeshStatic;
      }

      foreach (GameObject go in arrGameObject)
      {
        uint idBody = GetIdBodyFromGo(go);

        if (idBody != INDEX_NOT_FOUND)
        {
          bool couldBeDone = RigidbodyManager.Rg_changeMassMode(idBody, rigidMassType);
          if (couldBeDone)
          {
            SetBodyType(idBody, bodyType);  
          }
          else
          {
            DestroyBody(rigidNode, go);
          }
        }
        else
        {
          CreateBody(rigidNode, go);
        }             
      }
    }
    //-----------------------------------------------------------------------------------
    //-----------------------ANIMATED BODIES---------------------------------------------
    //-----------------------------------------------------------------------------------
    public void CreateBody(CNAnimatedbody animatedNode, GameObject go)
    {
      bool byArrPos;
      Mesh meshCollider;
      BodyType bodyType = BodyType.None;

      uint idBody = CreateBody(animatedNode, go, out byArrPos, out meshCollider);

      if (idBody != INDEX_NOT_FOUND && idBody != INDEX_NOT_ALLOWED)
      {
        Transform goTransform = go.transform;
        goTransform.hasChanged = false;

        if (byArrPos)
        {
          bodyType = BodyType.BodyMeshAnimatedByArrPos;
        }
        else
        {
          bodyType = BodyType.BodyMeshAnimatedByTransform;      
        }

        BodyManager.SetVisibility( idBody, animatedNode.IsNodeVisible );
      }

      AddBody( go, idBody, bodyType, animatedNode, meshCollider, meshCollider, false);
    }
    //----------------------------------------------------------------------------------
    private uint CreateBody(CNAnimatedbody animatedNode, GameObject go, out bool byArrPos, out Mesh meshCollider)
    {
      uint idBody  = INDEX_NOT_FOUND;
      byArrPos     = false;
      meshCollider = null;
      bool hasMesh = AddBodyComponent(go);

      if (hasMesh)
      {
        byArrPos = go.GetBakedMesh(out meshCollider);

        if (meshCollider != null)
        {
          RgInit rgInit = GetRgInit(animatedNode, go, meshCollider, meshCollider);
          ENUM_RIGID_MASS_TYPE creationMode;
          if (byArrPos)
          {
            UnityEngine.Object.DestroyImmediate(meshCollider);
            creationMode = ENUM_RIGID_MASS_TYPE.INFINITE_MASS_ANIMATED_BY_ARRPOS;
          }
          else
          {
            creationMode = ENUM_RIGID_MASS_TYPE.INFINITE_MASS_ANIMATED_BY_MATRIX;
          }

          idBody = RigidbodyManager.CreateRigidbody(rgInit, creationMode, true);
        }

        if ( idBody  == INDEX_NOT_FOUND )
        {
          CRDebug.LogWarning( go.name + " animated body could not be created." );
        }
        else if ( idBody == INDEX_NOT_ALLOWED )
        {
          CRDebug.LogWarning( "Free version: creation of " + go.name + " animated body is not allowed in the free version of CaronteFX." );
        }
      }

      return (idBody);
    }
    //-----------------------------------------------------------------------------------
    public void CheckBodyForChanges(CNAnimatedbody animatedNode, GameObject go, bool recreateIfInvalid)
    { 
      if (go != null)
      {
        int instanceId = go.GetInstanceID();

        Dict_IdUnityToIdBody   dictIdBody   = GetNodeDictIdBody(animatedNode);

        if ( dictIdBody.ContainsKey(instanceId) )
        {
          Dict_IdUnityToBodyInfo dictBodyInfo = GetNodeDictCRBodyInfo(animatedNode);
          CRCreationData creationData = dictBodyInfo[instanceId];

          if ( creationData.scale_ != go.transform.lossyScale || creationData.position_ != go.transform.position 
                || creationData.rotation_ != go.transform.rotation )
          {
            if( recreateIfInvalid )
            {
              DestroyBody( animatedNode, go );
              CreateBody( animatedNode, go );
            }

            CheckJointServosImplications( go );
          }
          else if ( recreateIfInvalid && !creationData.AreFingerprintsValid() )
          {
            DestroyBody( animatedNode, go );
            CreateBody( animatedNode, go );

            CheckJointServosImplications( go );
          }
        }
      }
    }
    //-----------------------------------------------------------------------------------
    //-----------------------SOFT BODIES------------------------------------------------
    //-----------------------------------------------------------------------------------
    public void CreateBody( CNSoftbody sbNode, GameObject go )
    {
      Mesh meshRender, meshCollider;
      BodyType bodyType = BodyType.None;
      uint idBody = CreateBody(sbNode, go, out meshRender, out meshCollider);
      if (idBody != INDEX_NOT_FOUND && idBody != INDEX_NOT_ALLOWED)
      {
        Transform goTransform = go.transform;
        goTransform.hasChanged = false;

        bodyType = BodyType.Softbody;           
        BodyManager.SetVisibility( idBody, sbNode.IsNodeVisible );
      }

      AddBody(go, idBody, bodyType, sbNode, meshRender, meshCollider, false);  
    }
    //-----------------------------------------------------------------------------------
    private uint CreateBody(CNSoftbody sbnode, GameObject go, out Mesh meshRender, out Mesh meshCollider)
    {
      uint idBody  = INDEX_NOT_FOUND;
      bool hasMesh = AddBodyComponent(go);
      meshRender   = null;
      meshCollider = null;
      
      if (hasMesh)
      {
        bool createConvex;
        GetRenderAndCollider(go, ref meshRender, ref meshCollider, out createConvex);

        if (meshRender != null && meshCollider != null)
        {
          SbInit sbInit = GetSbInit(sbnode, go, meshRender, meshCollider);
          idBody = SoftbodyManager.CreateSoftbody(sbInit, true);

          if (idBody == INDEX_NOT_FOUND)
          {
            CRDebug.LogWarning( go.name + " soft body could not be created." );
          }
          else if ( idBody == INDEX_NOT_ALLOWED )
          {
            CRDebug.LogWarning( "Free version: creation of " + go.name + " soft body is not allowed in the free version of CaronteFX." );
          }

        }
      }

      return (idBody);
    }
    //-----------------------------------------------------------------------------------
    public void DestroyBody(CNSoftbody sbNode, GameObject go)
    {
      int instanceId = go.GetInstanceID();
      DestroyBody(sbNode, instanceId);
    }
    //-----------------------------------------------------------------------------------
    public void DestroyBody(CNSoftbody sbNode, int instanceId)
    {
      uint idBody = INDEX_NOT_FOUND;
      Dict_IdUnityToIdBody dictIdUnity = GetNodeDictIdBody(sbNode);

      if ( dictIdUnity.ContainsKey(instanceId) )
      {
        idBody = dictIdUnity[instanceId];
        SoftbodyManager.DestroySoftbody(idBody);
      }
      RemoveBody(instanceId, idBody, sbNode);
    }
    //-----------------------------------------------------------------------------------
    public void CheckBodyForChanges(CNSoftbody sbNode, GameObject go, bool recreateIfInvalid)
    { 
      if ( go != null )
      {
        int instanceId = go.GetInstanceID();
        Dict_IdUnityToIdBody   dictIdBody   = GetNodeDictIdBody(sbNode);

        if ( dictIdBody.ContainsKey(instanceId) )
        {
          uint idBody           = dictIdBody[instanceId];

          Dict_IdUnityToBodyInfo dictBodyInfo = GetNodeDictCRBodyInfo(sbNode);
          CRCreationData crData = dictBodyInfo[instanceId];

          if ( crData.scale_ != go.transform.lossyScale )
          {
            if ( recreateIfInvalid )
            {
              DestroyBody( sbNode, go );
              CreateBody( sbNode, go );
            }

            CheckJointServosImplications( go );
          }
          else if ( recreateIfInvalid && !crData.AreFingerprintsValid() )
          {
            DestroyBody( sbNode, go );
            CreateBody( sbNode, go );

            CheckJointServosImplications( go );
          }
          else if ( crData.position_ != go.transform.position || crData.rotation_ != go.transform.rotation )
          {
            if ( recreateIfInvalid )
            {
              Matrix4x4 currentTr = go.transform.localToWorldMatrix;
              SoftbodyManager.Sb_locateByModel_WORLD( idBody, ref currentTr );
              crData.Update(go.transform.position, go.transform.rotation);
            }
 
            CheckJointServosImplications( go );
          }

        }
      }
    }
    //-----------------------------------------------------------------------------------
    public SbInit GetSbInit(CNSoftbody sbNode, GameObject go, Mesh meshRender, Mesh meshCollider)
    {
      SbInit sbInit = new SbInit();

      sbInit.name_                    = go.name;

      MeshComplex meshRender_car = new MeshComplex();
      meshRender_car.Set(meshRender);
      sbInit.meshRender_Model_ = meshRender_car;

      if (meshCollider != meshRender)
      {
        MeshComplex meshCollider_car = new MeshComplex();
        meshCollider_car.Set(meshCollider);
        sbInit.meshCollider_Model_ = meshCollider_car;
      }
      else
      {
        sbInit.meshCollider_Model_ = meshRender_car;
      }

      sbInit.m_MODEL_to_WORLD_        = go.transform.localToWorldMatrix;

      float creationMass    = 0f;
      float creationDensity = 0f;

      if (sbNode.Mass == -1f)
      {
        creationMass = -1f;
      }
      else
      {
        creationMass = Mathf.Clamp(sbNode.Mass, 1e-5f, float.MaxValue);
      }

      if (sbNode.Density == -1f)
      {
        creationDensity = -1f;
      }
      else
      {
        creationDensity = Mathf.Clamp(sbNode.Density, 1e-2f, float.MaxValue);
      }

      sbInit.mass_                    = creationMass;
      sbInit.density_                 = creationDensity;

      sbInit.resolution_              = (uint)sbNode.Resolution;
      sbInit.autoTessellation_        = false;

      float creationLengthStiffness = Mathf.Clamp(sbNode.LengthStiffness, 0.01f, 30f);
      float creationVolumeStiffness = Mathf.Clamp(sbNode.VolumeStiffness, 0.01f, float.MaxValue);

      sbInit.lengthStiffness_         = creationLengthStiffness;
      sbInit.volumeStiffness_         = creationVolumeStiffness;

      sbInit.areaStiffness_           = sbNode.AreaStiffness;

      sbInit.plasticity_              = sbNode.Plasticity;
      sbInit.threshold_in01_          = sbNode.Threshold_in01;

      sbInit.acquired_in01_           = sbNode.Acquired_in01;
      sbInit.compressionLimit_in01_   = sbNode.CompressionLimit_in01;
      sbInit.expansionLimit_in_1_100_ = sbNode.ExpansionLimit_in_1_100;

      sbInit.dampingPerSecond_CM_     = sbNode.DampingPerSecond_CM;
      sbInit.restitution_in01_        = sbNode.Restitution_in01;    
      sbInit.frictionKinetic_in01_    = sbNode.FrictionKinetic_in01;

      if (sbNode.FromKinetic)
      {
        sbInit.frictionStatic_in01_ = -1;
      }
      else
      {
        sbInit.frictionStatic_in01_ = sbNode.FrictionStatic_in01;
      }

      sbInit.gravity_                 = sbNode.Gravity;

      sbInit.dampingPerSecond_WORLD_  = sbNode.DampingPerSecond_WORLD;

      sbInit.velocityStart_           = sbNode.VelocityStart;
      sbInit.omegaStart_inRadSeg_     = sbNode.OmegaStart_inDegSeg * Mathf.Deg2Rad;

      sbInit.explosionOpacity_        = sbNode.ExplosionOpacity;
      sbInit.explosionResponsiveness_ = sbNode.ExplosionResponsiveness;

      return sbInit;
    }
     //-----------------------------------------------------------------------------------
    public void SetResolution(CNSoftbody sbNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( sbNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      int nBodies = listBodyIdAux_.Count;
      StringBuilder strBuilder = new StringBuilder();
      for (int i = 0; i < nBodies; i++)
      {     
        int bodyNumber = i + 1;
        strBuilder.AppendFormat("Changing resolution of {0} sofbodies. {1} of {2}", sbNode.Name, bodyNumber, nBodies);
        EditorUtility.DisplayProgressBar( "Softbodies resolution", strBuilder.ToString(), (float)i / (float)nBodies);
   
        uint idBody = listBodyIdAux_[i];
        SoftbodyManager.Sb_setResolution(idBody, (uint)sbNode.Resolution);
      }
      EditorUtility.ClearProgressBar();
    }
    //-----------------------------------------------------------------------------------
    public void SetLengthStiffness(CNSoftbody sbNode)
    {
      float lengthStiffness = Mathf.Clamp(sbNode.LengthStiffness, 0.01f, 30f);

      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( sbNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach (uint idBody in listBodyIdAux_)
      {
        SoftbodyManager.Sb_setLengthStiffness(idBody, lengthStiffness);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetVolumeStiffness(CNSoftbody sbNode)
    {
      float volumeStiffness = Mathf.Clamp(sbNode.VolumeStiffness, 0.01f, float.MaxValue);

      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( sbNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach (uint idBody in listBodyIdAux_)
      {
        SoftbodyManager.Sb_setVolumeStiffness(idBody, volumeStiffness);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetPlasticity(CNSoftbody sbNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( sbNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach (uint idBody in listBodyIdAux_)
      {
        SoftbodyManager.Sb_setPlasticity(idBody, sbNode.Plasticity);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetThreshold(CNSoftbody sbNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( sbNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach (uint idBody in listBodyIdAux_)
      {
        SoftbodyManager.Sb_setPlasticityThreshold(idBody, sbNode.Threshold_in01);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetCompressionLimit(CNSoftbody sbNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( sbNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach (uint idBody in listBodyIdAux_)
      {
        SoftbodyManager.Sb_setPlasticityCompressionLimit(idBody, sbNode.CompressionLimit_in01);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetExpansionLimit(CNSoftbody sbNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( sbNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach (uint idBody in listBodyIdAux_)
      {
        SoftbodyManager.Sb_setPlasticityExpansionLimit(idBody, sbNode.ExpansionLimit_in_1_100);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetAcquired(CNSoftbody sbNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( sbNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach (uint idBody in listBodyIdAux_)
      {
        SoftbodyManager.Sb_setPlasticityAcquired(idBody, sbNode.Acquired_in01);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetInternalDamping(CNSoftbody sbNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( sbNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach (uint idBody in listBodyIdAux_)
      {
        SoftbodyManager.SetDampingPerSecond_CM(idBody, sbNode.DampingPerSecond_CM);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetVelocity(CNSoftbody sbNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( sbNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);
      
      Vector3 velocity = sbNode.VelocityStart;
      Vector3 omega    = sbNode.OmegaStart_inDegSeg * Mathf.Deg2Rad;

      foreach (uint idBody in listBodyIdAux_)
      {
        SoftbodyManager.Sb_velocitateByCm_WORLD(idBody, ref velocity, ref omega);
      }
    }
    //-----------------------------------------------------------------------------------
    //--------------------------------CLOTH----------------------------------------------
    //-----------------------------------------------------------------------------------
    public void CreateBody( CNCloth clNode, GameObject go )
    {
      Mesh meshRender, meshCollider;
      BodyType bodyType = BodyType.None;

      uint idBody = CreateBody(clNode, go, out meshRender, out meshCollider);
      if (idBody != INDEX_NOT_FOUND && idBody != INDEX_NOT_ALLOWED)
      {
        Transform goTransform = go.transform;
        goTransform.hasChanged = false;

        bodyType = BodyType.Clothbody;           
        BodyManager.SetVisibility( idBody, clNode.IsNodeVisible );
      }

      AddBody(go, idBody, bodyType, clNode, meshRender, meshCollider, false);  
    }
    //-----------------------------------------------------------------------------------
    private uint CreateBody(CNCloth clNode, GameObject go, out Mesh meshRender, out Mesh meshCollider)
    {
      uint idBody  = INDEX_NOT_FOUND;
      bool hasMesh = AddBodyComponent(go);

      meshRender   = null;
      meshCollider = null;
      
      if (hasMesh)
      {
        bool createConvex;
        GetRenderAndCollider(go, ref meshRender, ref meshCollider, out createConvex);

        if (meshRender != null && meshCollider != null)
        {
          ClInit clInit = GetClInit(clNode, go, meshRender, meshCollider);
          idBody = ClothManager.CreateCloth(clInit, true);

          if (idBody == INDEX_NOT_FOUND)
          {
            CRDebug.LogWarning( go.name + " cloth body could not be created." );
          }
          else if (idBody == INDEX_NOT_ALLOWED)
          {
            CRDebug.LogWarning( "Free version: creation of " + go.name + " cloth body is not allowed in the free version of CaronteFX." );
          }
        }
      }

      return (idBody);
    }
    //-----------------------------------------------------------------------------------
    public void DestroyBody(CNCloth clNode, GameObject go)
    {
      int instanceId = go.GetInstanceID();
      DestroyBody(clNode, instanceId);
    }
    //-----------------------------------------------------------------------------------
    public void DestroyBody(CNCloth clNode, int instanceId)
    {
      uint idBody = INDEX_NOT_FOUND;
      Dict_IdUnityToIdBody dictIdUnity = GetNodeDictIdBody(clNode);

      if ( dictIdUnity.ContainsKey(instanceId) )
      {
        idBody = dictIdUnity[instanceId];
        ClothManager.DestroyCloth(idBody);
      }
      RemoveBody(instanceId, idBody, clNode);
    }
    //-----------------------------------------------------------------------------------
    public void CheckBodyForChanges(CNCloth clNode, GameObject go, bool recreateIfInvalid)
    { 
      if ( go != null )
      {
        int instanceId = go.GetInstanceID();
        Dict_IdUnityToIdBody   dictIdBody   = GetNodeDictIdBody(clNode);

        if ( dictIdBody.ContainsKey(instanceId) )
        {
          uint idBody = dictIdBody[instanceId];

          Dict_IdUnityToBodyInfo dictBodyInfo = GetNodeDictCRBodyInfo(clNode);
          CRCreationData crData = dictBodyInfo[instanceId];

          if ( crData.scale_ != go.transform.lossyScale )
          {
            if ( recreateIfInvalid )
            {
              DestroyBody( clNode, go );
              CreateBody(  clNode, go );
            }

            CheckJointServosImplications( go );
          }
          else if ( recreateIfInvalid && !crData.AreFingerprintsValid() )
          {
            DestroyBody( clNode, go );
            CreateBody(  clNode, go );

            CheckJointServosImplications( go );
          }
          else if ( crData.position_ != go.transform.position || crData.rotation_ != go.transform.rotation )
          {
            if ( recreateIfInvalid )
            {
              Matrix4x4 currentTr = go.transform.localToWorldMatrix;
              ClothManager.Cl_LocateByModel_WORLD( idBody, ref currentTr );
              crData.Update(go.transform.position, go.transform.rotation);
            }
 
            CheckJointServosImplications( go );
          }
        }
      }
    }
    //-----------------------------------------------------------------------------------
    public ClInit GetClInit(CNCloth clNode, GameObject go, Mesh meshRender, Mesh meshCollider)
    {
      ClInit clInit = new ClInit();

      clInit.name_ = go.name;

      MeshComplex meshRender_car = new MeshComplex();
      meshRender_car.Set(meshRender);
      clInit.meshRender_Model_ = meshRender_car;

      if (meshCollider != meshRender)
      {
        MeshComplex meshCollider_car = new MeshComplex();
        meshCollider_car.Set(meshCollider);
        clInit.meshCollider_Model_ = meshCollider_car;
      }
      else
      {
        clInit.meshCollider_Model_ = meshRender_car;
      }

      clInit.m_MODEL_to_WORLD_ = go.transform.localToWorldMatrix;

      float creationMass    = 0f;
      float creationDensity = 0f;

      if (clNode.Mass == -1f)
      {
        creationMass = -1f;
      }
      else
      {
        creationMass = Mathf.Clamp(clNode.Mass, 1e-5f, float.MaxValue);
      }

      if (clNode.Density == -1f)
      {
        creationDensity = -1f;
      }
      else
      {
        creationDensity = Mathf.Clamp(clNode.Density, 1e-2f, float.MaxValue);
      }

      clInit.mass_    = creationMass;
      clInit.density_ = creationDensity;

      clInit.cloth_bend_            = Mathf.Clamp( clNode.Cloth_Bend, 0.001f, float.MaxValue );
      clInit.cloth_stretch_         = clNode.Cloth_Stretch;
      clInit.cloth_dampingBend_     = clNode.Cloth_DampingBend;
      clInit.cloth_dampingStretch_  = clNode.Cloth_DampingStretch;
      clInit.cloth_collisionRadius_ = clNode.Cloth_CollisionRadius;

      clInit.disableNearBallColWhenJoined_ = clNode.DisableCollisionNearJoints;
  
      clInit.restitution_in01_      = clNode.Restitution_in01;
      clInit.frictionKinetic_in01_  = clNode.FrictionKinetic_in01;

      if (clNode.FromKinetic)
      {
        clInit.frictionStatic_in01_ = -1;
      }
      else
      {
        clInit.frictionStatic_in01_ = clNode.FrictionStatic_in01;
      }

      clInit.gravity_                 = clNode.Gravity;

      clInit.dampingPerSecond_WORLD_  = clNode.DampingPerSecond_WORLD;

      clInit.velocityStart_           = clNode.VelocityStart;
      clInit.omegaStart_inRadSeg_     = clNode.OmegaStart_inDegSeg * Mathf.Deg2Rad;

      clInit.explosionOpacity_        = clNode.ExplosionOpacity;
      clInit.explosionResponsiveness_ = clNode.ExplosionResponsiveness;

      return clInit;
    }
    //-----------------------------------------------------------------------------------
    public void SetVelocity(CNCloth clNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( clNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);
      
      Vector3 velocity = clNode.VelocityStart;
      Vector3 omega    = clNode.OmegaStart_inDegSeg * Mathf.Deg2Rad;

      foreach (uint idBody in listBodyIdAux_)
      {
        ClothManager.Cl_VelocitateByCm_WORLD(idBody, ref velocity, ref omega);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetBendStretch(CNCloth clNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( clNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);
      
      float bend    = Mathf.Clamp( clNode.Cloth_Bend, 0.001f, float.MaxValue );
      float stretch = clNode.Cloth_Stretch;

      foreach (uint idBody in listBodyIdAux_)
      {
        ClothManager.Cl_ReadjustStretchBend(idBody, stretch, bend);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetDampingStretchBend(CNCloth clNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( clNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);
      
      float dampingBend    = clNode.Cloth_DampingBend;
      float dampingStretch = clNode.Cloth_DampingStretch;

      foreach (uint idBody in listBodyIdAux_)
      {
        ClothManager.Cl_ReadjustDampingsStretchBend(idBody, dampingBend, dampingStretch);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetDisableCollisionNearJoints(CNCloth clNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( clNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);
      
      bool disableCollisionNearJoints = clNode.DisableCollisionNearJoints;

      foreach (uint idBody in listBodyIdAux_)
      {
        ClothManager.Cl_setDoDisableBalltreeBallsByJoints(idBody, disableCollisionNearJoints);
      }
    }
    //-----------------------------------------------------------------------------------
    //--------------------------------ROPES----------------------------------------------
    //-----------------------------------------------------------------------------------
    public void CreateBody( CNRope rpNode, GameObject go )
    {
      Mesh meshDefinition, meshTile;
      BodyType bodyType = BodyType.None;

      uint idBody = CreateBody( rpNode, go, out meshDefinition, out meshTile );
      if (idBody != INDEX_NOT_FOUND && idBody != INDEX_NOT_ALLOWED)
      {
        Transform goTransform = go.transform;
        goTransform.hasChanged = false;

        bodyType = BodyType.Ropebody;           
        BodyManager.SetVisibility( idBody, rpNode.IsNodeVisible );
      }

      AddBody(go, idBody, bodyType, rpNode, meshDefinition, meshTile, false);  
    }
    //-----------------------------------------------------------------------------------
    private uint CreateBody(CNRope rpNode, GameObject go, out Mesh meshDefinition, out Mesh meshTile)
    {
      uint idBody  = INDEX_NOT_FOUND;
      bool hasMesh = AddBodyComponent(go);

      meshDefinition  = null;
      meshTile        = null;
      
      if (hasMesh)
      {
        GetDefinitionAndTileMeshes(go, ref meshDefinition, ref meshTile);

        if (meshDefinition != null)
        {
          RpInit rpInit = GetRpInit(rpNode, go, meshDefinition, meshTile);
          idBody = RopeManager.CreateRope(rpInit, true);

          if (idBody == INDEX_NOT_FOUND)
          {
            CRDebug.LogWarning( go.name + " rope body could not be created." );
          }
          else if ( idBody == INDEX_NOT_ALLOWED )
          {
            CRDebug.LogWarning( "Free version: creation of " + go.name + " rope body is not allowed in the free version of CaronteFX." );
          }
        }
      }

      return (idBody);
    }
    //-----------------------------------------------------------------------------------
    public void DestroyBody(CNRope rpNode, GameObject go)
    {
      int instanceId = go.GetInstanceID();
      DestroyBody(rpNode, instanceId);
    }
    //-----------------------------------------------------------------------------------
    public void DestroyBody(CNRope rpNode, int instanceId)
    {
      uint idBody = INDEX_NOT_FOUND;
      Dict_IdUnityToIdBody dictIdUnity = GetNodeDictIdBody(rpNode);

      if ( dictIdUnity.ContainsKey(instanceId) )
      {
        idBody = dictIdUnity[instanceId];
        SoftbodyManager.DestroySoftbody(idBody);
      }
      RemoveBody(instanceId, idBody, rpNode);
    }
    //-----------------------------------------------------------------------------------
    public void CheckBodyForChanges(CNRope rpNode, GameObject go, bool recreateIfInvalid)
    { 
      if ( go != null )
      {
        int instanceId = go.GetInstanceID();
        Dict_IdUnityToIdBody   dictIdBody   = GetNodeDictIdBody(rpNode);

        if ( dictIdBody.ContainsKey(instanceId) )
        {
          uint idBody           = dictIdBody[instanceId];

          Dict_IdUnityToBodyInfo dictBodyInfo = GetNodeDictCRBodyInfo(rpNode);
          CRCreationData crData = dictBodyInfo[instanceId];

          if ( crData.scale_ != go.transform.lossyScale )
          {
            if ( recreateIfInvalid )
            {
              DestroyBody( rpNode, go );
              CreateBody( rpNode, go );
            }

            CheckJointServosImplications( go );
          }
          else if ( recreateIfInvalid && !crData.AreFingerprintsValid() )
          {
            DestroyBody( rpNode, go );
            CreateBody( rpNode, go );

            CheckJointServosImplications( go );
          }
          else if ( crData.position_ != go.transform.position || crData.rotation_ != go.transform.rotation )
          {
            if ( recreateIfInvalid )
            {
              Matrix4x4 currentTr = go.transform.localToWorldMatrix;
              SoftbodyManager.Sb_locateByModel_WORLD( idBody, ref currentTr );
              crData.Update(go.transform.position, go.transform.rotation);
            }
 
            CheckJointServosImplications( go );
          }

        }
      }
    }
    //-----------------------------------------------------------------------------------
    public RpInit GetRpInit(CNRope rpNode, GameObject go, Mesh meshDefinition, Mesh meshTile)
    {
      RpInit rpInit = new RpInit();

      rpInit.name_ = go.name;

      MeshComplex meshDefinition_car = new MeshComplex();
      meshDefinition_car.Set(meshDefinition);
      rpInit.meshDefinition_Model_ = meshDefinition_car;

      if (meshTile != null)
      {
        MeshComplex meshTile_car = new MeshComplex();
        meshTile_car.Set(meshTile);
        rpInit.meshTile_MODEL_ = meshTile_car;
      }
      else
      {
        rpInit.meshTile_MODEL_ = null;
      }


      rpInit.m_MODEL_to_WORLD_ = go.transform.localToWorldMatrix;

      float creationMass    = 0f;
      float creationDensity = 0f;

      if (rpNode.Mass == -1f)
      {
        creationMass = -1f;
      }
      else
      {
        creationMass = Mathf.Clamp(rpNode.Mass, 1e-5f, float.MaxValue);
      }

      if (rpNode.Density == -1f)
      {
        creationDensity = -1f;
      }
      else
      {
        creationDensity = Mathf.Clamp(rpNode.Density, 1e-2f, float.MaxValue);
      }

      rpInit.mass_    = creationMass;
      rpInit.density_ = creationDensity;

      rpInit.sides_   = (uint)rpNode.Sides;

      rpInit.stretch_ = rpNode.Stretch;
      rpInit.bend_    = rpNode.Bend;
      rpInit.torsion_ = rpNode.Torsion;

      rpInit.dampingPerSecond_CM_ = rpNode.DampingPerSecond_CM;
  
      rpInit.restitution_in01_      = rpNode.Restitution_in01;
      rpInit.frictionKinetic_in01_  = rpNode.FrictionKinetic_in01;

      if (rpNode.FromKinetic)
      {
        rpInit.frictionStatic_in01_ = -1;
      }
      else
      {
        rpInit.frictionStatic_in01_ = rpNode.FrictionStatic_in01;
      }

      rpInit.gravity_                 = rpNode.Gravity;

      rpInit.dampingPerSecond_WORLD_  = rpNode.DampingPerSecond_WORLD;

      rpInit.velocityStart_           = rpNode.VelocityStart;
      rpInit.omegaStart_inRadSeg_     = rpNode.OmegaStart_inDegSeg * Mathf.Deg2Rad;

      rpInit.explosionOpacity_        = rpNode.ExplosionOpacity;
      rpInit.explosionResponsiveness_ = rpNode.ExplosionResponsiveness;

      return rpInit;
    }
    //-----------------------------------------------------------------------------------
    public void SetStretchTorsionBend(CNRope rpNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( rpNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);
      
      float stretch = rpNode.Stretch;
      float torsion = rpNode.Torsion;
      float bend    = rpNode.Bend;

      foreach (uint idBody in listBodyIdAux_)
      {
        RopeManager.ReadjustRopeConstants( idBody, stretch, torsion, bend );
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetVelocity(CNRope rpNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( rpNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);
      
      Vector3 velocity = rpNode.VelocityStart;
      Vector3 omega    = rpNode.OmegaStart_inDegSeg * Mathf.Deg2Rad;

      foreach (uint idBody in listBodyIdAux_)
      {
        SoftbodyManager.Sb_velocitateByCm_WORLD(idBody, ref velocity, ref omega);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetInternalDamping(CNRope rpNode)
    {
      Dict_IdUnityToIdBody dictBodyInfo = GetNodeDictIdBody( rpNode );

      listBodyIdAux_.Clear();
      listBodyIdAux_.AddRange(dictBodyInfo.Values);

      foreach (uint idBody in listBodyIdAux_)
      {
        SoftbodyManager.SetDampingPerSecond_CM(idBody, rpNode.DampingPerSecond_CM);
      }
    }
    //-----------------------------------------------------------------------------------
    //-----------------------MULTI JOINTS------------------------------------------------
    //-----------------------------------------------------------------------------------
    public void CreateMultiJoint( CNJointGroups mjNode, GameObject[] arrObjectsA, GameObject[] arrObjectsB, Vector3[] arrLocatorsC, bool fieldAIsReallyEmpty, bool fieldBIsReallyEmpty )
    {
      uint[] arrIdBody_A = GetListIdBodyFromArrGo( arrObjectsA ).ToArray();
      uint[] arrIdBody_B = GetListIdBodyFromArrGo( arrObjectsB ).ToArray();

      CheckBodiesForChanges(arrIdBody_A);
      CheckBodiesForChanges(arrIdBody_B);

      uint idJointGroups = CreateJointGroups( mjNode, arrObjectsA, arrObjectsB, arrLocatorsC, fieldAIsReallyEmpty, fieldBIsReallyEmpty );
      if ( idJointGroups != INDEX_NOT_FOUND )
      {
        tableIdMultiJointToNode_[idJointGroups] = mjNode;
        tableMultiJointToIdJointGroups_[mjNode] = idJointGroups;

        SetVisibility( mjNode );

        mjNode.NeedsUpdate = false;
        EditorUtility.SetDirty( mjNode );    
      }     

      SceneView.RepaintAll();
    }
    //-----------------------------------------------------------------------------------
    public void DestroyMultiJoint( CNJointGroups mjNode, GameObject[] arrObjectsA, GameObject[] arrObjectsB )
    {
      if ( tableMultiJointToIdJointGroups_.ContainsKey(mjNode) )
      {
        uint idJointGroups = tableMultiJointToIdJointGroups_[mjNode];
        JointsManager.DestroyJointGroups(idJointGroups);

        uint[] arrIdBody_A = GetListIdBodyFromArrGo( arrObjectsA ).ToArray();
        uint[] arrIdBody_B = GetListIdBodyFromArrGo( arrObjectsB ).ToArray();

        UnregisterImplicatedBodies(mjNode, arrIdBody_A, arrIdBody_B);
              
        tableMultiJointToIdJointGroups_.Remove( mjNode );
        tableIdMultiJointToNode_.Remove( idJointGroups );
        
        if (mjNode != null)
        {
          mjNode.NeedsUpdate = true;
          EditorUtility.SetDirty(mjNode);
        }

        SceneView.RepaintAll();
      }
    }
    //-----------------------------------------------------------------------------------
    public bool CreateServoGroupReplacingJointGroups( CNJointGroups mjNode )
    {
      bool creationOK = false;
      if ( tableMultiJointToIdJointGroups_.ContainsKey(mjNode) )
      {
        uint idJointGroups = tableMultiJointToIdJointGroups_[mjNode];
        uint auxIdJointGroups = idJointGroups;

        uint idServoGroups = JointsManager.CreateServoGroupReplacingJointGroups( ref auxIdJointGroups );

        if (idServoGroups != INDEX_NOT_FOUND)
        {
          tableIdMultiJointToNode_.Remove(idJointGroups);
          tableMultiJointToIdJointGroups_.Remove(mjNode);
     
          tableIdServosToNode_[idServoGroups] = mjNode;
          tableMultiJointToIdServos_[mjNode]  = idServoGroups;

          SetVisibility( mjNode );

          creationOK = true;
        }
      }
      return creationOK;
    }
    //-----------------------------------------------------------------------------------
    public void DestroyServoGroup( CNJointGroups mjNode )
    {
      if ( tableMultiJointToIdServos_.ContainsKey(mjNode) )
      {
        uint idServosGroups = tableMultiJointToIdServos_[mjNode];
        
        tableIdServosToNode_.Remove(idServosGroups);
        tableMultiJointToIdServos_.Remove(mjNode);

        JointsManager.DestroyServoGroup(ref idServosGroups);
        
        if ( mjNode != null )
        {
          mjNode.NeedsUpdate = true;
          EditorUtility.SetDirty(mjNode);
        }

        SceneView.RepaintAll();
      }
    }
    //-----------------------------------------------------------------------------------
    private void CheckJointServosImplications( GameObject go )
    {
      int instanceId = go.GetInstanceID();
      List<CommandNode> listCommandNode = tableIdUnityToListNode_[instanceId];

      List<CNJointGroups> listJointGroupsToDestroy = new List<CNJointGroups>();

      CNManager manager = CNManager.Instance;

      foreach( CommandNode node in listCommandNode )
      {
        CNJointGroups mjNode = node as CNJointGroups;

        if ( mjNode != null)
        {
          listJointGroupsToDestroy.Add(mjNode);
        }
      }

      foreach(CNJointGroups mjNode in listJointGroupsToDestroy)
      {
        CommandNodeEditor cnEditor = manager.GetNodeEditor(mjNode);
        CNJointGroupsEditor jgEditor = cnEditor as CNJointGroupsEditor;
        jgEditor.DestroyEntities();
      }
    }
    //-----------------------------------------------------------------------------------
    public void EditMultiJoint( CNJointGroups mjNode )
    {
      JointGroupsEdit jgEdit = new JointGroupsEdit();     
      SetJointGroupsEdit(mjNode, jgEdit);

      if ( tableMultiJointToIdJointGroups_.ContainsKey(mjNode) )
      {
        uint idJointGroups = tableMultiJointToIdJointGroups_[mjNode];
        JointsManager.EditJointGroups(idJointGroups, jgEdit);
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetActivity( CNJointGroups mjNode )
    {
      if ( tableMultiJointToIdJointGroups_.ContainsKey( mjNode ) )
      {
        uint idMultiJoint = tableMultiJointToIdJointGroups_[mjNode];
        if ( mjNode.IsNodeEnabledInHierarchy )
        {
          JointsManager.ConnectJointGroups( idMultiJoint );
        }
        else
        {
          JointsManager.DisconnectJointGroups( idMultiJoint, false );
        }   
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetVisibility( CNJointGroups mjNode )
    {
      if ( tableMultiJointToIdJointGroups_.ContainsKey( mjNode ) )
      {
        uint idMultiJoint = tableMultiJointToIdJointGroups_[mjNode];
        JointsManager.SetRenderActive( idMultiJoint, mjNode.IsNodeVisibleInHierarchy );
      }

      if ( tableMultiJointToIdServos_.ContainsKey( mjNode) )
      {
        uint idServos = tableMultiJointToIdServos_[mjNode];
        ServosManager.SetRenderActive( idServos, mjNode.IsNodeVisibleInHierarchy );
      }

    }
    //-----------------------------------------------------------------------------------
    public bool IsMultiJointCreated( CNJointGroups mjNode )
    {
      return ( tableMultiJointToIdJointGroups_.ContainsKey( mjNode ) ||
               tableMultiJointToIdServos_.ContainsKey( mjNode ) );
    }
    //-----------------------------------------------------------------------------------
    public bool IsServosCreated( CNServos svNode )
    {
      return (tableServosToId_.ContainsKey( svNode ) );
    }
    //----------------------------------------------------------------------------------- 
    private uint CreateJointGroups(CNJointGroups mjNode, GameObject[] arrObjectsA, GameObject[] arrObjectsB, Vector3[] arrLocatorsC, bool fieldAIsReallyEmpty, bool fieldBIsReallyEmpty )
    {
      uint idJointGroups = INDEX_NOT_FOUND;
      JointGroupsInit jgInit = new JointGroupsInit();

      uint[] arrIdBody_A = GetListIdBodyFromArrGo( arrObjectsA ).ToArray();
      uint[] arrIdBody_B = GetListIdBodyFromArrGo( arrObjectsB ).ToArray();

      jgInit.arrIdBody_A_ = arrIdBody_A;
      jgInit.arrIdBody_B_ = arrIdBody_B;
      jgInit.arrPos_WORLD_C_ = arrLocatorsC;

      bool currentEmptyA = jgInit.arrIdBody_A_.Length == 0;
      bool currentEmptyB = jgInit.arrIdBody_B_.Length == 0;
      bool nullOrCurrentEmptyC = ( jgInit.arrPos_WORLD_C_ == null ) || ( jgInit.arrPos_WORLD_C_.Length == 0 );

      bool oneFilledAndLocators      = (currentEmptyA ^ currentEmptyB) && ( fieldAIsReallyEmpty ^ fieldBIsReallyEmpty ) && !nullOrCurrentEmptyC && mjNode.IsCreateModeAtLocators;
      bool twoFilledAndNotLocators   = !currentEmptyA && !currentEmptyB && !mjNode.IsCreateModeAtLocators;
      bool threeFilled               = !currentEmptyA && !currentEmptyB && !nullOrCurrentEmptyC;

      if ( oneFilledAndLocators || twoFilledAndNotLocators || threeFilled )
      {
        SetJointGroupsInit( mjNode, jgInit );

        idJointGroups = CaronteSharp.JointsManager.CreateJointGroups( jgInit, mjNode.IsNodeEnabledInHierarchy );

        if (idJointGroups != INDEX_NOT_FOUND) 
        {
          RegisterImplicatedBodies( mjNode, jgInit.arrIdBody_A_, jgInit.arrIdBody_B_ );
        }   
      }

      return idJointGroups;
    }
    //-----------------------------------------------------------------------------------
    private void SetJointGroupsInit( CNJointGroups mjNode, JointGroupsInit jgInit )
    {
       switch (mjNode.CreationMode)
      {
        case CNJointGroups.CreationModeEnum.ByContact:
          jgInit.generateByContact_          = true;
          jgInit.generateByStem_             = false;
          jgInit.generateByMatchingVertices_ = false;
          break;
        case CNJointGroups.CreationModeEnum.ByStem:
          jgInit.generateByContact_          = false;
          jgInit.generateByStem_             = true;
          jgInit.generateByMatchingVertices_ = false;
          break;

        case CNJointGroups.CreationModeEnum.ByMatchingVertices:
          jgInit.generateByContact_          = false;
          jgInit.generateByStem_             = false;
          jgInit.generateByMatchingVertices_ = true;
          break;

        case CNJointGroups.CreationModeEnum.AtLocatorsPositions:
          jgInit.generateByContact_          = false;
          jgInit.generateByStem_             = false;
          jgInit.generateByMatchingVertices_ = false;
          break;
        case CNJointGroups.CreationModeEnum.AtLocatorsBBoxCenters:
          jgInit.generateByContact_          = false;
          jgInit.generateByStem_             = false;
          jgInit.generateByMatchingVertices_ = false;
          break;
        case CNJointGroups.CreationModeEnum.AtLocatorsVertexes:
          jgInit.generateByContact_          = false;
          jgInit.generateByStem_             = false;
          jgInit.generateByMatchingVertices_ = false;
          break; ;
        default:
           throw new NotImplementedException();
      }

      switch (mjNode.ForceMaxMode)
      {
        case CNJointGroups.ForceMaxModeEnum.Unlimited:
          jgInit.unlimitedForce_ = true;
          break;
        case CNJointGroups.ForceMaxModeEnum.ConstantLimit:
          jgInit.unlimitedForce_ = false;
          break;
        default:
          throw new NotImplementedException();
      }

      jgInit.contactDistanceSearch_         = mjNode.ContactDistanceSearch;
      jgInit.contactAreaMin_                = mjNode.ContactAreaMin;
      jgInit.contactAngleMaxInDegrees_      = mjNode.ContactAngleMaxInDegrees;
      jgInit.contactNumberMax_              = (uint) mjNode.ContactNumberMax;
      jgInit.matchingDistanceSearch_        = mjNode.MatchingDistanceSearch;
      jgInit.limitNumberOfActiveJoints_     = mjNode.LimitNumberOfActiveJoints;
      jgInit.activeJointsMaxInABPair_       = (uint) mjNode.ActiveJointsMaxInABPair;

      jgInit.disableCollisionsByPairs_       = mjNode.DisableCollisionsByPairs;
      jgInit.disableAllCollisionsOfAsWithBs_ = mjNode.DisableAllCollisionsOfAsWithBs;


      jgInit.limitedForce_                  = !jgInit.unlimitedForce_;
      jgInit.limitedForceDependingOnDist_   = !jgInit.unlimitedForce_;

      jgInit.forceMax_                      = mjNode.ForceMax;
      jgInit.forceMaxRand_                  = mjNode.ForceMax * mjNode.ForceMaxRand;
      jgInit.distStepToDefineForceMax_      = mjNode.ForceRange / 5f;
      jgInit.forceMaxAtDist_.a_             = Mathf.Clamp(mjNode.ForceProfile.Evaluate(0f), 0f, 1f);
      jgInit.forceMaxAtDist_.b_             = Mathf.Clamp(mjNode.ForceProfile.Evaluate(0.2f), 0f, 1f);
      jgInit.forceMaxAtDist_.c_             = Mathf.Clamp(mjNode.ForceProfile.Evaluate(0.4f), 0f, 1f);
      jgInit.forceMaxAtDist_.d_             = Mathf.Clamp(mjNode.ForceProfile.Evaluate(0.6f), 0f, 1f);
      jgInit.forceMaxAtDist_.e_             = Mathf.Clamp(mjNode.ForceProfile.Evaluate(0.8f), 0f, 1f);
      jgInit.forceMaxAtDist_.f_             = Mathf.Clamp(mjNode.ForceProfile.Evaluate(1f), 0f, 1f);

      jgInit.enableCollisionIfBreak_        = mjNode.EnableCollisionIfBreak;
      jgInit.enableCollisionIfDistExcedeed_ = mjNode.EnableCollisionIfBreak;
      jgInit.distanceForEnableCol_          = mjNode.DistanceForBreak * 0.2f;
      jgInit.distanceForEnableColRand_      = mjNode.DistanceForBreakRand * 0.2f;

      jgInit.breakIfForceMax_               = mjNode.BreakIfForceMax;
      jgInit.breakAllIfLeftFewUnbroken_     = mjNode.BreakAllIfLeftFewUnbroken;
      jgInit.unbrokenNumberForBreakAll_     = (uint) mjNode.UnbrokenNumberForBreakAll;
      jgInit.breakIfDistExcedeed_           = mjNode.BreakIfDistExcedeed;
      jgInit.distanceForBreak_              = mjNode.DistanceForBreak;
      jgInit.distanceForBreakRand_          = mjNode.DistanceForBreak * mjNode.DistanceForBreakRand;
      jgInit.breakIfHinge_                  = mjNode.BreakIfHinge;

      jgInit.plasticity_                    = mjNode.Plasticity;
      jgInit.distanceForPlasticity_         = mjNode.DistanceForPlasticity;
      jgInit.distanceForPlasticityRand_     = mjNode.DistanceForPlasticity * mjNode.DistanceForPlasticityRand;
      jgInit.plasticityRateAcquired_        = mjNode.PlasticityRateAcquired;
    }
    //-----------------------------------------------------------------------------------
    private void SetJointGroupsEdit( CNJointGroups mjNode, JointGroupsEdit jgEdit)
    {
      switch (mjNode.ForceMaxMode)
      {
        case CNJointGroups.ForceMaxModeEnum.Unlimited:
          jgEdit.unlimitedForce_ = true;
          break;
        case CNJointGroups.ForceMaxModeEnum.ConstantLimit:
          jgEdit.unlimitedForce_ = false;
          break;
        default:
          throw new NotImplementedException();
      }

      jgEdit.limitedForce_                  = !jgEdit.unlimitedForce_;
      jgEdit.limitedForceDependingOnDist_   = !jgEdit.unlimitedForce_;

      jgEdit.forceMax_                      = mjNode.ForceMax;
      jgEdit.forceMaxRand_                  = mjNode.ForceMax * mjNode.ForceMaxRand;
      jgEdit.distStepToDefineForceMax_      = mjNode.ForceRange / 5f;
      jgEdit.forceMaxAtDist_.a_             = Mathf.Clamp(mjNode.ForceProfile.Evaluate(0f), 0f, 1f);
      jgEdit.forceMaxAtDist_.b_             = Mathf.Clamp(mjNode.ForceProfile.Evaluate(0.2f), 0f, 1f);
      jgEdit.forceMaxAtDist_.c_             = Mathf.Clamp(mjNode.ForceProfile.Evaluate(0.4f), 0f, 1f);
      jgEdit.forceMaxAtDist_.d_             = Mathf.Clamp(mjNode.ForceProfile.Evaluate(0.6f), 0f, 1f);
      jgEdit.forceMaxAtDist_.e_             = Mathf.Clamp(mjNode.ForceProfile.Evaluate(0.8f), 0f, 1f);
      jgEdit.forceMaxAtDist_.f_             = Mathf.Clamp(mjNode.ForceProfile.Evaluate(1f), 0f, 1f);

      jgEdit.enableCollisionIfBreak_        = mjNode.EnableCollisionIfBreak;
      jgEdit.enableCollisionIfDistExcedeed_ = mjNode.EnableCollisionIfBreak;
      jgEdit.distanceForEnableCol_          = mjNode.DistanceForBreak * 0.2f;
      jgEdit.distanceForEnableColRand_      = mjNode.DistanceForBreakRand * 0.2f;;

      jgEdit.breakIfForceMax_               = mjNode.BreakIfForceMax;
      jgEdit.breakAllIfLeftFewUnbroken_     = mjNode.BreakAllIfLeftFewUnbroken;
      jgEdit.unbrokenNumberForBreakAll_     = (uint) mjNode.UnbrokenNumberForBreakAll;
      jgEdit.breakIfDistExcedeed_           = mjNode.BreakIfDistExcedeed;
      jgEdit.distanceForBreak_              = mjNode.DistanceForBreak;
      jgEdit.distanceForBreakRand_          = mjNode.DistanceForBreak * mjNode.DistanceForBreakRand;
      jgEdit.breakIfHinge_                  = mjNode.BreakIfHinge;

      jgEdit.plasticity_                    = mjNode.Plasticity;
      jgEdit.distanceForPlasticity_         = mjNode.DistanceForPlasticity;
      jgEdit.distanceForPlasticityRand_     = mjNode.DistanceForPlasticity * mjNode.DistanceForPlasticityRand;
      jgEdit.plasticityRateAcquired_        = mjNode.PlasticityRateAcquired;
    }  
    //-----------------------------------------------------------------------------------
    //-----------------------SERVOS------------------------------------------------------
    //-----------------------------------------------------------------------------------
    public void CreateServos( CNServos svNode, GameObject[] arrObjectsA, GameObject[] arrObjectsB )
    {
      uint[] arrIdBody_A = GetListIdBodyFromArrGo( arrObjectsA ).ToArray();
      uint[] arrIdBody_B = GetListIdBodyFromArrGo( arrObjectsB ).ToArray();

      CheckBodiesForChanges(arrIdBody_A);
      CheckBodiesForChanges(arrIdBody_B);

      uint idServos = CreateServosInternal( svNode, arrObjectsA, arrObjectsB );
      if ( idServos != INDEX_NOT_FOUND )
      {
        tableIdServosToNode_[idServos] = svNode;
        tableServosToId_[svNode]       = idServos;

        svNode.NeedsUpdate = false;
        EditorUtility.SetDirty( svNode );    
      }     

      SceneView.RepaintAll();
    }
    //-----------------------------------------------------------------------------------
    public void DestroyServos( CNServos svNode, GameObject[] arrObjectsA, GameObject[] arrObjectsB )
    {
      if ( tableServosToId_.ContainsKey(svNode) )
      {
        uint idServos = tableServosToId_[svNode];
        ServosManager.DestroyServos(idServos);
        
        uint[] arrIdBody_A = GetListIdBodyFromArrGo( arrObjectsA ).ToArray();
        uint[] arrIdBody_B = GetListIdBodyFromArrGo( arrObjectsB ).ToArray();
      
        UnregisterImplicatedBodies(svNode, arrIdBody_A, arrIdBody_B);

        tableServosToId_    .Remove( svNode );
        tableIdServosToNode_.Remove( idServos );
        
        svNode.NeedsUpdate = true;
        EditorUtility.SetDirty(svNode);
        SceneView.RepaintAll();
      }
    }
    //-----------------------------------------------------------------------------------
    public void EditServos( CNServos svNode )
    {
      if ( tableServosToId_.ContainsKey(svNode) )
      {
        uint idServos = tableServosToId_[svNode];
        ServosEdit svEdit = new ServosEdit();
        SetServosEdit( svNode, svEdit );

        ServosManager.EditServos( idServos, svEdit );
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetActivity( CNServos svNode )
    {
      if ( tableServosToId_.ContainsKey(svNode) )
      {
        uint idServos = tableServosToId_[svNode];
        if (svNode.IsNodeEnabledInHierarchy)
        {
          ServosManager.ConnectServos( idServos );
        }
        else
        {
          ServosManager.DisconnectServos( idServos, false );
        }
      }
    }
    //----------------------------------------------------------------------------------- 
    private uint CreateServosInternal( CNServos svNode, GameObject[] arrObjectsA, GameObject[] arrObjectsB )
    {
      uint idServos = INDEX_NOT_FOUND;
      ServosInit svInit = new ServosInit();
      ServosEdit svEdit = new ServosEdit();

      uint[] arrIdBody_A = GetListIdBodyFromArrGo( arrObjectsA ).ToArray();
      uint[] arrIdBody_B = GetListIdBodyFromArrGo( arrObjectsB ).ToArray();

      svInit.arrIdBody_A_ = arrIdBody_A;
      svInit.arrIdBody_B_ = arrIdBody_B;
      svInit.arrIdBody_C_ = new uint[0];

      bool emptyA = arrIdBody_A.Length == 0;
      bool emptyB = arrIdBody_B.Length == 0;
      
      if( emptyA || emptyB )
        return idServos;
   
      SetServosInit( svNode, svInit );
      SetServosEdit( svNode, svEdit );

      idServos = ServosManager.CreateServos( svInit, svEdit, svNode.IsNodeEnabledInHierarchy );

      if (idServos != INDEX_NOT_FOUND) 
      {
        RegisterImplicatedBodies( svNode, svInit.arrIdBody_A_, svInit.arrIdBody_B_ );
      }
     
      return idServos;
    }
    //-----------------------------------------------------------------------------------
    private void SetServosInit( CNServos svNode, ServosInit svInit )
    {
      svInit.name_ = svNode.Name;
      svInit.isLinearOrAngular_ = svNode.IsLinearOrAngular;

      svInit.isPositionOrVelocity_ = svNode.IsPositionOrVelocity;
      svInit.isCreateModeNearest_  = svNode.IsCreateModeNearest;
      svInit.isCreateModeChain_    = svNode.IsCreateModeChain;

      svInit.isFreeX_ = svNode.IsFreeX;
      svInit.isFreeY_ = svNode.IsFreeY;
      svInit.isFreeZ_ = svNode.IsFreeZ;

      svInit.isBlockedX_ = svNode.IsBlockedX;
      svInit.isBlockedY_ = svNode.IsBlockedY;
      svInit.isBlockedZ_ = svNode.IsBlockedZ;

      svInit.disableCollisionByPairs_ = svNode.DisableCollisionByPairs;

    }
    //-----------------------------------------------------------------------------------
    private void SetServosEdit( CNServos svNode, ServosEdit svEdit )
    {
      svEdit.targetExternal_LOCAL_ = svNode.TargetExternal_LOCAL;

      svEdit.reactionTime_      = svNode.ReactionTime;
      svEdit.overreactionDelta_ = svNode.OverreactionDelta;

      svEdit.speedMax_      = svNode.MaximumSpeed ? 1.0e+12F : svNode.SpeedMax;
      svEdit.powerMax_      = svNode.MaximumPower ? 1.0e+12F : svNode.PowerMax;
      svEdit.forceMax_      = svNode.MaximumForce ? 1.0e+12F : svNode.ForceMax;
      svEdit.brakePowerMax_ = svNode.MaximumBrakePowerMax ? 1.0e+12F : svNode.BrakePowerMax;
      svEdit.brakeForceMax_ = svNode.MaximumBrakeForceMax ? 1.0e+12F : svNode.BrakeForceMax;

      svEdit.isBreakIfDist_ = svNode.IsBreakIfDist;
      svEdit.isBreakIfAng_  = svNode.IsBreakIfAng;
      svEdit.breakDistance_       = svNode.BreakDistance;
      svEdit.breakAngleInDegrees_ = svNode.BreakAngleInDegrees;

      svEdit.dampingForce_ = svNode.DampingForce;

      svEdit.distStepToDefineMultiplierDependingOnDist_ = svNode.DistStepToDefineMultiplierDependingOnDist / 6f;

      svEdit.functionMultiplierDependingOnDist_.a_ = svNode.FunctionMultiplierDependingOnDist.Evaluate(0.0f);
      svEdit.functionMultiplierDependingOnDist_.b_ = svNode.FunctionMultiplierDependingOnDist.Evaluate(0.2f);
      svEdit.functionMultiplierDependingOnDist_.c_ = svNode.FunctionMultiplierDependingOnDist.Evaluate(0.4f);
      svEdit.functionMultiplierDependingOnDist_.d_ = svNode.FunctionMultiplierDependingOnDist.Evaluate(0.6f);
      svEdit.functionMultiplierDependingOnDist_.e_ = svNode.FunctionMultiplierDependingOnDist.Evaluate(0.8f);
      svEdit.functionMultiplierDependingOnDist_.f_ = svNode.FunctionMultiplierDependingOnDist.Evaluate(1.0f);

      svEdit.multiplier_ = svNode.Multiplier;      
    }
    //-----------------------------------------------------------------------------------
    //-----------------------ENTITIES----------------------------------------------------
    //-----------------------------------------------------------------------------------
    public void DestroyEntity(CNEntity entityNode)
    {
      if ( tableEntityNodeToId_.ContainsKey(entityNode) )
      {
        uint entityId    = tableEntityNodeToId_[entityNode];
        uint oldEntityId = entityId;

        EntityManager.DestroyEntity( ref entityId );
        if (entityId == INDEX_NOT_FOUND)
        {
          tableEntityNodeToId_.Remove(entityNode);
          tableIdEntityToNode_.Remove(oldEntityId);
        }
      }
    }
    
    public void SetVisibility(CNEntity entityNode)
    {
      if ( tableEntityNodeToId_.ContainsKey(entityNode) )
      {
        uint entityId    = tableEntityNodeToId_[entityNode];
        EntityManager.SetVisibility( entityId, entityNode.IsNodeVisibleInHierarchy );
      }
    }

    public void SetActivity(CNEntity entityNode)
    {
      if ( tableEntityNodeToId_.ContainsKey(entityNode) )
      {
        uint entityId    = tableEntityNodeToId_[entityNode];
        EntityManager.SetActivity( entityId, entityNode.IsNodeEnabledInHierarchy );
      }
    }
    //-----------------------------------------------------------------------------------
    //-----------------------EXPLOSIONS--------------------------------------------------
    //-----------------------------------------------------------------------------------
    public void CreateExplosion( CNExplosion exNode )
    {
      ExplosionInit exInit = GetExplosionInit(exNode);

      uint entityId = DaemonManager.CreateExplosion( exInit );
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[exNode]   = entityId;
        tableIdEntityToNode_[entityId] = exNode;
        EntityManager.SetActivity( entityId,   exNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, exNode.IsNodeVisibleInHierarchy );
      }
    }

    public void RecreateExplosion(CNExplosion exNode, GameObject[] arrGameObjectAffected)
    {
      List<uint>   listIdBody  = GetListIdBodyFromArrGo( arrGameObjectAffected );

      ExplosionInit exInit = GetExplosionInit(exNode);

      uint entityId    = tableEntityNodeToId_[exNode];
      uint oldEntityId = entityId;

      DaemonManager.RecreateExplosion( ref entityId, exInit, listIdBody.ToArray() );
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[exNode]   = entityId;
        tableIdEntityToNode_[entityId] = exNode;
        EntityManager.SetActivity( entityId, exNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, exNode.IsNodeVisibleInHierarchy );
      }
      else
      {
        tableEntityNodeToId_.Remove(exNode);
        tableIdEntityToNode_.Remove(oldEntityId);
      }
    }

    public CNExplosion GetExplosionNode( uint idExplosion )
    {
      if ( tableIdEntityToNode_.ContainsKey(idExplosion) )
      {
        return (CNExplosion) tableIdEntityToNode_[idExplosion]; 
      }
      return null;
    }

    private ExplosionInit GetExplosionInit (CNExplosion exNode)
    {
      ExplosionInit exInit = new ExplosionInit();

      if ( exNode.Explosion_Transform != null)
      {
        exInit.r_WORLD_ = exNode.Explosion_Transform.position;
        exInit.q_WORLD_ = exNode.Explosion_Transform.rotation;

        Transform explosionTr = exNode.Explosion_Transform;
        uint bodyId = INDEX_NOT_FOUND;

        while ( (bodyId == INDEX_NOT_FOUND) && (explosionTr != null) )
        {
          bodyId = GetIdBodyFromGo(explosionTr.gameObject);
          explosionTr = explosionTr.parent;
        }

        exInit.bodyParentId_ = bodyId;
      }
      else
      {
        exInit.r_WORLD_      = Vector3.zero;
        exInit.q_WORLD_      = Quaternion.identity;
        exInit.bodyParentId_ = INDEX_NOT_FOUND;
      }

      exInit.resolution_ = (uint) exNode.Resolution + 2;
      exInit.waveSpeed_         = exNode.Wave_front_speed;
      exInit.rangeDistance_     = exNode.Range;
      exInit.decay_             = exNode.Decay;
      exInit.momentum_          = exNode.Momentum;
      exInit.timer_             = exNode.Timer;
      exInit.objectsLimitSpeed_ = exNode.Objects_limit_speed;
    
      exInit.assymetry_                       = exNode.Asymmetry;
      exInit.asymmetry_randomSeed_            = (uint)exNode.Asymmetry_random_seed;
      exInit.asymmetry_bumpNumber_            = (uint)exNode.Asymmetry_bump_number;
      exInit.asymmetry_additionalSpeedRatio_  = exNode.Asymmetry_additional_speed_ratio;

      return exInit;
    }

    //-----------------------------------------------------------------------------------
    //-------------------------GRAVITY---------------------------------------------------
    //-----------------------------------------------------------------------------------
    public void CreateGravity( CNGravity gravityNode )
    {
      uint entityId = DaemonManager.CreateGravity();
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[gravityNode]  = entityId;
        tableIdEntityToNode_[entityId]     = gravityNode;
        EntityManager.SetActivity( entityId, gravityNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, gravityNode.IsNodeVisibleInHierarchy );

      }
    }

    public void RecreateGravity( CNGravity gravityNode, GameObject[] arrGameObject )
    {
      List<uint> listIdBody = GetListIdBodyFromArrGo( arrGameObject );

      GravityInit gravityInit = GetGravityInit( gravityNode, listIdBody.ToArray() );

      uint entityId    = tableEntityNodeToId_[gravityNode];
      uint oldEntityId = entityId;

      DaemonManager.RecreateGravity( ref entityId, gravityInit );
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[gravityNode]   = entityId;
        tableIdEntityToNode_[entityId]      = gravityNode;
        EntityManager.SetActivity( entityId, gravityNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, gravityNode.IsNodeVisibleInHierarchy );
      }
      else
      {
        tableEntityNodeToId_.Remove(gravityNode);
        tableIdEntityToNode_.Remove(oldEntityId);
      }
    }

    public GravityInit GetGravityInit( CNGravity gravityNode, uint[] arrBodyId )
    {
      GravityInit gravityInit = new GravityInit();

      gravityInit.name_         = gravityNode.Name;
      gravityInit.timer_        = gravityNode.Timer;

      gravityInit.acceleration_ = gravityNode.Gravity;
      gravityInit.arrBodyId_    = arrBodyId;

      return gravityInit;
    }

    //-----------------------------------------------------------------------------------
    //-------------------------PARAMETER MODIFIER----------------------------------------
    //-----------------------------------------------------------------------------------
    public void CreateParameterModifier( CNParameterModifier pmNode )
    {
      uint entityId = EntityManager.CreateParameterModifier();
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[pmNode]      = entityId;
        tableIdEntityToNode_[entityId]    = pmNode;
        EntityManager.SetActivity( entityId, pmNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, pmNode.IsNodeVisibleInHierarchy );
      }
    }

    public void RecreateParameterModifier( CNParameterModifier pmNode, GameObject[] arrGameObject, CommandNode[] arrCommandNode )
    {
      List<uint> listIdBody = GetListIdBodyFromArrGo( arrGameObject );
      
      List<uint> listIdMultiJoint;
      List<uint> listIdRigidGlue;
      GetListIdMultiJointFromArrNodes( arrCommandNode, out listIdMultiJoint, out listIdRigidGlue );
      
      List<uint> listIdServos = GetListIdServosFromArrNodes( arrCommandNode );
      List<uint> listIdEntity = GetListIdEntityFromArrNodes( arrCommandNode );

      listIdServos.AddRange( listIdRigidGlue );

      PmInit pmInit = GetParameterModifierInit( pmNode, listIdBody.ToArray(), listIdMultiJoint.ToArray(),  listIdServos.ToArray(), listIdEntity.ToArray() );

      uint entityId    = tableEntityNodeToId_[pmNode];
      uint oldEntityId = entityId;

      EntityManager.RecreateParameterModifier( ref entityId, pmInit );
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[pmNode]   = entityId;
        tableIdEntityToNode_[entityId] = pmNode;
        EntityManager.SetActivity( entityId, pmNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, pmNode.IsNodeVisibleInHierarchy );
      }
      else
      {
        tableEntityNodeToId_.Remove(pmNode);
        tableIdEntityToNode_.Remove(oldEntityId);
      }
    }

    private PmInit GetParameterModifierInit( CNParameterModifier pmNode, uint[] arrIdBody, uint[] arrIdMultiJoint, uint[] arrIdServos, uint[] arrIdEntity )
    {
      PmInit pmInit = new PmInit();
      pmInit.name_  = pmNode.Name;
      pmInit.timer_ = pmNode.Timer;

      pmInit.arrBodyId_   = arrIdBody;
      pmInit.arrJointId_  = arrIdMultiJoint;
      pmInit.arrServoId_  = arrIdServos;
      pmInit.arrEntityId_ = arrIdEntity;

      List<ParameterModifierCommand> listPmCommand = pmNode.ListPmCommand;
      int listPmCommand_size = listPmCommand.Count;

      PmCommand[] arrPmCommand = new PmCommand[ listPmCommand_size ];

      for (int i = 0; i < listPmCommand_size; i++)
      {
        ParameterModifierCommand pmCommand = listPmCommand[i];
        arrPmCommand[i] = new PmCommand();

        switch ( pmCommand.target_ )
        {
          case PARAMETER_MODIFIER_PROPERTY.VELOCITY_LINEAL:
            arrPmCommand[i].target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.VELOCITY_LINEAL;
          break;

          case PARAMETER_MODIFIER_PROPERTY.VELOCITY_ANGULAR:
            arrPmCommand[i].target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.VELOCITY_ANGULAR;
            break;

          case PARAMETER_MODIFIER_PROPERTY.ACTIVITY:
            arrPmCommand[i].target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.ACTIVITY;
            break;

          case PARAMETER_MODIFIER_PROPERTY.VISIBILITY:
            arrPmCommand[i].target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.VISIBILITY;
            break;

          case PARAMETER_MODIFIER_PROPERTY.FORCE_MULTIPLIER:
            arrPmCommand[i].target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.FORCE_MULTIPLIER;
            break;

          case PARAMETER_MODIFIER_PROPERTY.FREEZE:
            arrPmCommand[i].target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.FREEZE;
            break;

          case PARAMETER_MODIFIER_PROPERTY.PLASTICITY:
            arrPmCommand[i].target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.PLASTICITY;
            break;

          default:
            throw new NotImplementedException();
        }

        if ( pmCommand.target_ == PARAMETER_MODIFIER_PROPERTY.VELOCITY_LINEAL  || 
             pmCommand.target_ == PARAMETER_MODIFIER_PROPERTY.VELOCITY_ANGULAR || 
             pmCommand.target_ == PARAMETER_MODIFIER_PROPERTY.FORCE_MULTIPLIER )
        {
          arrPmCommand[i].valueVector3_ = pmCommand.valueVector3_;
        }
        else
        {
          arrPmCommand[i].valueIndex_ = (uint)pmCommand.valueInt_;
        }
      }

      pmInit.arrPmCommand_ = arrPmCommand;

      return pmInit;
    }
    //-----------------------------------------------------------------------------------
    //-------------------------TRIGGERS--------------------------------------------------
    //-----------------------------------------------------------------------------------
    public void CreateTriggerByTime( CNTriggerByTime triggerNode )
    {
      uint entityId = TriggerManager.CreateTriggerByTime();
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[triggerNode]      = entityId;
        tableIdEntityToNode_[entityId]    = triggerNode;

        EntityManager.SetActivity( entityId,   triggerNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, triggerNode.IsNodeVisibleInHierarchy );
      }
    }

    public void RecreateTriggerByTime( CNTriggerByTime triggerNode, CommandNode[] arrCommandNode )
    {
      List<uint> listIdEntity     = GetListIdEntityFromArrNodes( arrCommandNode );

      TriggerByTimeInit tbtInit = GetTriggerByTimeInit( triggerNode, listIdEntity.ToArray() );

      uint entityId    = tableEntityNodeToId_[triggerNode];
      uint oldEntityId = entityId;

      TriggerManager.RecreateTriggerByTime( ref entityId, tbtInit );
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[triggerNode] = entityId;
        tableIdEntityToNode_[entityId] = triggerNode;
        EntityManager.SetActivity( entityId,   triggerNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, triggerNode.IsNodeVisibleInHierarchy );
      }
      else
      {
        tableEntityNodeToId_.Remove(triggerNode);
        tableIdEntityToNode_.Remove(oldEntityId);
      }
    }

    private TriggerByTimeInit GetTriggerByTimeInit( CNTriggerByTime triggerNode, uint[] arrIdEntity )
    {

      TriggerByTimeInit tbtInit = new TriggerByTimeInit();

      tbtInit.name_                 = triggerNode.Name;
      tbtInit.timer_                = triggerNode.Timer;
      tbtInit.arrAttentiveEntityId_ = arrIdEntity;
   
      return tbtInit;
    }

    public void CreateTriggerByContact( CNTriggerByContact triggerNode )
    {
      uint entityId = TriggerManager.CreateTriggerByContact();
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[triggerNode] = entityId;
        tableIdEntityToNode_[entityId]    = triggerNode;
        EntityManager.SetActivity( entityId,   triggerNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, triggerNode.IsNodeVisibleInHierarchy );
      }
    }

    public void RecreateTriggerByContact( CNTriggerByContact triggerNode, GameObject[] arrGameObjectA, GameObject[] arrGameObjectB, CommandNode[] arrCommandNode )
    {
      List<uint> listIdBodyA  = GetListIdBodyFromArrGo( arrGameObjectA );
      List<uint> listIdBodyB  = GetListIdBodyFromArrGo( arrGameObjectB );
      List<uint> listIdEntity = GetListIdEntityFromArrNodes( arrCommandNode );

      TriggerByContactInit tbcInit = GetTriggerByContactInit( triggerNode, listIdBodyA.ToArray(), listIdBodyB.ToArray(), listIdEntity.ToArray() );

      uint entityId    = tableEntityNodeToId_[triggerNode];
      uint oldEntityId = entityId;

      TriggerManager.RecreateTriggerByContact( ref entityId, tbcInit );
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[triggerNode] = entityId;
        tableIdEntityToNode_[entityId] = triggerNode;
        EntityManager.SetActivity( entityId,   triggerNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, triggerNode.IsNodeVisibleInHierarchy );
      }
      else
      {
        tableEntityNodeToId_.Remove(triggerNode);
        tableIdEntityToNode_.Remove(oldEntityId);
      }
    }

    private TriggerByContactInit GetTriggerByContactInit( CNTriggerByContact triggerNode, uint[] a_arrIdBody, uint[] b_arrIdBody, uint[] arrIdEntity )
    {

      TriggerByContactInit tbcInit = new TriggerByContactInit();

      tbcInit.name_                     = triggerNode.Name;
      tbcInit.a_arrBodyId_              = a_arrIdBody;
      tbcInit.b_arrBodyId_              = b_arrIdBody;
      tbcInit.arrAttentiveEntityId_     = arrIdEntity;
      tbcInit.speedMin_N_               = triggerNode.SpeedMinN;
      tbcInit.speedMin_T_               = triggerNode.SpeedMinT;
      tbcInit.triggerForInvolvedBodies_ = triggerNode.TriggerForInvolvedBodies;
   
      return tbcInit;
    }

    public void CreateTriggerByExplosion( CNTriggerByExplosion triggerNode )
    {
      uint entityId = TriggerManager.CreateTriggerByExplosion();
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[triggerNode] = entityId;
        tableIdEntityToNode_[entityId]    = triggerNode;
        EntityManager.SetActivity( entityId,   triggerNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, triggerNode.IsNodeVisibleInHierarchy );
      }
    }

    public void RecreateTriggerByExplosion( CNTriggerByExplosion triggerNode, CommandNode[] arrExplosioNode, GameObject[] arrGameObject, CommandNode[] arrEntityNode )
    {
      List<uint> listIdExplosion  = GetListIdEntityFromArrNodes( arrExplosioNode );
      List<uint> listIdBody       = GetListIdBodyFromArrGo( arrGameObject );
      List<uint> listIdEntity     = GetListIdEntityFromArrNodes( arrEntityNode );

      TriggerByExplosionInit tbeInit = GetTriggerByExplosionInit( triggerNode, listIdExplosion.ToArray(), listIdBody.ToArray(), listIdEntity.ToArray() );

      uint entityId    = tableEntityNodeToId_[triggerNode];
      uint oldEntityId = entityId;

      TriggerManager.RecreateTriggerByExplosion( ref entityId, tbeInit );
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[triggerNode] = entityId;
        tableIdEntityToNode_[entityId]    = triggerNode;
        EntityManager.SetActivity( entityId,   triggerNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, triggerNode.IsNodeVisibleInHierarchy );
      }
      else
      {
        tableEntityNodeToId_.Remove(triggerNode);
        tableIdEntityToNode_.Remove(oldEntityId);
      }
    }

    private TriggerByExplosionInit GetTriggerByExplosionInit( CNTriggerByExplosion triggerNode, uint[] arrIdExplosion, uint[] arrIdBody, uint[] arrIdEntity )
    {

      TriggerByExplosionInit tbeInit = new TriggerByExplosionInit();

      tbeInit.name_                 = triggerNode.Name;
      tbeInit.arrExplosionId_       = arrIdExplosion;
      tbeInit.arrBodyId_            = arrIdBody;
      tbeInit.arrAttentiveEntityId_ = arrIdEntity;
   
      return tbeInit;
    }
    //-----------------------------------------------------------------------------------
    //-------------------------SUBSTITUTER-----------------------------------------------
    //-----------------------------------------------------------------------------------
    public void CreateSubstituter( CNSubstituter substituterNode )
    {
      uint entityId = SubstituterManager.CreateSubstituter();
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[substituterNode] = entityId;
        tableIdEntityToNode_[entityId]        = substituterNode;
        EntityManager.SetActivity( entityId,   substituterNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, substituterNode.IsNodeVisibleInHierarchy );
      }
    }

    public void RecreateSubstituter( CNSubstituter substituterNode, GameObject[] arrGameObjectA, GameObject[] arrGameObjectB )
    {
      List<uint> listIdBodyA      = GetListIdBodyFromArrGo( arrGameObjectA );
      List<uint> listIdBodyB      = GetListIdBodyFromArrGo( arrGameObjectB );

      SubstituterInit subsInit = GetSubstituterInit( substituterNode, listIdBodyA.ToArray(), listIdBodyB.ToArray() );

      uint entityId    = tableEntityNodeToId_[substituterNode];
      uint oldEntityId = entityId;

      SubstituterManager.RecreateSubstituter( ref entityId, subsInit );
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[substituterNode] = entityId;
        tableIdEntityToNode_[entityId]        = substituterNode;

        EntityManager.SetActivity( entityId,   substituterNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, substituterNode.IsNodeVisibleInHierarchy );
      }
      else
      {
        tableEntityNodeToId_.Remove(substituterNode);
        tableIdEntityToNode_.Remove(oldEntityId);
      }
    }

    private SubstituterInit GetSubstituterInit( CNSubstituter substituterNode, uint[] a_arrIdBody, uint[] b_arrIdBody )
    {

      SubstituterInit subsInit = new SubstituterInit();

      subsInit.name_                 = substituterNode.Name;
      subsInit.timer_                = substituterNode.Timer;
      subsInit.a_arrBodyId_          = a_arrIdBody;
      subsInit.b_arrBodyId_          = b_arrIdBody;
      subsInit.probability_          = substituterNode.Probability;
      subsInit.probabilitySeed_      = substituterNode.ProbabilitySeed;
   
      return subsInit;
    }
    //-----------------------------------------------------------------------------------
    //-------------------------WIND------------------------------------------------------
    //-----------------------------------------------------------------------------------
    public void CreateWind( CNWind windNode )
    {
      uint entityId = DaemonManager.CreateWind();
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[windNode] = entityId;
        tableIdEntityToNode_[entityId] = windNode;
        EntityManager.SetActivity( entityId,   windNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, windNode.IsNodeVisibleInHierarchy );
      }
    }

    public void RecreateWind( CNWind windNode, GameObject[] arrGameObjectBody )
    {
      List<uint> listBodyId = GetListIdBodyFromArrGo( arrGameObjectBody );

      WindInit windInit = GetWindInit( windNode, listBodyId.ToArray() );

      uint entityId    = tableEntityNodeToId_[windNode];
      uint oldEntityId = entityId;

      DaemonManager.RecreateWind( ref entityId, windInit );
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[windNode] = entityId;
        tableIdEntityToNode_[entityId] = windNode;

        EntityManager.SetActivity( entityId,   windNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, windNode.IsNodeVisibleInHierarchy );
      }
      else
      {
        tableEntityNodeToId_.Remove(windNode);
        tableIdEntityToNode_.Remove(oldEntityId);
      }
    }

    private WindInit GetWindInit( CNWind windNode, uint[] arrIdBody )
    {
      WindInit windInit = new WindInit();

      windInit.name_  = windNode.Name;
      windInit.timer_ = windNode.Timer;

      windInit.fluidDensity_ = windNode.FluidDensity;
      windInit.velocity_     = windNode.Velocity;
 
      windInit.speedDeltaMax_ = windNode.SpeedDeltaMax;
      windInit.angleDeltaMax_ = windNode.AngleDeltaMax;
 
      windInit.periodTime_    = windNode.PeriodTime;
      windInit.periodSpace_   = windNode.PeriodSpace;

      windInit.highFrequency_am_ = windNode.HighFrequency_am;
      windInit.highFrequency_sp_ = windNode.HighFrequency_sp;

      windInit.arrBodyId_ = arrIdBody;
  
      return windInit;
    }

    //-----------------------------------------------------------------------------------
    //-------------------------AIMED FORCE-----------------------------------------------
    //-----------------------------------------------------------------------------------
    public void CreateAimedForce( CNAimedForce afNode )
    {
      uint entityId = DaemonManager.CreateDrivenTarget();
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[afNode]   = entityId;
        tableIdEntityToNode_[entityId] = afNode;
        EntityManager.SetActivity( entityId,   afNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, afNode.IsNodeVisibleInHierarchy );
      }
    }

    public void RecreateAimedForce( CNAimedForce afNode, GameObject[] arrGameObjectBody, GameObject[] arrGameObjectAim )
    {
      List<uint> listBodyId = GetListIdBodyFromArrGo( arrGameObjectBody );

      DrivenTargetInit dtInit = GetDrivenTargetInit( afNode, listBodyId.ToArray(), arrGameObjectAim );

      uint entityId    = tableEntityNodeToId_[afNode];
      uint oldEntityId = entityId;

      DaemonManager.RecreateDrivenTarget( ref entityId, dtInit );
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[afNode]   = entityId;
        tableIdEntityToNode_[entityId] = afNode;

        EntityManager.SetActivity( entityId,   afNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, afNode.IsNodeVisibleInHierarchy );
      }
      else
      {
        tableEntityNodeToId_.Remove(afNode);
        tableIdEntityToNode_.Remove(oldEntityId);
      }
    }

    private DrivenTargetInit GetDrivenTargetInit( CNAimedForce afNode, uint[] arrIdBody, GameObject[] arrGameObjectAim )
    {
      DrivenTargetInit dtInit = new DrivenTargetInit();

      dtInit.name_                = afNode.Name;
      dtInit.timer_               = afNode.Timer;

      dtInit.timeDuration_        = afNode.TimeDuration;         

      dtInit.multiplier_r_  = afNode.Multiplier_r;
      dtInit.multiplier_q_  = afNode.Multiplier_q;

      GameObject[] arrBodiesGO = GetArrGOFromArrIdBody(arrIdBody);

      List<Tuple2<int,int>> listGOConnections; 
      CalculateGameObjectConnections( arrBodiesGO, arrGameObjectAim, out listGOConnections );

      List<uint>             listBodyId = new List<uint>();
      List<DrivenTargetData> listDtData = new List<DrivenTargetData>();

      int nGOConnections = listGOConnections.Count;

      for ( int i = 0; i < nGOConnections; i++)
      {
        Tuple2<int,int> connection = listGOConnections[i];

        int idxBody          = connection.First;
        int idxGameObjectAim = connection.Second;

        uint bodyId = arrIdBody[idxBody];
        listBodyId.Add(bodyId);

        GameObject go = arrGameObjectAim[idxGameObjectAim];

        DrivenTargetData dtData = new DrivenTargetData();
        dtData.r_ = go.transform.position;
        dtData.q_ = go.transform.rotation;

        listDtData.Add(dtData);
      }
      
      dtInit.arrBodyId_           = listBodyId.ToArray();
      dtInit.arrDrivenTargetData_ = listDtData.ToArray();
  
      return dtInit;
    }

    private void CalculateGameObjectConnections( GameObject[] arrGameObjectA, GameObject[] arrGameObjectB, out List<Tuple2<int,int>> listGOConnections )
    {
      listGOConnections = new List<Tuple2<int,int>>();

      int nGameObjectsA = arrGameObjectA.Length;
      int nGameObjectsB = arrGameObjectB.Length;

      BitArray arrAlreadySelectedB = new BitArray(nGameObjectsB, false);

      for (int i = 0; i < nGameObjectsA; i++)
      {
        GameObject goA = arrGameObjectA[i];
        Mesh meshA = goA.GetMesh();
        if (meshA == null)
        {
          continue;
        }

        byte[] meshAfp = new byte[256];
        CRGeometryUtils.CalculateFingerprint( meshA, meshAfp );

        List<int> listMeshMatches = new List<int>();

        for (int j = 0; j < nGameObjectsB; j++)
        {
          GameObject goB = arrGameObjectB[j];
          Mesh meshB = goB.GetMesh();
          if ( meshB == null || arrAlreadySelectedB[j] )
          {
            continue;
          }

          byte[] meshBfp = new byte[256];
          CRGeometryUtils.CalculateFingerprint( meshB, meshBfp );

          if ( CRGeometryUtils.AreFingerprintsEqual( meshAfp, meshBfp ) )
          {
            listMeshMatches.Add(j);
          }
        }

        int bestMatchIdx;
        SelectBestMatchByDistance( goA, arrGameObjectB, listMeshMatches, out bestMatchIdx );

        if (bestMatchIdx != -1)
        {
          arrAlreadySelectedB[bestMatchIdx] = true;
          listGOConnections.Add( Tuple2.New( i, bestMatchIdx ) );
        }
      }

    }

    private void SelectBestMatchByDistance( GameObject goA, GameObject[] arrGameObjectB, List<int> listMeshMatches, out int bestMatchIdx)
    {   
      bestMatchIdx = -1;
      int nMatches = listMeshMatches.Count;
        
      if ( nMatches > 0 )
      {
        int         matchBIdx = listMeshMatches[0];
        GameObject  matchGO   = arrGameObjectB[matchBIdx];

        if ( nMatches > 1 )
        {
          Vector3 posA       = goA.transform.position;
          Vector3 posMatchGO = matchGO.transform.position;

          float curDistance = (posMatchGO - posA).sqrMagnitude; 

          for (int j = 1; j < nMatches; j++)
          {
            int goBIndex   = listMeshMatches[j];
            GameObject goB = arrGameObjectB[goBIndex];

            Vector3 posB = goB.transform.position;

            float sqrDistance = (posB - posA).sqrMagnitude;

            if ( sqrDistance < curDistance )
            {
              matchBIdx = goBIndex;
              matchGO   = arrGameObjectB[matchBIdx];
            }
          }
        }
        bestMatchIdx = matchBIdx;
      }
    }

    //-----------------------------------------------------------------------------------
    //-------------------------SPEED LIMITER---------------------------------------------
    //-----------------------------------------------------------------------------------
    public void CreateSpeedLimiter( CNSpeedLimiter slNode )
    {
      uint entityId = DaemonManager.CreateSpeedLimiter();
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[slNode]  = entityId;
        tableIdEntityToNode_[entityId] = slNode;

        EntityManager.SetActivity( entityId, slNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, slNode.IsNodeVisibleInHierarchy );

      }
    }

    public void RecreateSpeedLimiter( CNSpeedLimiter slNode, GameObject[] arrGameObject )
    {
      List<uint> listIdBody = GetListIdBodyFromArrGo( arrGameObject );

      SpeedLimiterInit slInit = GetSpeedLimiterInit( slNode, listIdBody.ToArray() );

      uint entityId    = tableEntityNodeToId_[slNode];
      uint oldEntityId = entityId;

      DaemonManager.RecreateSpeedLimiter( ref entityId, slInit );
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[slNode]   = entityId;
        tableIdEntityToNode_[entityId] = slNode;

        EntityManager.SetActivity  ( entityId, slNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, slNode.IsNodeVisibleInHierarchy );
      }
      else
      {
        tableEntityNodeToId_.Remove(slNode);
        tableIdEntityToNode_.Remove(oldEntityId);
      }
    }

    public SpeedLimiterInit GetSpeedLimiterInit( CNSpeedLimiter slNode, uint[] arrBodyId )
    {
      SpeedLimiterInit slInit = new SpeedLimiterInit();

      slInit.name_       = slNode.Name;
      slInit.timer_      = slNode.Timer;

      slInit.speedLimit_        = slNode.SpeedLimit;
      slInit.fallingSpeedLimit_ = slNode.FallingSpeedLimit;

      slInit.arrBodyId_ = arrBodyId;

      return slInit;
    }

    //-----------------------------------------------------------------------------------
    //-------------------------JET-------------------------------------------------------
    //-----------------------------------------------------------------------------------
    public void CreateJet( CNJet jetNode )
    {
      uint entityId = DaemonManager.CreateJet();
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[jetNode]  = entityId;
        tableIdEntityToNode_[entityId] = jetNode;
        EntityManager.SetActivity( entityId,   jetNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, jetNode.IsNodeVisibleInHierarchy );
      }
    }

    public void RecreateJet( CNJet jetNode, GameObject[] arrGameObjectBody, GameObject[] arrLocators )
    {
      List<uint> listBodyId = GetListIdBodyFromArrGo( arrGameObjectBody );

      List<Vector3>    listTranslation = new List<Vector3>();
      List<Quaternion> listRotation    = new List<Quaternion>();

      foreach( GameObject locator in arrLocators )
      {
        Transform tr = locator.transform;
        if (tr.childCount == 0)
        {
          listTranslation.Add( tr.position );
          listRotation.Add( tr.rotation );
        }
      }
  
      JetInit jetInit = GetJetInit( jetNode, listBodyId.ToArray(), listTranslation.ToArray(), listRotation.ToArray() );

      uint entityId    = tableEntityNodeToId_[jetNode];
      uint oldEntityId = entityId;

      DaemonManager.RecreateJet( ref entityId, jetInit );
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[jetNode]  = entityId;
        tableIdEntityToNode_[entityId] = jetNode;

        EntityManager.SetActivity( entityId,   jetNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, jetNode.IsNodeVisibleInHierarchy );
      }
      else
      {
        tableEntityNodeToId_.Remove(jetNode);
        tableIdEntityToNode_.Remove(oldEntityId);
      }
    }

    private JetInit GetJetInit( CNJet jetNode, uint[] arrIdBody, Vector3[] arrLocatorTranslation, Quaternion[] arrLocatorRotation )
    {
      JetInit jetInit = new JetInit();

      jetInit.name_  = jetNode.Name;
      jetInit.timer_ = jetNode.Timer;

      jetInit.force_      = jetNode.Force;
      jetInit.speedLimit_ = jetNode.SpeedLimit;
 
      jetInit.forceDeltaMax_ = jetNode.ForceDeltaMax;
      jetInit.angleDeltaMax_ = jetNode.AngleDeltaMax;
 
      jetInit.periodTime_    = jetNode.PeriodTime;
      jetInit.periodSpace_   = jetNode.PeriodSpace;

      jetInit.highFrequency_amplitudeRate_ = jetNode.HighFrequency_am;
      jetInit.highFrequency_speedUp_       = jetNode.HighFrequency_sp;

      jetInit.arrBodyId_      = arrIdBody;
      jetInit.arrTranslation_ = arrLocatorTranslation;
      jetInit.arrRotation_    = arrLocatorRotation;

      return jetInit;
    }


    //-----------------------------------------------------------------------------------
    //-------------------------CONTACT EMITTER-------------------------------------------
    //-----------------------------------------------------------------------------------
    public void CreateContactEmitter( CNContactEmitter ceNode )
    {
      uint entityId = EventEmitterManager.CreateEmitterByContact();
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[ceNode]   = entityId;
        tableIdEntityToNode_[entityId] = ceNode;
        EntityManager.SetActivity( entityId,   ceNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, ceNode.IsNodeVisibleInHierarchy );
      }
    }

    public void RecreateContactEmitter( CNContactEmitter ceNode, GameObject[] arrGameObjectA, GameObject[] arrGameObjectB )
    {
      List<uint> listIdBodyA      = GetListIdBodyFromArrGo( arrGameObjectA );
      List<uint> listIdBodyB      = GetListIdBodyFromArrGo( arrGameObjectB );

      EmitterByContactInit ebcInit = GetEmitterByContactInit( ceNode, listIdBodyA.ToArray(), listIdBodyB.ToArray() );

      uint entityId    = tableEntityNodeToId_[ceNode];
      uint oldEntityId = entityId;

      EventEmitterManager.RecreateEmitterByContact( ref entityId, ebcInit );
      if (entityId != INDEX_NOT_FOUND)
      {
        tableEntityNodeToId_[ceNode] = entityId;
        tableIdEntityToNode_[entityId] = ceNode;
        EntityManager.SetActivity( entityId,   ceNode.IsNodeEnabledInHierarchy );
        EntityManager.SetVisibility( entityId, ceNode.IsNodeVisibleInHierarchy );
      }
      else
      {
        tableEntityNodeToId_.Remove(ceNode);
        tableIdEntityToNode_.Remove(oldEntityId);
      }
    }

    private EmitterByContactInit GetEmitterByContactInit( CNContactEmitter ceNode, uint[] a_arrIdBody, uint[] b_arrIdBody )
    {
      EmitterByContactInit ebcInit = new EmitterByContactInit();

      ebcInit.name_                     = ceNode.Name;
      ebcInit.a_arrBodyId_              = a_arrIdBody;
      ebcInit.b_arrBodyId_              = b_arrIdBody;
      ebcInit.maxEventsPerSecond_       = (uint)ceNode.MaxEventsPerSecond;
      ebcInit.speedMin_N_               = ceNode.RelativeSpeedMin_N;
      ebcInit.speedMin_T_               = ceNode.RelativeSpeedMin_T;
      ebcInit.momentum_N_               = ceNode.RelativeMomentum_N;
      ebcInit.momentum_T_               = ceNode.RelativeSpeedMin_T;   
   
      return ebcInit;
    }
    //-----------------------------------------------------------------------------------
    //-------------------------COMMON METHODS--------------------------------------------
    //-----------------------------------------------------------------------------------
    private void CheckBodiesForChanges(uint[] arrIdBody)
    {
      CNManager manager = CNManager.Instance;

      HashSet<CNBody> changedBodyNodes = new HashSet<CNBody>();
      foreach (uint idBody in arrIdBody)
      {
        if ( tableIdBodyToNode_.ContainsKey( idBody ) )
        {
          CNBody bodyNode = tableIdBodyToNode_[idBody];
          changedBodyNodes.Add( bodyNode );
        }
      }

      foreach (CNBody bodyNode in changedBodyNodes)
      {
        CNBodyEditor bodyEditor = (CNBodyEditor) manager.GetNodeEditor( bodyNode );
        bodyEditor.CheckBodiesForChanges(true);
      }
    }
    //-----------------------------------------------------------------------------------
    private void RegisterImplicatedBodies(CommandNode node, uint[] arrIdA, uint[] arrIdB)
    {
      for (int i = 0; i < arrIdA.Length; i++)
      {
        GameObject go = GetGOFromIdBody( arrIdA[i] );
        AddIdUnityNodeRef( go.GetInstanceID(), node );
      }
      for (int i = 0; i < arrIdB.Length; i++)
      {
        GameObject go = GetGOFromIdBody( arrIdB[i] );
        AddIdUnityNodeRef( go.GetInstanceID(), node );
      }
    }
    //-----------------------------------------------------------------------------------
    private void UnregisterImplicatedBodies(CommandNode node, uint[] arrIdA, uint[] arrIdB)
    {
      for (int i = 0; i < arrIdA.Length; i++)
      {
        GameObject go = GetGOFromIdBody( arrIdA[i] );
        RemoveIdUnityNodeRef( go.GetInstanceID(), node );
      }

      for (int i = 0; i < arrIdB.Length; i++)
      {
        GameObject go = GetGOFromIdBody( arrIdB[i] );
        RemoveIdUnityNodeRef( go.GetInstanceID(), node );
      }
    }
    //-----------------------------------------------------------------------------------
    private bool AddBodyComponent( GameObject go )
    {
      bool hasMesh = go.HasMesh();
  
      if (hasMesh)
      {
        Caronte_Fx_Body bodyComponent = go.GetComponent<Caronte_Fx_Body>();
        bool hasComponent = bodyComponent != null;
        if ( !hasComponent )
        {
          bodyComponent = go.AddComponent<Caronte_Fx_Body>();
        }
      }

      return hasMesh;
    }
    //-----------------------------------------------------------------------------------
    private void GetRenderAndCollider( GameObject go, ref Mesh meshRender, ref Mesh meshCollider, out bool createConvex )
    {     
      Caronte_Fx_Body bodyComponent = go.GetComponent<Caronte_Fx_Body>(); 

      meshRender   = go.GetMesh();
      meshCollider = meshRender;

      if (bodyComponent.colliderType_ == Caronte_Fx_Body.ColliderType.CustomMesh)
      {
        Mesh collider = bodyComponent.GetCustomColliderMesh();   
        if (collider != null)
        {
          meshCollider = collider;       
        }
      }    

      createConvex = bodyComponent.IsConvexHull();
    }
    //-----------------------------------------------------------------------------------
    private void GetDefinitionAndTileMeshes( GameObject go, ref Mesh meshDefinition, ref Mesh meshTile)
    {     
      meshDefinition = go.GetMesh();

      Caronte_Fx_Body bodyComponent = go.GetComponent<Caronte_Fx_Body>(); 
      if (bodyComponent != null)
      {
        meshTile = bodyComponent.GetTileMesh();   
      }    
    }

    //-----------------------------------------------------------------------------------
    public void AddBody( GameObject go, uint idBody, BodyType type, CNBody bodyNode, Mesh renderMesh, Mesh colliderMesh, bool isConvex )
    {
      int idUnity = go.GetInstanceID();
      AddToBodyNodeDicts( go, idBody, bodyNode, type, renderMesh, colliderMesh, isConvex);
      AddIdUnityNodeRef( idUnity, bodyNode );

      if (idBody != INDEX_NOT_FOUND && idBody != INDEX_NOT_ALLOWED)
      {
        tableIdBodyToGO_.Add( idBody, go );
        tableIdBodyToNode_[idBody] = bodyNode;
        tableIdBodyToType_[idBody] = type;
      }
    }
    //-----------------------------------------------------------------------------------
    public void RemoveBody( int instanceId, uint idBody, CNBody node )
    {   
      RemoveFromBodyNodeDicts( instanceId, idBody, node );      
      RemoveIdUnityNodeRefFromBody( instanceId, node );

      if (idBody != INDEX_NOT_FOUND && idBody != INDEX_NOT_ALLOWED)
      {
        tableIdBodyToGO_  .RemoveByFirst( idBody );
        tableIdBodyToType_.Remove( idBody );
        tableIdBodyToNode_.Remove( idBody );
      }
    }
    //-----------------------------------------------------------------------------------
    public BodyType GetBodyType( uint idBody )
    {
      return tableIdBodyToType_[idBody];
    }
    //-----------------------------------------------------------------------------------
    public void SetBodyType( uint idBody, BodyType bodyType )
    {
      tableIdBodyToType_[idBody] = bodyType;
    }
    //-----------------------------------------------------------------------------------
    public Transform GetBodyTransformRef( uint idBody )
    {
      return ( dictBodyTransformSim_[idBody] );
    }
    //-----------------------------------------------------------------------------------
    public Tuple2<Mesh, MeshUpdater> GetBodyMeshRenderUpdaterRef( uint idBody )
    {
      return ( dictBodyMeshRenderSim_[idBody] );
    }
    //-----------------------------------------------------------------------------------
    public Mesh GetBodyMeshColliderRef( uint idBody )
    {
      return ( dictBodyMeshColliderSim_[idBody] );
    }
    //-----------------------------------------------------------------------------------
    public Tuple2<Mesh, Vector3> GetRopeInit( uint idBody )
    {
      return ( dictBodyRopeInit_[idBody] );
    }
    //-----------------------------------------------------------------------------------
    public bool HasBodyMeshColliderRef( uint idBody )
    {
      return ( dictBodyMeshColliderSim_.ContainsKey(idBody) );
    }
    //-----------------------------------------------------------------------------------
    public CNBody GetBodyNode( uint idBody )
    {
      return ( tableIdBodyToNode_[idBody] );
    }
    //-----------------------------------------------------------------------------------
    public bool IsRigidbody(uint idBody)
    {
      return tableIdBodyToType_[idBody] == BodyType.Rigidbody;
    }
    //-----------------------------------------------------------------------------------
    public bool IsSoftbody( uint idBody )
    {
      return tableIdBodyToType_[idBody] == BodyType.Softbody;
    }
    //-----------------------------------------------------------------------------------
    public bool IsCloth( uint idBody )
    {
      return tableIdBodyToType_[idBody] == BodyType.Clothbody;
    }
    //-----------------------------------------------------------------------------------
    public bool IsRope( uint idBody )
    {
      return tableIdBodyToType_[idBody] == BodyType.Ropebody;
    }
    //-----------------------------------------------------------------------------------
    public bool IsBMeshStatic(uint idBody)
    {
      return tableIdBodyToType_[idBody] == BodyType.BodyMeshStatic;
    }
    //-----------------------------------------------------------------------------------
    public bool IsBMeshAnimatedByTransform( uint idBody )
    {
      return tableIdBodyToType_[idBody] == BodyType.BodyMeshAnimatedByTransform;
    }
    //-----------------------------------------------------------------------------------
    public bool IsBMeshAnimatedByArrPos( uint idBody )
    {
      return tableIdBodyToType_[idBody] == BodyType.BodyMeshAnimatedByArrPos;
    }
    //-----------------------------------------------------------------------------------
    public bool IsGameObjectAnimated( GameObject go )
    {
      uint idBody = INDEX_NOT_FOUND;
      bool ok = tableIdBodyToGO_.TryGetBySecond( go, out idBody );
      if ( ok )
      {
        return ( IsBMeshAnimatedByArrPos(idBody) || IsBMeshAnimatedByTransform(idBody) );             
      }
      return false;
    }
    //-----------------------------------------------------------------------------------
    public uint GetIdBodyFromGo( GameObject go )
    {
      uint idBody = INDEX_NOT_FOUND;
      bool ok = tableIdBodyToGO_.TryGetBySecond( go, out idBody );
      if ( ok )
        return ( idBody );
      else
        return INDEX_NOT_FOUND;
    }
    //-----------------------------------------------------------------------------------
    public uint GetIdBodyFromGOForSimulatingOrReplaying( GameObject go )
    {
      bool okSimMode = dictGOtoBodyIdSim_.ContainsKey( go );
      if (okSimMode)
      {
        return dictGOtoBodyIdSim_[go];
      }
      else
      {
        return INDEX_NOT_FOUND;
      }
    }
    //-----------------------------------------------------------------------------------
    public GameObject[] GetBodyObjects( List<GameObject> listGameObject )
    {
      List<GameObject> listBodyGO = new List<GameObject>();
      foreach( GameObject go in listGameObject )
      {
        uint idBody = GetIdBodyFromGo( go );
        if ( idBody != INDEX_NOT_FOUND )
        {
          listBodyGO.Add(go);
        }
      }
      return listBodyGO.ToArray();
    }
    //-----------------------------------------------------------------------------------
    public uint GetNumberOfBodyObjects( int[] arrIdObject )
    {
      uint count = 0;
      int nObjects = arrIdObject.Length;
      for (int i = 0; i < nObjects; i++)
      {
        int idObject = arrIdObject[i];
        GameObject go = (GameObject)EditorUtility.InstanceIDToObject(idObject);
        uint idBody = GetIdBodyFromGo( go );
        if ( idBody != INDEX_NOT_FOUND )
        {
          count++;
        }
      }
      return count;
    }
    //-----------------------------------------------------------------------------------
    public uint GetNumberOfBodyObjects( GameObject[] arrGameObject )
    {
      uint count = 0;
      int nObjects = arrGameObject.Length;
      for (int i = 0; i < nObjects; i++)
      {
        GameObject go = arrGameObject[i];
        uint idBody = GetIdBodyFromGo( go );
        if ( idBody != INDEX_NOT_FOUND )
        {
          count++;
        }
      }
      return count;
    }
    //-----------------------------------------------------------------------------------
    public GameObject GetGOFromIdBody(uint idBody)
    {
      GameObject go;
      bool ok = tableIdBodyToGO_.TryGetByFirst(idBody, out go);
      if ( ok )
        return ( go );
      else 
        return null;
    }
    //-----------------------------------------------------------------------------------
    private GameObject[] GetArrGOFromArrIdBody( uint[] arrIdBody )
    {
      int nBodies = arrIdBody.Length;
      GameObject[] arrGO = new GameObject[nBodies];
      for (int i = 0; i < nBodies; i++)
      {
        uint idBody = arrIdBody[i];
        arrGO[i] = GetGOFromIdBody(idBody);
      }
      return arrGO;
    }
    //-----------------------------------------------------------------------------------
    private List<uint> GetListIdBodyFromArrGo( GameObject[] arrGameObject )
    {
      List<uint> listIdBody = new List<uint>();
      int arrSize = arrGameObject.Length;
      for (int i = 0; i < arrSize; i++ )
      {
        GameObject go = arrGameObject[i];
        uint idBody = GetIdBodyFromGo(go);
        if ( idBody != INDEX_NOT_FOUND )
        {
          listIdBody.Add(idBody);
        }
      }
      return listIdBody;
    }
    //-----------------------------------------------------------------------------------
    private void GetListIdMultiJointFromArrNodes( CommandNode[] arrCommandNode, out List<uint> listIdMultiJoint, out List<uint> listIdRigidGlue )
    {
      listIdMultiJoint = new List<uint>();
      listIdRigidGlue  = new List<uint>();

      foreach (CommandNode node in arrCommandNode)
      {
        CNJointGroups jointNode = node as CNJointGroups;
        if (jointNode != null)
        {
          if ( tableMultiJointToIdJointGroups_.ContainsKey(jointNode) )
          {
            uint idMultiJoint = tableMultiJointToIdJointGroups_[jointNode];
            listIdMultiJoint.Add(idMultiJoint);
          }
          if ( tableMultiJointToIdServos_.ContainsKey(jointNode) )
          {
            uint idServos = tableMultiJointToIdServos_[jointNode];
            listIdRigidGlue.Add(idServos);
          }
        }
      }
    }
    //-----------------------------------------------------------------------------------
    public void GetListIdMultiJointFromListNodes( List<CommandNode> listCommandNode, List<uint> listIdMultiJoint, List<uint> listIdRigidGlue )
    {
      listIdMultiJoint.Clear();
      listIdRigidGlue .Clear();

      foreach (CommandNode node in listCommandNode)
      {
        CNJointGroups jointNode = node as CNJointGroups;
        if (jointNode != null)
        {
          if ( tableMultiJointToIdJointGroups_.ContainsKey(jointNode) )
          {
            uint idMultiJoint = tableMultiJointToIdJointGroups_[jointNode];
            listIdMultiJoint.Add(idMultiJoint);
          }
          if ( tableMultiJointToIdServos_.ContainsKey(jointNode) )
          {
            uint idServos = tableMultiJointToIdServos_[jointNode];
            listIdRigidGlue.Add(idServos);
          }
        }
      }
    }
    //-----------------------------------------------------------------------------------
    private List<uint> GetListIdServosFromArrNodes( CommandNode[] arrCommandNode )
    {
      List<uint> listIdServos = new List<uint>();
      foreach( CommandNode node in arrCommandNode)
      {
        CNServos servosNode = node as CNServos;
        if (servosNode != null)
        {
          if ( tableServosToId_.ContainsKey(servosNode) )
          {
            uint idServos = tableServosToId_[servosNode];
            listIdServos.Add( idServos );
          }
        }
      }
      return listIdServos;
    }
    //-----------------------------------------------------------------------------------
    private List<uint> GetListIdEntityFromArrNodes( CommandNode[] arrCommandNode )
    {
      List<uint> listIdEntity = new List<uint>();
      foreach( CommandNode node in arrCommandNode)
      {
        CNEntity entityNode = node as CNEntity;
        if (entityNode != null)
        {
          if ( tableEntityNodeToId_.ContainsKey(entityNode) )
          {
            uint idEntity = tableEntityNodeToId_[entityNode];
            listIdEntity.Add( idEntity );
          }
        }
      }
      return listIdEntity;
    }
    //-----------------------------------------------------------------------------------
    public void AddToBodyNodeDicts( GameObject go, uint idBody, CNBody bodyNode, BodyType type, Mesh renderMesh, Mesh colliderMesh, bool isConvex )
    {
      int instanceId = go.GetInstanceID();
      CRCreationData crData = new CRCreationData(go, type, renderMesh, colliderMesh, isConvex);
      
      Dict_IdUnityToBodyInfo dictNodeBodyInfo = GetNodeDictCRBodyInfo(bodyNode);     
      dictNodeBodyInfo[instanceId] = crData;
   
      if (idBody != INDEX_NOT_FOUND && idBody != INDEX_NOT_ALLOWED)
      {
        Dict_IdUnityToIdBody dictNodeIdBody = GetNodeDictIdBody(bodyNode);
        dictNodeIdBody[instanceId] = idBody;
      }
    }
    //-----------------------------------------------------------------------------------
    public void RemoveFromBodyNodeDicts( int instanceId, uint idBody, CNBody bodyNode )
    {
      Dict_IdUnityToBodyInfo dictNodeBodyInfo = GetNodeDictCRBodyInfo(bodyNode);
      dictNodeBodyInfo.Remove(instanceId);

      if ( idBody != INDEX_NOT_FOUND && idBody != INDEX_NOT_ALLOWED)
      {
        Dict_IdUnityToIdBody dictNodeIdBody = GetNodeDictIdBody(bodyNode);
        dictNodeIdBody.Remove(instanceId);
      } 
    }
    //-----------------------------------------------------------------------------------
    public void AddIdUnityNodeRef(int idUnity, CommandNode node)
    {
      List<CommandNode> listCommandNode = null;
      if ( tableIdUnityToListNode_.ContainsKey( idUnity ) )
      {
        listCommandNode = tableIdUnityToListNode_[idUnity];
      }
      else
      {
        listCommandNode = new List<CommandNode>();
        tableIdUnityToListNode_[idUnity] = listCommandNode;
      }

      if( !listCommandNode.Contains( node ) )
      {
        listCommandNode.Add(node);
      } 
    }
    //-----------------------------------------------------------------------------------
    public void RemoveIdUnityNodeRefFromBody(int idUnity, CommandNode node)
    {
      if ( tableIdUnityToListNode_.ContainsKey(idUnity) )
      {
        List<CommandNode> listCommandNode = tableIdUnityToListNode_[idUnity];
        listCommandNode.Remove(node);

        foreach(CommandNode refNode in listCommandNode)
        {
          refNode.NeedsUpdate = true;
        }  
      }
    }
    //-----------------------------------------------------------------------------------
    public void RemoveIdUnityNodeRef(int idUnity, CommandNode node)
    {
      if ( tableIdUnityToListNode_.ContainsKey(idUnity) )
      {
        List<CommandNode> listCommandNode = tableIdUnityToListNode_[idUnity];
        listCommandNode.Remove(node);
      }
    }
    //-----------------------------------------------------------------------------------
    public List<CommandNode> GetListNodeReferences(uint idBody)
    {
      GameObject go = GetGOFromIdBody(idBody);
      int idUnity = go.GetInstanceID();
      return tableIdUnityToListNode_[idUnity];     
    }
    //-----------------------------------------------------------------------------------
    public void CreateBodiesTmpGameObjects()
    {
      foreach (var pair in tableBodyNodeToDictIdBody_)
      {
        CNBody bodyNode = pair.Key;

        Dict_IdUnityToIdBody   dictIdBody   = pair.Value;
        List<GameObject> listGameObjectToDestroy = new List<GameObject>();

        listBodyIdAux_.Clear();
        listBodyIdAux_.AddRange(dictIdBody.Values);

        if (listBodyIdAux_.Count > 0)
        {
          GameObject bodyNodeGO = new GameObject(bodyNode.Name);
          bodyNodeGO.transform.parent = tmpSimulationGO.transform;

          foreach (uint idBody in listBodyIdAux_)
          {
            GameObject bodyGO = GetGOFromIdBody(idBody);
            GameObject clonedGO = (GameObject)UnityEngine.Object.Instantiate( bodyGO );

            listGameObjectToDestroy.Clear();
            GetRemoveBodyChildrenList( bodyGO, clonedGO, listGameObjectToDestroy );

            foreach( GameObject go in listGameObjectToDestroy )
            {
              Object.DestroyImmediate(go);
            }

            clonedGO.name = bodyGO.name;

            CREditorUtils.ReplaceSkinnedMeshRenderer( clonedGO );

            UpdateBodyReference( idBody, bodyGO, clonedGO, bodyNodeGO );  

            clonedGO.SetActive( bodyGO.activeInHierarchy );
            listGameObjectState_.Add( Tuple2.New( bodyGO, bodyGO.activeSelf) );
          }
        }
      }
    }
    //-----------------------------------------------------------------------------------
    public void GetRemoveBodyChildrenList( GameObject bodyGO, GameObject clonedGO, List<GameObject> listGameObjectToDestroy )
    {
      Transform bodyTr   = bodyGO.transform;
      Transform clonedTr = clonedGO.transform;

      int childCount = bodyTr.childCount;

      for(int i = 0; i < childCount; i++)
      {
        Transform childTr  = bodyTr.GetChild(i);
        GameObject childGO = childTr.gameObject;

        Transform childClonTr = clonedTr.GetChild(i);
        GameObject childClonGO = childClonTr.gameObject;

        uint idBody = GetIdBodyFromGo( childGO );
        if ( idBody != INDEX_NOT_FOUND )
        {
          listGameObjectToDestroy.Add( childClonGO );
        }
        else
        {
          GetRemoveBodyChildrenList( childGO, childClonGO, listGameObjectToDestroy);
        }   
      }      
    }
    //-----------------------------------------------------------------------------------
    public void DestroyBodiesTmpGameObjects()
    {
      foreach( var idbodyMeshData in dictBodyMeshRenderSim_ )
      {
        Tuple2<Mesh, MeshUpdater> meshData = idbodyMeshData.Value;

        Mesh mesh               = meshData.First;
        MeshUpdater meshUpdater = meshData.Second;

        if (mesh != null)
        {
          UnityEngine.Object.DestroyImmediate(mesh);
        }  
        if (meshUpdater != null)
        {
          CaronteSharp.Tools.DestroyMeshUpdater( meshUpdater );
        }
      }

      foreach( var pair in dictBodyMeshColliderSim_ )
      {
        Mesh mesh = pair.Value;
        if (mesh != null)
        {
          UnityEngine.Object.DestroyImmediate(mesh);
        }  

      }

      foreach( var pair in dictBodyRopeInit_ )
      {
        Tuple2<Mesh, Vector3> ropeInit = pair.Value;
        Mesh mesh = ropeInit.First;

        if (mesh != null && !AssetDatabase.Contains( mesh.GetInstanceID() ) )   
        {
          UnityEngine.Object.DestroyImmediate(mesh);
        }
      }

      if (tmpSimulationGO_ != null)
      {
        UnityEngine.Object.DestroyImmediate(tmpSimulationGO_);
      }
      
      dictBodyTransformSim_   .Clear();
      dictBodyMeshRenderSim_  .Clear();
      dictBodyMeshColliderSim_.Clear();
      dictGOtoBodyIdSim_      .Clear();
      dictBodyRopeInit_       .Clear();

      RestoreEditingObjectsState();
    }
    //-----------------------------------------------------------------------------------
    private void UpdateBodyReference(uint idBody, GameObject editionGO, GameObject simulationGO, GameObject bodyNodeGO)
    {
      Vector3 localScale    = editionGO.transform.localScale;
      Vector3 localRotation = editionGO.transform.localEulerAngles;
      Vector3 localPosition = editionGO.transform.localPosition;

      simulationGO.transform.parent = editionGO.transform.parent;

      simulationGO.transform.localScale       = localScale;
      simulationGO.transform.localEulerAngles = localRotation;
      simulationGO.transform.localPosition    = localPosition;

      simulationGO.transform.parent = bodyNodeGO.transform;

      dictBodyTransformSim_[idBody]    = simulationGO.transform;
      dictGOtoBodyIdSim_[simulationGO] = idBody;

      if ( IsBMeshAnimatedByArrPos(idBody) )
      {
        MeshFilter meshFilter;
        Mesh mesh = simulationGO.GetMesh(out meshFilter);   
        if (mesh != null && meshFilter != null)
        {
          Mesh bakedMesh;
          bool wasBaked = editionGO.GetBakedMesh(out bakedMesh);
          if (wasBaked)
          {
            bakedMesh.hideFlags        = HideFlags.DontSave;
            meshFilter.sharedMesh      = bakedMesh;

            MeshComplex bakedMesh_car = new MeshComplex();
            bakedMesh_car.Set( bakedMesh );

            dictBodyMeshRenderSim_[idBody] = Tuple2.New( bakedMesh, CaronteSharp.Tools.CreateMeshUpdater(bakedMesh_car) );
            simulationGO.transform.localScale = Vector3.one;
          } 
        }
      }
  
      if ( IsSoftbody(idBody) || IsCloth(idBody) || IsRope(idBody) )
      {
        MeshFilter meshFilter;
        Mesh mesh = simulationGO.GetMesh(out meshFilter);
        if (mesh != null && meshFilter != null)
        {
          MeshComplex sbMesh_car = new MeshComplex();

          if ( IsRope(idBody) )
          {    
            Vector3 center = BodyManager.GetRunningBoxWorldCenter(idBody);
            simulationGO.transform.localPosition = center;
            simulationGO.transform.localRotation = Quaternion.identity;
            simulationGO.transform.localScale    = Vector3.one;

            MeshSimple meshSimple = SoftbodyManager.GetMeshCollider(idBody);
            UnityEngine.Mesh colliderMesh = new UnityEngine.Mesh();
            colliderMesh.name = mesh.name + "_collider";
            colliderMesh.vertices  = meshSimple.arrPosition_;
            colliderMesh.triangles = meshSimple.arrIndices_;
            colliderMesh.RecalculateNormals();
            colliderMesh.RecalculateBounds();
            colliderMesh.hideFlags  = HideFlags.DontSave;

            Caronte_Fx_Body cfxBody = simulationGO.GetComponent<Caronte_Fx_Body>();
            cfxBody.SetCustomCollider(colliderMesh);
            dictBodyMeshColliderSim_[idBody] = colliderMesh;
            

            MeshComplex meshComplex = RopeManager.GetMeshRender(idBody);

            UnityEngine.Mesh renderMesh = new UnityEngine.Mesh();
            renderMesh.name = mesh.name + "_render";
            renderMesh.vertices  = meshComplex.arrPosition_;
            renderMesh.normals   = meshComplex.arrNormal_;
            renderMesh.tangents  = meshComplex.arrTan_;
            renderMesh.uv        = meshComplex.arrUV_;
            renderMesh.triangles = meshComplex.arrIndex_;

            meshFilter.sharedMesh = renderMesh;

            sbMesh_car.Set( renderMesh );
            dictBodyMeshRenderSim_[idBody] = Tuple2.New( renderMesh, CaronteSharp.Tools.CreateMeshUpdater(sbMesh_car) );

            Mesh originalRenderMesh = Object.Instantiate(renderMesh);
            originalRenderMesh.name = renderMesh.name;
            dictBodyRopeInit_.Add( idBody, Tuple2.New(originalRenderMesh, center ) );
          }
          else
          {
            Mesh sbMesh_un = Object.Instantiate(mesh);
            sbMesh_un.name = mesh.name;

            sbMesh_un.hideFlags   = HideFlags.DontSave;
            meshFilter.sharedMesh = sbMesh_un;

            sbMesh_car.Set( sbMesh_un );
            dictBodyMeshRenderSim_[idBody] = Tuple2.New( sbMesh_un, CaronteSharp.Tools.CreateMeshUpdater(sbMesh_car) );

            if ( IsCloth(idBody) )
            {
              Caronte_Fx_Body cfxBody = simulationGO.GetComponent<Caronte_Fx_Body>();

              if ( cfxBody != null && cfxBody.IsCustomCollider() )
              {
                MeshSimple meshSimple = ClothManager.GetMeshCollider(idBody);
                UnityEngine.Mesh colliderMesh = new UnityEngine.Mesh();

                colliderMesh.vertices = meshSimple.arrPosition_;
                colliderMesh.triangles = meshSimple.arrIndices_;

                Vector3[] arrNormal = new Vector3[meshSimple.arrPosition_.Length];
                colliderMesh.normals   = arrNormal;

                dictBodyMeshColliderSim_[idBody] = colliderMesh;
              }
            }
            else if ( IsSoftbody(idBody) )
            {
              Caronte_Fx_Body cfxBody = simulationGO.GetComponent<Caronte_Fx_Body>();

              if ( cfxBody != null && cfxBody.IsCustomCollider() )
              {
                MeshSimple meshSimple = SoftbodyManager.GetMeshCollider(idBody);
                UnityEngine.Mesh colliderMesh = new UnityEngine.Mesh();

                colliderMesh.vertices  = meshSimple.arrPosition_;
                colliderMesh.triangles = meshSimple.arrIndices_;

                Vector3[] arrNormal = new Vector3[meshSimple.arrPosition_.Length];
                colliderMesh.normals   = arrNormal;

                dictBodyMeshColliderSim_[idBody] = colliderMesh;
              }
            }   
          }
        }
      }    
    }
    //-----------------------------------------------------------------------------------
    public void DisableEditingObjects()
    {
      foreach (Tuple_GameObjectState gameobjectState in listGameObjectState_)
      {
        GameObject gameObject = gameobjectState.First;

        gameObject.SetActive(false);
      }
    }
    //-----------------------------------------------------------------------------------
    private void RestoreEditingObjectsState()
    {
      foreach (Tuple_GameObjectState gameobjectState in listGameObjectState_)
      {
        GameObject go   = gameobjectState.First;
        bool activeSelf = gameobjectState.Second;
        if (go != null)
        {
          go.SetActive(activeSelf);
        }
      }

      listGameObjectState_.Clear();
    }
    //-----------------------------------------------------------------------------------
    public bool CreateAnimatorTmpGameObjects( List<CRAnimationData> listAnimationData )
    {
      if (listAnimationData.Count == 0)
        return false;

      foreach( CRAnimationData animationData in listAnimationData )
      {
        GameObject[] arrAnimatorGO = animationData.arrRootGameObjects_;

        for (int i = 0; i < arrAnimatorGO.Length; i++)
        {
          GameObject animatorGO = arrAnimatorGO[i];

          Vector3 localScale    = animatorGO.transform.localScale;
          Vector3 localRotation = animatorGO.transform.localEulerAngles;
          Vector3 localPosition = animatorGO.transform.localPosition;
          
          GameObject clonedGO = (GameObject)UnityEngine.Object.Instantiate(animatorGO);

          Transform animatorGOParentTr = animatorGO.transform.parent;

          GameObject dummy = null;
          if (animatorGOParentTr != null)
          {
            dummy = new GameObject( "dummy_" + animatorGO.name );
            
            Transform animatorGOGrandParentTr = animatorGOParentTr.parent;

            dummy.transform.parent        = animatorGOGrandParentTr;
            dummy.transform.localPosition = animatorGOParentTr.localPosition;
            dummy.transform.localRotation = animatorGOParentTr.localRotation;
            dummy.transform.localScale    = animatorGOParentTr.localScale;
          }

          clonedGO.transform.parent            = animatorGOParentTr;
          clonedGO.transform.localScale        = localScale;
          clonedGO.transform.localEulerAngles  = localRotation;
          clonedGO.transform.localPosition     = localPosition;

          if (dummy != null)
          {
            dummy.transform.parent = tmpSimulationGO.transform;
            clonedGO.transform.parent = dummy.transform;
          }
          else
          {
            clonedGO.transform.parent = tmpSimulationGO.transform;
          }

          clonedGO.SetActive( true );

          Animator clonedAnimator = clonedGO.GetComponent<Animator>();
          clonedAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

          listAnimatorObjectsBuffering_.Add(clonedGO);

          arrAnimatorGO[i] = clonedGO;
        }

        animationData.UpdateAnimatorInfo();
        animationData.SetModeAnimation(true);
      }

      return true;
    }
    //-----------------------------------------------------------------------------------
    public void DestroyAnimatorTmpGameObjects()
    {
      for (int i = 0; i < listAnimatorObjectsBuffering_.Count; i++)
      {
        GameObject go = listAnimatorObjectsBuffering_[i];
        if (go.transform.parent != tmpSimulationGO.transform)
        {
          UnityEngine.Object.DestroyImmediate(go.transform.parent.gameObject);
        }
        else
        {
          UnityEngine.Object.DestroyImmediate(listAnimatorObjectsBuffering_[i]);
        }   
      }

      listAnimatorObjectsBuffering_.Clear();
    }

    //-----------------------------------------------------------------------------------
    public Dictionary<uint, CNContactEmitter> GetTableIdToContactEmitter()
    {
      Dictionary<uint, CNContactEmitter> tableIdToContactEmitter = new Dictionary<uint,CNContactEmitter>();
      
      foreach( KeyValuePair<uint, CNEntity> pair in tableIdEntityToNode_)
      {
        CNContactEmitter ceNode = pair.Value as CNContactEmitter;
        if (ceNode != null && ceNode.IsNodeEnabledInHierarchy && !ceNode.IsNodeExcludedInHierarchy )
        {
          tableIdToContactEmitter.Add( pair.Key, ceNode );
        }
      }

      return tableIdToContactEmitter;
    }
    //-----------------------------------------------------------------------------------
    public void GetListBodyRendererAndCollider( out List<Renderer> listBodyRenderer, out List<MonoBehaviour> listBodyCollider )
    {
      listBodyRenderer = listBodyRenderer_;
      listBodyCollider = listBodyCollider_;

      listGameObjectAux_.Clear();
      listTransformAux_.Clear();   
      listBodyRenderer_.Clear();
      listBodyCollider_.Clear();

      if ( SimulationManager.IsEditing() )
      {
        listGameObjectAux_.AddRange( tableIdBodyToGO_.AllSecond() );
        foreach(GameObject go in listGameObjectAux_)
        {
          if (go != null)
          {
            Renderer rn = go.GetComponent<Renderer>();
            Caronte_Fx_Body cfx = go.GetComponent<Caronte_Fx_Body>();

            if (rn != null && cfx != null && cfx.IsCustomCollider() )
            {
              listBodyRenderer_.Add(rn);
              listBodyCollider_.Add(cfx);       
            }
          }
        }
      }
      else
      {
        listTransformAux_.AddRange( dictBodyTransformSim_.Values );
        foreach( Transform tr in listTransformAux_ )
        {
          if ( tr != null)
          {
            Renderer rn = tr.gameObject.GetComponent<Renderer>();
            Caronte_Fx_Body cfx = tr.gameObject.GetComponent<Caronte_Fx_Body>();
            if (rn != null && cfx != null && cfx.IsCustomCollider() )
            {
              listBodyRenderer_.Add(rn);
              listBodyCollider_.Add(cfx);
            }
          }
        }
      }
    }
  }
}
