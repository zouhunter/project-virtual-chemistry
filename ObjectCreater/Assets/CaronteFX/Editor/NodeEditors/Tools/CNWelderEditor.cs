using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;


namespace CaronteFX
{

  public class CNWelderEditor : CNMonoFieldEditor
  {
    public static Texture icon_;

    public override Texture TexIcon { get{ return icon_; } }

    new CNWelder Data { get; set; }

    public CNWelderEditor( CNWelder data, CommandNodeEditorState state )
      : base(data, state)
    {
      Data = (CNWelder)data;
    }

    public void Weld()
    {
      GameObject[] arrGOtoWeld = FieldController.GetUnityGameObjects();

      int arrGOtoWeld_size = arrGOtoWeld.Length;
      if (arrGOtoWeld_size == 0)
      {
        EditorUtility.DisplayDialog( "CaronteFX", "Input objects are mandatory", "Ok");
        return;
      }

      GameObject[] arrWeldedObject;
      Mesh[]       arrWeldedMesh;
      int          interiorMaterialIdx;

      EditorUtility.DisplayProgressBar(Data.Name, "Welding...", 1.0f);
      CRGeometryUtils.WeldObjects( arrGOtoWeld, Data.Name, null, out arrWeldedObject, out arrWeldedMesh, out interiorMaterialIdx );
      EditorUtility.ClearProgressBar();
  
      if (cnManager.IsFreeVersion() && arrWeldedObject == null )
      {
         EditorUtility.DisplayDialog("CaronteFX - Free version", "CaronteFX Free version can only weld the meshes included in the example scenes and the unity primitives (cube, plane, sphere, etc.).", "Ok");
      }

      if (arrWeldedObject != null)
      {
        List<GameObject> listWeldedObjects = new List<GameObject>();
        listWeldedObjects.AddRange(arrWeldedObject);

        Bounds bounds = CREditorUtils.GetGlobalBoundsWorld(listWeldedObjects);

        GameObject go = new GameObject( Data.Name + "_output" );
        go.transform.position = bounds.center;

        foreach( GameObject weldedGO in listWeldedObjects )
        {
          weldedGO.transform.parent = go.transform;
        }

        Data.WeldGameObject = go;

        UnityEditor.Selection.activeGameObject = go;
      }
    }

    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);

      RenderTitle(isEditable, false, false);

      EditorGUI.BeginDisabledGroup(!isEditable);
      RenderFieldObjects( "Objects", FieldController, true, true, CNFieldWindow.Type.normal );

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUILayout.BeginHorizontal();
      if ( GUILayout.Button("Weld", GUILayout.Height(30f) ) )
      {
        Weld();
      }
      if ( GUILayout.Button("Save asset...", GUILayout.Height(30f), GUILayout.Width(100f) ) )
      {
        SaveWeldResult();
      }
      EditorGUILayout.EndHorizontal();

      EditorGUI.EndDisabledGroup();
      CRGUIUtils.DrawSeparator();

      scroller_ = EditorGUILayout.BeginScrollView(scroller_);

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      float originalLabelwidth = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = 200f;
    
      CRGUIUtils.Splitter();
      GUIStyle centerLabel = new GUIStyle(EditorStyles.largeLabel);
      centerLabel.alignment = TextAnchor.MiddleCenter;
      centerLabel.fontStyle = FontStyle.Bold;
      EditorGUILayout.LabelField("Output", centerLabel);

      EditorGUI.BeginDisabledGroup(Data.WeldGameObject == null);

      EditorGUILayout.Space();
      
      if ( GUILayout.Button("Select welded object") )
      {
        Selection.activeGameObject = Data.WeldGameObject;
      }
      EditorGUILayout.Space();

      EditorGUI.EndDisabledGroup();

      EditorGUIUtility.labelWidth = originalLabelwidth;

      EditorGUILayout.EndScrollView();

      GUILayout.EndArea();
    } // RenderGUI


    public bool HasUnsavedWeldReferences()
    {
      if (Data.WeldGameObject == null)
      {
        return false;
      }

      if (CREditorUtils.IsAnyUnsavedMeshInHierarchy(Data.WeldGameObject) )
      {   
        return true;
      }
      else
      {
        return false;
      }    
    }

    public void SaveWeldResult()
    {
      if (!HasUnsavedWeldReferences())
      {
        EditorUtility.DisplayDialog("CaronteFX - Info", "There is not any mesh to save in assets.", "ok" );
        return;
      }

      CREditorUtils.SaveAnyUnsavedMeshInHierarchy(Data.WeldGameObject, false);

    } //Save weld result


  } // class CNWelderView

} //namespace CaronteFX
