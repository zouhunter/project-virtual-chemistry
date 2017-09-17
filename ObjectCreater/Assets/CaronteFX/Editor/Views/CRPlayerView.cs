using UnityEngine;
using UnityEditor;
using System.Collections;
using CaronteSharp;

namespace CaronteFX
{
  public class CRPlayerView : IView
  { 
    const int nIterations = 16;

    GUILayoutOption buttonLayoutWidth  = GUILayout.Width(40f);
    GUILayoutOption buttonLayoutHeight = GUILayout.Height(25f);
    GUIStyle        maxTimeStyle_;
    GUIStyle        buttonStyle_;

    CRTimeSliderView  timeSliderView_;
    CRPlayer          player_;

    public static Texture first_;
    public static Texture last_;
    public static Texture prev_;
    public static Texture next_;
    public static Texture play_;
    public static Texture pause_;
    public static Texture stop_;
    public static Texture loop_;
    public static Texture rec_;

    public CRPlayerView( CRPlayer player )
    {
      timeSliderView_         = new CRTimeSliderView(player);
      player_                 = player;

      maxTimeStyle_           = new GUIStyle(EditorStyles.label);
      maxTimeStyle_.alignment = TextAnchor.MiddleRight;

      buttonStyle_ = new GUIStyle(EditorStyles.miniButton);
      buttonStyle_.onActive  = buttonStyle_.focused;
      buttonStyle_.onNormal  = buttonStyle_.focused;
      buttonStyle_.onFocused = buttonStyle_.focused;
      buttonStyle_.active    = buttonStyle_.focused;
    }
    //----------------------------------------------------------------------------------
    public void RenderGUI( Rect area, bool isEditable )
    {
      GUILayout.BeginArea( area );

      EditorGUILayout.Space();

      DrawTimeLine();

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUILayout.BeginHorizontal();

      bool isSimulating = player_.IsSimulating;
      bool isReplaying  = player_.IsReplaying;
      bool isSimulatingOrReplaying = isSimulating || isReplaying;

      GUILayout.Space(40f);

      #region Player
      EditorGUI.BeginDisabledGroup( !isSimulatingOrReplaying );
      
      EditorGUI.BeginDisabledGroup(player_.IsSimulating);   
      if (GUILayout.Button(new GUIContent("",first_), EditorStyles.miniButton, buttonLayoutWidth, buttonLayoutHeight))
      {
        player_.frw();
      }
      if (GUILayout.Button(new GUIContent("",prev_), EditorStyles.miniButtonLeft, buttonLayoutWidth, buttonLayoutHeight))
      {
        player_.rw();
      }
      EditorGUI.EndDisabledGroup();
 
      EditorGUI.EndDisabledGroup();


      if (player_.IsSimulating)
      {
        EditorGUI.BeginDisabledGroup(player_.StopRequested);
        if (GUILayout.Button(new GUIContent("", stop_), EditorStyles.miniButtonMid, buttonLayoutWidth, buttonLayoutHeight))
        {
          player_.stop();
        }
        EditorGUI.EndDisabledGroup();
      }
      else if (player_.IsReplaying)
      {
        if (player_.IsPause && !player_.UserPlaying)
        {
          if (GUILayout.Button(new GUIContent("", play_), EditorStyles.miniButtonMid, buttonLayoutWidth, buttonLayoutHeight))
          {
            player_.play();
          }
        }
        else
        {
          if (GUILayout.Button(new GUIContent("", pause_), EditorStyles.miniButtonMid, buttonLayoutWidth, buttonLayoutHeight))
          {
            player_.pause();
          }
        }
      }

      EditorGUI.BeginDisabledGroup( !isSimulatingOrReplaying );
      EditorGUI.BeginDisabledGroup(player_.IsSimulating);
      if (GUILayout.Button(new GUIContent("",next_), EditorStyles.miniButtonRight, buttonLayoutWidth, buttonLayoutHeight))
      {
        player_.fw();
      }

      if (GUILayout.Button(new GUIContent("", last_), EditorStyles.miniButton, buttonLayoutWidth, buttonLayoutHeight))
      {
        player_.ffw();
      }

      GUILayout.Space(10f);

      EditorGUI.BeginChangeCheck();
      player_.Loop = GUILayout.Toggle( player_.Loop, new GUIContent("", loop_),  buttonStyle_, buttonLayoutWidth, buttonLayoutHeight );
      if (EditorGUI.EndChangeCheck())
      {
        player_.ResetUserPlaying();
      }

      EditorGUI.EndDisabledGroup();

      #endregion

      GUILayout.Space(47f);

      #region Statistics
      GUILayout.Label( "Time: ", EditorStyles.miniLabel, GUILayout.Width(50f) );
      GUILayout.Label(player_.Time.ToString("F2"), EditorStyles.miniLabel, GUILayout.Width(50f) );
      GUILayout.Label( "of ", EditorStyles.miniLabel);
      GUILayout.FlexibleSpace();
      GUILayout.Label(player_.MaxTimeString, EditorStyles.miniLabel );
      #endregion
  
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.Space();
      EditorGUILayout.Space();
      CRGUIUtils.Splitter();
      EditorGUILayout.Space();

      EditorGUILayout.BeginHorizontal();

      GUILayout.Space(10f);

      GUIStyle style = new GUIStyle(EditorStyles.miniButton);
      style.alignment = TextAnchor.MiddleLeft;

      EditorGUI.BeginDisabledGroup(isSimulating);
      if (GUILayout.Button(new GUIContent(" Bake simulation", rec_), style, GUILayout.Width(135f), GUILayout.Height(25f) ) )
      {
        if (!CRBakeSimulationMenu.IsOpen)
        {
          CRBakeSimulationMenu bakeSimMenu = ScriptableObject.CreateInstance<CRBakeSimulationMenu>();
          bakeSimMenu.titleContent = new GUIContent("CaronteFX - Bake Simulation Menu");
        }
        CRBakeSimulationMenu.Instance.ShowUtility();
        CRBakeFrameMenu.CloseIfOpen();
      }

      if (GUILayout.Button(new GUIContent("Bake current frame", rec_), style, GUILayout.Width(135f), GUILayout.Height(25f) ) )
      {
        if (!CRBakeFrameMenu.IsOpen)
        {
          CRBakeFrameMenu bakeFrameMenu = ScriptableObject.CreateInstance<CRBakeFrameMenu>();
          bakeFrameMenu.titleContent = new GUIContent("CaronteFX - Bake Current Frame Menu");
        }
        CRBakeFrameMenu.Instance.ShowUtility();
        CRBakeSimulationMenu.CloseIfOpen();
      }
      EditorGUI.EndDisabledGroup();

      GUILayout.Space(10f);
      if (GUILayout.Button("Change to edit mode", EditorStyles.miniButton, GUILayout.Width(125f), GUILayout.Height(25f) ))
      {
        CRPlayerWindow.CloseIfOpen();
      }
      GUILayout.Space(10f);

      DrawProgressBox();

      GUILayout.Space(6f);

      EditorGUILayout.EndHorizontal();

      GUILayout.EndArea();
    }
    //----------------------------------------------------------------------------------
    private void DrawProgressBox()
    {
      Rect progressRect = GUILayoutUtility.GetRect( 105f, 25f );
      Rect smallProgressRect = new Rect( progressRect.xMin, progressRect.yMin + 7.5f, progressRect.width, progressRect.height - 10f );
      GUI.Box( smallProgressRect, "");

      float singleWidth = (smallProgressRect.width / 15f);
      int nIteration = player_.CurrentIteration;

      Color currentColor = GUI.color;
      if (player_.IsReplaying)
      {
        GUI.color = Color.blue;
      }
      else
      {
        GUI.color = Color.green;
      }
      
      for (int i = 0; i < nIteration; i++)
      {
        Rect singleRect = new Rect( smallProgressRect.xMin + (i * singleWidth), smallProgressRect.yMin, singleWidth, smallProgressRect.height );
        GUI.Box(singleRect, "x");
      }

      GUI.color = currentColor;
    }
    //----------------------------------------------------------------------------------
    public void DrawTimeLine()
    {
      GUILayout.BeginHorizontal(GUI.skin.box);
      timeSliderView_.RenderGUI();
      GUILayout.EndHorizontal();
    }

  } // class CRPlayerView...

} //namespace Caronte...
