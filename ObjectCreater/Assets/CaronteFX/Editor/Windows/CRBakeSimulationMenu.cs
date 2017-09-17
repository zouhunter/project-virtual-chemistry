using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


namespace CaronteFX
{
  public class CRBakeSimulationMenu : CRWindow<CRBakeSimulationMenu>
  {
    CRBakerGOAnim   simulationBaker_;
    CREntityManager entityManager_;

    float width  = 350f;
    float height = 440f;

    Vector2    scroller_;

    List<CNBody> listBodyNode_;
    BitArray bitArrNeedsBaking_;
    BitArray bitArrNeedsCollapsing_;

    CNManager Controller { get; set; }

    int StartFrame
    {
      get
      {
        return simulationBaker_.FrameStart;
      }
    }
   
    int EndFrame
    {
      get
      {
        return simulationBaker_.FrameEnd;
      }
    }

    int MaxFrames
    {
      get { return simulationBaker_.MaxFrames; }
    }

    void OnEnable()
    {
      Instance = this;

      this.minSize = new Vector2(width, height);
      this.maxSize = new Vector2(width, height);

      Controller = CNManager.Instance;
      Controller.Player.pause();

      simulationBaker_ = Controller.SimulationBaker;

      listBodyNode_          = simulationBaker_.listBodyNode_;
      bitArrNeedsBaking_     = simulationBaker_.bitArrNeedsBaking_;
      bitArrNeedsCollapsing_ = simulationBaker_.bitArrNeedsCollapsing_;
    }

    void SetStartEndFrames(int startFrame, int endFrame)
    {
      simulationBaker_.FrameStart = Mathf.Min(startFrame, endFrame - 1 );
      simulationBaker_.FrameEnd   = Mathf.Max(endFrame,   startFrame + 1 );
    }

    void OnGUI()
    {
      Rect nodesArea    = new Rect( 5, 5, width - 10, ( (height - 10) / 2 ) ); 
      Rect nodesAreaBox = new Rect( nodesArea.xMin, nodesArea.yMin, nodesArea.width + 1, nodesArea.height + 1 );
      GUI.Box(nodesAreaBox, "");

      GUILayout.BeginArea(nodesArea);
      GUILayout.BeginHorizontal();

      GUIStyle styleTitle = new GUIStyle(GUI.skin.label);
      styleTitle.fontStyle = FontStyle.Bold;

      GUILayout.Label( "Nodes to bake:", styleTitle);
      GUILayout.EndHorizontal();

      CRGUIUtils.DrawSeparator();

      GUILayout.BeginHorizontal();

      int bodyNodeCount = listBodyNode_.Count;
      EditorGUILayout.BeginHorizontal();
      DrawToggleMixed( bodyNodeCount );
      Rect rect = GUILayoutUtility.GetLastRect();
      GUILayout.Space( 65f );
      EditorGUILayout.LabelField("Collapse");
      EditorGUILayout.EndHorizontal();
      GUILayout.FlexibleSpace();

      GUILayout.EndHorizontal();
      
      Rect boxRect      = new Rect( nodesAreaBox.xMin - 5, rect.yMax, nodesAreaBox.width - 70f, nodesAreaBox.yMax - rect.yMax );
      Rect collapseRect = new Rect( boxRect.xMax, boxRect.yMin, 70f, boxRect.height );

      GUI.Box(boxRect, "");
      GUI.Box(collapseRect, "");

      scroller_ = GUILayout.BeginScrollView(scroller_);
      
      for (int i = 0; i < bodyNodeCount; i++)
      {
        GUILayout.BeginHorizontal();
        CNBody bodyNode = listBodyNode_[i];
        string name = bodyNode.Name;

        bitArrNeedsBaking_[i] = EditorGUILayout.ToggleLeft(name, bitArrNeedsBaking_[i], GUILayout.Width(250f) );
        GUILayout.Space(40f);

        bool isRigid    = bodyNode is CNRigidbody;
        bool isAnimated = bodyNode is CNAnimatedbody;

        if (isRigid && !isAnimated)
        {
          bitArrNeedsCollapsing_[i] = EditorGUILayout.Toggle(bitArrNeedsCollapsing_[i]);
        }
        
        GUILayout.EndHorizontal();
      }

      EditorGUILayout.EndScrollView();

      GUILayout.EndArea();
      GUILayout.FlexibleSpace();

      Rect optionsArea = new Rect( 5, nodesArea.yMax, width - 10, (height - 10) / 2 );
      GUILayout.BeginArea(optionsArea);
      
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUILayout.LabelField("Frame Start : " + StartFrame );
      EditorGUILayout.LabelField("Frame End   : " + EndFrame );

      float start = StartFrame;
      float end   = EndFrame;
      EditorGUILayout.MinMaxSlider( new GUIContent("Frames:"), ref start, ref end, 0, MaxFrames );

      SetStartEndFrames( (int)start, (int)end );

      EditorGUILayout.Space();

      GUILayout.BeginHorizontal();
      simulationBaker_.sbCompression_ = EditorGUILayout.Toggle("Vertex compression", simulationBaker_.sbCompression_);
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      simulationBaker_.sbTangents_ = EditorGUILayout.Toggle("Save tangents", simulationBaker_.sbTangents_);
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      simulationBaker_.bakeEvents_ = EditorGUILayout.Toggle("Save events", simulationBaker_.bakeEvents_);
      GUILayout.EndHorizontal();

      EditorGUILayout.Space();

      GUILayout.BeginHorizontal();
      simulationBaker_.animationName_ = EditorGUILayout.TextField("Animation name", simulationBaker_.animationName_);
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      simulationBaker_.bakeObjectName_ = EditorGUILayout.TextField("Root gameobject name", simulationBaker_.bakeObjectName_ );
      GUILayout.EndHorizontal();

      EditorGUILayout.Space();
      EditorGUILayout.Space();
      if (GUILayout.Button("Bake!"))
      {
        EditorApplication.delayCall += () => 
        { 
          simulationBaker_.BakeSimulationAsAnim(); 
          Close();
        };
      }

      GUILayout.BeginHorizontal();

      GUILayout.EndHorizontal();

      GUILayout.EndArea();
    }

   void DrawToggleMixed( int bodyNodeCount )
   {
     EditorGUI.BeginChangeCheck();
     if (bodyNodeCount > 0)
     {
       bool value = bitArrNeedsBaking_[0];
       for (int i = 1; i < bodyNodeCount; ++i)
       {
         if ( value != bitArrNeedsBaking_[i] )
         {
           EditorGUI.showMixedValue = true;
           break;
         }
       }
       simulationBaker_.bakeAllNodes_ = value;
     }

     simulationBaker_.bakeAllNodes_ = EditorGUILayout.ToggleLeft("All", simulationBaker_.bakeAllNodes_);
     EditorGUI.showMixedValue = false;
     if (EditorGUI.EndChangeCheck())
     {
       for (int i = 0; i < bodyNodeCount; ++i)
       {
         bitArrNeedsBaking_[i] = simulationBaker_.bakeAllNodes_;
       }
     }
     EditorGUI.showMixedValue = false;
   }
  }
}

