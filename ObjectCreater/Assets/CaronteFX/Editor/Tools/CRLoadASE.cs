
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using CaronteSharp;

namespace CaronteFX
{
  public class CRLoadASE : EditorWindow
  {
    public UnityEngine.GameObject go;
    private static string ASEpath;
    //-----------------------------------------------------------------------------------
    //[MenuItem("Caronte FX/Debug - LoadASE...", false, 2)]
    static void Init()
    {
      ASEpath = EditorUtility.OpenFilePanel("Load ASE...", "", "ase");
      if (!ASEpath.EndsWith(".ase") && !ASEpath.EndsWith(".ASE"))
      {
        ASEpath = "";
        
      }
      else
      {
        LoadASE();
      }
    }
    //-----------------------------------------------------------------------------------

    static void LoadASE()
    {
      Material defaultmaterial = new Material(Shader.Find("Diffuse"));
      defaultmaterial.name = "default";
      CaronteSharp.MeshComplex[] arrMeshes;
      CaronteSharp.MatrixRTS[] arrMatrixRTS;
      CaronteSharp.Tools.LoadASE(ASEpath, out arrMeshes, out arrMatrixRTS);
      
      int numMeshes = arrMeshes.Length;

      for (int i = 0; i < numMeshes; i++)
      {
        CaronteSharp.MeshComplex    mesh = arrMeshes[i];
        CaronteSharp.MatrixRTS matrixRTS = arrMatrixRTS[i];

        GameObject go = new GameObject(/*mesh.name_*/);
        MeshFilter meshfilter = go.AddComponent<MeshFilter>();
        UnityEngine.Mesh unityMesh = new UnityEngine.Mesh();

       // unityMesh.name      = mesh.name_;
        unityMesh.vertices  = mesh.arrPosition_;
        unityMesh.normals   = mesh.arrNormal_;
        unityMesh.uv        = mesh.arrUV_;
        unityMesh.triangles = mesh.arrIndex_;

        unityMesh.RecalculateBounds();

        go.transform.position = matrixRTS.translation_;
        go.transform.rotation = matrixRTS.rotation_;
        go.transform.localScale = matrixRTS.scale_;

        meshfilter.sharedMesh = unityMesh;
        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = defaultmaterial;
      }
    }//
  }
}