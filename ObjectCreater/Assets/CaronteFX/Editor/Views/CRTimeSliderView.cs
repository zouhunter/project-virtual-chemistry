using UnityEngine;
using UnityEditor;
using System.Collections;
using CaronteSharp;


namespace CaronteFX
{
  public class CRTimeSliderView
  {
    GUIStyle maxFramesStyle_;
    CRPlayer player_;

    int frameRequest_;
    int lastFrameRequest_;

    public CRTimeSliderView( CRPlayer player )
    {
      player_ = player;
      maxFramesStyle_ = new GUIStyle(EditorStyles.label);
      maxFramesStyle_.alignment = TextAnchor.MiddleRight;
      lastFrameRequest_ = -1;
    }

    public void RenderGUI()
    {
        EditorGUI.BeginDisabledGroup( player_.IsSimulating );
        EditorGUI.BeginChangeCheck();
        frameRequest_ = EditorGUILayout.IntSlider( player_.Frame, 0, player_.MaxFrames, 
                                                   GUILayout.ExpandWidth(true), GUILayout.Height(20f) );
        if (EditorGUI.EndChangeCheck() && ( lastFrameRequest_ != frameRequest_ ) )
        {
          lastFrameRequest_ = frameRequest_;
          player_.SetFrame(frameRequest_, true);
        }
        GUILayout.Space(5f);
        EditorGUILayout.LabelField("of ", GUILayout.Width(20f), GUILayout.ExpandWidth(false));
        EditorGUILayout.LabelField(player_.MaxFramesString, maxFramesStyle_, GUILayout.Width(65f) ); 
 
        EditorGUI.EndDisabledGroup();
    }
  }
}
