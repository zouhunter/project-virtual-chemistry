using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


namespace CaronteFX
{
    public class CRMaterialSubstituterEditor : CRWindow<CRMaterialSubstituterEditor>
    {
      private GameObject rootObject_;
      private Material   originalMaterial_;
      private Material   newMaterial_;

      float width  = 350f;
      float height = 110f;

      void OnEnable()
      {
        Instance = this;

        this.minSize = new Vector2(width, height);
        this.maxSize = new Vector2(width, height);
      }

      void OnGUI()
      {
        EditorGUILayout.Space();
        rootObject_ = (GameObject)EditorGUILayout.ObjectField( "Root object", rootObject_, typeof(GameObject), true);
        EditorGUILayout.Space();

        originalMaterial_ = (Material)EditorGUILayout.ObjectField( "Original material", originalMaterial_, typeof(Material), true );
        newMaterial_      = (Material)EditorGUILayout.ObjectField( "New material", newMaterial_, typeof(Material), true );

        bool isValid = rootObject_ != null &&  newMaterial_ != null;

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(!isValid);

        if ( GUILayout.Button("Substitute material in hierarchy") )
        {
          DoSubstitution();
        }

        EditorGUI.EndDisabledGroup();
	    }

      void DoSubstitution () 
      {
        Undo.RecordObject(rootObject_, "Substitute material in hierarchy");
        
        List<GameObject> listGameObject = new List<GameObject>();
        listGameObject.Add( rootObject_ );
        GameObject[] arrGameObject = CREditorUtils.GetAllChildObjectsWithGeometry(rootObject_, true);
        listGameObject.AddRange(arrGameObject);

        foreach( GameObject go in listGameObject )
        {
          Renderer rn = go.GetComponent<Renderer>();
          if (rn != null)
          {
            Material[] arrMaterial = rn.sharedMaterials;
            bool modifiedArray= false;
            for (int i = 0; i < arrMaterial.Length; i++)
            {
              Material mat = arrMaterial[i];
              if (mat == originalMaterial_)
              {
                modifiedArray = true;
                arrMaterial[i] = newMaterial_;
              }
            }

            if (modifiedArray)
            {
              Undo.RecordObject(rn, "Change materials");
              rn.sharedMaterials = arrMaterial;
            }
          }     
        }
        Undo.SetCurrentGroupName("Substitute material in hierarchy");
        Undo.CollapseUndoOperations( Undo.GetCurrentGroup() );
	    }
  }
}

