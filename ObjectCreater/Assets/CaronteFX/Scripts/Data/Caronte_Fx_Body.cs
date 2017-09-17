using UnityEngine;
using System.Collections;

namespace CaronteFX
{
  [DisallowMultipleComponent]
  public class Caronte_Fx_Body : MonoBehaviour
  {
    public enum ColliderType
    {
      MeshFilter,
      MeshFilterConvexHull,
      CustomMesh,
    }

    public enum ColliderRenderMode
    {
      Wireframe,
      Solid
    }

    public ColliderType       colliderType_       = ColliderType.MeshFilter;
    public Mesh               colliderMesh_       = null;
    public Color              colliderColor_      = Color.red;
    public ColliderRenderMode colliderRenderMode_ = ColliderRenderMode.Solid;

    public Mesh               tileMesh_           = null;

    public void Start()
    {

    }

    public bool IsConvexHull()
    {
      return colliderType_ == ColliderType.MeshFilterConvexHull;
    }

    public bool IsCustomCollider()
    {
      if ( ( colliderType_ == ColliderType.CustomMesh && colliderMesh_ != null ) )
      {
        return true;
      }

      return false;
    }

    public void SetCustomCollider(Mesh colliderMesh)
    {
      colliderType_ = ColliderType.CustomMesh;
      colliderMesh_ = colliderMesh;
    }

    public Mesh GetCustomColliderMesh()
    {
      if ( colliderType_ == ColliderType.CustomMesh )
      {
        return colliderMesh_;
      }
   
      return null;
    }

    public Mesh GetTileMesh()
    {
      return tileMesh_;  
    }



    void OnDrawGizmos()
    {
      if ( this.enabled && IsCustomCollider() )
      {
        Color current = Gizmos.color;
        Gizmos.color = colliderColor_;
        if (colliderRenderMode_ == ColliderRenderMode.Solid)
        {
          Gizmos.DrawMesh( colliderMesh_, transform.position, transform.rotation, transform.lossyScale ); 
        }
        else
        {
          Gizmos.DrawWireMesh( colliderMesh_, transform.position, transform.rotation, transform.lossyScale );
        }
        
        Gizmos.color = current;
      }
    }
  }
}

