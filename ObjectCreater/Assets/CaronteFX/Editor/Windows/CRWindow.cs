using UnityEngine;
using UnityEditor;

namespace CaronteFX
{
  public class CRWindow<T> : EditorWindow 
    where T:EditorWindow
  {

    public static T Instance { get; protected set; }
    public static bool IsOpen
    {
      get
      {
        return ( Instance != null );
      }
    }

    public static bool IsFocused
    {
      get
      {
        return ( EditorWindow.focusedWindow == Instance );
      }
    }

    public static void CloseIfOpen()
    {
      if (Instance != null)
      {
        Instance.Close();
      }
    }


    public static void RepaintIfOpen()
    {
      if (Instance != null)
      {
        Instance.Repaint();
      }
    }


  }
}

