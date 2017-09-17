using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CaronteFX
{
  public class CNAnimatedbody : CNRigidbody
  { 
    [SerializeField]
    bool overrideAnimationController_ = true;
    public bool OverrideAnimationController
    {
      get { return overrideAnimationController_;  }
      set { overrideAnimationController_ = value; }
    }

    [SerializeField]
    AnimationClip un_animationClip_;
    public AnimationClip UN_AnimationClip
    {
      get { return un_animationClip_; }
      set { un_animationClip_ = value; }
    }

    [SerializeField]
    float timeStart_ = 0.0f;
    public float TimeStart
    {
      get { return timeStart_; }
      set { timeStart_ = value; }
    }

    [SerializeField]
    float timeLength_ = float.MaxValue;
    public float TimeLength
    {
      get { return timeLength_; }
      set { timeLength_ = value; }
    }

    public override CommandNode DeepClone(GameObject dataHolder)
    {
      CNAnimatedbody clone = CNAnimatedbody.CreateInstance<CNAnimatedbody>(dataHolder);

      clone.field_ = field_.DeepClone();

      clone.Name                    = Name;
      clone.isFiniteMass_           = isFiniteMass_;

      clone.mass_                   = mass_;
      clone.density_                = density_;
      clone.restitution_in01_       = restitution_in01_;
      clone.frictionKinetic_in01_   = frictionKinetic_in01_;
      clone.frictionStatic_in01_    = frictionStatic_in01_;
      clone.gravity_                = gravity_;
      clone.dampingPerSecond_WORLD_ = dampingPerSecond_WORLD_;

      clone.velocityStart_          = velocityStart_;
      clone.omegaStart_inRadSeg_    = omegaStart_inRadSeg_;

      clone.un_animationClip_       = un_animationClip_;
      clone.timeStart_              = timeStart_;
      clone.timeLength_             = timeLength_;

      return clone;
    }
  }
}