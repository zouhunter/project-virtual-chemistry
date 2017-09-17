using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace CaronteFX
{
  public class CNMissing : CommandNode
  {
    public override string ListName 
    {
      get
      {
        return "[n] (missing node)"; 
      }
    }
    
    [SerializeField]
    private CommandNode parentNodeOfMissing_;
    public CommandNode ParentNodeOfMissing
    {
      get
      {
        return parentNodeOfMissing_;
      }
      set
      {
        parentNodeOfMissing_ = value;
      }
    }

    [SerializeField]
    private int childIdxOfParent_;
    public int ChildIdxOfParent
    {
      get
      {
        return childIdxOfParent_;
      }
      set
      {
        childIdxOfParent_ = value;
      }
    }

    public override CommandNode DeepClone( GameObject go )
    {
      throw new NotImplementedException();
    }

    public override bool UpdateNodeReferences(Dictionary<CommandNode, CommandNode> dictNodeToClonedNode)
    {
      throw new NotImplementedException();
    }
  }
}