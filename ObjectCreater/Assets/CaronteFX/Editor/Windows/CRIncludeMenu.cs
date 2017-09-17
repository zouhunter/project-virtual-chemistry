using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace CaronteFX
{
  public class CRIncludeMenu : CRWindow<CRIncludeMenu>
  {

    float width_  = 350f;
    float height_ = 350f;
    
    Vector2 scrollPos_;

    List< Tuple3<Caronte_Fx, int, bool> > listCaronteFx_  = new List< Tuple3<Caronte_Fx, int, bool> >();
    bool[]  arrEffectsToInclude;

    CNManager Controller
    {
      get;
      set;
    }

    void OnEnable()
    {
      Instance = this;
      minSize = new Vector2(width_, height_);
      maxSize = new Vector2(width_, height_);

      Controller = CNManager.Instance;

      BuildEffectList();
    }

    /// <summary>
    /// Builds a list with all the scene fx and their status(included/not included)
    /// </summary>
    void BuildEffectList()
    {
      List<Tuple2<Caronte_Fx, int>> listCaronteFx;
      CREditorUtils.GetCaronteFxsRelations(Controller.FxData, out listCaronteFx);

      for (int i = 0; i < listCaronteFx.Count; i++)
      {
        Caronte_Fx fx     = listCaronteFx[i].First;
        int depth         = listCaronteFx[i].Second;
        bool alreadyAdded = Controller.IsEffectIncluded(fx);

        listCaronteFx_.Add( Tuple3.New( fx, depth, alreadyAdded ) );
      }
      arrEffectsToInclude = new bool[listCaronteFx_.Count];
      for (int i = 0; i < arrEffectsToInclude.Length; i++)
      {
        arrEffectsToInclude[i] = listCaronteFx_[i].Third;
      }   
    }

    void OnGUI()
    {
      Rect effectsArea = new Rect( 5, 5, width_ - 10, (height_ - 10) * 0.9f );

      GUILayout.BeginArea(effectsArea, GUI.skin.box);
      EditorGUILayout.BeginHorizontal();
      {
        GUIStyle styleTitle = new GUIStyle(GUI.skin.label);
        styleTitle.fontStyle = FontStyle.Bold;
        GUILayout.Label("Child/Brother includible FXs:", styleTitle);   
      }
      EditorGUILayout.EndHorizontal();

      CRGUIUtils.Splitter();
      EditorGUILayout.Space();


      scrollPos_ = GUILayout.BeginScrollView(scrollPos_);
      {
        for (int i = 0; i < listCaronteFx_.Count; i++)
        {
          Caronte_Fx fx = listCaronteFx_[i].First;
          if (fx != Controller.FxData)
          {
            GUILayout.BeginHorizontal();
            {
              string name = fx.name;
              arrEffectsToInclude[i] = EditorGUILayout.ToggleLeft(name, arrEffectsToInclude[i]);
            }
            GUILayout.EndHorizontal();
          }
        }
      }
      GUILayout.EndScrollView();

      GUILayout.EndArea();
      Rect buttonsArea = new Rect( effectsArea.xMin, effectsArea.yMax, width_ - 10, (height_ - 10) * 0.1f );
      
      GUILayout.BeginArea(buttonsArea);
      {
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        {
          if (GUILayout.Button("Ok"))
          {
            Close();
            ModifyInclusions();    
          }
          if (GUILayout.Button("Cancel"))
          {
            Close();
          }
        }
        GUILayout.EndHorizontal();
      }
      GUILayout.EndArea();
    }

    private void ModifyInclusions()
    {

      List<GameObject> listGameObjectFxToInclude = new List<GameObject>();
      List<GameObject> listGameObjectFxToDeinclude = new List<GameObject>();
      for (int i = 0; i < listCaronteFx_.Count; i++)
      {
        bool wasIncluded = listCaronteFx_[i].Third;
        bool hasToBeIncluded = arrEffectsToInclude[i];

        if (wasIncluded != hasToBeIncluded)
        {
          Caronte_Fx fx = listCaronteFx_[i].First;
          if (hasToBeIncluded)
          {
            listGameObjectFxToInclude.Add(fx.gameObject);
          }
          else
          {
            listGameObjectFxToDeinclude.Add(fx.gameObject);
          }
        }
      }

      //Controller.DettachCaronteFxGameObjects(listGameObjectFxToDeinclude);
      Controller.AddCaronteFxGameObjects(listGameObjectFxToInclude);
      
    }
  }
}

