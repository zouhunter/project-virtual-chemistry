using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CaronteFX
{
  public class CNWind : CNEntity
  {
    [SerializeField]
    private float fluidDensity_ = 1.2f;
    public float FluidDensity
    {
      get { return fluidDensity_; }
      set { fluidDensity_ = Mathf.Clamp(value, 0f, float.MaxValue); }
    }

    [SerializeField]
    private Vector3 velocity_ = new Vector3(10f, 0f, 0f);
    public Vector3 Velocity
    {
      get { return velocity_; }
      set { velocity_ = value; }
    }

    [SerializeField]
    private float speedDeltaMax_ = 5f;
    public float SpeedDeltaMax
    {
      get { return speedDeltaMax_; }
      set { speedDeltaMax_ = Mathf.Clamp(value, 0f, float.MaxValue); }
    }

    [SerializeField]
    private float angleDeltaMax_ = 45f;
    public float AngleDeltaMax
    {
      get { return angleDeltaMax_; }
      set { angleDeltaMax_ = Mathf.Clamp(value, 0f, 360f); }
    }

    [SerializeField]
    private float periodTime_ = 1.0f;
    public float PeriodTime
    {
      get { return periodTime_; }
      set { periodTime_ = Mathf.Clamp(value, 0f, float.MaxValue); }
    }

    [SerializeField]
    private float periodSpace_ = 1.0f;
    public float PeriodSpace
    {
      get { return periodSpace_; }
      set { periodSpace_ = Mathf.Clamp(value, 0f, float.MaxValue) ; }
    }

    [SerializeField]
    private float highFrequency_am_ = 0.1f;
    public float HighFrequency_am
    {
      get { return highFrequency_am_; }
      set { highFrequency_am_ = Mathf.Clamp(value, 0f, float.MaxValue); }
    }

    [SerializeField]
    private float highFrequency_sp_ = 10f;
    public float HighFrequency_sp
    {
      get { return highFrequency_sp_;  }
      set { highFrequency_sp_ = Mathf.Clamp(value, 0f, float.MaxValue); }
    }

    [SerializeField]
    private bool noiseFoldout_ = false;
    public bool NoiseFoldout
    {
      get { return noiseFoldout_; }
      set { noiseFoldout_ = value; }
    }

    [SerializeField]
    private bool hfFoldout_ = false;
    public bool HfFoldout
    {
      get { return hfFoldout_; }
      set { hfFoldout_ = value; }
    }

    public override CommandNode DeepClone(GameObject dataHolder)
    {
      CNWind clone = CNWind.CreateInstance<CNWind>(dataHolder);

      clone.field_ = Field.DeepClone();

      clone.Name          = Name;
      clone.fluidDensity_ = fluidDensity_;
      clone.velocity_     = velocity_;

      clone.speedDeltaMax_ = speedDeltaMax_;
      clone.angleDeltaMax_ = angleDeltaMax_;

      clone.periodTime_       = periodTime_;
      clone.periodSpace_      = periodSpace_;

      clone.highFrequency_am_ = highFrequency_am_;
      clone.highFrequency_sp_ = highFrequency_sp_;

      return clone;
    }

  }
}
