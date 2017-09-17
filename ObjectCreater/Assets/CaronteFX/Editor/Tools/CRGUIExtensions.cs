using UnityEngine;
using UnityEditor;
using System;
using System.Collections;




namespace CaronteFX
{
  public static class CRGUIExtension
  {
    public static void CheckKeyPressedOrReset( string controlName, string currentFocusedControlName, Action action )
    {
      if (CRGUIExtension.KeyPressed(controlName, currentFocusedControlName, KeyCode.Return | KeyCode.KeypadEnter)
          || ( (currentFocusedControlName != controlName) ))
      {
        action();
      }
    }

    public static bool KeyPressed(string controlName, string currentFocusedControlName, KeyCode key)
    {
      if (currentFocusedControlName == controlName)
      {
        if ((Event.current.type == EventType.KeyUp) && ((Event.current.keyCode & key) == KeyCode.Return || (Event.current.keyCode & key) == KeyCode.KeypadEnter))
        {
          Event.current.Use();
          return true;
        }
      }
      return false;
    }

    public static float FloatTextField( float floatValue, float minValue, float maxValue, float defaultValue, string text, params GUILayoutOption[] options)
    {
      string auxString;
      float auxFloat;

      //if ( floatValue < minValue || floatValue > maxValue )
      if ( floatValue == defaultValue)
      {
        auxString = EditorGUILayout.TextField(text, options );
        bool isTextAFloat = float.TryParse(auxString, out auxFloat);
        if (isTextAFloat)
        {
          return auxFloat;
        }
        else
        {
          return defaultValue;
        }
      }
      else
      {
        return ( EditorGUILayout.FloatField(floatValue, options ) );
      }
    }

    public static float FloatTextField( string label, float floatValue, float minValue, float maxValue, float defaultValue, string text, params GUILayoutOption[] options)
    {
      string auxString;
      float auxFloat;

      if ( floatValue == defaultValue )
      {
        auxString = EditorGUILayout.TextField(label, text, options );
        bool isTextAFloat = float.TryParse(auxString, out auxFloat);
        if (isTextAFloat)
        {
          return auxFloat;
        }
        else
        {
          return defaultValue;
        }
      }
      else
      {
        return ( EditorGUILayout.FloatField(label, floatValue, options ) );
      }
    }
  }
}


