using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace CaronteFX
{
  public class CRGUIUtils
  {

    public static string GetSelectionFolder()
    {
      if ( Selection.activeGameObject != null )
      {
        string path = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() );
        if ( !string.IsNullOrEmpty( path ) )
        {
          int dot = path.LastIndexOf('.');
          int slash = Mathf.Max( path.LastIndexOf('/'), path.LastIndexOf('\\') );
          if ( slash > 0)
          {
            return ( ( dot > slash ) ? path.Substring( 0, slash + 1 ) : path + "/" );
          }
        }
      }
      return "Assets/";
    }

    public static void DrawSeparator()
    {
      GUILayout.Space(7f);

      if (Event.current.type == EventType.Repaint )
      {
        Texture2D tex = EditorGUIUtility.whiteTexture;
        Rect rect = GUILayoutUtility.GetLastRect();
        GUI.color = new Color( 0f, 0f, 0f, 0.25f);
        GUI.DrawTexture( new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
        GUI.DrawTexture( new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
        GUI.DrawTexture( new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
        GUI.color = Color.white;
      }
    }

    public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, int size)
    {
      size = Mathf.Max(size, 1);
      Texture2D point = new Texture2D(1, 1);
      point.SetPixel(0, 0, color);
      point.Apply();
      float distance = Vector2.Distance(pointA, pointB);
      Rect rect = new Rect();
      rect.size = new Vector2(size, size);
      for (int i = 0; i <= Mathf.RoundToInt(distance); i++)
      {

        rect.position = Vector2.Lerp(pointA, pointB, i / distance);
        GUI.DrawTexture(rect, point);
      }
    }
 
    public static readonly GUIStyle splitter;
 
    static CRGUIUtils() {
 
         splitter = new GUIStyle();
         splitter.normal.background = EditorGUIUtility.whiteTexture;
         splitter.stretchWidth = true;
         splitter.margin = new RectOffset(0, 0, 7, 0);
    }
 
    private static readonly Color splitterColor = EditorGUIUtility.isProSkin ? new Color(0.157f, 0.157f, 0.157f) : new Color(0.5f, 0.5f, 0.5f);

    // GUILayout Style
    public static void Splitter(Color rgb, float thickness = 1)
    {
      Rect position = GUILayoutUtility.GetRect(GUIContent.none, splitter, GUILayout.Height(thickness));

      if (Event.current.type == EventType.Repaint)
      {
        Color restoreColor = GUI.color;
        GUI.color = rgb;
        splitter.Draw(position, false, false, false, false);
        GUI.color = restoreColor;
      }
    }

    public static void Splitter(float thickness, GUIStyle splitterStyle)
    {
      Rect position = GUILayoutUtility.GetRect(GUIContent.none, splitterStyle, GUILayout.Height(thickness));

      if (Event.current.type == EventType.Repaint)
      {
        Color restoreColor = GUI.color;
        GUI.color = splitterColor;
        splitterStyle.Draw(position, false, false, false, false);
        GUI.color = restoreColor;
      }
    }

    public static void Splitter(float thickness = 1)
    {
      Splitter(thickness, splitter);
    }

    // GUI Style
    public static void Splitter(Rect position)
    {
      if (Event.current.type == EventType.Repaint)
      {
        Color restoreColor = GUI.color;
        GUI.color = splitterColor;
        splitter.Draw(position, false, false, false, false);
        GUI.color = restoreColor;
      }
    }

    public static void Splitter(Color color, Rect position)
    {
      if (Event.current.type == EventType.Repaint)
      {
        Color restoreColor = GUI.color;
        GUI.color = color;
        splitter.Draw(position, false, false, false, false);
        GUI.color = restoreColor;
      }
    }

    public static GameObject[] GetGameObjectsFromIds(int[] arrIds)
    {
      GameObject[] arrGameObject = new GameObject[arrIds.Length];
      for (int i = 0; i < arrGameObject.Length; i++)
      {
        arrGameObject[i] = (GameObject) EditorUtility.InstanceIDToObject(arrIds[i]);
      }
      return arrGameObject;
    }

    public static void GenerateBoxMeshes( List<Bounds> listBounds, List<Mesh> listBodyMesh )
    {
      List<Vector3> listVertices = new List<Vector3>();
      List<Vector3> listNormals  = new List<Vector3>();
      List<int>     listIndices  = new List<int>();

      int nBounds = listBounds.Count;

      int verticesPerBox = 24;
      int maxBoxesPerMesh = 65535 / verticesPerBox;

      int triangleOffset = 0;

      for( int i = 0; i < nBounds; i++ )
      {
        Bounds bounds = listBounds[i];

        AddVertices( listVertices, bounds.center, bounds.size );
        AddNormals( listNormals );
        AddIndices( listIndices, triangleOffset );

        triangleOffset++;

        int div = (i + 1) % maxBoxesPerMesh;

        if ( (div == 0) || i == (nBounds - 1) )
        {
          Mesh boundsMesh = new Mesh();

          boundsMesh.vertices  = listVertices.ToArray();
          boundsMesh.normals   = listNormals.ToArray();
          boundsMesh.triangles = listIndices.ToArray();

          listVertices.Clear();
          listNormals .Clear();
          listIndices .Clear();

          boundsMesh.RecalculateBounds();

          listBodyMesh.Add(boundsMesh);

          triangleOffset = 0;
        }
      }
    }

    public static void GenerateBoxMeshesWithId( uint id, List<Bounds> listBounds, List<TupleIdMesh> listIdBodyMesh )
    {
      List<Vector3> listVertices = new List<Vector3>();
      List<Vector3> listNormals  = new List<Vector3>();
      List<int>     listIndices  = new List<int>();

      int nBounds = listBounds.Count;

      int verticesPerBox = 24;
      int maxBoxesPerMesh = 65535 / verticesPerBox;

      int triangleOffset = 0;

      for( int i = 0; i < nBounds; i++ )
      {
        Bounds bounds = listBounds[i];

        AddVertices( listVertices, bounds.center, bounds.size );
        AddNormals( listNormals );
        AddIndices( listIndices, triangleOffset );

        triangleOffset++;

        int div = (i + 1) % maxBoxesPerMesh;

        if ( (div == 0) || i == (nBounds - 1) )
        {
          Mesh boundsMesh = new Mesh();

          boundsMesh.vertices  = listVertices.ToArray();
          boundsMesh.normals   = listNormals.ToArray();
          boundsMesh.triangles = listIndices.ToArray();

          listVertices.Clear();
          listNormals .Clear();
          listIndices .Clear();

          boundsMesh.RecalculateBounds();

          listIdBodyMesh.Add( new TupleIdMesh(id,boundsMesh) );

          triangleOffset = 0;
        }
      }
    }

    private static void AddVertices ( List<Vector3> listVertices, Vector3 center, Vector3 size)
    {
      float cubeLength  = size.x * .5f;
      float cubeWidth   = size.y * .5f;
      float cubeHeight  = size.z * .5f;

      Vector3 vertice_0 = new Vector3 ( -cubeLength, -cubeWidth,  cubeHeight ) + center;
      Vector3 vertice_1 = new Vector3 (  cubeLength, -cubeWidth,  cubeHeight ) + center;
      Vector3 vertice_2 = new Vector3 (  cubeLength, -cubeWidth, -cubeHeight ) + center;
      Vector3 vertice_3 = new Vector3 ( -cubeLength, -cubeWidth, -cubeHeight ) + center;	
      Vector3 vertice_4 = new Vector3 ( -cubeLength,  cubeWidth,  cubeHeight ) + center;
      Vector3 vertice_5 = new Vector3 (  cubeLength,  cubeWidth,  cubeHeight ) + center;
      Vector3 vertice_6 = new Vector3 (  cubeLength,  cubeWidth, -cubeHeight ) + center;
      Vector3 vertice_7 = new Vector3 ( -cubeLength,  cubeWidth, -cubeHeight ) + center;

      Vector3[] vertices = new Vector3[]
      {
        // Bottom Polygon
        vertice_0, vertice_1, vertice_2, vertice_0,
        // Left Polygon
        vertice_7, vertice_4, vertice_0, vertice_3,
        // Front Polygon
        vertice_4, vertice_5, vertice_1, vertice_0,
        // Back Polygon
        vertice_6, vertice_7, vertice_3, vertice_2,
        // Right Polygon
        vertice_5, vertice_6, vertice_2, vertice_1,
        // Top Polygon
        vertice_7, vertice_6, vertice_5, vertice_4
      };

      listVertices.AddRange( vertices );

    }

    private static void AddNormals(List<Vector3> listNormals)
    {
        Vector3 up    = Vector3.up;
        Vector3 down  = Vector3.down;
        Vector3 front = Vector3.forward;
        Vector3 back  = Vector3.back;
        Vector3 left  = Vector3.left;
        Vector3 right = Vector3.right;
    
        Vector3[] normals = new Vector3[]
        {
          // Bottom Side Render
          down, down, down, down,
                    
          // LEFT Side Render
          left, left, left, left,
                    
          // FRONT Side Render
          front, front, front, front,
                    
          // BACK Side Render
          back, back, back, back,
                    
          // RIGTH Side Render
          right, right, right, right,
                    
          // UP Side Render
          up, up, up, up
        };

        listNormals.AddRange( normals );
     }

    private static void AddIndices(List<int> listIndices, int cubeIndex)
    {
      int offset = cubeIndex * 24;

      int botOffset   = 4 * 0 + offset;
      int leftOffset  = 4 * 1 + offset;
      int frontOffset = 4 * 2 + offset;
      int backOffset  = 4 * 3 + offset;
      int rigthOffset = 4 * 4 + offset;
      int topOffset   = 4 * 5 + offset;

      int[] triangles = new int[]
      {
        // Cube Bottom Side Triangles
        3 + botOffset, 1 + botOffset, 0 + botOffset,
        3 + botOffset, 2 + botOffset, 1 + botOffset,	
        // Cube Left Side Triangles
        3 + leftOffset, 1 + leftOffset, 0 + leftOffset,
        3 + leftOffset, 2 + leftOffset, 1 + leftOffset,
        // Cube Front Side Triangles
        3 + frontOffset, 1 + frontOffset, 0 + frontOffset,
        3 + frontOffset, 2 + frontOffset, 1 + frontOffset,
        // Cube Back Side Triangles
        3 + backOffset, 1 + backOffset, 0 + backOffset,
        3 + backOffset, 2 + backOffset, 1 + backOffset,
        // Cube Rigth Side Triangles
        3 + rigthOffset, 1 + rigthOffset, 0 + rigthOffset,
        3 + rigthOffset, 2 + rigthOffset, 1 + rigthOffset,
        // Cube Top Side Triangles
        3 + topOffset, 1 + topOffset, 0 + topOffset,
        3 + topOffset, 2 + topOffset, 1 + topOffset,
      };

      listIndices.AddRange(triangles);
    }

    //-----------------------------------------------------------------------------------
    public static void DrawToggleMixedMonoBehaviours(string toggleString, List<MonoBehaviour> listMonoBehaviour, float width)
    {
      EditorGUI.BeginChangeCheck();  
      int nMonoBehaviours = listMonoBehaviour.Count;

      EditorGUI.showMixedValue = false;
      bool value = false;
      
      if (nMonoBehaviours > 0)
      {
        value = listMonoBehaviour[0].enabled;
        for (int i = 1; i < nMonoBehaviours; ++i)
        {
          MonoBehaviour mbh = listMonoBehaviour[i];
          if ( value != mbh.enabled )
          {
            EditorGUI.showMixedValue = true;
            break;
          }
        }
      }
      EditorGUI.BeginDisabledGroup( nMonoBehaviours == 0 );
#if UNITY_PRO_LICENSE
      value = EditorGUILayout.ToggleLeft(toggleString, value, GUILayout.MaxWidth(width) );
#else
      value = GUILayout.Toggle(value, toggleString, GUILayout.MaxWidth(width));
#endif
      EditorGUI.showMixedValue = false;
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObjects(listMonoBehaviour.ToArray(), "CaronteFX - Change " + toggleString);
        for (int i = 0; i < nMonoBehaviours; ++i)
        {
          listMonoBehaviour[i].enabled = value;
        }
      }
      EditorGUI.EndDisabledGroup();
    }
    //-----------------------------------------------------------------------------------
    public static void DrawToggleMixedRenderers(string toggleString, List<Renderer> listRenderer, float width)
    {
      EditorGUI.BeginChangeCheck();  
      int nRenderer = listRenderer.Count;

      EditorGUI.showMixedValue = false;
      bool value = false;
      
      if (nRenderer > 0)
      {
        value = listRenderer[0].enabled;
        for (int i = 1; i < nRenderer; ++i)
        {
          Renderer mbh = listRenderer[i];
          if ( value != mbh.enabled )
          {
            EditorGUI.showMixedValue = true;
            break;
          }
        }
      }
      EditorGUI.BeginDisabledGroup( nRenderer == 0 );

#if UNITY_PRO_LICENSE
      value = EditorGUILayout.ToggleLeft(toggleString, value, GUILayout.MaxWidth(width) );
#else
      value = GUILayout.Toggle(value, toggleString, GUILayout.MaxWidth(width));
#endif

      EditorGUI.showMixedValue = false;
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObjects(listRenderer.ToArray(), "CaronteFX - Change " + toggleString);
        for (int i = 0; i < nRenderer; ++i)
        {
          listRenderer[i].enabled = value;
        }
      }
      EditorGUI.EndDisabledGroup();
    }
    //-----------------------------------------------------------------------------------





  } //class CREditorUtil
} //namespace Caronte



