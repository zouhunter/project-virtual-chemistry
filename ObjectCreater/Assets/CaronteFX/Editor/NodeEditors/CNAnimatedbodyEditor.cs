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
using System.Collections;
using System.Collections.Generic;


namespace CaronteFX
{
  public class CNAnimatedbodyEditor : CNRigidbodyEditor
  {
    public static Texture icon_;
    public override Texture TexIcon{ get{ return icon_; } }

    List<Animator> listAnimator_ = new List<Animator>();

    List<RuntimeAnimatorController> listRtAnimatorController_   = new List<RuntimeAnimatorController>();
    List<RuntimeAnimatorController> listOvrrAnimatorController_ = new List<RuntimeAnimatorController>();

    new public CNAnimatedbody Data { get; set; }

    public CNAnimatedbodyEditor(CNAnimatedbody data, CNBodyEditorState state)
      : base( data, state )
    {
      Data = (CNAnimatedbody)data;
    }
    //-----------------------------------------------------------------------------------
    public GameObject[] GetAnimatorGameObjects()
    {
      List<GameObject> listGameObject = new List<GameObject>();
      
      List<GameObject> listBodyObjects = GetGameObjects();
      foreach(var go in listBodyObjects)
      {
        Animator animator = CREditorUtils.GetFirstAnimatorInHierarchy(go);
        if (animator != null)
        {
          GameObject animatorGO = animator.gameObject;
          if ( !listGameObject.Contains( animatorGO ) )
          {
            listGameObject.Add( animatorGO );
          }
        }
      }

      return listGameObject.ToArray();
    }
    //-----------------------------------------------------------------------------------
    public override void FreeResources()
    {
      base.FreeResources();
    }
    //----------------------------------------------------------------------------------
    public override void LoadInfo()
    {
      base.LoadInfo();
    }
    //----------------------------------------------------------------------------------
    public override void StoreInfo()
    {
      base.StoreInfo();
    }
    //-----------------------------------------------------------------------------------
    public override void AddGameObjects( UnityEngine.Object[] draggedObjects, bool recalculateFields )
    {
      FieldController.AddGameObjects( draggedObjects, recalculateFields );
    }
    //-----------------------------------------------------------------------------------
    public override void CreateBodies( GameObject[] arrGameObject )
    {
      CreateBodies(arrGameObject, "Caronte FX - Animated body creation", "Creating " + Data.Name + " animated bodies. ");
    }
    //-----------------------------------------------------------------------------------
    public override void DestroyBodies(GameObject[] arrGameObject)
    {
      DestroyBodies(arrGameObject, "Caronte FX - Animated body destruction", "Destroying " + Data.Name + " animated bodies. ");
    }
    //----------------------------------------------------------------------------------
    private void SampleAnimationController( GameObject go )
    {
      if (Data.UN_AnimationClip != null)
      {
        Animator animator = CREditorUtils.GetFirstAnimatorInHierarchy(go);
        if (animator != null)
        {
          if (Data.OverrideAnimationController)
          {
            OverrideAnimatorController(animator);
          }
          animator.Update(0.0f);
        }
      }
    }
    //----------------------------------------------------------------------------------
    private void OverrideAnimatorController(Animator animator)
    {
      RuntimeAnimatorController controller = animator.runtimeAnimatorController;
 
      AnimatorOverrideController ovrrController = new AnimatorOverrideController();

      listAnimator_              .Add(animator);
      listRtAnimatorController_  .Add(controller);
      listOvrrAnimatorController_.Add(ovrrController);

      ovrrController.runtimeAnimatorController = CRAnimationData.animatorSampler_;
      animator.runtimeAnimatorController = ovrrController;

#if CR_UNITY_5_X
      AnimationClip[] clips = ovrrController.animationClips;
      foreach (AnimationClip animClip in clips)
      {
        ovrrController[animClip] = Data.UN_AnimationClip;
      }
#endif

#if CR_UNITY_4_X
      AnimationClipPair[] clips = ovrrController.clips;
      foreach (AnimationClipPair animClipPair in clips)
      {
        animClipPair.overrideClip = un_animationClip_;
      }
      animator.runtimeAnimatorController = ovrrController;
#endif
    }
    //----------------------------------------------------------------------------------
    private void RestoreAnimatorController( GameObject go )
    {
      for (int i = 0; i < listRtAnimatorController_.Count; i++)
      {
        Animator animator              = listAnimator_[i];
        RuntimeAnimatorController rt   = listRtAnimatorController_[i];
        RuntimeAnimatorController ovrr = listOvrrAnimatorController_[i];

        animator.runtimeAnimatorController = rt;
        Object.DestroyImmediate(ovrr);
      }

      listAnimator_              .Clear();
      listRtAnimatorController_  .Clear();
      listOvrrAnimatorController_.Clear();
    }
    //----------------------------------------------------------------------------------
    protected override void ActionCreateBody(GameObject go)
    {
      SampleAnimationController(go);
      eManager.CreateBody(Data, go);
      RestoreAnimatorController( go );
    }
    //----------------------------------------------------------------------------------
    protected override void ActionDestroyBody(GameObject go)
    {
      eManager.DestroyBody(Data, go);
    }
    //----------------------------------------------------------------------------------
    protected override void ActionCheckBodyForChanges( GameObject go, bool recreateIfInvalid )
    {
      if (recreateIfInvalid)
      {
        SampleAnimationController( go );
      }
      eManager.CheckBodyForChanges(Data, go, recreateIfInvalid);
      if (recreateIfInvalid)
      {
        RestoreAnimatorController( go );
      }
    }
    //----------------------------------------------------------------------------------
    private void DrawAnimationClip()
    {
      /*
      EditorGUI.BeginChangeCheck();
      Data.OverrideAnimationController = EditorGUILayout.Toggle("Override animation controller", Data.OverrideAnimationController );
      if ( EditorGUI.EndChangeCheck() )
      {
        EditorUtility.SetDirty(Data);
        EditorApplication.delayCall += RecreateBodies;   
      }
      */

      EditorGUI.BeginDisabledGroup( !Data.OverrideAnimationController );
      EditorGUI.BeginChangeCheck();
      Data.UN_AnimationClip = (AnimationClip)EditorGUILayout.ObjectField("Animation clip(unity)", Data.UN_AnimationClip, typeof(AnimationClip), true);
      if ( EditorGUI.EndChangeCheck() )
      {
        EditorUtility.SetDirty(Data);
        EditorApplication.delayCall += RecreateBodies;  
      }
      EditorGUI.EndDisabledGroup();
    }
    //----------------------------------------------------------------------------------
    private void DrawTimeStart()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( new GUIContent("Time start"), Data.TimeStart );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject( Data, "Change time start");
        Data.TimeStart = Mathf.Clamp(value, 0f, float.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    private void DrawTimeLength()
    {
      EditorGUI.BeginChangeCheck();
      var timelengthValue = CRGUIExtension.FloatTextField("Time length", Data.TimeLength, 0.0f, 10000.0f, float.MaxValue, "-");
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject( Data, "Change time length");
        Data.TimeLength = timelengthValue;
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    private void DrawTimeLengthReset()
    {
      GUI.SetNextControlName("reset");
      if (GUILayout.Button("reset", EditorStyles.miniButton, GUILayout.Width(50f)))
      {
        GUI.FocusControl("reset");
        Undo.RecordObject( Data, "Change time length");
        Data.TimeLength = float.MaxValue;
        EditorUtility.SetDirty(Data);
      }
    }
    //----------------------------------------------------------------------------------
    protected override void RenderFieldsBody(bool isEditable)
    {
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      CRGUIUtils.DrawSeparator();
      CRGUIUtils.Splitter();

      float originalLabelWidth = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = label_width;

      scroller_ = EditorGUILayout.BeginScrollView(scroller_);
      EditorGUI.BeginDisabledGroup( !isEditable );
      EditorGUILayout.Space();

      DrawDoCollide();
      EditorGUILayout.Space();
      
      EditorGUILayout.Space();
      EditorGUILayout.Space();
      DrawAnimationClip();
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      DrawTimeStart();

      GUILayout.BeginHorizontal();
      DrawTimeLength();
      DrawTimeLengthReset();
      GUILayout.EndHorizontal();
      EditorGUILayout.Space();

      CRGUIUtils.Splitter();
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      DrawRestitution();
      DrawFrictionKinetic();

      GUILayout.BeginHorizontal();
      EditorGUI.BeginDisabledGroup(Data.FromKinetic);
      DrawFrictionStatic();
      EditorGUI.EndDisabledGroup();
      DrawFrictionStaticFromKinetic();
      GUILayout.EndHorizontal();

      EditorGUIUtility.labelWidth = originalLabelWidth;

      EditorGUI.EndDisabledGroup();
      EditorGUILayout.EndScrollView();
    }
  }


}

