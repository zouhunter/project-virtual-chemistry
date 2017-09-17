using UnityEngine;
using System.Collections;


namespace CaronteFX
{
  public class CNRope : CNBody
  {
    [SerializeField]
    private int sides_ = 6;
    public int Sides
    {
      get { return sides_; }
      set { sides_ = value; }
    }

    [SerializeField]
    private float stretch_ = 500f;
    public float Stretch
    {
      get { return stretch_;} 
      set { stretch_ = value; }
    }

    [SerializeField]
    private float bend_ = 0.1f;
    public float Bend
    {
      get { return bend_;} 
      set { bend_ = value; }
    }

    [SerializeField]
    private float torsion_ = 0.1f;
    public float Torsion
    {
      get { return torsion_;} 
      set { torsion_ = value; }
    }

    [SerializeField]
    private bool autoCollide_ = true;
    public bool AutoCollide
    {
      get { return autoCollide_; }
      set { autoCollide_ = value; }
    }
   
    [SerializeField]   
    private float dampingPerSecond_CM_ = 1000f;
    public float DampingPerSecond_CM
    {
      get { return dampingPerSecond_CM_; }
      set { dampingPerSecond_CM_ = value; }
    }

    public override CommandNode DeepClone(GameObject dataHolder)
    {
      CNRope clone = CommandNode.CreateInstance<CNRope>(dataHolder);
      
      clone.field_ = field_.DeepClone();

      clone.Name        = Name;
      
      clone.mass_       = mass_;
      clone.density_    = density_;

      clone.sides_      = sides_;

      clone.stretch_ = stretch_;
      clone.bend_    = bend_;
      clone.torsion_ = torsion_;

      clone.dampingPerSecond_CM_  = dampingPerSecond_CM_;

      clone.restitution_in01_     = restitution_in01_;
      clone.frictionKinetic_in01_ = frictionKinetic_in01_;
      clone.frictionStatic_in01_  = frictionStatic_in01_;

      clone.gravity_ = gravity_;

      clone.dampingPerSecond_WORLD_ = dampingPerSecond_WORLD_;

      clone.velocityStart_       = velocityStart_;
      clone.omegaStart_inRadSeg_ = omegaStart_inRadSeg_;

      return clone;
    }

  } // CNRope...

} //namespace Caronte...
