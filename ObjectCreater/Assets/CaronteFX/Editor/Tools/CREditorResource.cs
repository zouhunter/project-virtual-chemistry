using UnityEngine;
using UnityEditor;

using System.IO;


namespace CaronteFX
{
  public struct CREditorResource 
  {
    private static string skinsDir = null;
    private static string textureEditorDir = null;
    private static string materialEditorDir = null;
    private static string animationControllerDir = null;
        
    public static GUISkin LoadSkin(string name)
    {
      if (skinsDir == null)
      {
        skinsDir = GetDir(name + ".guiskin");
      }
      return AssetDatabase.LoadAssetAtPath(
        string.Format("{0}{1}.guiskin", skinsDir, name), typeof(GUISkin)) as GUISkin;
    }

    public static Texture LoadEditorTexture(string name)
    {
      if (textureEditorDir == null)
      {
        textureEditorDir = GetDir(name + ".png");
      }
      return AssetDatabase.LoadAssetAtPath(
        string.Format("{0}{1}.png", textureEditorDir, name), typeof(Texture)) as Texture;
    }


    public static Material LoadEditorMaterial(string name)
    {
      if (materialEditorDir == null)
      {
        materialEditorDir = GetDir(name + ".mat");
      }
      return AssetDatabase.LoadAssetAtPath(
        string.Format("{0}{1}.mat", materialEditorDir, name), typeof(Material)) as Material;
    }

    public static RuntimeAnimatorController LoadEditorAnimationController(string name)
    {
      if (animationControllerDir == null)
      {
        animationControllerDir = GetDir(name +".controller");
      }
      return AssetDatabase.LoadAssetAtPath(
          string.Format("{0}{1}.controller", animationControllerDir, name), typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
    }

    public static string GetDir(string filename)
    {
      string ret = "";
      string path = FindPath(filename, "Assets", true);
      if( path != null )
      {
        int lastInd = path.LastIndexOf('/');
        if(lastInd != -1)
        {
          ret = path.Substring(0, lastInd + 1);
        }
      }
      return ret;
    }

    public static string FindPath(string filename, string dir, bool recursive)
    {
      string[] paths = Directory.GetFiles(dir, filename, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

      return paths.Length > 0 ? paths[0].Replace('\\', '/') : null;
    }
  }// class CREditorResource 
}// namespace Caronte

