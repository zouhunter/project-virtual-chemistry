using UnityEngine;
using System.Collections;

namespace CaronteFX
{
  public class CNSelector : CNMonoField
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
    
    public enum SELECTION_MODE
    {
      INSIDE,
      OUTSIDE
    };

    [SerializeField]
    GameObject selectorGO_;
    public GameObject SelectorGO
    {
      get { return selectorGO_; }
      set { selectorGO_ = value; }
    }

    [SerializeField]
    private SELECTION_MODE selectionMode_ = SELECTION_MODE.INSIDE;
    public SELECTION_MODE SelectionMode
    {
      get { return selectionMode_; }
      set { selectionMode_ = value; }
    }

    [SerializeField]
    private bool frontierPieces_ = true;
    public bool FrontierPieces
    {
      get { return frontierPieces_; }
      set { frontierPieces_ = value; }
    }

    [SerializeField]
    private bool complementary_ = false;
    public bool Complementary
    {
      get { return complementary_; }
      set { complementary_ = value; }
    }  

    public override CommandNode DeepClone(GameObject dataHolder)
    {
      CNSelector clone = CommandNode.CreateInstance<CNSelector>(dataHolder);

      clone.field_ = Field.DeepClone();

      clone.Name = Name;    
      return clone;
    }

  } //namespace CNWelder
}

