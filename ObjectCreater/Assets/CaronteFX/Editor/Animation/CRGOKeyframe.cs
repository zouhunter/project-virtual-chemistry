using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;


namespace CaronteFX
{
  public class CRGOKeyframe
  {
    BROADCASTFLAG frameFlags_;
    Vector3       framePosition_;
    Quaternion    frameRotation_;

    List<Vector3> listFrameVertexPos_;
    List<Vector3> listFrameVertexNor_;
    List<Vector4> listFrameVertexTan_;
    Bounds?       frameBounds_;

    int vertexCount_;
    public int VertexCount
    {
      get { return vertexCount_; }
    }

    bool isInvisibleAllFrames_;
    public bool IsInvisibleAllFrames
    {
      get { return isInvisibleAllFrames_; }
      set { isInvisibleAllFrames_ = value; }
    }

    public CRGOKeyframe(int vertexCount)
    {
      vertexCount_ = vertexCount;
      isInvisibleAllFrames_ = true;

      if (vertexCount > 0)
      {
        frameBounds_ = new Bounds();

        listFrameVertexPos_ = new List<Vector3>();
        listFrameVertexPos_.Capacity = vertexCount;

        listFrameVertexNor_ = new List<Vector3>();
        listFrameVertexNor_.Capacity = vertexCount;

        listFrameVertexTan_ = new List<Vector4>();
        listFrameVertexTan_.Capacity = vertexCount;
      }
    }

    public bool SetBodyKeyframe( BROADCASTFLAG broadcastFlag, Vector3 position, Quaternion rotation )
    {
      frameFlags_ = broadcastFlag;

      bool isVisible = ( broadcastFlag & BROADCASTFLAG.VISIBLE ) == BROADCASTFLAG.VISIBLE;
      bool isGhost   = ( broadcastFlag & BROADCASTFLAG.GHOST )   == BROADCASTFLAG.GHOST;
    
      if ( !(isVisible || isGhost) )
      {
        return false;
      }

      isInvisibleAllFrames_ = false;
      rotation.CoordinateWith(frameRotation_);

      framePosition_ = position;
      frameRotation_ = rotation;

      return true;
    }

    public void SetVertexKeyframe( Vector3[] arrVertexPos, Vector3[] arrVertexNor, Vector4[] arrVertexTan, Bounds bounds )
    {
      listFrameVertexPos_.Clear();
      listFrameVertexPos_.AddRange( arrVertexPos );

      listFrameVertexNor_.Clear();
      listFrameVertexNor_.AddRange( arrVertexNor );

      listFrameVertexTan_.Clear();
      listFrameVertexTan_.AddRange( arrVertexTan );

      frameBounds_ = new Bounds(bounds.center, bounds.size);   
    }

    public BROADCASTFLAG GetFrameFlags()
    {
      return frameFlags_;
    }

    public Vector3 GetPositionInFrame()
    {
      return framePosition_;
    }

    public Quaternion GetRotationInFrame()
    {
      return frameRotation_;
    }

    public List<Vector3> GetVertexesPosInFrame()
    {
      return listFrameVertexPos_;
    }

    public List<Vector3> GetVertexesNorInFrame()
    {
      return listFrameVertexNor_;
    }

    public List<Vector4> GetVertexesTanInFrame()
    {
      return listFrameVertexTan_;
    }

    public Bounds? GetBoundsInFrame()
    {
      return frameBounds_;
    }

  }

} //namespace Caronte