using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace CaronteFX
{
  public class CNWelder : CNMonoField
  {
    public override CNField Field
    {
      get
      {
        if (field_ == null)
        {
          field_ = new CNField(false, false);
        }
        return field_;
      }
    }

    [SerializeField]
    GameObject weldGameObject_;
    public UnityEngine.GameObject WeldGameObject
    {
      get { return weldGameObject_; }
      set { weldGameObject_ = value; }
    }


    public override CommandNode DeepClone(GameObject dataHolder)
    {
      CNWelder clone = CommandNode.CreateInstance<CNWelder>(dataHolder);

      clone.field_ = Field.DeepClone();
      clone.Name = Name;
      
      return clone;
    }

  } //namespace CNWelder
}