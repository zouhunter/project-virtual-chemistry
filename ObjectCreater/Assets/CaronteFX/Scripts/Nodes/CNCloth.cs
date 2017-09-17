using System.Collections;
using UnityEngine;

namespace CaronteFX
{
  public class CNCloth : CNBody
  {
    [SerializeField]
    bool cloth_autoCollide_ = true;
    public bool Cloth_AutoCollide
    {
      get { return cloth_autoCollide_; }
      set { cloth_autoCollide_ = value; }
    }


    [SerializeField]
    bool disableCollisionNearJoints_ = true;
    public bool DisableCollisionNearJoints
    {
      get { return disableCollisionNearJoints_; }
      set { disableCollisionNearJoints_ = value; }
    }

    [SerializeField]  
    float cloth_collisionRadius_ = 0.05f; // 5 cm   
    public float Cloth_CollisionRadius
    {
      get { return cloth_collisionRadius_; }
      set { cloth_collisionRadius_ = value; }
    }

    [SerializeField]   
    float cloth_bend_ = 3.0f;       // 0.07 Newtons to bend 0.01 m a square fabric 1x1m
    public float Cloth_Bend
    {
      get { return cloth_bend_; }
      set { cloth_bend_ = value; }
    }
   
    [SerializeField]        
    float cloth_stretch_ = 1000.0f; // 1000 Newtons to enlarge 0.01 m a square fabric 1x1m
    public float Cloth_Stretch
    {
      get { return cloth_stretch_; }
      set { cloth_stretch_ = value; }
    }
    
    [SerializeField]     
    float cloth_dampingBend_ = 10f;
    public float Cloth_DampingBend
    {
      get { return cloth_dampingBend_; }
      set { cloth_dampingBend_ = value; }
    }
    
    [SerializeField]  
    float cloth_dampingStretch_ = 10f;
    public float Cloth_DampingStretch
    {
      get { return cloth_dampingStretch_; }
      set { cloth_dampingStretch_ = value; }
    }
  
    public override CommandNode DeepClone(GameObject dataHolder)
    {
      CNCloth clone = CommandNode.CreateInstance<CNCloth>(dataHolder);
      
      clone.field_ = field_.DeepClone();

      clone.Name          = Name;
      
      clone.mass_         = mass_;
      clone.density_      = density_;

      clone.cloth_bend_            = cloth_bend_;             
      clone.cloth_stretch_         = cloth_stretch_;
      clone.cloth_dampingBend_     = cloth_dampingBend_;
      clone.cloth_dampingStretch_  = cloth_dampingStretch_;
      clone.cloth_collisionRadius_ = cloth_collisionRadius_; 

      clone.restitution_in01_       = restitution_in01_;
      clone.frictionKinetic_in01_   = frictionKinetic_in01_;
      clone.frictionStatic_in01_    = frictionStatic_in01_;

      clone.gravity_ = gravity_;

      clone.dampingPerSecond_WORLD_ = dampingPerSecond_WORLD_;

      clone.velocityStart_       = velocityStart_;
      clone.omegaStart_inRadSeg_ = omegaStart_inRadSeg_;

      return clone;
    }
  }

}
