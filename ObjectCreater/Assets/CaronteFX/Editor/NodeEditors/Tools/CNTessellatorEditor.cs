using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace CaronteFX
{
  public class CNTessellatorEditor : CNMonoFieldEditor 
  {
    public static Texture icon_;

    public override Texture TexIcon { get{ return icon_; } }

    new CNTessellator Data { get; set; }

    public CNTessellatorEditor( CNTessellator data, CommandNodeEditorState state )
      : base(data, state)
    {
      Data = (CNTessellator)data;
    }

    public void Tessellate()
    {
      GameObject[] arrGOtoTessellate = FieldController.GetUnityGameObjects();

      DeleteOldObjects();

      GameObject[] arrGOTessellated;
      Mesh[] arrMeshTessellated;

      EditorUtility.DisplayProgressBar( Data.Name, "Tessellating...", 1.0f);
      CRGeometryUtils.TessellateObjects( arrGOtoTessellate, Data.MaxEdgeDistance, Data.LimitByMeshDimensions, out arrGOTessellated, out arrMeshTessellated );  
      EditorUtility.ClearProgressBar();

      if (cnManager.IsFreeVersion() && arrGOTessellated == null )
      {
         EditorUtility.DisplayDialog("CaronteFX - Free version", "CaronteFX Free version can only tessellate the meshes included in the example scenes and the unity primitives (cube, plane, sphere, etc.).", "Ok");
      }

      if (arrGOTessellated != null)
      {
        List<GameObject> listGameObject = new List<GameObject>();
        listGameObject.AddRange( arrGOTessellated );

        Bounds bounds = CREditorUtils.GetGlobalBoundsWorld( listGameObject );
        GameObject nodeGO = new GameObject( Data.Name );

        nodeGO.transform.position = bounds.center;

        foreach ( GameObject go in arrGOTessellated )
        {
          go.transform.parent = nodeGO.transform;
        }

        Data.NodeGO             = nodeGO;
        Data.ArrTessellatedGO   = arrGOTessellated;
        Data.ArrTessellatedMesh = arrMeshTessellated;

        Selection.activeGameObject = nodeGO;
      }
    }

    public void DeleteOldObjects()
    { 
      GameObject[] arrGOTessellated = Data.ArrTessellatedGO;
      
      if (arrGOTessellated != null)
      {
        foreach( GameObject go in arrGOTessellated )
        {
          if (go != null)
          {
            Object.DestroyImmediate(go);
          }
        }
      }

      Mesh[] arrMeshTessellated     = Data.ArrTessellatedMesh;

      if (arrMeshTessellated != null)
      {
        foreach( Mesh mesh in arrMeshTessellated )
        {
          if (mesh != null)
          {
            Object.DestroyImmediate(mesh);
          }
        }
      }

      GameObject nodeGO = Data.NodeGO;
      if (nodeGO != null)
      {
        Object.DestroyImmediate(nodeGO);
      }

    }

    private void DrawMaxEdgeLength()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Max edge length", Data.MaxEdgeDistance );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change max edge length - " + Data.Name);
        Data.MaxEdgeDistance = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawLimitByMeshDimensions()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle( "Limit to mesh dimensions", Data.LimitByMeshDimensions );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change limit to mesh dimensions - " + Data.Name );
        Data.LimitByMeshDimensions = value;
        EditorUtility.SetDirty(Data);
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
      if ( GUILayout.Button("Tessellate", GUILayout.Height(30f) ) )
      {
        Tessellate();
      }

      if ( GUILayout.Button("Save assets...", GUILayout.Height(30f), GUILayout.Width(100f) ) )
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

      DrawMaxEdgeLength();
      DrawLimitByMeshDimensions();

      EditorGUILayout.Space();

      CRGUIUtils.Splitter();

      GUIStyle centerLabel = new GUIStyle(EditorStyles.largeLabel);
      centerLabel.alignment = TextAnchor.MiddleCenter;
      centerLabel.fontStyle = FontStyle.Bold;
      EditorGUILayout.LabelField("Output", centerLabel);

      EditorGUI.BeginDisabledGroup(Data.NodeGO == null);

      EditorGUILayout.Space();

      if ( GUILayout.Button("Select tessellated objects") )
      {
        Selection.activeGameObject = Data.NodeGO;
      }
      EditorGUILayout.Space();

      EditorGUI.EndDisabledGroup();

      EditorGUIUtility.labelWidth = originalLabelwidth;

      EditorGUILayout.EndScrollView();

      GUILayout.EndArea();
    } // RenderGUI

    private bool HasUnsavedTessellatorReferences()
    {
      if (Data.NodeGO == null)
      {
        return false;
      }

      if ( !CREditorUtils.IsAnyUnsavedMeshInHierarchy(Data.NodeGO) )
      {
        return false;
      }

      return true;
    }

    //SaveAssets
    private void SaveAssets()
    {
      if (!HasUnsavedTessellatorReferences())
      {
        EditorUtility.DisplayDialog("CaronteFX - Info", "There is not any mesh to save in assets.", "ok" );
        return;
      }

      CREditorUtils.SaveAnyUnsavedMeshInHierarchy(Data.NodeGO, false);

    } //Save tessellator result
  }

}
