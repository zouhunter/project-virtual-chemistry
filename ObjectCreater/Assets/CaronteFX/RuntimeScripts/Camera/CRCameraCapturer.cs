using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CaronteFX
{
  [DisallowMultipleComponent]
  public class CRCameraCapturer : MonoBehaviour 
  {
      public CRAnimation cranimation;
      public string folder;
      public int supersize;
 
      public int frameRate;
      public int frameCount;

      private int currentFrame;

      void Start() 
      {
        if (cranimation != null)
        {
          cranimation.LoadAnimation(false);

          frameRate  = (int)cranimation.fps_;
          frameCount = (int)(cranimation.frameCount_ / cranimation.speed);
            
          cranimation.SetFrame(0f);

          Time.captureFramerate = frameRate;
          currentFrame = 0;
        }

      }

      [ExecuteInEditMode]
      void Update() 
      {
        if (cranimation == null)
        {
          return; 
        }

        #if UNITY_EDITOR
        if ( currentFrame >= (frameCount - 2) )
        {
          EditorApplication.isPlaying = false;
          EditorUtility.RevealInFinder(folder);
        }
        #endif
        // Append filename to folder name (format is '0005 shot.png"')
        string name = string.Format("{0}/{1:D04} shot.png", folder, currentFrame);

        
	      // Capture the screenshot to the specified file.
        Application.CaptureScreenshot(name, supersize);
        currentFrame++;
      }

  }
}

