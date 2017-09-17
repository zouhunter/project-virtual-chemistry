using UnityEngine;
using System;
using System.Collections.Generic;


namespace CaronteFX
{
 
  public abstract class CRTreeNode : MonoBehaviour
  {

    [SerializeField]
    private CRTreeNode parent_;
    public  CRTreeNode Parent
    {
      get 
      {
        return parent_;
      }
      set
      {
        if (value == parent_)
        {
          return;
        }
        if (parent_ != null)
        {
          parent_.Children.Remove(this);
        }
        if (value != null && !value.Children.Contains(this) )
        {
          value.Children.Add(this);
        }
        parent_ = value;
      }
    }

    [SerializeField]
    private CRTreeNodeList children_;
    public CRTreeNodeList Children
    {
      get
      {
        if (children_ == null)
        {
          children_ = new CRTreeNodeList(this);
        }
        return children_;
      }
      set 
      { 
        children_ = value; 
      }
    }

    public int ChildCount
    {
      get { return Children.Count; }
    }

    public CRTreeNode Root
    {
      get
      {
        CRTreeNode node = this;
        while(node.Parent != null)
        {
          node = node.Parent;
        }
        return node;
      }
    }

    public int Depth
    {
      get
      {
        int depth = 0;
        CRTreeNode node = this;
        while (node.Parent != null)
        {
          node = node.Parent;
          depth++;
        }
        return depth;
      }
    }

    public static T CreateInstance<T>(GameObject dataHolder)
      where T: CRTreeNode
    {
      T instance = dataHolder.AddComponent<T>();
      instance.Parent = null;
      instance.Children = new CRTreeNodeList(instance);
      return instance;
    }

    public static T CreateInstance<T>(GameObject dataHolder, CRTreeNode parent)
      where T: CRTreeNode
    {
      T instance = dataHolder.AddComponent<T>();
      instance.Parent = parent;
      instance.Children = new CRTreeNodeList(instance);
      return instance;
    }

    public bool isAncestorOf(CRTreeNode node)
    {
      if (this == node.Parent )
      {
        return true;
      }

      while (node.Parent != null)
      {
        return isAncestorOf(node.Parent);
      }

      return false;
    }

    public delegate void CRTreeTraversalDelegate( CRTreeNode node );

    public void Traversal(CRTreeTraversalDelegate treeNodeTraversal)
    {
      treeNodeTraversal(this);

      for (int i = 0; i < ChildCount; ++i)
      {
        CRTreeNode childNode = Children[i];
        if (childNode != null)
        {
          childNode.Traversal(treeNodeTraversal);
        }
      }
    }

    public void GetHierarchyPlainList(List<CRTreeNode> listCommandNode)
    {
      Traversal(listCommandNode.Add);
    }
  }
}