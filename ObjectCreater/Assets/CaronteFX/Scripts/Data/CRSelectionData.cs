using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace CaronteFX
{
  /// <summary>
  /// Selection Data class
  /// </summary>
  [Serializable]
  public class CRSelectionData
  { 
    [SerializeField]
    private CommandNode focusedNode_;
    public CommandNode FocusedNode
    {
      get { return focusedNode_; }
      set { focusedNode_ = value; }
    }

    [SerializeField]
    private CommandNode lastSelectedNode_;
    public CommandNode LastSelectedNode
    {
      get { return lastSelectedNode_; }
      set { lastSelectedNode_ = value; }
    }

    [SerializeField]
    private List<CommandNode> listSelectedNode_ = new List<CommandNode>();
    public List<CommandNode> ListSelectedNode
    {
      get { return listSelectedNode_; }
      set { listSelectedNode_ = value; }
    }

    public int SelectedNodeCount
    {
      get { return listSelectedNode_.Count; }
    }

    [SerializeField]
    private List<CommandNode> listSelectedNodeAndChildren_ = new List<CommandNode>();
    public List<CommandNode> ListSelectedNodeAndChildren
    {
      get { return listSelectedNodeAndChildren_; }
      set { listSelectedNodeAndChildren_ = value;}
    }
  }

}
