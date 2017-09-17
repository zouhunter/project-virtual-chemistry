using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;

namespace CaronteFX
{
  public class CNNodesController : IListController 
  {
    CNFieldController fController_;
    CNField           field_;
    CNManager         manager_;
    CNHierarchy       hierarchy_;
    CommandNode       ownerNode_;

    List<CommandNode>  listCommandNodeAllowed_;
    List<CommandNode>  listCommandNodeCurrent_;

    List<CRTreeNode> listTreeNodeAux_;

    List<int> listSelectedIdx_;
    int lastSelectedIdx_;

    public bool AnyNodeSelected
    {
      get {  return (listSelectedIdx_.Count > 0); }
    }

    private int itemIdxEditing_ = -1;
    public int ItemIdxEditing
    {
      get { return itemIdxEditing_; }
      set { itemIdxEditing_ = value; }
    }

    private string itemIdxEditingName_ = string.Empty;
    public string ItemIdxEditingName
    {
      get { return itemIdxEditingName_; }
      set { itemIdxEditingName_ = value; }
    }
    
    bool blockEdition_ = false;
    public bool BlockEdition 
    {
      get { return blockEdition_; }
    }

    public int NumVisibleElements
    {
      get { return listCommandNodeCurrent_.Count; }
    }
    //-----------------------------------------------------------------------------------
    public CNNodesController( CNFieldController fController, CommandNodeEditor ownerEditor )
    {
      fController_ = fController;
      field_       = fController.Field;
      manager_     = CNManager.Instance;
      hierarchy_   = manager_.Hierarchy;
      ownerNode_   = ownerEditor.Data;
      
      listCommandNodeAllowed_ = new List<CommandNode>();
      listCommandNodeCurrent_ = new List<CommandNode>();

      listSelectedIdx_ = new List<int>();
      listTreeNodeAux_ = new List<CRTreeNode>();

      SetSelectableNodes();  
    }
    //-----------------------------------------------------------------------------------
    public void SetSelectableNodes()
    {
      List<CommandNode> listCommadNodeGUI;
      if (field_.ShowOwnerGroupOnly)
      {
        listCommadNodeGUI = hierarchy_.GetListNodeGUIAux(ownerNode_);
      }
      else
      {
        listCommadNodeGUI = hierarchy_.GetListNodeGUIAux(null);
      }
      
      listCommandNodeAllowed_.Clear();
      foreach( CommandNode cNode in listCommadNodeGUI )
      {
        if ( cNode != ownerNode_ )        
        {
          if ( ( (cNode is CNGroup) && HasAnyChildrenAllowed(cNode) ) ||
                ( CheckIfAllowedBody(cNode) ||  CheckIfAllowedMultiJoint(cNode) || 
                  CheckIfAllowedServos(cNode) || CheckIfAllowedEntity(cNode) ) )
          {
            listCommandNodeAllowed_.Add(cNode);
          }   
        }
      } 

      FilterAlreadySelectedNodes();
    }
    //-----------------------------------------------------------------------------------
    public bool HasAnyChildrenAllowed( CommandNode node )
    {
      listTreeNodeAux_.Clear();
      node.GetHierarchyPlainList(listTreeNodeAux_);

      foreach( CRTreeNode treeNode in listTreeNodeAux_)
      {
        CommandNode cNode = (CommandNode) treeNode;
        if ( CheckIfAllowedBody(cNode) ||  CheckIfAllowedMultiJoint(cNode) || 
             CheckIfAllowedServos(cNode) || CheckIfAllowedEntity(cNode) )
        {
          return true;
        }
      }
      return false;
    }
    //-----------------------------------------------------------------------------------
    private bool CheckIfAllowedBody(CommandNode cNode)
    {
      bool allowedBody      = ( field_.AllowedType & CNField.AllowedTypes.BodyNode)      == CNField.AllowedTypes.BodyNode;
      bool allowedRigidbody = ( field_.AllowedType & CNField.AllowedTypes.RigidBodyNode) == CNField.AllowedTypes.RigidBodyNode;

      if ( ( allowedBody && ( cNode is CNBody ) ) || 
           ( allowedRigidbody && ( cNode is CNRigidbody ) ) )
      {
        return true;
      }
      return false;
    }
    //-----------------------------------------------------------------------------------
    private bool CheckIfAllowedMultiJoint(CommandNode cNode)
    {
      bool allowedMultiJoint = (field_.AllowedType & CNField.AllowedTypes.MultiJointNode) == CNField.AllowedTypes.MultiJointNode;
      if (allowedMultiJoint && ( cNode is CNJointGroups ) )
      {
        return true;
      }
      return false;
    }
    //-----------------------------------------------------------------------------------
    private bool CheckIfAllowedServos(CommandNode cNode)
    {
      bool allowedServos = (field_.AllowedType & CNField.AllowedTypes.ServosNode) == CNField.AllowedTypes.ServosNode;
      if (allowedServos && ( cNode is CNServos) )
      {
        return true;
      }
      return false;
    }
    //-----------------------------------------------------------------------------------
    private bool CheckIfAllowedEntity(CommandNode cNode)
    {
      bool allowedGravity           = ( field_.AllowedType & CNField.AllowedTypes.GravityNode ) == CNField.AllowedTypes.GravityNode;
      bool allowedExplosion         = ( field_.AllowedType & CNField.AllowedTypes.ExplosionNode ) == CNField.AllowedTypes.ExplosionNode;
      bool allowedTrigger           = ( field_.AllowedType & CNField.AllowedTypes.TriggerNode ) == CNField.AllowedTypes.TriggerNode;
      bool allowedParameterModifier = ( field_.AllowedType & CNField.AllowedTypes.ParameterModifierNode ) == CNField.AllowedTypes.ParameterModifierNode;
      bool allowedSubstituter       = ( field_.AllowedType & CNField.AllowedTypes.SubstituterNode ) == CNField.AllowedTypes.SubstituterNode;
      bool allowedAimedForce        = ( field_.AllowedType & CNField.AllowedTypes.AimedForceNode ) == CNField.AllowedTypes.AimedForceNode;
      bool allowedWind              = ( field_.AllowedType & CNField.AllowedTypes.WindNode ) == CNField.AllowedTypes.WindNode;
      bool allowedSpeedLimiter      = ( field_.AllowedType & CNField.AllowedTypes.SpeedLimiter ) == CNField.AllowedTypes.SpeedLimiter;
      bool allowedJet               = ( field_.AllowedType & CNField.AllowedTypes.Jet ) == CNField.AllowedTypes.Jet;
      
      if (   ( allowedGravity   && cNode is CNGravity ) 
          || ( allowedExplosion && cNode is CNExplosion )
          || ( allowedTrigger   && cNode is CNTrigger )
          || ( allowedParameterModifier && cNode is CNParameterModifier)
          || ( allowedSubstituter && cNode is CNSubstituter ) 
          || ( allowedAimedForce  && cNode is CNAimedForce ) 
          || ( allowedWind && cNode is CNWind )
          || ( allowedSpeedLimiter && cNode is CNSpeedLimiter )
          || ( allowedJet && cNode is CNJet )
         )
      {
        return true;
      }
      return false;
    }
    //-----------------------------------------------------------------------------------
    public void FilterAlreadySelectedNodes()
    {
      listCommandNodeCurrent_.Clear();
      listCommandNodeCurrent_.AddRange( listCommandNodeAllowed_ );
      
      foreach(CommandNode cNode in listCommandNodeAllowed_)
      {
        if ( field_.ContainsNode( cNode )  )
        {
          FilterChildrenAndMergeFieldWithParents(cNode);
        }
      }

      CRManagerEditor instance = CRManagerEditor.Instance;
      instance.RepaintSubscribers();
    }
    //-----------------------------------------------------------------------------------
    private void FilterChildrenAndMergeFieldWithParents(CommandNode cNode)
    {
      listTreeNodeAux_.Clear();
      cNode.GetHierarchyPlainList( listTreeNodeAux_ );
      foreach( CRTreeNode treeNode in listTreeNodeAux_ )
      {
        CommandNode myCurrentNode = (CommandNode) treeNode;
   
        if ( listCommandNodeCurrent_.Contains( myCurrentNode )  )
        {
          listCommandNodeCurrent_.Remove( myCurrentNode );
        }

        if ( myCurrentNode != cNode && field_.ContainsNode( myCurrentNode )  )
        {
          field_.RemoveNode( myCurrentNode );
        }
      }
    }
    //-----------------------------------------------------------------------------------
    public void AddSelectedNodes()
    {
      Undo.RecordObject(ownerNode_, "CaronteFX - Add selectedNodes");
      foreach(int nodeIdx in listSelectedIdx_)
      {
        field_.AddNode( listCommandNodeCurrent_[nodeIdx] );
      }
      listSelectedIdx_.Clear();

      SetSelectableNodes();

      if (fController_.WantsUpdate != null)
      {
        fController_.WantsUpdate();
      }
    }
    //-----------------------------------------------------------------------------------
    public void UnselectSelected()
    {
      listSelectedIdx_.Clear();

      CRManagerEditor window = CRManagerEditor.Instance;
      window.RepaintSubscribers();
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsNull(int itemIdx)
    {
      CommandNode node = listCommandNodeCurrent_[itemIdx];
      return (node == null);
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsBold( int itemIdx )
    {
      return false;
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsSelectable( int itemIdx )
    {
      return true;
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsSelected( int itemIdx )
    {
      if ( listSelectedIdx_.Contains(itemIdx) )
      {
        return true;
      }
      return false;
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsGroup( int itemIdx )
    {
      CommandNode cNode = listCommandNodeCurrent_[itemIdx];
      return ( cNode is CNGroup );
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsExpanded( int itemIdx )
    {
      CommandNode node = listCommandNodeCurrent_[itemIdx];
      CNGroup groupNode = (CNGroup)node;
      
      if (itemIdx == 0)
      { 
        groupNode.IsOpenAux = true;
        SetSelectableNodes();
      }

      return groupNode.IsOpenAux;
    }
    //-----------------------------------------------------------------------------------
    public void ItemSetExpanded( int itemIdx, bool open )
    {
      CommandNode node = listCommandNodeCurrent_[itemIdx];
      CNGroup groupNode = (CNGroup)node;

      if (itemIdx == 0)
      {
        groupNode.IsOpenAux = true;
      }
      else
      {
        if (open != groupNode.IsOpenAux)
        {
          groupNode.IsOpenAux = open;
          SetSelectableNodes();
        }
      }
    }
    //-----------------------------------------------------------------------------------
    public int ItemIndentLevel( int itemIdx )
    {
      CommandNode cNode = listCommandNodeCurrent_[itemIdx];
      int depth = cNode.Depth;
      if (field_.ShowOwnerGroupOnly)
      {
        depth -= (ownerNode_.Depth - 1);
      }
      return depth;
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsDraggable( int itemIdx )
    {
      return false;
    }  
    //-----------------------------------------------------------------------------------
    public bool ItemIsValidDragTarget( int itemIdx, string dragDropIdentifier )
    { 
      return false;
    }
    //-----------------------------------------------------------------------------------
    public bool ItemHasContext( int itemIdx )
    {
      return true;
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsEditable(int itemIdx )
    {
      return false;
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsDisabled( int itemIdx )
    {
      CommandNode node = listCommandNodeCurrent_[itemIdx];
      return (!node.IsNodeEnabledInHierarchy);
    }
    //-----------------------------------------------------------------------------------
    public bool ItemIsExcluded( int itemIdx )
    {
      CommandNode node = listCommandNodeCurrent_[itemIdx];
      return (node.IsNodeExcludedInHierarchy);
    }
    //-----------------------------------------------------------------------------------
    public bool ListIsValidDragTarget()
    {
      return false;
    }
    //-----------------------------------------------------------------------------------
    public void MoveDraggedItem(int itemfromIdx, int itemToIdx, bool edgeDrag)
    {

    }
    //-----------------------------------------------------------------------------------
    public void AddDraggedObjects(int itemToIdx, UnityEngine.Object[] objects )
    {
    }
    //-----------------------------------------------------------------------------------
    public void RemoveSelected()
    {
    }
    //-----------------------------------------------------------------------------------
    public void GUIUpdateRequested()
    {

    }
    //-----------------------------------------------------------------------------------
    public string GetItemName( int itemIdx )
    {
      return listCommandNodeCurrent_[itemIdx].Name;
    }
    //-----------------------------------------------------------------------------------
    public string GetItemListName( int itemIdx )
    {
      return listCommandNodeCurrent_[itemIdx].ListName;
    }
    //-----------------------------------------------------------------------------------
    public Texture GetItemIcon(int nodeIdx)
    {
      CommandNode node = listCommandNodeCurrent_[nodeIdx];
      return hierarchy_.GetNodeIcon( node );
    }
    //-----------------------------------------------------------------------------------
    public void SceneSelection()
    {
    }
    //-----------------------------------------------------------------------------------
    public void SetItemName( int itemIdx, string name )
    {
    }
    //-----------------------------------------------------------------------------------
    public void OnClickItem( int itemIdx, bool ctrlPressed, bool shiftPressed, bool altPressed, bool isUpClick )
    {
      int prevIdx  = lastSelectedIdx_;
      lastSelectedIdx_ = itemIdx;

      if (!isUpClick)
      {
        if (shiftPressed)
        {
          if (prevIdx != -1)
          {
            int range = itemIdx - prevIdx;
            int increment = (range > 0) ? 1 : -1;

            while (prevIdx != itemIdx)
            {
              int currentIdx = prevIdx + increment;

              if (listSelectedIdx_.Contains(currentIdx))
              {
                listSelectedIdx_.Remove(currentIdx);
              }
              else
              {
                listSelectedIdx_.Add(currentIdx);
              }
              prevIdx += increment;
            }
            SceneSelection();
          }
        }   
        else if(ctrlPressed)
        {
          if(listSelectedIdx_.Contains(itemIdx))
          {
            listSelectedIdx_.Remove(itemIdx);
          }
          else
          {
            listSelectedIdx_.Add(itemIdx);
          }
        }
        else if ( !listSelectedIdx_.Contains(itemIdx)  )
        {
          listSelectedIdx_.Clear();
          listSelectedIdx_.Add(itemIdx);
          SceneSelection();
        }
        else
        {
          return;
        }
      }
      else
      {
        if (!ctrlPressed && !shiftPressed)
        {
          listSelectedIdx_.Clear();
          listSelectedIdx_.Add(itemIdx);
          lastSelectedIdx_ = itemIdx;
          SceneSelection();
        }
      }
      CRManagerEditor instance = CRManagerEditor.Instance;
      instance.RepaintSubscribers();
    }
    //-----------------------------------------------------------------------------------
    public void OnDoubleClickItem( int itemIdx )
    {
      listSelectedIdx_.Clear();
      listSelectedIdx_.Add(itemIdx);
      AddSelectedNodes();
    }
    //----------------------------------------------------------------------------------
    public void OnContextClickItem(int itemIdx, bool ctrlPressed, bool shiftPressed, bool altPressed )
    {

    }
    //----------------------------------------------------------------------------------
    public void OnContextClickList()
    {

    }
    //-----------------------------------------------------------------------------------
    public void BuildListItems()
    {

    }
  }
}

