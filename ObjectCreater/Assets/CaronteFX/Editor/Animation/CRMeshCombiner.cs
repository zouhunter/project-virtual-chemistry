using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CaronteFX
{
  public class CRMeshCombiner
  {
    const int maxVertices_ = 65535;

    bool isSkinnedCombine_;

    List<Vector3> listVertex_   = new List<Vector3>();
    List<Vector3> listNormal_   = new List<Vector3>(); 
    List<Vector2> listUVs_      = new List<Vector2>();
    List<Vector4> listTangents_ = new List<Vector4>();

    List<int> listIndex_            = new List<int>();
    List<int> listIndexMaterialIdx_ = new List<int>();

    List<Transform>  listBone_       = new List<Transform>();
    List<Matrix4x4>  listBindPose_   = new List<Matrix4x4>();
    List<BoneWeight> listBoneWeight_ = new List<BoneWeight>();

    List<Material> listMaterial_   = new List<Material>();

    Matrix4x4 m_WORLD_to_LOCALCOMBINED_;

    Dictionary<int, int> dictSubmeshIdxMaterialIdxTmp = new Dictionary<int,int>();

    int goOffset = 0;


    public CRMeshCombiner(bool isSkinnedCombine)
    {
      isSkinnedCombine_ = isSkinnedCombine;
    }

    public void Clear()
    {
      listVertex_  .Clear();
      listNormal_  .Clear();
      listUVs_     .Clear();
      listTangents_.Clear();

      listIndex_           .Clear();
      listIndexMaterialIdx_.Clear();

      listBone_      .Clear();
      listBindPose_  .Clear();
      listBoneWeight_.Clear();

      listMaterial_  .Clear();

      goOffset = 0;
    }

    public void SetWorldToLocalClearingInfo( Matrix4x4 m_WORLD_to_LOCALCOMBINED )
    {
      m_WORLD_to_LOCALCOMBINED_ = m_WORLD_to_LOCALCOMBINED;
      Clear();
    }

    public bool CanAddGameObject( GameObject go )
    {
      Mesh mesh = go.GetMesh();

      if ( mesh != null )
      {
        int nVertices = mesh.vertexCount;
        int currentVertices = listVertex_.Count;

        if ( (currentVertices + nVertices) > maxVertices_ )
        {
          return false;
        }
      }

      return true;
    }

    public bool CanGenerateMesh()
    {
      int currentVertices = listVertex_.Count;
      return (currentVertices > 0);
    }

    public void AddGameObject(GameObject go, GameObject bakedBone)
    {
      Mesh mesh = go.GetMesh();

      if ( mesh != null )
      {
        Matrix4x4 m_LOCAL_to_WORLD = go.transform.localToWorldMatrix;
        Matrix4x4 m_LOCAL_to_LOCALCOMBINED = m_WORLD_to_LOCALCOMBINED_ * m_LOCAL_to_WORLD;

        Mesh meshAux;
        CRGeometryUtils.CreateMeshTransformed( mesh, m_LOCAL_to_LOCALCOMBINED, out meshAux );

        Vector3[] arrVertex  = meshAux.vertices;
        Vector3[] arrNormal  = meshAux.normals;
        Vector2[] arrUV      = meshAux.uv;
        Vector4[] arrTangent = meshAux.tangents;

        Renderer rn = go.GetComponent<Renderer>();
        if ( rn != null )
        {
          Material[] arrMaterial = rn.sharedMaterials;
          AddMaterials( arrMaterial );
        }

        AddMesh( go, bakedBone, meshAux, arrVertex, arrNormal, arrUV, arrTangent );
        goOffset++;

        UnityEngine.Object.DestroyImmediate(meshAux);
      }
    }

    public void AddGameObject(GameObject go)
    {
      Mesh mesh = go.GetMesh();

      if ( mesh != null )
      {
        Matrix4x4 m_LOCAL_to_WORLD = go.transform.localToWorldMatrix;
        Matrix4x4 m_LOCAL_to_LOCALCOMBINED = m_WORLD_to_LOCALCOMBINED_ * m_LOCAL_to_WORLD;

        Mesh meshAux;
        CRGeometryUtils.CreateMeshTransformed( mesh, m_LOCAL_to_LOCALCOMBINED, out meshAux );

        Vector3[] arrVertex  = meshAux.vertices;
        Vector3[] arrNormal  = meshAux.normals;
        Vector2[] arrUV      = meshAux.uv;
        Vector4[] arrTangent = meshAux.tangents;

        Renderer rn = go.GetComponent<Renderer>();
        if ( rn != null )
        {
          Material[] arrMaterial = rn.sharedMaterials;
          AddMaterials( arrMaterial );
        }

        AddMesh( go, null, meshAux, arrVertex, arrNormal, arrUV, arrTangent );
        goOffset++;

        UnityEngine.Object.DestroyImmediate(meshAux);
      }
    }



    private void AddMesh(GameObject go, GameObject bakedBone, Mesh mesh, Vector3[] arrVertex, Vector3[] arrNormal, Vector2[] arrUV, Vector4[] arrTangent)
    {
      int vertOffset = listVertex_.Count;

      int subMeshCount = mesh.subMeshCount;

      for (int i = 0; i < subMeshCount; i++)
      {
        int materialIdx = dictSubmeshIdxMaterialIdxTmp[i];
        int[] arrIndex = mesh.GetTriangles(i);

        foreach( int index in arrIndex )
        {
          listIndex_.Add( index + vertOffset );
          listIndexMaterialIdx_.Add( materialIdx );
        }
      }

      if (isSkinnedCombine_)
      {
        Transform boneTransform = bakedBone.transform;
        Matrix4x4 bindpose = boneTransform.worldToLocalMatrix * m_WORLD_to_LOCALCOMBINED_.inverse;

        listBone_    .Add( boneTransform );
        listBindPose_.Add( bindpose );

        int nVertex = arrVertex.Length;
        for(int i = 0; i < nVertex; i++)
        {
          BoneWeight bw = new BoneWeight();
          bw.weight0 = 1.0f;
          bw.boneIndex0 = goOffset;

          listBoneWeight_.Add( bw );
        }
      }

      foreach( Vector3 v in arrVertex )
      {
        listVertex_.Add( v );
      }

      foreach( Vector3 n in arrNormal )
      {
        listNormal_.Add( n );
      }

      foreach( Vector2 uv in arrUV )
      {
        listUVs_.Add( uv );
      }

      foreach( Vector4 t in arrTangent )
      {
        listTangents_.Add( t );
      }
    }

    public Mesh GenerateMesh( out Material[] arrMaterial, out Transform[] arrBone )
    {
      Mesh mesh = new Mesh();

      mesh.SetVertices(listVertex_);

      int nVertexes = listVertex_.Count;
      int nNormals  = listNormal_.Count;
      int nUVs      = listUVs_.Count;
      int nTangents = listTangents_.Count;
    
      if (nVertexes == nNormals)
      {
        mesh.SetNormals(listNormal_);
      }

      if (nVertexes == nUVs)
      {
        mesh.SetUVs(0, listUVs_);
      }
      
      if (nVertexes == nTangents)
      {
        mesh.SetTangents(listTangents_); 
      }

      SetTriangles(mesh);

      mesh.boneWeights = listBoneWeight_.ToArray();
      mesh.bindposes   = listBindPose_.ToArray();

      arrMaterial = listMaterial_.ToArray();
      arrBone = listBone_.ToArray();
      
      mesh.RecalculateBounds();
      mesh.Optimize();

      return mesh;
    }


    public Mesh GenerateMesh( out Material[] arrMaterial )
    {
      Mesh mesh = new Mesh();

      mesh.SetVertices(listVertex_);
      int nVertexes = listVertex_.Count;
      int nNormals  = listNormal_.Count;
      int nUVs      = listUVs_.Count;
      int nTangents = listTangents_.Count;
    
      if (nVertexes == nNormals)
      {
        mesh.SetNormals(listNormal_);
      }

      if (nVertexes == nUVs)
      {
        mesh.SetUVs(0, listUVs_);
      }
      
      if (nVertexes == nTangents)
      {
        mesh.SetTangents(listTangents_); 
      }

      SetTriangles(mesh);

      arrMaterial = listMaterial_.ToArray();
      
      mesh.RecalculateBounds();
      mesh.Optimize();

      return mesh;
    }

    private void SetTriangles(Mesh skinnedMesh)
    {
      int[] arrIndex = listIndex_.ToArray();
      int[] arrIndexMaterialIdx = listIndexMaterialIdx_.ToArray();

      List< List<int> > listListSubmeshTriangle;
      
      GenerateListSubmeshTriangles( arrIndex, arrIndexMaterialIdx, out listListSubmeshTriangle );

      int nSubmeshes = listListSubmeshTriangle.Count;

      for( int i = 0; i < nSubmeshes; i++ )
      {
        List<int> listSubmeshTriangle = listListSubmeshTriangle[i];
        SetSubMeshTriangles(skinnedMesh, listSubmeshTriangle, i);
      }
    }

    private void GenerateListSubmeshTriangles( int[] arrIndex, int[] arrIndexMaterialIdx, out List< List<int> > listListSubmeshTriangle )
    {
      Dictionary<int, int> dictionary = new Dictionary<int, int>(); 

      listListSubmeshTriangle = new List< List<int> >();

      for(int i = 0; i < arrIndex.Length; i++)
      {
        int index = arrIndex[i];
        int indexMaterialIdx = arrIndexMaterialIdx[i];

        if ( !dictionary.ContainsKey(indexMaterialIdx) )
        {
          int nextIndex = listListSubmeshTriangle.Count;
          listListSubmeshTriangle.Add( new List<int>() );
          dictionary.Add( indexMaterialIdx, nextIndex );
        }

        int realIdx = dictionary[indexMaterialIdx];

        listListSubmeshTriangle[realIdx].Add( index );
      }
    }

    private void SetSubMeshTriangles( Mesh skinnedMesh, List<int> listIndices, int submeshIdx )
    {
      skinnedMesh.subMeshCount = submeshIdx + 1;
      skinnedMesh.SetTriangles( listIndices.ToArray(), submeshIdx );
    }

    private void AddMaterials(Material[] arrMaterial)
    {
      dictSubmeshIdxMaterialIdxTmp.Clear();

      for (int i = 0; i < arrMaterial.Length; i++)
      {
        Material mat = arrMaterial[i];

        if ( mat == null )
        {
          dictSubmeshIdxMaterialIdxTmp.Add( i, int.MaxValue );
        }
        else
        {
          int materialIdx = listMaterial_.FindIndex( (material) => material == mat );
          if ( materialIdx == -1 )
          {
            materialIdx = listMaterial_.Count;
            listMaterial_.Add( mat );
          }
      
          dictSubmeshIdxMaterialIdxTmp.Add( i, materialIdx );    
        }    
      }
    }
  }
}

