using UnityEngine;
using UnityEditor;
using System.Collections;


namespace CaronteFX
{
  public class CNEditView : IView
  {
    public CNHierarchy Controller { get; private set; }
    //-----------------------------------------------------------------------------------
    private string nodeName_;
    private int itemIdx_;
    private string title_;
    //-----------------------------------------------------------------------------------
    public CNEditView(CNHierarchy controller, int itemIdx, string title)
    {
      Controller = controller;
      itemIdx_ = itemIdx;
      nodeName_ = Controller.GetItemName(itemIdx);
      title_ = title;
    }
    //-----------------------------------------------------------------------------------
    public void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(new Rect(0, 0, area.width, area.height));

      GUILayout.BeginHorizontal(EditorStyles.toolbar);
      GUILayout.Label(title_);
      GUILayout.EndHorizontal();

      GUILayout.EndArea();

      GUILayout.Space(10);

      EditorGUILayout.BeginHorizontal();

      EditorGUILayout.LabelField("Node", GUILayout.MinWidth(60));

      GUI.SetNextControlName("textfield");
      nodeName_ = EditorGUILayout.TextField(nodeName_, GUILayout.MinWidth(100));
      if (GUI.GetNameOfFocusedControl() == string.Empty)
      {
        GUI.FocusControl("textfield");
      }
      EditorGUILayout.EndHorizontal();

      GUILayout.Space(20);

      EditorGUILayout.BeginHorizontal();
      if (GUILayout.Button("Cancel"))
      {
        CNItemPopupWindow.CloseIfOpen();
      }
      if (GUILayout.Button("OK"))
      {
        Controller.SetItemName(itemIdx_, nodeName_);
        CNItemPopupWindow.CloseIfOpen();
      }

      EditorGUILayout.EndHorizontal();
    }

  } // class CNEditView

} // namespace Caronte...
