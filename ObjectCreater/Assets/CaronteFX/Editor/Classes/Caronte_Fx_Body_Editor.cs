using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;


namespace CaronteFX
{
  public class CRBodyData
  {
    public uint              idBody_   = uint.MaxValue;
    public BodyType          bodyType_ = BodyType.None;
    public List<CommandNode> listNode_ = new List<CommandNode>();
  }

  [CanEditMultipleObjects]
  [CustomEditor(typeof(Caronte_Fx_Body))]
  public class Caronte_Fx_Body_Editor : Editor
  {
    SerializedProperty serializedColliderType_;
    SerializedProperty serializedColliderMesh_;
    SerializedProperty serializedColliderColor_;
    SerializedProperty serializedColliderRenderMode_;
    SerializedProperty serializedTileMesh_;
 
    List<CRBodyData>  listBodyData_ = new List<CRBodyData>();

    CRManagerEditor window_;
    CNManager       manager_;
    CNHierarchy     hierarchy_;

    Vector2         scrollVecDefinition_;
    Vector2         scrollVecReferenced_;

    void OnEnable()
    {
      serializedColliderType_       = serializedObject.FindProperty("colliderType_");
      serializedColliderMesh_       = serializedObject.FindProperty("colliderMesh_");
      serializedColliderColor_      = serializedObject.FindProperty("colliderColor_");
      serializedColliderRenderMode_ = serializedObject.FindProperty("colliderRenderMode_");
      serializedTileMesh_           = serializedObject.FindProperty("tileMesh_");

    }

    void OnDisable()
    {
      if (window_ != null)
      {
        window_.WantRepaint -= Repaint;
      }
    }

    public override void OnInspectorGUI()
    {
      serializedObject.Update();
      bool isEditing = false;

      if (CRManagerEditor.IsOpen)
      {
        window_ = CRManagerEditor.Instance;
        window_.WantRepaint -= Repaint;
        window_.WantRepaint += Repaint;
      }
      
      if (CNManager.IsInitedStatic)
      {
        manager_   = CNManager.Instance;
        hierarchy_ = manager_.Hierarchy;
        manager_.GetBodiesData( listBodyData_ );

        isEditing = manager_.Player.IsEditing;
      }
      else
      {
        listBodyData_.Clear();
      }

      CRBodyData bdData = null;
      uint idBody = uint.MaxValue;

      int nBodyData = listBodyData_.Count;

      BodyType bodyType;
      string   bodyTypeText;
      string   bodyIdText;

      if (nBodyData == 0)
      {
        bodyType     = BodyType.None;
        bodyTypeText = "-";
        bodyIdText   = "-";
      }
      else
      {
        bdData = listBodyData_[0];

        bodyType     = bdData.bodyType_;
        bodyTypeText = GetBodyTypeString(bdData.bodyType_);
        idBody       = bdData.idBody_;

        for (int i = 1; i < nBodyData; i++)
        {
          bdData = listBodyData_[i];

          if (bdData.bodyType_ != bodyType)
          {
            bodyType     = BodyType.None;
            bodyTypeText = "-";
            bodyIdText   = "-"; 
            break;
          }
        }

        if (idBody == uint.MaxValue || nBodyData > 1)
        {
          bodyIdText = "-";
        }
        else
        {
          bodyIdText = idBody.ToString();
        }
      }

      HashSet<CommandNode> setBodyDefinition = new HashSet<CommandNode>();
      HashSet<CommandNode> setBodyReference  = new HashSet<CommandNode>();

      for (int i = 0; i < nBodyData; i++)
      {
        bdData = listBodyData_[i];
        List<CommandNode> bdDataNodes = bdData.listNode_;
        int nDataNodes = bdDataNodes.Count;
        for (int j = 0; j < nDataNodes; j++)
        {
          CommandNode node = bdDataNodes[j];
          if (j == 0)
          {
            setBodyDefinition.Add(node);
          }
          else
          {
            setBodyReference.Add(node);
          }
        }
      }

      EditorGUILayout.Space();

      EditorGUILayout.LabelField("Body type: ", bodyTypeText );
      EditorGUILayout.LabelField("Body id:",    bodyIdText );

      EditorGUILayout.Space();

      if (bodyType == BodyType.None)
      {
        DrawFullBodySection();
      }
      else if (bodyType == BodyType.Ropebody)
      {
        DrawRopeColliderSection(isEditing);
      }
      else
      {
        DrawBodyColliderSection();
      }
     
      EditorGUILayout.Space();
      EditorGUILayout.LabelField("Body Definition: ");

      scrollVecDefinition_ = GUILayout.BeginScrollView(scrollVecDefinition_, GUILayout.ExpandHeight(false));

      DrawNodeGUI( setBodyDefinition );
      GUILayout.EndScrollView();

      EditorGUILayout.Space();
      EditorGUILayout.LabelField("Referenced in: ");

      scrollVecReferenced_ = GUILayout.BeginScrollView(scrollVecReferenced_, GUILayout.ExpandHeight(false));
      DrawNodeGUI( setBodyReference );
      GUILayout.EndScrollView();
      CRGUIUtils.Splitter();

      if (!CRManagerEditor.IsOpen)
      {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();

        if (GUILayout.Button("Open CaronteFx Editor", GUILayout.Height(30f)))
        {
          window_ = (CRManagerEditor)CRManagerEditor.Init();
        }

        EditorGUILayout.Space();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
      }

      serializedObject.ApplyModifiedProperties();
  }

  private void DrawFullBodySection()
  {
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PropertyField(serializedColliderType_, new GUIContent("Collider"));
    EditorGUILayout.EndHorizontal();

    EditorGUI.BeginDisabledGroup( serializedColliderType_.enumValueIndex != 2 );
    {
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PropertyField(serializedColliderMesh_, new GUIContent("Custom Mesh") );
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PropertyField(serializedColliderColor_, new GUIContent("Color") );
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PropertyField(serializedColliderRenderMode_, new GUIContent("Render mode") );
      EditorGUILayout.EndHorizontal();
    }
    EditorGUI.EndDisabledGroup();

    EditorGUILayout.Space();

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PropertyField(serializedTileMesh_, new GUIContent("Tile Mesh") );
    EditorGUILayout.EndHorizontal();
  }

  private void DrawBodyColliderSection()
  {
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PropertyField(serializedColliderType_, new GUIContent("Collider"));
    EditorGUILayout.EndHorizontal();

    EditorGUI.BeginDisabledGroup( serializedColliderType_.enumValueIndex != 2 );
    {
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PropertyField(serializedColliderMesh_, new GUIContent("Custom Mesh") );
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PropertyField(serializedColliderColor_, new GUIContent("Color") );
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PropertyField(serializedColliderRenderMode_, new GUIContent("Render mode") );
      EditorGUILayout.EndHorizontal();
    }
    EditorGUI.EndDisabledGroup();

    EditorGUILayout.Space();
  }

  private void DrawRopeColliderSection(bool isEditing)
  {
    if (!isEditing)
    {
      EditorGUI.BeginDisabledGroup(true);

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PropertyField(serializedColliderType_, new GUIContent("Collider"));
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PropertyField(serializedColliderMesh_, new GUIContent("Custom Mesh") );
      EditorGUILayout.EndHorizontal();
      EditorGUI.EndDisabledGroup();

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PropertyField(serializedColliderColor_, new GUIContent("Color") );
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.PropertyField(serializedColliderRenderMode_, new GUIContent("Render mode") );
      EditorGUILayout.EndHorizontal();
    }
    
    EditorGUI.BeginDisabledGroup(!isEditing);
    EditorGUILayout.Space();
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PropertyField(serializedTileMesh_, new GUIContent("Tile Mesh") );
    EditorGUILayout.EndHorizontal();
    EditorGUI.EndDisabledGroup();
  }

  private void DrawNodeGUI(HashSet<CommandNode> setNode )
  {
    if (setNode.Count == 0)
    {
      GUILayout.Label("-");
    }
    foreach( CommandNode node in setNode)
    {
      GUILayout.BeginHorizontal();
      Rect boxRect = GUILayoutUtility.GetRect(new GUIContent(""), EditorStyles.textField, GUILayout.ExpandWidth(true));
      Rect labelRect = new Rect(boxRect.xMin + 5, boxRect.yMin, boxRect.width * 0.75f - 5, boxRect.height);
      GUI.Box(labelRect, "");
      GUI.Label(labelRect, node.Name);
      Rect buttonRect = new Rect(labelRect.xMax + 10, boxRect.yMin, boxRect.width * 0.25f - 15, boxRect.height);
        
      if (GUI.Button(buttonRect, "Select", EditorStyles.miniButton))
      {
        window_ = (CRManagerEditor) CRManagerEditor.Init();
        hierarchy_.FocusAndSelect(node);
      }

      GUILayout.EndHorizontal();
    }
  }

  string GetBodyTypeString(BodyType bodyType)
  {
    switch(bodyType)
    {
      case BodyType.Rigidbody:
        return "Rigid Body";

      case BodyType.Softbody:
        return "Soft Body";
 
      case BodyType.BodyMeshStatic:
        return "Static Body Mesh";

      case BodyType.BodyMeshAnimatedByArrPos:
        return "Animated Body Mesh";

      case BodyType.BodyMeshAnimatedByTransform:
        return "Animated Body Mesh";

      case BodyType.Clothbody:
        return "Cloth Body";

      case BodyType.Ropebody:
        return "Rope Body";

      default:
        return "None";
    }
  }

  }
}
