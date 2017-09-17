using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;

namespace CaronteFX
{
  public static class CRGeometryUtils
  {

    public static void CalculateFingerprint(Mesh mesh, byte[] meshFingerprint)
    {
      MeshComplex car_mesh = new MeshComplex();
      car_mesh.Set(mesh);
      CaronteSharp.Tools.CalculateMeshFingerprint( car_mesh, meshFingerprint );   
    }

    public static bool AreFingerprintsEqual( byte[] currentFingerprint, byte[] oldFingerprint )
    {
      for (int i = 0; i < 256; i++)
      {
        byte a = currentFingerprint[i];
        byte b = oldFingerprint[i];

        if ( a != b )
        {
          return false;
        }
      }
      return true;
    }

    public static void CreateMeshTransformed(Mesh originalMesh, Matrix4x4 transform, out Mesh m)
    {
      m = (Mesh) Object.Instantiate(originalMesh);
      m.name = originalMesh.name;

      Matrix4x4 inv_transform = transform.inverse;
      Matrix4x4 transposed_inv_transform = inv_transform.transpose;
    
      Vector3[] arrPos = m.vertices;
      Vector3[] arrNor = m.normals;
      Vector4[] arrTan = m.tangents;

      int vertexCount = arrPos.Length;
      int nTangents   = arrTan.Length;

      if (nTangents == 0)
      {
        for (int i = 0; i < vertexCount; ++i)
        {
          arrPos[i] = transform.MultiplyPoint3x4(arrPos[i]);
          arrNor[i] = transposed_inv_transform.MultiplyVector(arrNor[i]);
        }
      }
      else
      {
        for (int i = 0; i < vertexCount; ++i)
        {
          arrPos[i] = transform.MultiplyPoint3x4(arrPos[i]);
          arrNor[i] = transposed_inv_transform.MultiplyVector(arrNor[i]);
          arrTan[i] = transposed_inv_transform.MultiplyVector(arrTan[i]);
        }
      }

      m.vertices = arrPos;
      m.normals  = arrNor;
      m.tangents = arrTan;

      m.RecalculateBounds();
    }

    public static void CreateBoundsTransformed(Bounds bounds, Matrix4x4 transform, out Bounds boundsOut)
    {
      Vector3 center = transform.MultiplyPoint3x4(bounds.center);
      Vector3 size   = transform.MultiplyVector(bounds.size);

      boundsOut = new Bounds(center, size);
    }

    public static void TransformListMesh(List<GameObject> listParentGO, List<UnityEngine.Mesh> listMesh_un, Matrix4x4 m_BoundsWorld_to_BoundsLocal, out List<CaronteSharp.MeshComplex> listMesh_car)
    {
      listMesh_car = new List<CaronteSharp.MeshComplex>();
      int numParentGameObjects = listParentGO.Count;
      for (int i = 0; i < numParentGameObjects; ++i)
      {
        UnityEngine.Mesh un_mesh = listMesh_un[i];
        GameObject parent_go = listParentGO[i];

        Matrix4x4 m_Local_to_World = parent_go.transform.localToWorldMatrix;
        Matrix4x4 m_Local_to_Bounds = m_BoundsWorld_to_BoundsLocal * m_Local_to_World;

        UnityEngine.Mesh transformedMesh;
        CRGeometryUtils.CreateMeshTransformed(un_mesh, m_Local_to_Bounds, out transformedMesh);

        CaronteSharp.MeshComplex car_mesh = new CaronteSharp.MeshComplex();
        car_mesh.Set(transformedMesh);

        UnityEngine.Object.DestroyImmediate(transformedMesh);
        listMesh_car.Add(car_mesh);
      }
    }

    public static GameObject CreatePiece( int id, Mesh mesh, MeshParentInfo parentInfo, Vector3 goPosition, Tuple2<int,int> submeshRange, 
                                          Material interiorMaterial, GameObject parentGO, GameObject chopGO, out int interiorSubmeshIdx )
    {
      string name = parentGO.name + "_" + id;
      GameObject pieceGO = new GameObject(name);
      mesh.name = name;

      pieceGO.transform.parent = chopGO.transform;
      pieceGO.transform.position = goPosition;

      pieceGO.AddMesh(mesh);

      AssignPieceMaterials( pieceGO, parentGO, parentInfo, interiorMaterial, submeshRange, out interiorSubmeshIdx );
 
      return pieceGO;
    }

    private static void AssignPieceMaterials( GameObject pieceGO, GameObject parentGO, MeshParentInfo parentInfo, Material interiorMaterial, Tuple2<int,int> submeshRange, out int interiorSubmeshIdx )
    {
      interiorSubmeshIdx = -1;

      MeshRenderer pieceRenderer  = pieceGO.GetComponent<MeshRenderer>();
      MeshRenderer parentRenderer = parentGO.GetComponent<MeshRenderer>();
      if ( parentRenderer != null )
      {
        Material[] arrMaterials       = parentRenderer.sharedMaterials; 
        List<Material> pieceMaterials = new List<Material>();

        uint[] arrParentSubmeshIdx = parentInfo.arrParentSubMeshIdx_;

        for ( int i = submeshRange.First; i <= submeshRange.Second; i++ )
        {
          uint submeshIndex = arrParentSubmeshIdx[i];
          if (submeshIndex == uint.MaxValue)
          {
            pieceMaterials.Add( interiorMaterial );
            interiorSubmeshIdx = i;
          }
          else
          {
            pieceMaterials.Add( arrMaterials[submeshIndex] );
          }
        }
        pieceRenderer.sharedMaterials = pieceMaterials.ToArray();
      }
    }


    public static void CreateMeshesFromCaronte(CaronteSharp.MeshComplex[] arrCaronteMesh, out UnityEngine.Mesh[] arrMesh, out Tuple2<int,int>[] arrSubmeshRange, out int[] meshComplexIndex)
    {
      int numMeshes = arrCaronteMesh.Length;

      List< Mesh >             listMesh = new List< Mesh >();
      List< Tuple2<int,int> >  listSubmeshRange = new List< Tuple2<int,int> >();
      List< int >              listMeshComplexIndex = new List< int >();

      for (int i = 0; i < numMeshes; i++)
      {
        CaronteSharp.MeshComplex caronteMesh = arrCaronteMesh[i];

        Mesh[] arrAuxMesh;
        Tuple2<int, int>[] arrAuxSubmeshRange;

        CRGeometryUtils.CreateMeshesFromComplex(caronteMesh, out arrAuxMesh, out arrAuxSubmeshRange);
        listMesh        .AddRange(arrAuxMesh);
        listSubmeshRange.AddRange(arrAuxSubmeshRange);

        for (int j = 0; j < arrAuxMesh.Length; j++)
        {
          listMeshComplexIndex.Add( i );
        }
      }

      arrMesh          = listMesh.ToArray();
      arrSubmeshRange  = listSubmeshRange.ToArray();
      meshComplexIndex = listMeshComplexIndex.ToArray();
    }

    public static void WeldObjects(GameObject[] arrGOtoWeld, string name, Material interiorMaterial, out GameObject[] arrWeldedObject, out Mesh[] arrWeldedMesh, out int interiorMaterialIdx)
    {     
      List<GameObject> listParentGO;
      List<Mesh>       listParentMesh_un;
      List<Material>   listMaterial;
      List<ArrayIndex> listMaterialInfo;

      GenerateWeldLists( arrGOtoWeld, interiorMaterial, out listParentGO, out listParentMesh_un, out listMaterial, out listMaterialInfo, out interiorMaterialIdx );

      GameObject boundsObject;
      Matrix4x4  boundsMatrixWorldToLocal;
      CreateGlobalBoundsObject( listParentGO, out boundsObject, out boundsMatrixWorldToLocal );

      List<CaronteSharp.MeshComplex> listParentMesh_car;
      List<Matrix4x4> listMeshMatrixLocalToWorld;
      CreateListMeshComplexFromUnity( listParentGO, listParentMesh_un, out listParentMesh_car, out listMeshMatrixLocalToWorld );

      CaronteSharp.MeshComplex  mesh_COLLAPSED_car;
      CaronteSharp.ArrayIndex   mesh_COLLAPSED_info;
      CaronteSharp.Tools.CollapseMeshes( boundsMatrixWorldToLocal, listParentMesh_car.ToArray(), listMeshMatrixLocalToWorld.ToArray(), listMaterialInfo.ToArray(), out mesh_COLLAPSED_car, out mesh_COLLAPSED_info );

      if ( mesh_COLLAPSED_car != null )
      {
        CreateMeshCollapsedUnity(name, mesh_COLLAPSED_car, mesh_COLLAPSED_info, boundsObject, listMaterial, out arrWeldedObject, out arrWeldedMesh);
      }
      else
      {
        arrWeldedObject     = null;
        arrWeldedMesh       = null;
        interiorMaterialIdx = -1;
      }

      Object.DestroyImmediate( boundsObject );
    }

    private static void GenerateWeldLists( GameObject[] arrGOtoWeld, Material interiorMaterial, out List<GameObject> listParentGO, out List<Mesh> listParentMesh_un, 
                                           out List<Material> listMaterial, out List<ArrayIndex> listMaterialInfo, out int interiorMaterialIdx )
    {    
      listParentGO = new List<GameObject>();
      listParentMesh_un = new List<UnityEngine.Mesh>();

      listMaterial     = new List<Material>();
      listMaterialInfo = new List<ArrayIndex>();

      interiorMaterialIdx = -1;

      for (int i = 0; i < arrGOtoWeld.Length; i++)
      {
        GameObject go = arrGOtoWeld[i];
        UnityEngine.Mesh un_mesh = go.GetMesh();
        if (un_mesh != null)
        {
          listParentMesh_un.Add(un_mesh);
          listParentGO.Add(go);

          int submeshCount            = un_mesh.subMeshCount;
          ArrayIndex meshMaterialInfo = new ArrayIndex();
          meshMaterialInfo.arrIdx_    = new uint[submeshCount];

          Renderer renderer = go.GetComponent<Renderer>();
          if( renderer != null )
          {
            Material[] arrMaterials = renderer.sharedMaterials;
            int nMaterials          = arrMaterials.Length;

            for (int j = 0; j < submeshCount || j < nMaterials; j++)
            {
              Material material = arrMaterials[j];
              if( material != null )
              {
                if ( !listMaterial.Contains( material ) )
                {
                  if (interiorMaterial != null && material == interiorMaterial )
                  {
                    interiorMaterialIdx = listMaterial.Count;
                  }
                  listMaterial.Add( material );
                }

                int indexMaterial = listMaterial.IndexOf(material);
                meshMaterialInfo.arrIdx_[j] = (uint)indexMaterial;
              }
              else
              {
                meshMaterialInfo.arrIdx_[j] = uint.MaxValue;
              }
            }     
          }
          else
          {
            for (int j = 0; j < submeshCount; j++)
            {
              meshMaterialInfo.arrIdx_[j] = uint.MaxValue;
            }
          }
          listMaterialInfo.Add(meshMaterialInfo);
        }
      }
    }

    private static void CreateGlobalBoundsObject( List<GameObject> listGO, out GameObject boundsObject, out Matrix4x4 boundsMatrixWorldToLocal)
    {
      Bounds objectsBounds = CREditorUtils.GetGlobalBoundsWorld(listGO);
      boundsObject = new GameObject();
      Transform boundsTransform = boundsObject.transform;
      boundsTransform.position  = objectsBounds.center;
      boundsMatrixWorldToLocal  = boundsTransform.worldToLocalMatrix;
    }

    private static void CreateListMeshComplexFromUnity( List<GameObject> listGameObject, List<Mesh> listMesh, out List<MeshComplex> listMeshComplex, out List<Matrix4x4> listMeshComplexLocalToWorld )
    {
      listMeshComplex = new List<CaronteSharp.MeshComplex>();
      listMeshComplexLocalToWorld = new List<Matrix4x4>();
      int nParentGameObjects = listGameObject.Count;
      for (int i = 0; i < nParentGameObjects; i++)
      {
        UnityEngine.Mesh un_mesh = listMesh[i];
        GameObject parent_go     = listGameObject[i];

        CaronteSharp.MeshComplex car_mesh = new CaronteSharp.MeshComplex();
        car_mesh.Set(un_mesh);
        listMeshComplex.Add(car_mesh);

        Matrix4x4 m_Local_to_World = parent_go.transform.localToWorldMatrix;
        listMeshComplexLocalToWorld.Add( m_Local_to_World );
      }
    }

    private static void CreateMeshCollapsedUnity( string name, MeshComplex mesh_COLLAPSED_car, ArrayIndex mesh_COLLAPSED_info, GameObject boundsObject, 
                                                  List<Material> listMaterial, out GameObject[] arrGameObject, out Mesh[] arrMesh )
    {
      UnityEngine.Mesh[] arrMesh_COLLAPSED_un;
      Tuple2<int,int>[]  arrSubmeshRange;

      CRGeometryUtils.CreateMeshesFromComplex( mesh_COLLAPSED_car, out arrMesh_COLLAPSED_un, out arrSubmeshRange );

      int nMeshUnity = arrMesh_COLLAPSED_un.Length;

      List<GameObject> listGameObject = new List<GameObject>();
      List<Mesh> listMesh = new List<Mesh>();

      for (int i = 0; i < nMeshUnity; i++)
      {
        Mesh mesh_COLLAPSED_un = arrMesh_COLLAPSED_un[i];
        mesh_COLLAPSED_un.name = name + "_" + i;
        listMesh.Add( mesh_COLLAPSED_un );

        GameObject go = Object.Instantiate( boundsObject );
        go.name = name + "_" + i;
        go.AddMesh( mesh_COLLAPSED_un );
        listGameObject.Add(go);

        Tuple2<int,int> submeshRange = arrSubmeshRange[i];

        List<Material> collapsedMaterials = new List<Material>();      
        uint[] arrMaterialIdx   = mesh_COLLAPSED_info.arrIdx_;

        for( int j = submeshRange.First; j <= submeshRange.Second; j++ )
        {
          uint materialIdx = arrMaterialIdx[j];
          if ( materialIdx != uint.MaxValue )
          {
            Material mat = listMaterial[(int)materialIdx];
            collapsedMaterials.Add( mat );
          }
          else
          {
            collapsedMaterials.Add( null );
          }

        }

        Renderer renderer = go.GetComponent<Renderer>();
        renderer.sharedMaterials = collapsedMaterials.ToArray();
      }

      arrGameObject = listGameObject.ToArray();
      arrMesh       = listMesh.ToArray();

    }

    public static void TessellateObjects(GameObject[] arrGameObjectToTessellate, float maxEdgeLength, bool limitByMeshDimensions, out GameObject[] arrGameObjectTessellated, out Mesh[] arrMeshTessellated)
    {
      int nGameObject = arrGameObjectToTessellate.Length;

      List< GameObject >        listGameObjectTessellated      = new List< GameObject >();
      List< Mesh >              listMeshTessellated            = new List< Mesh >();
      List< Tuple2<int, int> >  listMeshTessellatedSubmesRange = new List< Tuple2<int, int> >();

      Dictionary< Mesh, List<int> > dictMeshListMeshIdx = new Dictionary< Mesh, List<int> >();
      MeshComplex auxMesh = new MeshComplex();

      double tssDistance = (double)maxEdgeLength;

      for (int i = 0; i < nGameObject; i++)
      {
        GameObject go = arrGameObjectToTessellate[i];
        Mesh mesh     = go.GetMesh();

        if (mesh != null)
        {
          bool alreadyTessellated = dictMeshListMeshIdx.ContainsKey(mesh);
          if (!alreadyTessellated)
          {
            if (limitByMeshDimensions)
            {
              Bounds bounds = mesh.bounds;
              Vector3 size = bounds.size;

              float minDistance  = size.magnitude / 50f;
              tssDistance = (double)Mathf.Clamp( maxEdgeLength, minDistance, float.MaxValue );
            }

            auxMesh.Set( mesh );
            MeshComplex tessellatedMesh; 
            CaronteSharp.Tools.TessellateMesh( auxMesh, (double)tssDistance, out tessellatedMesh );

            if (tessellatedMesh == null)
            {
              arrGameObjectTessellated = null;
              arrMeshTessellated = null;
              return;
            }
            
            Mesh[]            auxArrMeshTessellated;
            Tuple2<int,int>[] auxArrSubmeshRange;

            CreateMeshesFromComplex( tessellatedMesh, out auxArrMeshTessellated, out auxArrSubmeshRange );

            List<int> listMeshTessellatedIdx = new List<int>();
            for (int j = 0; j < auxArrMeshTessellated.Length; j++)
            {
              Mesh auxMeshTessellated = auxArrMeshTessellated[j];
              Tuple2<int, int> auxSubmeshRange = auxArrSubmeshRange[j];
              auxMeshTessellated.name = mesh.name + "_" + j + "_tss";

              listMeshTessellatedIdx.Add( listMeshTessellated.Count ) ;
              listMeshTessellated.Add( auxMeshTessellated );  
              listMeshTessellatedSubmesRange.Add( auxSubmeshRange );
            }

 
            dictMeshListMeshIdx.Add( mesh, listMeshTessellatedIdx );
          }

          List<int> listMeshIdx = dictMeshListMeshIdx[mesh];
          for (int j = 0; j < listMeshIdx.Count; j++)
          {
            GameObject goTessellated = Object.Instantiate<GameObject>(go);

            goTessellated.name                    = go.name + "_" + j + "_tss";
            goTessellated.transform.parent        = go.transform.parent;
            goTessellated.transform.localPosition = go.transform.localPosition;
            goTessellated.transform.localRotation = go.transform.localRotation;
            goTessellated.transform.localScale    = go.transform.localScale;

            int idx = listMeshIdx[j];
            Mesh meshTessellated = listMeshTessellated[idx] ;

            MeshFilter mf = goTessellated.GetComponent<MeshFilter>();
            if (mf != null)
            {
              mf.sharedMesh = meshTessellated;
            }

            Renderer rn = goTessellated.GetComponent<Renderer>();
            if (rn != null)
            {
              Tuple2<int,int> submeshRange = listMeshTessellatedSubmesRange[idx];
              Material[] arrMaterial = rn.sharedMaterials;

              List<Material> listMaterial = new List<Material>();
              for (int k = 0; k < arrMaterial.Length; k++)
              {
                if ( k >= submeshRange.First && k <= submeshRange.Second )
                {
                  Material mat = arrMaterial[k];
                  listMaterial.Add(mat);
                }
              }
              rn.sharedMaterials = listMaterial.ToArray();
            }

            SkinnedMeshRenderer smr = goTessellated.GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
            {
              smr.sharedMesh = meshTessellated;
            }
              
            listGameObjectTessellated.Add(goTessellated);
          }
        }
      }

      arrGameObjectTessellated = listGameObjectTessellated.ToArray();
      arrMeshTessellated       = listMeshTessellated.ToArray();
    }

    public static void CreateMeshFromSimple(CaronteSharp.MeshSimple inputMesh, out UnityEngine.Mesh outputMesh)
    {
      outputMesh = new UnityEngine.Mesh();

      outputMesh.vertices  = inputMesh.arrPosition_;
      outputMesh.triangles = inputMesh.arrIndices_;

      outputMesh.RecalculateNormals();
      outputMesh.RecalculateBounds();

      outputMesh.Optimize();
    }

    private static void CreateMeshesFromComplex(CaronteSharp.MeshComplex inputMesh, out UnityEngine.Mesh[] outputMeshes, out Tuple2<int,int>[] outputSubmeshRange)
    {
      Vector3[] vertices = inputMesh.arrPosition_;
      Vector3[] normals  = inputMesh.arrNormal_;
      Vector4[] tangents = inputMesh.arrTan_;
      Vector2[] uv       = inputMesh.arrUV_;

      bool hasNormals  = (normals != null) && (normals.Length > 0);
      bool hasTangents = (tangents != null) && (tangents.Length > 0);
      bool hasUVs      = (uv != null) && (uv.Length > 0);

      int maxVertexesPerMesh = 65533;

      List< UnityEngine.Mesh > listMeshes       = new List< UnityEngine.Mesh >();
      List< Tuple2<int,int>  > listSubmeshRange = new List< Tuple2<int,int> >();

      List<Vector3> listCurPos = new List<Vector3>();
      List<Vector3> listCurNor = new List<Vector3>();
      List<Vector4> listCurTan = new List<Vector4>();
      List<Vector2> listCurUVs = new List<Vector2>();

      List< List<int> > listListCurIndices   = new List< List<int> >();

      Dictionary<int,int> dictOldIndexToNew = new Dictionary<int,int>();

      int indexMin = 0;
      int indexMax = 0;

      int submeshIdMin = 0;

      int[] subMeshesIdx = inputMesh.arrSubmeshIdx_;    
      int nSubmeshIdx = subMeshesIdx.Length;

      for (int i = 0; i < nSubmeshIdx; i++)
      {
        indexMin = indexMax;
        indexMax = subMeshesIdx[i] * 3;

        List<int> listCurIndices = new List<int>();
        listListCurIndices.Add( listCurIndices );

        for (int j = indexMin; j < indexMax; j++)
        {     
          int oldIndex = inputMesh.arrIndex_[j];
          if ( !dictOldIndexToNew.ContainsKey(oldIndex) )
          {
            dictOldIndexToNew.Add( oldIndex, listCurPos.Count );

            listCurPos.Add( vertices[oldIndex] );
            if (hasNormals)
            {
              listCurNor.Add( normals[oldIndex]  );
            }
            if (hasTangents)
            {
              listCurTan.Add( tangents[oldIndex] );
            }
            if (hasUVs)
            {
              listCurUVs.Add( uv[oldIndex]       );
            }
          }

          int index = dictOldIndexToNew[oldIndex];
          listCurIndices.Add(index);

          bool isTriangleEnd      = ( (j % 3) == 2 );
          bool isMaxVertexReached = ( listCurPos.Count >= maxVertexesPerMesh );
          bool isLastMeshTriangle = ( ( i == (nSubmeshIdx - 1) ) && ( j == (indexMax - 1) ) );

          if ( (isMaxVertexReached && isTriangleEnd) || isLastMeshTriangle )
          {
            Mesh mesh;
            GenerateUnityMesh( listCurPos, listCurNor, listCurTan, listCurUVs, listListCurIndices, out mesh );

            listMeshes      .Add( mesh );
            listSubmeshRange.Add( Tuple2.New( submeshIdMin, i ) );

            submeshIdMin = i;

            listListCurIndices.Add( listCurIndices );
            dictOldIndexToNew.Clear();
          }
        }
      }
      
      outputMeshes       = listMeshes      .ToArray();
      outputSubmeshRange = listSubmeshRange.ToArray();
    }

    private static void GenerateUnityMesh( List<Vector3> listPos, List<Vector3> listNor, List<Vector4> listTan, List<Vector2> listUV, List< List<int> > listListIndices, out Mesh mesh )
    {
      mesh = new Mesh();

      mesh.vertices = listPos.ToArray();
      mesh.normals  = listNor.ToArray();
      mesh.tangents = listTan.ToArray();
      mesh.uv       = listUV.ToArray();

      mesh.subMeshCount = listListIndices.Count;

      for (int i = 0; i < listListIndices.Count; i++)
      {
        List<int> listIndices = listListIndices[i];
        mesh.SetTriangles( listIndices.ToArray(), i );
        listIndices.Clear();
      }

      mesh.RecalculateBounds();
      mesh.Optimize();

      listPos        .Clear();
      listNor        .Clear();
      listTan        .Clear();
      listUV         .Clear();
      listListIndices.Clear();
    }


    public static GameObject GenerateGameObjectFromVertexes( List<Vector3> listPos, List<Vector3> listNor, List<Vector4> listTan, List<Vector2> listUV1s, List<Vector2> listUV2s, 
                                                             List<Vector2> listMatrixIdx, List< List<int> > listListIndices, List<Material> listMaterial )
    {
      GameObject go = new GameObject();
      Mesh     mesh = new Mesh();

      mesh.vertices = listPos.ToArray();
      mesh.normals  = listNor.ToArray();
      mesh.tangents = listTan.ToArray();

      mesh.uv  = listUV1s.ToArray();
      mesh.uv2 = listUV2s.ToArray();
      mesh.uv4 = listMatrixIdx.ToArray();

      int nSubmeshes = listListIndices.Count;
      for (int i = 0; i < nSubmeshes; i++)
      {
        List<int> listIndices = listListIndices[i];
        mesh.SetTriangles(listIndices.ToArray(), i);
      }
      
      mesh.Optimize();
      mesh.RecalculateBounds();

      MeshFilter   mf = go.AddComponent<MeshFilter>();
      MeshRenderer mr = go.AddComponent<MeshRenderer>();

      mf.sharedMesh = mesh;
      mr.sharedMaterials = listMaterial.ToArray();

      return go;
    }

  }
}