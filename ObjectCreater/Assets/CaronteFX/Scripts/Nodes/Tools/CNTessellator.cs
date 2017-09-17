using UnityEngine;
using System.Collections;

namespace CaronteFX
{
  public class CNTessellator : CNMonoField
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
    float maxEdgeLength_ = 0.5f;
    public float MaxEdgeDistance
    {
      get { return maxEdgeLength_; }
      set { maxEdgeLength_ = Mathf.Clamp( value, 0.0001f, float.MaxValue ); }
    }

    [SerializeField]
    bool limitByMeshDimensions_ = true;
    public bool LimitByMeshDimensions
    {
      get { return limitByMeshDimensions_; }
      set { limitByMeshDimensions_ = value; }
    }

    [SerializeField]
    Mesh[] arrTessellatedMesh_;
    public Mesh[] ArrTessellatedMesh
    {
      get { return arrTessellatedMesh_; }
      set { arrTessellatedMesh_ = value; }
    }

    [SerializeField]
    GameObject[] arrTessellatedGO_;
    public GameObject[] ArrTessellatedGO
    {
      get { return arrTessellatedGO_; }
      set { arrTessellatedGO_ = value; }
    }

    [SerializeField]
    GameObject nodeGO_;
    public GameObject NodeGO
    {
      get { return nodeGO_; }
      set { nodeGO_ = value; }
    }

    public override CommandNode DeepClone(GameObject dataHolder)
    {
      CNTessellator clone = CommandNode.CreateInstance<CNTessellator>(dataHolder);

      clone.field_ = Field.DeepClone();

      clone.Name = Name;    
      return clone;
    }

  } //namespace CNWelder
}
