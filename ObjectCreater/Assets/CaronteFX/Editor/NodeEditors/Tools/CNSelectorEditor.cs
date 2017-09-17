using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;

namespace CaronteFX
{
  public class CNSelectorEditor : CNMonoFieldEditor 
  {
    public static Texture icon_;
    public override Texture TexIcon { get { return icon_; } }

    string[] selectionModes_ = new string[] { "INSIDE", "INSIDE + BOUNDARY" };
    int   selectionModeIdx_;

    new CNSelector Data { get; set; }

    public CNSelectorEditor( CNSelector data, CommandNodeEditorState state )
      : base(data, state)
    {
      Data = (CNSelector)data;
    }

    private void LoadGUISelectionMode()
    {
      if ( Data.SelectionMode == CNSelector.SELECTION_MODE.INSIDE )
      {
        if ( !Data.FrontierPieces )
        {
          selectionModeIdx_ = 0;
        }
        else
        {
          selectionModeIdx_ = 1;
        }
      }
      else if ( Data.SelectionMode == CNSelector.SELECTION_MODE.OUTSIDE )
      {
        if (!Data.FrontierPieces)
        {
          selectionModeIdx_ = 2;
        }
        else
        {
          selectionModeIdx_ = 3;
        }
      }
    }

    public void ClassifySelection()
    {
      GameObject selectorGO = Data.SelectorGO;
      if (selectorGO == null)
      {  
        EditorUtility.DisplayDialog( "CaronteFX", "A selector geometry is mandatory", "Ok");
        return;
      }

      Mesh selectorMesh = selectorGO.GetMesh();
      if (selectorMesh == null)
      {
        EditorUtility.DisplayDialog( "CaronteFX", "A selector geometry is mandatory", "Ok");
        return;
      } 

      EditorUtility.DisplayProgressBar( Data.Name, "Selecting...", 1.0f);
      GameObject[] arrGOtoClassify = FieldController.GetUnityGameObjects();
       
      Mesh auxSelectorMesh;
      CRGeometryUtils.CreateMeshTransformed(selectorMesh, selectorGO.transform.localToWorldMatrix, out auxSelectorMesh);

      MeshSimple auxCropMesh_un = new MeshSimple();
      auxCropMesh_un.Set( auxSelectorMesh );
      Object.DestroyImmediate(auxSelectorMesh);

      int nGameObjectToClassify = arrGOtoClassify.Length;

      List<GameObject> listGOToClassify         = new List<GameObject>();
      List<MeshSimple> listAuxMeshToClassify_un = new List<MeshSimple>();
      for  (int i = 0; i < nGameObjectToClassify; i++)
      {
        GameObject go       = arrGOtoClassify[i];
        Mesh meshToClassify = go.GetMesh();

        if (meshToClassify != null)
        {
          listGOToClassify.Add( go );

          Mesh auxMeshToClassify;
          CRGeometryUtils.CreateMeshTransformed(meshToClassify, go.transform.localToWorldMatrix, out auxMeshToClassify);
          MeshSimple auxMeshToClassify_un = new MeshSimple();
          auxMeshToClassify_un.Set(auxMeshToClassify);

          Object.DestroyImmediate(auxMeshToClassify);
          listAuxMeshToClassify_un.Add(auxMeshToClassify_un);
        }
      }

      int[] arrIdxClassified;
      CaronteSharp.Tools.SplitInsideOutsideByGeometry(listAuxMeshToClassify_un.ToArray(), auxCropMesh_un, out arrIdxClassified, Data.FrontierPieces, true);
  
      List<GameObject> listGameObjectOutside = new List<GameObject>();
      List<GameObject> listGameObjectInside  = new List<GameObject>();

      for( int i = 0; i < arrIdxClassified.Length; i++ )
      {
        int idxClassified = arrIdxClassified[i];
        
        GameObject go = listGOToClassify[i];

        if (idxClassified == 0)
        {
          listGameObjectOutside.Add( go );
        }
        else if (idxClassified == 1)
        {
          listGameObjectInside.Add( go);
        }
      }

      if ( Data.SelectionMode == CNSelector.SELECTION_MODE.OUTSIDE )
      {
        if (Data.Complementary)
        {
          Selection.objects = listGameObjectInside.ToArray();
        }
        else
        {
          Selection.objects = listGameObjectOutside.ToArray();
        }

      }
      else if ( Data.SelectionMode == CNSelector.SELECTION_MODE.INSIDE )
      {
        if (Data.Complementary)
        {
          Selection.objects = listGameObjectOutside.ToArray(); 
        }
        else
        {
          Selection.objects = listGameObjectInside.ToArray();
        }  
      }

      EditorUtility.ClearProgressBar();
    
    }

    void SetSelectionMode( int valueIdx )
    {
      if ( valueIdx == 0 )
      {
        Data.SelectionMode = CNSelector.SELECTION_MODE.INSIDE;
        Data.FrontierPieces = false;
      }
      else if (valueIdx == 1)
      {
        Data.SelectionMode = CNSelector.SELECTION_MODE.INSIDE;
        Data.FrontierPieces = true;
      }
      else if ( valueIdx == 2 )
      {
        Data.SelectionMode = CNSelector.SELECTION_MODE.OUTSIDE;
        Data.FrontierPieces = false;
      }
      else if ( valueIdx == 3 )
      {
        Data.SelectionMode = CNSelector.SELECTION_MODE.OUTSIDE;
        Data.FrontierPieces = true;
      }
    }

    private void DrawSelectorGO()
    {
      EditorGUI.BeginChangeCheck();
      var value = (GameObject) EditorGUILayout.ObjectField("Selector geometry", Data.SelectorGO, typeof(GameObject), true );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject( Data, "Change selector geometry - " + Data.Name);
        Data.SelectorGO = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawSelectionMode()
    {
      LoadGUISelectionMode();
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Popup( "Selection mode", selectionModeIdx_, selectionModes_ );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject( Data, "Change selection mode - " + Data.Name );
        SetSelectionMode(value);
        EditorUtility.SetDirty( Data );
      } 
    }

    private void DrawComplementarySelection()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle("Complementary selection", Data.Complementary);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject( Data, "Change complementary selection - " + Data.Name );
        Data.Complementary = value;
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

      if ( GUILayout.Button("Select", GUILayout.Height(30f) ) )
      {
        ClassifySelection();
      }

      EditorGUI.EndDisabledGroup();
      CRGUIUtils.DrawSeparator();

      scroller_ = EditorGUILayout.BeginScrollView(scroller_);

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      float originalLabelwidth = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = 200f;

      DrawSelectorGO(); 
      DrawSelectionMode();
      DrawComplementarySelection();

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUIUtility.labelWidth = originalLabelwidth;

      EditorGUILayout.EndScrollView();

      GUILayout.EndArea();
    } // RenderGUI

  }

}
