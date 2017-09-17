using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaronteFX
{
  public abstract class CommandNode : CRTreeNode, IMonoDeepClonable<CommandNode>
  {
    [SerializeField]
    private string name_ = string.Empty;
    public string Name 
    { 
      get
      {
        return name_;
      }
      set
      {
        name_ = value;
      }
    }

    public virtual string ListName
    {
      get 
      {
        if ( needsUpdate_ )
        {
          return Name + "(*)";
        }
        else
        {
          return Name;
        }

      }
    }

    public CNGroup EffectRoot
    {
      get
      {
        CNGroup nodeGroup     = this as CNGroup;
        if (nodeGroup != null && nodeGroup.IsEffectRoot )
        {
          return nodeGroup;
        }

        CNGroup nodeParent = (CNGroup)this.Parent;
        while ( nodeParent != null && !nodeParent.IsEffectRoot )
        {
          nodeParent = (CNGroup)nodeParent.Parent;
        }
        return (CNGroup) nodeParent;
      }
    }

    public bool IsGroup
    {
      get
      {
        CNGroup nodeGroup = this as CNGroup;
        if (nodeGroup != null )
        {
          return true;
        }
        return false;
      }
    }

    public bool IsEffectRoot
    {
      get
      {
        CNGroup nodeGroup = this as CNGroup;
        if (nodeGroup != null && nodeGroup.IsEffectRoot)
        {
          return true;
        }
        return false;
      }
    }

    public bool IsSubeffectsFolder
    {
      get
      {
        CNGroup nodeGroup = this as CNGroup;
        if (nodeGroup != null && nodeGroup.IsSubeffectsFolder)
        {
          return true;
        }
        return false;
      }
    }

    public class DepthComparer : IComparer<CommandNode>  
    {

      public int Compare( CommandNode x, CommandNode y )  
      {
          return( y.Depth - x.Depth );
      }
    }

    [NonSerialized]
    protected bool needsUpdate_ = false;
    public bool NeedsUpdate
    {
      get { return needsUpdate_; }
      set { needsUpdate_ = value; }
    }

    [SerializeField]
    protected bool enabled_ = true;
    public bool IsNodeEnabled
    {
      get { return enabled_; }
      set { enabled_ = value; }
    }

    [SerializeField]
    protected bool visible_ = true;
    public bool IsNodeVisible
    {
      get { return visible_; }
      set { visible_ = value; }
    }

    [SerializeField]
    protected bool excluded_ = false;
    public bool IsNodeExcluded
    {
      get { return excluded_; }
      set { excluded_ = value; }
    }

    public bool IsNodeEnabledInHierarchy
    {
      get 
      { 
        bool parentEnabled = true;
        if ( Parent != null )
        {
          parentEnabled = ((CommandNode)Parent).IsNodeEnabledInHierarchy;
        }
        return enabled_ && parentEnabled; 
      }
    }

    public bool IsNodeVisibleInHierarchy
    {
      get 
      { 
        bool parentVisible = true;
        if ( Parent != null )
        {
          parentVisible = ((CommandNode)Parent).IsNodeVisibleInHierarchy;
        }
        return visible_ && parentVisible; 
      }
    }

    public bool IsNodeExcludedInHierarchy
    {
      get 
      { 
        bool parentExcluded = false;
        if ( Parent != null )
        {
          parentExcluded = ((CommandNode)Parent).IsNodeExcludedInHierarchy;
        }
        return excluded_ || parentExcluded; 
      }
    }

    public abstract CommandNode DeepClone(GameObject dataHolder);
    public abstract bool UpdateNodeReferences(Dictionary<CommandNode, CommandNode> dictNodeToClonedNode);
  }
}