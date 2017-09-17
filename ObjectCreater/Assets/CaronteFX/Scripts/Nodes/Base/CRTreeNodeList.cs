using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace CaronteFX
{
  [Serializable]
  public class CRTreeNodeList : IEnumerable
  {
    [SerializeField]
    private CRTreeNode Parent;

    [SerializeField]
    private List<CRTreeNode>  listTreeNode_;
 
    public CRTreeNodeList(CRTreeNode parent)
    {
      this.Parent = parent;
      listTreeNode_ = new List<CRTreeNode>();
    }
 
    public void Add(CRTreeNode node)
    {
      listTreeNode_.Add(node);
      node.Parent = Parent;
    }

    public void AddAfter(CRTreeNode node, CRTreeNode previousNode)
    {
      int indexOfPrevious = listTreeNode_.IndexOf( previousNode );
      if (indexOfPrevious == -1)
      {
        listTreeNode_.Insert(0, node);
      }
      else
      {
        listTreeNode_.Insert(indexOfPrevious+1, node);
      }   
      node.Parent = Parent;
    }

    public bool Remove(CRTreeNode node)
    {
      return (listTreeNode_.Remove(node));
    }

    public bool Contains(CRTreeNode node)
    {
      return (listTreeNode_.Contains(node));
    }

    public void RemoveAt(int childIdx)
    {
      listTreeNode_.RemoveAt(childIdx);
    }

    public int Count
    {
      get
      {
        return listTreeNode_.Count;
      }
    }

    public CRTreeNode this[int i]
    {
      get { return listTreeNode_[i]; }
      set { listTreeNode_[i] = value; }
    }

    public override string ToString()
    {
        return "Count=" + listTreeNode_.Count.ToString();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
       return (IEnumerator) GetEnumerator();
    }

    public CRTreeNodeEnum GetEnumerator()
    {
        return new CRTreeNodeEnum(listTreeNode_);
    }

  }

  public class CRTreeNodeEnum : IEnumerator
  {
    public List<CRTreeNode> listTreeNode_;
    int position = -1;

    public CRTreeNodeEnum(List<CRTreeNode> list)
    {
      listTreeNode_ = list;
    }

    public bool MoveNext()
    {
      position++;
      return (position < listTreeNode_.Count);
    }

    public void Reset()
    {
      position = -1;
    }

    object IEnumerator.Current
    {
      get
      {
        return Current;
      }
    }

    public CRTreeNode Current
    {
      get
      {
        try
        {
          return listTreeNode_[position];
        }
        catch (IndexOutOfRangeException)
        {
          throw new InvalidOperationException();
        }
      }
    }
  }

}

