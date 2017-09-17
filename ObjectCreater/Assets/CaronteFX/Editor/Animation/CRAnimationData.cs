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

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;

namespace CaronteFX
{

  public class CRAnimationData
  {
    public class CRAnimatorInfo
    {
      Animator                   animator_;
      RuntimeAnimatorController  runtimeAnimationController_;
      AnimatorOverrideController ovrrAnimationController_;

      uint[]                arrIdBodyNormalGameObjects_;
      MeshRenderer[]        arrNormalMeshRenderer_;

      uint[]                arrIdBodySkinnedGameObjects_;
      SkinnedMeshRenderer[] arrSkinnedMeshRenderer_;

      public CRAnimatorInfo(GameObject rootGameObject)
      {
        CREditorUtils.GetRenderersFromRoot(rootGameObject, out arrNormalMeshRenderer_, out arrSkinnedMeshRenderer_);
        AssignBodyIds();
      }

      public void AssignAnimatorController(GameObject rootGameObject, AnimationClip animationClip)
      {
        CREditorUtils.GetRenderersFromRoot(rootGameObject, out arrNormalMeshRenderer_, out arrSkinnedMeshRenderer_);
 
        animator_ = rootGameObject.GetComponent<Animator>();
        animator_.runtimeAnimatorController = animatorSampler_;

        runtimeAnimationController_ = animator_.runtimeAnimatorController;
        ovrrAnimationController_ = new AnimatorOverrideController();

        ovrrAnimationController_.runtimeAnimatorController = runtimeAnimationController_;

#if CR_UNITY_5_X
        AnimationClip[] clips = ovrrAnimationController_.animationClips;
        foreach (AnimationClip animClip in clips)
        {
          ovrrAnimationController_[animClip] = animationClip;
        }
        animator_.runtimeAnimatorController = ovrrAnimationController_;
#endif

#if CR_UNITY_4_X
        AnimationClipPair[] clips = ovrrAnimationController_.clips;
        foreach (AnimationClipPair animClipPair in clips)
        {
          animClipPair.overrideClip = animationClip;
        }
        animator_.runtimeAnimatorController = ovrrAnimationController_;
#endif
      }

      private void AssignBodyIds()
      {
        CNManager cnManager = CNManager.Instance;
        CREntityManager entityManager = cnManager.EntityManager;

        GameObject[] normalObjects = getNormalObjects();

        arrIdBodyNormalGameObjects_ = new uint[normalObjects.Length];

        for (int i = 0; i < normalObjects.Length; i++)
        {
          GameObject go = normalObjects[i];
          if ( entityManager.IsGameObjectAnimated( go ) )
          {
            uint idBody = entityManager.GetIdBodyFromGo( go );
            arrIdBodyNormalGameObjects_[i] = idBody;
          }    
          else
          {
            arrIdBodyNormalGameObjects_[i] = uint.MaxValue;
          }
        }

       
        GameObject[] skinnedObjects = getSkinnedObjects();

        arrIdBodySkinnedGameObjects_ = new uint[skinnedObjects.Length];

        for (int i = 0; i < skinnedObjects.Length; i++)
        {
          GameObject go = skinnedObjects[i];
          if ( entityManager.IsGameObjectAnimated( go ) )
          {
            uint idBody = entityManager.GetIdBodyFromGo( go );
            arrIdBodySkinnedGameObjects_[i] = idBody;
          }
          else
          {
            arrIdBodySkinnedGameObjects_[i] = uint.MaxValue;
          }
        }

      }

      public GameObject[] getNormalObjects()
      {
        List<GameObject> listGameObject = new List<GameObject>();
        foreach( MeshRenderer mr in arrNormalMeshRenderer_ )
        {
          listGameObject.Add(mr.gameObject);
        }

        return listGameObject.ToArray();
      }

      public GameObject[] getSkinnedObjects()
      {
        List<GameObject> listGameObject = new List<GameObject>();
        foreach (SkinnedMeshRenderer smr in arrSkinnedMeshRenderer_)
        {
          listGameObject.Add(smr.gameObject);
        }

        return listGameObject.ToArray();
      }

      public void UpdateSimulating( CRAnimationData animData, UnityEngine.Mesh animBakingMesh, float eventTime, float deltaTime )
      {
        animator_.Update( deltaTime );

        double targetTime = eventTime + deltaTime;
        
        for (int i = 0; i < arrSkinnedMeshRenderer_.Length; ++i)
        {
          uint idBody                    = arrIdBodySkinnedGameObjects_[i];
          SkinnedMeshRenderer smRenderer = arrSkinnedMeshRenderer_[i];

          GameObject gameObject          = smRenderer.gameObject;

          smRenderer.BakeMesh(animBakingMesh);

          if (idBody != uint.MaxValue)
          {

            Matrix4x4 m_MODEL_TO_WORLD = gameObject.transform.localToWorldMatrix;

            RigidbodyManager.Rg_addEventTargetArrPos_WORLD( (double)eventTime, targetTime, idBody, ref m_MODEL_TO_WORLD, animBakingMesh.vertices);
          }
        }

        for (int i = 0; i < arrNormalMeshRenderer_.Length; ++i)
        {
          uint idBody = arrIdBodyNormalGameObjects_[i];
          MeshRenderer renderer = arrNormalMeshRenderer_[i];
          GameObject gameObject = renderer.gameObject;

          if (idBody != uint.MaxValue)
          {
            Matrix4x4 m_MODEL_TO_WORLD = gameObject.transform.localToWorldMatrix;

            RigidbodyManager.Rg_addEventTargetPos_WORLD( (double)eventTime, targetTime, idBody, ref m_MODEL_TO_WORLD, 0.01) ;
          }
        }
        animData.timeAnimated_ += deltaTime;   
      }

      public void Reset()
      {
        Object.DestroyImmediate(ovrrAnimationController_);
      }

      public void SetModeAnimation(bool active)
      {
        foreach( uint idBody in arrIdBodyNormalGameObjects_ )
        {
          if (idBody != uint.MaxValue)
          {
            RigidbodyManager.Rg_setAnimatingMode( idBody, active );
          }

        }

        foreach (uint idBody in arrIdBodySkinnedGameObjects_)
        {
          if (idBody != uint.MaxValue)
          {
            RigidbodyManager.Rg_setAnimatingMode( idBody, active );
          }
        }
      }

    }

    public float timeStart_;
    public float timeLenght_;
    public float timeAnimated_;

    public AnimationClip clip_un_;

    public GameObject[]           arrRootGameObjects_;
    public List<CRAnimatorInfo>   listAnimatorInfo_;  
    
    public static RuntimeAnimatorController animatorSampler_;
   

    public CRAnimationData(CNAnimatedbodyEditor animNodeEditor)
    {
      timeStart_    = animNodeEditor.Data.TimeStart;
      timeLenght_   = animNodeEditor.Data.TimeLength;
      timeAnimated_ = 0;

      clip_un_      = animNodeEditor.Data.UN_AnimationClip;

      //TODO: do not create a new fieldcontroller
      arrRootGameObjects_ = animNodeEditor.GetAnimatorGameObjects();
      
      listAnimatorInfo_ = new List<CRAnimatorInfo>();
      BuildAnimatorInfo();
    }

    private void BuildAnimatorInfo()
    {
      listAnimatorInfo_.Clear();
      foreach (GameObject rootGameObject in arrRootGameObjects_)
      {
        listAnimatorInfo_.Add( new CRAnimatorInfo(rootGameObject) );
      }
    }

    public void UpdateAnimatorInfo()
    {
      for (int i = 0; i < listAnimatorInfo_.Count; i++)
      {
        CRAnimatorInfo animatorInfo   = listAnimatorInfo_[i];
        GameObject     rootGameObject = arrRootGameObjects_[i];

        animatorInfo.AssignAnimatorController(rootGameObject, clip_un_);
      }
    }

    public GameObject[] getNormalObjects()
    {
      List<GameObject> listGameObject_ = new List<GameObject>();
      foreach(CRAnimatorInfo animatorInfo in listAnimatorInfo_)
      {
        listGameObject_.AddRange( animatorInfo.getNormalObjects() );
      }
      return listGameObject_.ToArray();
    }

    public GameObject[] getSkinneObjects()
    {
      List<GameObject> listGameObject_ = new List<GameObject>();
      foreach (CRAnimatorInfo animatorInfo in listAnimatorInfo_)
      {
        listGameObject_.AddRange( animatorInfo.getSkinnedObjects() );
      }
      return listGameObject_.ToArray();
    }

    public void UpdateSimulating( UnityEngine.Mesh animBakingMesh, float eventTime, float deltaTime )
    {   
      foreach( CRAnimatorInfo animatorInfo in listAnimatorInfo_ )
      {
        animatorInfo.UpdateSimulating( this, animBakingMesh, eventTime, deltaTime );
      }
    }

    public void Reset()
    {
      foreach(CRAnimatorInfo animatorInfo in listAnimatorInfo_)
      {
        animatorInfo.Reset();
      }
    }

    public void SetModeAnimation(bool active)
    {
      foreach (CRAnimatorInfo animatorInfo in listAnimatorInfo_)
      {
        animatorInfo.SetModeAnimation(active);
      }
    }
  }

}

#endif