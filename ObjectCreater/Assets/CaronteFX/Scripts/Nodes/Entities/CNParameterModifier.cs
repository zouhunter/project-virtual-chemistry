using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CaronteFX
{
  public enum PARAMETER_MODIFIER_PROPERTY
  {
    VELOCITY_LINEAL  = 0,
    VELOCITY_ANGULAR = 1,
    ACTIVITY         = 2,
    PLASTICITY       = 3,
    FREEZE           = 4,
    VISIBILITY       = 5,
    FORCE_MULTIPLIER = 6,
    UNKNOWN
  };

  [Serializable]
  public class ParameterModifierCommand : IDeepClonable<ParameterModifierCommand>
  {
    public PARAMETER_MODIFIER_PROPERTY target_;
    public Vector3 valueVector3_;
    public int     valueInt_;

    public ParameterModifierCommand()
    {
      target_       = PARAMETER_MODIFIER_PROPERTY.ACTIVITY;
      valueVector3_ = new Vector3( 0.0f, 0.0f, 0.0f );
      valueInt_     = 0;
    }

    public ParameterModifierCommand DeepClone()
    {
      ParameterModifierCommand clone = new ParameterModifierCommand();

      clone.target_       = target_;
      clone.valueVector3_ = valueVector3_;
      clone.valueInt_     = valueInt_;
      
      return clone;
    }
  }

  public class CNParameterModifier : CNEntity
  {   
 
    public override CNField Field
    {
      get
      {
        if (field_ == null)
        {
          CNField.AllowedTypes allowedTypes =   CNField.AllowedTypes.Bodies
                                              | CNField.AllowedTypes.JointServosNode
                                              | CNField.AllowedTypes.DaemonNode
                                              | CNField.AllowedTypes.TriggerNode;

          field_ = new CNField(false, allowedTypes, false);
        }
        return field_;
      }
    }

    [SerializeField]
    private List<ParameterModifierCommand> listPmCommand_ = new List<ParameterModifierCommand>();
    public List<ParameterModifierCommand> ListPmCommand
    {
      get { return listPmCommand_; }
    }

    public override CommandNode DeepClone(GameObject dataHolder)
    {
      CNParameterModifier clone = CommandNode.CreateInstance<CNParameterModifier>(dataHolder);
      
      clone.field_ = field_.DeepClone();

      clone.Name   = Name;
      clone.timer_ = Timer; 
      clone.listPmCommand_ = new List<ParameterModifierCommand>();

      foreach( ParameterModifierCommand pmCommand in listPmCommand_ )
      {
        clone.listPmCommand_.Add( pmCommand.DeepClone() );
      }
    
      return clone;
    }

  }
}
