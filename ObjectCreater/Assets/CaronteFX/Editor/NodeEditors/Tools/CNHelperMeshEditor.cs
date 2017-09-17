using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;


namespace CaronteFX
{

  public class CNHelperMeshEditor : CommandNodeEditor
  {
    public static Texture  icon_;
    public static Material material_;

    public override Texture TexIcon { get{ return icon_; } }

    new CNHelperMesh Data { get; set; }

    public CNHelperMeshEditor( CNHelperMesh data, CommandNodeEditorState state )
      : base(data, state)
    {
      Data = (CNHelperMesh)data;
    }

    public void CreateHelperMesh()
    {
      EditorUtility.DisplayProgressBar( Data.Name, "Creating helper mesh...", 1.0f);
      MeshSimple helperMesh_un;
      CaronteSharp.Tools.CreateHelperMesh( Data.RandomSeed, Data.Resolution + 2, Data.NBumps, Data.RadiusMin, Data.RadiusMax, out helperMesh_un );

      Mesh helperMesh;
      CRGeometryUtils.CreateMeshFromSimple( helperMesh_un, out helperMesh );

      helperMesh.name = Data.Name;

      GameObject go = new GameObject();
      go.name = Data.Name;
      MeshFilter mf = go.AddComponent<MeshFilter>();

      mf.sharedMesh = helperMesh;

      MeshRenderer mr = go.AddComponent<MeshRenderer>();
      mr.sharedMaterial = material_;

      if (Data.HelperGO != null)
      {
        go.transform.parent        = Data.HelperGO.transform.parent;
        go.transform.localPosition = Data.HelperGO.transform.localPosition;
        go.transform.localRotation = Data.HelperGO.transform.localRotation;
        go.transform.localScale    = Data.HelperGO.transform.localScale;

        Object.DestroyImmediate( Data.HelperGO );
      }

      if (Data.HelperMesh != null)
      {
        Object.DestroyImmediate( Data.HelperMesh );
      }

      Data.HelperMesh = helperMesh;
      Data.HelperGO   = go;

      UnityEditor.Selection.activeGameObject = go;

      EditorUtility.ClearProgressBar();
    }

    private void DrawRandomSeed()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField( "Random seed", (int)Data.RandomSeed );   
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change random seed - " + Data.Name);
        Data.RandomSeed = (uint)value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawResolution()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntSlider( "Resolution", (int)Data.Resolution, 1, 3 );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change resolution - " + Data.Name);
        Data.Resolution = (uint)value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawNBumps()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField( "Bumps number", (int)Data.NBumps);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change bumps number - " + Data.Name);
        Data.NBumps = (uint)value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRadiusMin()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Min. radius", Data.RadiusMin);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change min. radius - " + Data.Name );
        Data.RadiusMin = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRadiusMax()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Max. radius", Data.RadiusMax);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change max. radius - " + Data.Name );
        Data.RadiusMax = value;
        EditorUtility.SetDirty(Data);
      }
    }

    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);

      RenderTitle(isEditable, false, false);

      EditorGUI.BeginDisabledGroup(!isEditable);

      EditorGUILayout.BeginHorizontal();
      if ( GUILayout.Button("Create helper mesh", GUILayout.Height(30f) ) )
      {
        CreateHelperMesh();
      }
 
      if ( GUILayout.Button("Save asset...", GUILayout.Height(30f), GUILayout.Width(100f) ) )
      {
        SaveAssets();
      }
      EditorGUILayout.EndHorizontal();

      EditorGUI.EndDisabledGroup();
      CRGUIUtils.DrawSeparator();

      scroller_ = EditorGUILayout.BeginScrollView(scroller_);

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      float originalLabelwidth = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = 200f;

      DrawRandomSeed();                    
      DrawResolution(); 
      DrawNBumps();   
      DrawRadiusMin();           
      DrawRadiusMax();

      CRGUIUtils.Splitter();
      GUIStyle centerLabel = new GUIStyle(EditorStyles.largeLabel);
      centerLabel.alignment = TextAnchor.MiddleCenter;
      centerLabel.fontStyle = FontStyle.Bold;
      EditorGUILayout.LabelField("Output", centerLabel);

      EditorGUI.BeginDisabledGroup(Data.HelperGO == null);

      EditorGUILayout.Space();
      
      if ( GUILayout.Button("Select helper object") )
      {
        Selection.activeGameObject = Data.HelperGO;
      }
      EditorGUILayout.Space();

      EditorGUI.EndDisabledGroup();

      EditorGUIUtility.labelWidth = originalLabelwidth;

      EditorGUILayout.EndScrollView();

      GUILayout.EndArea();
    } // RenderGUI


    private bool HasHelperMeshReferences()
    {
      if (Data.HelperGO == null)
      {
        return false;
      }

      if ( !CREditorUtils.IsAnyUnsavedMeshInHierarchy(Data.HelperGO) )
      {
        return false;
      }

      return true;
    }

    //SaveAssets
    private void SaveAssets()
    {
      if (!HasHelperMeshReferences())
      {
        EditorUtility.DisplayDialog("CaronteFX - Info", "There is not any mesh to save in assets.", "ok" );
        return;
      }

      CREditorUtils.SaveAnyUnsavedMeshInHierarchy(Data.HelperGO, false);

    } //Save result

  } // class CNWelderView

} //namespace CaronteFX
