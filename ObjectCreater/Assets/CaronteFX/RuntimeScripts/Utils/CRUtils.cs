using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaronteFX
{
  public static class CRUtils
  {
    public static bool HasMesh(this GameObject gameObject)
    {
      bool hasMesh = false;
      MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
      if (meshFilter != null)
      {
        Mesh mesh = meshFilter.sharedMesh;
        if (mesh != null)
        {
          hasMesh = true;
        }
      }
      SkinnedMeshRenderer smRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
      if (smRenderer != null)
      {
        Mesh mesh = smRenderer.sharedMesh;
        if (mesh != null)
        {
          hasMesh = true;
        }
      }
      return hasMesh;
    }

    public static Mesh GetMesh(this GameObject gameObject)
    {
      MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
      if (meshFilter != null)
      {
        Mesh mesh = meshFilter.sharedMesh;
        if (mesh != null)
        {
          return mesh;
        }
      }
      SkinnedMeshRenderer smRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
      if (smRenderer != null)
      {
        Mesh mesh = smRenderer.sharedMesh;
        if (mesh != null)
        {
          return mesh;
        }
      }
      return null;
    }

    public static Mesh GetMesh(this GameObject gameObject, out MeshFilter meshFilter)
    {
      meshFilter = gameObject.GetComponent<MeshFilter>();
      if (meshFilter != null)
      {
        Mesh mesh = meshFilter.sharedMesh;
        if (mesh != null)
        {
          return mesh;
        }
      }
      return null;
    }

    public static Mesh GetMeshInstance(this GameObject gameObject)
    {
      MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
      if (meshFilter != null)
      {
        Mesh mesh = meshFilter.mesh;
        if (mesh != null)
        {
          return mesh;
        }
      }
      return null;
    }

  }// class CRUtils
} // namespace Caronte

