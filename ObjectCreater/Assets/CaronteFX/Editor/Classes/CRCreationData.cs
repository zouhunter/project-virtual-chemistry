using UnityEngine;
using CaronteSharp;
using System.Collections;

namespace CaronteFX
{
  public class CRCreationData
  {
    public GameObject gameObject_;

    public BodyType bodyType_;
    public bool isConvexHull_;

    public byte[] renderFingerprint_;
    public byte[] colliderFingerprint_;

    public Vector3    position_;
    public Quaternion rotation_;
    public Vector3    scale_;

    public CRCreationData( GameObject go, BodyType bodyType, Mesh renderMesh, Mesh colliderMesh, bool isConvex )
    {
      gameObject_   = go;
      bodyType_     = bodyType;
      isConvexHull_ = isConvex;

      renderFingerprint_   = null;
      colliderFingerprint_ = null;

      if (renderMesh != null)
      {
        renderFingerprint_   = new byte[256];
        CRGeometryUtils.CalculateFingerprint( renderMesh, renderFingerprint_ );
      }

      if (colliderMesh != null)
      {
        colliderFingerprint_ = new byte[256];
        CRGeometryUtils.CalculateFingerprint( colliderMesh, colliderFingerprint_ );
      }

      position_ = go.transform.position;
      rotation_ = go.transform.rotation;
      scale_    = go.transform.lossyScale;
    }

    public void Update( Vector3 newPosition, Quaternion newRotation )
    {
      position_ = newPosition;
      rotation_ = newRotation;
    }

    public bool AreFingerprintsValid()
    {
      if (  bodyType_ == BodyType.BodyMeshAnimatedByArrPos )
      {
        return AreFingerprintsValidAnimatedByArrPos();      
      }
      else
      {
        return AreFingerprintsValidNonAnimatedByArrPos();      
      }
    }

    private bool AreFingerprintsValidAnimatedByArrPos()
    {
      bool isValid = true;

      Mesh renderMesh;
      gameObject_.GetBakedMesh(out renderMesh);

      byte[] fingerPrint = new byte[256];
      if (renderFingerprint_ != null)
      {
        if (renderMesh != null)
        {
          CRGeometryUtils.CalculateFingerprint( renderMesh, fingerPrint );
          isValid &= CRGeometryUtils.AreFingerprintsEqual( fingerPrint, renderFingerprint_ );
        }
        else
        {
          return false;
        }
      }

      if (renderMesh != null)
      {
        Object.DestroyImmediate(renderMesh);
      }

      return true;
    }

    private bool AreFingerprintsValidNonAnimatedByArrPos()
    {
      bool isValid = true;

      Mesh renderMesh = gameObject_.GetMesh();

      byte[] fingerPrint = new byte[256];
      if (renderFingerprint_ != null)
      {
        if (renderMesh != null)
        {
          CRGeometryUtils.CalculateFingerprint( renderMesh, fingerPrint );
          isValid &= CRGeometryUtils.AreFingerprintsEqual( fingerPrint, renderFingerprint_ );
        }
        else
        {
          return false;
        }
      }

      if (colliderFingerprint_ != null)
      {
        Caronte_Fx_Body fxBody = gameObject_.GetComponent<Caronte_Fx_Body>();
        if (fxBody != null)
        {
          if ( fxBody.IsConvexHull() != isConvexHull_ )
          {
            return false;
          }

          if (fxBody.tileMesh_ != null)
          {
              CRGeometryUtils.CalculateFingerprint( fxBody.tileMesh_, fingerPrint );
              isValid &= CRGeometryUtils.AreFingerprintsEqual( fingerPrint, colliderFingerprint_ );
          }
          else if ( fxBody.colliderType_ == Caronte_Fx_Body.ColliderType.MeshFilter )
          {
            if ( renderMesh != null )
            {
              CRGeometryUtils.CalculateFingerprint( renderMesh, fingerPrint );
              isValid &= CRGeometryUtils.AreFingerprintsEqual( fingerPrint, colliderFingerprint_ );
            }
            else
            {
              return false;
            }
          }
          else if ( fxBody.colliderType_ == Caronte_Fx_Body.ColliderType.MeshFilterConvexHull)
          {
            if ( renderMesh != null )
            {
              CRGeometryUtils.CalculateFingerprint( renderMesh, fingerPrint );
              isValid &= CRGeometryUtils.AreFingerprintsEqual( fingerPrint, colliderFingerprint_ );
            }
            else
            {
              return false;
            }
          }
          else if ( fxBody.colliderType_ == Caronte_Fx_Body.ColliderType.CustomMesh)
          {
            Mesh colliderMesh = fxBody.colliderMesh_;
            if (colliderMesh != null)
            {
              CRGeometryUtils.CalculateFingerprint( colliderMesh, fingerPrint );
              isValid &= CRGeometryUtils.AreFingerprintsEqual( fingerPrint, colliderFingerprint_ );
            }
            else if (renderMesh != null)
            {
              CRGeometryUtils.CalculateFingerprint( renderMesh, fingerPrint );
              isValid &= CRGeometryUtils.AreFingerprintsEqual( fingerPrint, colliderFingerprint_ );
            }
            else
            {
              return false;
            }
          }
        }
        else
        {
          return false;
        }
      }
      return isValid;
    }

  }
}
