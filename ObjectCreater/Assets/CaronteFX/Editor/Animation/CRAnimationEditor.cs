using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CaronteFX
{
  [CustomEditor(typeof(CRAnimation))]
  public class CRAnimationEditor : Editor
  {
    CRAnimation ac_;

    float  editorFrame_;
    bool   previewInEditor_;

    void OnEnable()
    {
      ac_ = (CRAnimation)target;

      editorFrame_     = ac_.lastReadFrame_;
      previewInEditor_ = false;
    }

    public override void OnInspectorGUI()
    {
      serializedObject.Update();
      if ( ac_.activeAnimation != ac_.animationLastLoaded_  )
      {
        previewInEditor_ = false;
        ac_.CloseAnimation();
      }

      DrawDefaultInspector();
      CRGUIUtils.Splitter();

      /*
      EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
      EditorGUI.BeginChangeCheck();
      previewInEditor_ = EditorGUILayout.Toggle("Preview", previewInEditor_);
      if ( EditorGUI.EndChangeCheck() )
      {
        AnimationPreview( previewInEditor_ );
      }
      EditorGUI.EndDisabledGroup();
      */


      bool isPlayingOrPreviewInEditor = EditorApplication.isPlayingOrWillChangePlaymode || previewInEditor_;
      EditorGUI.BeginDisabledGroup( !isPlayingOrPreviewInEditor );
      EditorGUI.BeginChangeCheck();

      editorFrame_ = Mathf.Clamp(ac_.lastReadFrame_, 0, ac_.lastFrame_);
      editorFrame_ = EditorGUILayout.Slider(new GUIContent("Frame"), editorFrame_, ac_.firstFrame_, ac_.lastFrame_);
      if (EditorGUI.EndChangeCheck() && isPlayingOrPreviewInEditor )
      {   
        ac_.SetFrame(editorFrame_);      
        SceneView.RepaintAll();
      }

      EditorGUI.EndDisabledGroup();
      EditorGUILayout.LabelField(new GUIContent("Time"),             new GUIContent(ac_.time_.ToString("F3")) );
      EditorGUILayout.LabelField(new GUIContent("Frame Count"),      new GUIContent(ac_.frameCount_.ToString()) );
      EditorGUILayout.LabelField(new GUIContent("FPS"),              new GUIContent(ac_.fps_.ToString()) );
      EditorGUILayout.LabelField(new GUIContent("Animation Length"), new GUIContent(ac_.animationLength_.ToString()) );
      CRGUIUtils.Splitter();

      EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);

      CRGUIUtils.Splitter();
      if ( GUILayout.Button(new GUIContent("Record screenshots in play mode") ) )
      {
        MakeCameraScreenshots();
      }

      if ( GUILayout.Button(new GUIContent("Remove camera recorder component") ) ) 
      {
        RemoveCameraRecorder();
      }

      EditorGUI.EndDisabledGroup();

      Repaint();
    }

    private void AnimationPreview( bool isPreview )
    {
      if ( isPreview )
      {
        ac_.LoadAnimation(true);
        ac_.SetFrame(editorFrame_);
      }
      else
      {
        ac_.CloseAnimation();
      }
    }

    private void MakeCameraScreenshots()
    {
      Camera mainCamera = Camera.main;

      if (mainCamera != null)
      {
        string folderPath = EditorUtility.SaveFolderPanel("CaronteFX - Select Folder", "", "");
        if (folderPath != string.Empty)
        {
          EditorApplication.isPlaying = true;

          GameObject go = mainCamera.gameObject;
          EditorGUIUtility.PingObject( go );
;
          CRCameraCapturer cameraCapturer = go.GetComponent<CRCameraCapturer>();
          if (cameraCapturer == null)
          {
            cameraCapturer = Undo.AddComponent<CRCameraCapturer>(go);
          }

          Undo.RecordObject( go, "Change camera capturer ");
          
          cameraCapturer.enabled     = true;
          cameraCapturer.cranimation = ac_;
          cameraCapturer.folder      = folderPath;

          Undo.CollapseUndoOperations( Undo.GetCurrentGroup() );
        }
      }
    }

    private void RemoveCameraRecorder()
    {
      Camera mainCamera = Camera.main;
      if (mainCamera != null)
      {
        GameObject go = mainCamera.gameObject;
        CRCameraCapturer cameraCapturer = go.GetComponent<CRCameraCapturer>();
        if (cameraCapturer != null)
        {
          Undo.DestroyObjectImmediate(cameraCapturer);
          EditorGUIUtility.PingObject( go );
        }
      }
    }

  }
}

