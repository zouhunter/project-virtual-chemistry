using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


namespace CaronteFX
{
  public class CRAboutWindow: CRWindow<CRAboutWindow>
  {
    static float width_    = 380f;
    static float height_   = 160f;
    
    string versionString_;

    public static CRAboutWindow ShowWindow()
    {
      if (Instance == null)
      {
        Instance = (CRAboutWindow)EditorWindow.GetWindow(typeof(CRAboutWindow), true, "About CaronteFX");
      }
    
      Instance.minSize = new Vector2(width_, height_);
      Instance.maxSize = new Vector2(width_, height_);
    
      Instance.Focus();    
      return Instance;
    }

    void OnEnable()
    {
      string version = CaronteSharp.Caronte.GetNativeDllVersion();
      string freeVersion = CaronteSharp.Caronte.IsFreeVersion() ? " FREE" : string.Empty;

      versionString_ = "Version: " + version + freeVersion;
    }


    void OnLostFocus()
    {
      Close();
    }

    public void OnGUI()
    {
      GUI.DrawTexture( new Rect(-20f, -20f, 200f, 200f), CRManagerEditor.ic_logoCaronte_ );
      GUILayout.BeginArea( new Rect( 180f, 5f, 195f, 150f ) );
      
      GUILayout.FlexibleSpace();

      GUILayout.Label( new GUIContent("Powered by Caronte physics engine."), EditorStyles.miniLabel );
      GUILayout.Label( new GUIContent("(c) 2015 Next Limit Technologies."), EditorStyles.miniLabel );
      GUILayout.Label( new GUIContent( versionString_ ), EditorStyles.miniLabel );

      EditorGUILayout.Space();

      GUILayout.EndArea();
    }

  }
}
