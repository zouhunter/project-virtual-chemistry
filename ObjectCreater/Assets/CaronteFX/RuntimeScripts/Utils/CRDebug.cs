using UnityEngine;

namespace CaronteFX
{
  public class CRDebug
  {
    //-----------------------------------------------------------------------------------
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Assert(bool comparison)
    {
      if (!comparison)
      {
        Debug.Break();
      }
    }
    //-----------------------------------------------------------------------------------
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Assert(bool comparison, string message)
    {
      if (!comparison)
      {
        Debug.LogError("[CaronteFX] " + message);
        Debug.Break();
      }
    }
    //-----------------------------------------------------------------------------------
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Log(string message)
    {
      Debug.Log("[CaronteFX] " + message);
    }
    //-----------------------------------------------------------------------------------
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogWarning(string message)
    {
      Debug.LogWarning("[CaronteFX] " + message);
    }
    //-----------------------------------------------------------------------------------
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Throw(string message)
    {
      Debug.LogError("[CaronteFX] " + message);
    }
}


}