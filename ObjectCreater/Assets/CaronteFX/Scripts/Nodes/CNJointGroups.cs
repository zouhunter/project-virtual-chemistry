using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CaronteFX
{
  public class CNJointGroups : CommandNode
  {
    public enum ForceMaxModeEnum
    {
      Unlimited,
      ConstantLimit,
    }

    public enum CreationModeEnum
    {
      ByContact,
      ByStem,
      ByMatchingVertices,
      AtLocatorsPositions,
      AtLocatorsBBoxCenters,
      AtLocatorsVertexes
    }

    #region createParams
    [SerializeField]
    CNField objectsA_;
    public CNField ObjectsA
    {
      get
      {
        if ( objectsA_ == null )
        {
          objectsA_ = new CNField( false, CNField.AllowedTypes.Geometry | CNField.AllowedTypes.BodyNode, 
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
          objectsB_ = new CNField( false, CNField.AllowedTypes.Geometry | CNField.AllowedTypes.BodyNode, 
                                   CNField.ScopeFlag.Inherited, false );
        }
        return objectsB_;
      }
    }

    [SerializeField]
    CNField locatorsC_;
    public CNField LocatorsC
    {
      get
      {
        if ( locatorsC_ == null )
        {
          locatorsC_ = new CNField( false, CNField.AllowedTypes.Locator | CNField.AllowedTypes.Geometry,
                                    CNField.ScopeFlag.Inherited, false );
        }
        return locatorsC_;
      }
    }

    [SerializeField]
    CreationModeEnum creationMode_ = CreationModeEnum.ByContact;
    public CreationModeEnum CreationMode
    {
      get { return creationMode_; }
      set { creationMode_ = value; }
    }

    [SerializeField]
    bool isRigidGlue_ = false;
    public bool IsRigidGlue
    {
      get { return isRigidGlue_; }
      set { isRigidGlue_ = value; }
    }


    #region By Contact Parameters
    [SerializeField]
    float contactDistanceSearch_ = 0.01f;
    public float ContactDistanceSearch
    {
      get { return contactDistanceSearch_; }
      set { contactDistanceSearch_ = value; }
    }

    [SerializeField]
    float contactAreaMin_ = 0.0001f;
    public float ContactAreaMin
    {
      get { return contactAreaMin_; }
      set { contactAreaMin_ = value; }
    }
    
    [SerializeField]
    float contactAngleMaxInDegrees_ = 1.0f;
    public float ContactAngleMaxInDegrees
    {
      get { return contactAngleMaxInDegrees_; }
      set { contactAngleMaxInDegrees_ = value; }
    }

    [SerializeField]
    int contactNumberMax_ = 4;
    public int ContactNumberMax
    {
      get { return contactNumberMax_; }
      set { contactNumberMax_ = Mathf.Clamp( value, 0, int.MaxValue ); }
    }

    #endregion

    [SerializeField]
    float matchingDistanceSearch_ = 0.002f;
    public float MatchingDistanceSearch
    {
      get { return matchingDistanceSearch_; }
      set { matchingDistanceSearch_ = value; }
    }

    [SerializeField]
    bool limitNumberOfActiveJoints_ = false;
    public bool LimitNumberOfActiveJoints
    {
      get { return limitNumberOfActiveJoints_; }
      set { limitNumberOfActiveJoints_ = value; }
    }

    [SerializeField]
    int activeJointsMaxInABPair_ = 0;
    public int ActiveJointsMaxInABPair
    {
      get { return activeJointsMaxInABPair_; }
      set { activeJointsMaxInABPair_ = value; }
    }

    [SerializeField]
    bool disableCollisionsByPairs_ = false;
    public bool DisableCollisionsByPairs
    {
      get { return disableCollisionsByPairs_; }
      set { disableCollisionsByPairs_ = value; }
    }

    [SerializeField]
    bool disableAllCollisionsOfAsWithBs_ = false;
    public bool DisableAllCollisionsOfAsWithBs
    {
      get { return disableAllCollisionsOfAsWithBs_; }
      set { disableAllCollisionsOfAsWithBs_ = value; }
    }

    #endregion

    #region editParams

    #region Forces
    [SerializeField]
    ForceMaxModeEnum forcemaxMode_ = ForceMaxModeEnum.Unlimited;
    public ForceMaxModeEnum ForceMaxMode
    {
      get { return forcemaxMode_; }
      set { forcemaxMode_ = value; }
    }

    [SerializeField]
    float forceMax_ = 150000.0f;
    public float ForceMax
    {
      get {  return forceMax_; }
      set { forceMax_ = value; }
    }

    [SerializeField]
    float forceMaxRand_ = 0.0f;                 //!< Random forces in [0, forceMaxRand_] will be added to forceMax_
    public float  ForceMaxRand
    {
      get {  return forceMaxRand_; }
      set { forceMaxRand_ = value; }
    }

    [SerializeField]
    float forceRange_ = 0.1f; 
    public float  ForceRange
    {
      get {  return forceRange_; }
      set { forceRange_ = value; }
    }

    [SerializeField]
    AnimationCurve forceProfile_ = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    public AnimationCurve ForceProfile
    {
      get {  return forceProfile_; }
      set { forceProfile_ = value; }
    }

    #endregion

    #region Collisions
    [SerializeField]
    bool enableCollisionIfBreak_ = true;
    public bool EnableCollisionIfBreak
    {
      get { return enableCollisionIfBreak_; }
      set { enableCollisionIfBreak_ = value; }
    }
    #endregion

    #region Break
    [SerializeField]
    bool breakIfForceMax_ = false;
    public bool BreakIfForceMax
    {
      get { return breakIfForceMax_; }
      set { breakIfForceMax_ = value; }
    }

    [SerializeField]
    bool breakAllIfLeftFewUnbroken_ = false;
    public bool BreakAllIfLeftFewUnbroken
    {
      get { return breakAllIfLeftFewUnbroken_; }
      set { breakAllIfLeftFewUnbroken_ = value; }
    }

    [SerializeField]
    int unbrokenNumberForBreakAll_ = 2;
    public int UnbrokenNumberForBreakAll
    {
      get { return unbrokenNumberForBreakAll_; }
      set { unbrokenNumberForBreakAll_ = value; }
    }

    [SerializeField]
    bool breakIfDistExcedeed_ = false;
    public bool BreakIfDistExcedeed
    {
      get { return breakIfDistExcedeed_; }
      set { breakIfDistExcedeed_ = value; }
    }

    [SerializeField]
    float distanceForBreak_ = 0.01f;
    public float DistanceForBreak
    {
      get { return distanceForBreak_; }
      set { distanceForBreak_ = value; }
    }

    [SerializeField]
    float distanceForBreakRand_ = 0.0f;         //!< Random distances in [0, distanceForBreakRand_] will be added to distanceForBreak_
    public float DistanceForBreakRand
    {
      get { return distanceForBreakRand_; }
      set { distanceForBreakRand_ = value; }
    }
    #endregion

    [SerializeField]
    bool breakIfHinge_ = false;
    public bool BreakIfHinge
    {
      get { return breakIfHinge_; }
      set { breakIfHinge_ = value; }
    }

    [SerializeField]
    bool plasticity_ = false;
    public bool Plasticity
    {
      get { return plasticity_; }
      set { plasticity_ = value; }
    }

    [SerializeField]
    float distanceForPlasticity_ = 0.005f;
    public float DistanceForPlasticity
    {
      get { return distanceForPlasticity_; }
      set { distanceForPlasticity_ = value; }
    }
    
    [SerializeField]
    float distanceForPlasticityRand_ = 0.005f;    //!< Random distances in [0, distanceForBreakRand_] will be added to distanceForBreak_
    public float DistanceForPlasticityRand
    {
      get { return distanceForPlasticityRand_; }
      set { distanceForPlasticityRand_ = value; }
    }

    [SerializeField]
    float plasticityRateAcquired_ = 0.05f;
    public float PlasticityRateAcquired
    {
      get { return plasticityRateAcquired_; }
      set { plasticityRateAcquired_ = value; }
    }

    [SerializeField]
    bool forcesFoldout_;
    public bool ForcesFoldout
    {
      get { return forcesFoldout_; }
      set { forcesFoldout_ = value; }
    }

    [SerializeField]
    bool collisionsFoldout_;
    public bool CollisionFoldout
    {
      get { return collisionsFoldout_; }
      set { collisionsFoldout_ = value; }
    }

    [SerializeField]
    bool breakFoldout_;
    public bool BreakFoldout
    {
      get { return breakFoldout_; }
      set { breakFoldout_ = value; }
    }

    [SerializeField]
    bool plasticityFoldout_;
    public bool PlasticityFoldout
    {
      get { return plasticityFoldout_; }
      set { plasticityFoldout_ = value; }
    }

    #endregion

    public bool IsCreateModeAtLocators
    {
      get
      {
        return  CreationMode == CreationModeEnum.AtLocatorsPositions ||
                CreationMode == CreationModeEnum.AtLocatorsBBoxCenters ||
                CreationMode == CreationModeEnum.AtLocatorsVertexes;
      }
    }

    public override CommandNode DeepClone(GameObject dataHolder)
    {
      CNJointGroups clone = CRTreeNode.CreateInstance<CNJointGroups>(dataHolder);
      
      clone.objectsA_  = ObjectsA.DeepClone();
      clone.objectsB_  = ObjectsB.DeepClone();
      clone.locatorsC_ = LocatorsC.DeepClone();

      clone.Name = Name;
      clone.needsUpdate_ = needsUpdate_;

      clone.creationMode_               = creationMode_;
      clone.isRigidGlue_                = isRigidGlue_;
      clone.contactDistanceSearch_      = contactDistanceSearch_;
      clone.contactAreaMin_             = contactAreaMin_;
      clone.contactAngleMaxInDegrees_   = contactAngleMaxInDegrees_;
      clone.contactNumberMax_           = contactNumberMax_;

      clone.matchingDistanceSearch_     = matchingDistanceSearch_;
      clone.limitNumberOfActiveJoints_  = limitNumberOfActiveJoints_;
      clone.activeJointsMaxInABPair_    = activeJointsMaxInABPair_;

      clone.disableCollisionsByPairs_       = disableCollisionsByPairs_;
      clone.disableAllCollisionsOfAsWithBs_ = disableAllCollisionsOfAsWithBs_;

      clone.forcemaxMode_                = forcemaxMode_;
      clone.forceMax_                    = forceMax_;
      clone.forceMaxRand_                = forceMaxRand_;
      clone.forceProfile_                = new AnimationCurve();

      int nKeys = forceProfile_.length;
      Keyframe[] arrKeys = forceProfile_.keys;

      Keyframe[] arrClonedKey = new Keyframe[ nKeys ];

      for (int i = 0; i < nKeys; i++)
      {
        arrClonedKey[i] = arrKeys[i];
      }
      clone.forceProfile_.keys = arrClonedKey;
      
      clone.enableCollisionIfBreak_    = enableCollisionIfBreak_;
      clone.breakIfForceMax_           = breakIfForceMax_;
      clone.breakAllIfLeftFewUnbroken_ = breakAllIfLeftFewUnbroken_;
      clone.unbrokenNumberForBreakAll_ = unbrokenNumberForBreakAll_;
      clone.breakIfDistExcedeed_       = breakIfDistExcedeed_;
      clone.distanceForBreak_          = distanceForBreak_;
      clone.distanceForBreakRand_      = distanceForBreakRand_;
      clone.breakIfHinge_              = breakIfHinge_;

      clone.plasticity_                = plasticity_;
      clone.distanceForPlasticity_     = distanceForPlasticity_;
      clone.plasticityRateAcquired_    = plasticityRateAcquired_;
      
      return clone;
    }

    public override bool UpdateNodeReferences(Dictionary<CommandNode, CommandNode> dictNodeToClonedNode)
    {
      bool updatedA = objectsA_.UpdateNodeReferences(dictNodeToClonedNode);
      bool updatedB = objectsB_.UpdateNodeReferences(dictNodeToClonedNode);
      bool updatedC = locatorsC_.UpdateNodeReferences(dictNodeToClonedNode);

      return ( updatedA || updatedB || updatedC );
    }

  }
}