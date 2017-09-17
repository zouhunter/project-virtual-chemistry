using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CaronteFX
{
  public class CNServos : CommandNode
  {

    #region createParams
    [SerializeField]
    CNField objectsA_;
    public CNField ObjectsA
    {
      get
      {
        if ( objectsA_ == null )
        {
          objectsA_ = new CNField( false, CNField.AllowedTypes.Geometry | CNField.AllowedTypes.RigidBodyNode, 
                                   CNField.ScopeFlag.Inherited, false );
        }
        return objectsA_;
      }
    }

    [SerializeField]
    CNField objectsB_;
    public CNField ObjectsB
    {
      get
      {
        if ( objectsB_ == null )
        {
          objectsB_ = new CNField(false, CNField.AllowedTypes.Geometry | CNField.AllowedTypes.RigidBodyNode, 
                                   CNField.ScopeFlag.Inherited, false );
        }
        return objectsB_;
      }
    }

    [SerializeField]
    bool isLinearOrAngular_;
    public bool IsLinearOrAngular
    {
      get { return isLinearOrAngular_; }
      set { isLinearOrAngular_ = value; }
    }

    [SerializeField]
    bool isPositionOrVelocity_;
    public bool IsPositionOrVelocity
    {
      get { return isPositionOrVelocity_; }
      set { isPositionOrVelocity_ = value; }
    }
    
    [SerializeField]
    bool isCreateModeNearest_ = true;
    public bool IsCreateModeNearest
    {
      get { return isCreateModeNearest_; }
      set { isCreateModeNearest_ = value; }
    }

    [SerializeField]
    bool isCreateModeChain_ = false;
    public bool IsCreateModeChain
    {
      get { return isCreateModeChain_; }
      set { isCreateModeChain_ = value; }
    }

    [SerializeField]
    bool isFreeX_ = false;
    public bool IsFreeX
    {
      get { return isFreeX_; }
      set { isFreeX_ = value; }
    }

    [SerializeField]
    bool isFreeY_ = false;
    public bool IsFreeY
    {
      get { return isFreeY_; }
      set { isFreeY_ = value; }
    }

    [SerializeField]
    bool isFreeZ_ = false;
    public bool IsFreeZ
    {
      get { return isFreeZ_;}
      set { isFreeZ_ = value; }
    }

    [SerializeField]
    bool isBlockedX_ = false;
    public bool IsBlockedX
    {
      get { return isBlockedX_; }
      set { isBlockedX_ = value; }
    }

    [SerializeField]
    bool isBlockedY_ = false;
    public bool IsBlockedY
    {
      get { return isBlockedY_; }
      set { isBlockedY_ = value; }
    }

    [SerializeField]
    bool isBlockedZ_ = false;
    public bool IsBlockedZ
    {
      get { return isBlockedZ_; }
      set { isBlockedZ_ = value; }
    }

    [SerializeField]
    bool disableCollisionByPairs_ = false;
    public bool DisableCollisionByPairs
    {
      get { return disableCollisionByPairs_; }
      set { disableCollisionByPairs_ = value; }
    }

    [SerializeField]
    bool multiplierFoldOut_ = false;
    public bool MultiplierFoldOut
    {
      get { return multiplierFoldOut_; }
      set { multiplierFoldOut_ = value; }
    }

    [SerializeField]
    bool breakFoldOut_ = false;
    public bool BreakFoldout
    {
      get { return breakFoldOut_; }
      set { breakFoldOut_ = value; }
    }

    #endregion

    #region editParams

    [SerializeField]
    Vector3 targetExternal_LOCAL_ = Vector3.zero;
    public Vector3 TargetExternal_LOCAL
    {
      get { return targetExternal_LOCAL_; }
      set { targetExternal_LOCAL_ = value; }
    }

    [SerializeField]
    float reactionTime_ = 0.01f;
    public float ReactionTime
    {
      get { return reactionTime_; }
      set { reactionTime_ = Mathf.Clamp(value, 0f, float.MaxValue); }
    }

    [SerializeField]
    float overreactionDelta_ = 0.01f;
    public float OverreactionDelta
    {
      get { return overreactionDelta_; }
      set { overreactionDelta_ = Mathf.Clamp(value, 0f, float.MaxValue); }
    }

    [SerializeField]
    float speedMax_ = 20.0f;
    public float SpeedMax
    {
      get { return speedMax_; }
      set { speedMax_ = value; }
    }

    [SerializeField]
    bool maximumSpeed_ = false;
    public bool MaximumSpeed
    {
      get { return maximumSpeed_; }
      set { maximumSpeed_ = value; }
    }

    [SerializeField]
    float powerMax_ = 1000.0f;
    public float PowerMax
    {
      get { return powerMax_; }
      set { powerMax_ = value; }
    }

    [SerializeField]
    bool maximumPower_ = false;
    public bool MaximumPower
    {
      get { return maximumPower_; }
      set { maximumPower_ = value; }
    }

    [SerializeField]
    float forceMax_ = 1000.0f;
    public float ForceMax
    {
      get { return forceMax_; }
      set { forceMax_ = value; }
    }

    [SerializeField]
    bool maximumForce_ = false;
    public bool MaximumForce
    {
      get { return maximumForce_; }
      set { maximumForce_ = value; }
    }

    [SerializeField]
    float brakePowerMax_ = 5000.0f;
    public float BrakePowerMax
    {
      get { return brakePowerMax_; }
      set { brakePowerMax_ = value; }
    }

    [SerializeField]
    bool maximumBrakePowerMax_ = false;
    public bool MaximumBrakePowerMax
    {
      get { return maximumBrakePowerMax_; }
      set { maximumBrakePowerMax_ = value; }
    }

    [SerializeField]
    float brakeForceMax_ = 5000.0f;
    public float BrakeForceMax
    {
      get { return brakeForceMax_; }
      set { brakeForceMax_ = value; }
    }

    [SerializeField]
    bool maximumBrakeForceMax_ = false;
    public bool MaximumBrakeForceMax
    {
      get { return maximumBrakeForceMax_; }
      set { maximumBrakeForceMax_ = value; }
    }

    [SerializeField]
    bool isBreakIfDist_ = false;
    public bool IsBreakIfDist
    {
      get { return isBreakIfDist_; }
      set { isBreakIfDist_ = value; }
    }

    [SerializeField]
    bool isBreakIfAng_ = false;
    public bool IsBreakIfAng
    {
      get { return isBreakIfAng_; }
      set { isBreakIfAng_ = value; }
    }

    [SerializeField]
    float breakDistance_ = 1.0f;
    public float BreakDistance
    {
      get { return breakDistance_; }
      set { breakDistance_ = Mathf.Clamp(value, 0f, float.MaxValue); }
    }

    [SerializeField]
    float breakAngleInDegrees_ = 1.0f;
    public float BreakAngleInDegrees
    {
      get { return breakAngleInDegrees_; }
      set { breakAngleInDegrees_ = Mathf.Clamp(value, 0f, float.MaxValue); }
    }

    [SerializeField]
    float dampingForce_ = 0.0f;
    public float DampingForce
    {
      get { return dampingForce_; }
      set { dampingForce_ = Mathf.Clamp(value, 0f, float.MaxValue); }
    }

    [SerializeField]
    float distStepToDefineMultiplierDependingOnDist_ = 1.0f;
    public float DistStepToDefineMultiplierDependingOnDist
    {
      get { return distStepToDefineMultiplierDependingOnDist_; }
      set { distStepToDefineMultiplierDependingOnDist_ = value; }
    }

    [SerializeField]
    AnimationCurve functionMultiplierDependingOnDist_ = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    public AnimationCurve FunctionMultiplierDependingOnDist
    {
      get { return functionMultiplierDependingOnDist_; }
      set { functionMultiplierDependingOnDist_ = value; }
    }

    [SerializeField]
    float multiplier_ = 1.0f;
    public float Multiplier
    {
      get { return multiplier_; }
      set { multiplier_ = value; }
    }

    #endregion

    public override CommandNode DeepClone(GameObject dataHolder)
    {
      CNServos clone = CRTreeNode.CreateInstance<CNServos>(dataHolder);
      
      clone.objectsA_  = ObjectsA.DeepClone();
      clone.objectsB_  = ObjectsB.DeepClone();

      clone.Name         = Name;

      clone.isLinearOrAngular_ = isLinearOrAngular_;

      clone.isPositionOrVelocity_ = isPositionOrVelocity_ ;
      clone.isCreateModeNearest_  = isCreateModeNearest_;
      clone.isCreateModeChain_    = isCreateModeChain_;

      clone.isFreeX_ = isFreeX_;
      clone.isFreeY_ = isFreeY_;
      clone.isFreeZ_ = isFreeZ_;

      clone.isBlockedX_ = isBlockedX_;
      clone.isBlockedY_ = isBlockedY_;
      clone.isBlockedZ_ = isBlockedZ_;

      clone.disableCollisionByPairs_ = disableCollisionByPairs_;

      clone.targetExternal_LOCAL_ = targetExternal_LOCAL_;

      clone.reactionTime_ = reactionTime_;
      clone.overreactionDelta_ = overreactionDelta_;;

      clone.speedMax_ = speedMax_;
      clone.powerMax_ = powerMax_;
      clone.forceMax_ = forceMax_;

      clone.brakePowerMax_ = brakePowerMax_;
      clone.brakeForceMax_ = brakeForceMax_;

      clone.isBreakIfDist_ = isBreakIfDist_;

      clone.isBreakIfAng_ = isBreakIfAng_;

      clone.breakDistance_       = breakDistance_;
      clone.breakAngleInDegrees_ = breakAngleInDegrees_;

      clone.dampingForce_ = dampingForce_;

      clone.distStepToDefineMultiplierDependingOnDist_ = distStepToDefineMultiplierDependingOnDist_;

      clone.functionMultiplierDependingOnDist_         = new AnimationCurve();

      int nKeys = functionMultiplierDependingOnDist_.length;
      Keyframe[] arrKeys = functionMultiplierDependingOnDist_.keys;

      Keyframe[] arrClonedKey = new Keyframe[ nKeys ];

      for (int i = 0; i < nKeys; i++)
      {
        arrClonedKey[i] = arrKeys[i];
      }
      clone.functionMultiplierDependingOnDist_.keys = arrClonedKey;

      clone.multiplier_ = multiplier_;
      return clone;
    }

    public override bool UpdateNodeReferences(Dictionary<CommandNode, CommandNode> dictNodeToClonedNode)
    {
      bool updatedA = objectsA_.UpdateNodeReferences(dictNodeToClonedNode);
      bool updatedB = objectsB_.UpdateNodeReferences(dictNodeToClonedNode);

      return (updatedA || updatedB);
    }


  }
}
