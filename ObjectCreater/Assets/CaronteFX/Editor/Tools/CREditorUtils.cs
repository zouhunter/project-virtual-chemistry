using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;

namespace CaronteFX
{
  public static class CREditorUtils
  {

    public static string GetUniqueGameObjectName(string objectName)
    {
      string newName = objectName;

      int id = 0;
  
      while ( FindSceneObjectWithName(newName) )
      {
        id++;
        newName = objectName + "_" + id;      
      }
      
      return newName;
    }

    public static bool FindSceneObjectWithName(string name)
    {
      GameObject[] arrGameObject = Resources.FindObjectsOfTypeAll<GameObject>();

      for (int i = 0; i < arrGameObject.Length; i++)
      {
        GameObject go = arrGameObject[i];
        if (go.IsInScene() && go.name == name)
        {
          return true;
        }
      }
      return false;
    }

    public static GameObject CreateDummy(this GameObject gameObject, string name )
    {
      GameObject dummyGO              = new GameObject(name);
      dummyGO.transform.parent        = gameObject.transform.parent;
      dummyGO.transform.localPosition = gameObject.transform.localPosition;

      return dummyGO;
    }

    public static GameObject GetChildObjectAt(this GameObject go, int childIdx)
    {
      Transform tr = go.transform;
      int numChildren = tr.childCount;
      if (numChildren <= childIdx)
      {
        return null;
      }
      else
      {
        return (tr.GetChild(childIdx).gameObject);
      }
    }

    public static GameObject[] GetChildObjects(this GameObject go)
    {
      List<GameObject> listGO = new List<GameObject>();
      Transform tr = go.transform;
      int numChildren = tr.childCount;
      for (int i = 0; i < numChildren; i++)
      {
        GameObject childGO = tr.GetChild(i).gameObject;
        listGO.Add(childGO);
      }
      return listGO.ToArray();
    }

    public static void GetChildObjectsIds( GameObject go, List<int> listObjectIds )
    {
      listObjectIds.Clear();

      Transform tr = go.transform;
      int numChildren = tr.childCount;
      for (int i = 0; i < numChildren; i++)
      {
        int instanceId = tr.GetChild(i).gameObject.GetInstanceID();
        listObjectIds.Add(instanceId);
      }
    }

    public static GameObject[] GetRootAndChildObjects(this GameObject go, bool getInactives)
    {
      List<GameObject> listGO = new List<GameObject>();
      listGO.Add(go);
      Transform tr = go.transform;
      int numChildren = tr.childCount;
      for (int i = 0; i < numChildren; i++)
      {
        GameObject childGO = tr.GetChild(i).gameObject;
        if (childGO.activeInHierarchy || getInactives)
        {
          Transform childGOTr = childGO.transform;
          if (childGOTr.childCount > 0)
          {
            GameObject[] arrChildGO = GetAllChildObjects(childGO, getInactives);
            listGO.AddRange(arrChildGO);
          }
          listGO.Add(childGO);
        }
      }
      return listGO.ToArray();
    }

    public static GameObject[] GetAllChildObjects(this GameObject go, bool getInactives)
    {
      List<GameObject> listGO = new List<GameObject>();
      Transform tr = go.transform;
      int numChildren = tr.childCount;
      for (int i = 0; i < numChildren; i++)
      {
        GameObject childGO = tr.GetChild(i).gameObject;
        if (childGO.activeInHierarchy || getInactives )
        {
          Transform childGOTr = childGO.transform;
          if (childGOTr.childCount > 0)
          {
            GameObject[] arrChildGO = GetAllChildObjects(childGO, getInactives);
            listGO.AddRange(arrChildGO);
          }
          listGO.Add(childGO);
        }
      }
      return listGO.ToArray();
    }

    public static GameObject[] GetAllChildObjectsWithGeometry(this GameObject go, bool getInactives)
    {
      List<GameObject> listGO = new List<GameObject>();
      Transform tr = go.transform;
      int numChildren = tr.childCount;
      for (int i = 0; i < numChildren; i++)
      {
        GameObject childGO = tr.GetChild(i).gameObject;
        if (childGO.activeInHierarchy || getInactives )
        {
          Transform childGOTr = childGO.transform;
          if (childGOTr.childCount > 0)
          {
            GameObject[] arrChildGO = GetAllChildObjectsWithGeometry(childGO, getInactives);
            listGO.AddRange(arrChildGO);
          }
          if ( childGO.HasMesh() )
          {
            listGO.Add(childGO);
          }
        }
      }
      return listGO.ToArray();
    }

    public static void GetRenderersFromRoot(GameObject go, out MeshRenderer[] arrNormalRenderers, out SkinnedMeshRenderer[] arrSkinnedRenderers)
    {
      List<MeshRenderer>        listMeshRenderer        = new List<MeshRenderer>();
      List<SkinnedMeshRenderer> listSkinnedMeshRenderer = new List<SkinnedMeshRenderer>();

      GameObject[] arrGameObject = GetRootAndChildObjects(go, true);
      
      foreach ( GameObject gameObject in arrGameObject )
      {
        SkinnedMeshRenderer skinnedr = gameObject.GetComponent<SkinnedMeshRenderer>();
        if ( skinnedr != null)
        {
          listSkinnedMeshRenderer.Add(skinnedr);
        }
        else
        {
          MeshRenderer normalr = gameObject.GetComponent<MeshRenderer>();
          if ( normalr != null )
          {
            listMeshRenderer.Add(normalr);
          }
        }
      }

      arrNormalRenderers  = listMeshRenderer.ToArray();
      arrSkinnedRenderers = listSkinnedMeshRenderer.ToArray();
    }

    public static string GetGameObjectPath(this GameObject obj)
    {
      string path = "/" + obj.name;
      while (obj.transform.parent != null)
      {
        obj = obj.transform.parent.gameObject;
        path = "/" + obj.name + path;
      }
      return path;
    }

    public static List<GameObject> GetSceneRootObjects()
    {
      List<GameObject> rootObjects = new List<GameObject>();
      Object[] sceneObjects = UnityEngine.Object.FindObjectsOfType(typeof(GameObject));

      foreach (GameObject obj in sceneObjects)
      {
        if (obj.transform.parent == null)
        {
          rootObjects.Add(obj);
        }
      }
      return rootObjects;
    }

    public static Bounds GetGlobalBoundsWorld( List<GameObject> gameObjects )
    {
      Bounds globalBounds = new Bounds();
      int numGameObjects = gameObjects.Count;

      bool first = true;
      for (int i = 0; i < numGameObjects; i++ )
      {
        GameObject go = gameObjects[i];
        Renderer goRenderer = go.GetComponent<Renderer>();
        if (goRenderer != null)
        {
          Bounds goBounds = goRenderer.bounds;
          if ( first )
          {
            globalBounds.center = goBounds.center;
            globalBounds.SetMinMax( goBounds.min, goBounds.max );
            first = false;
          }
          else
          {
            globalBounds.Encapsulate(goBounds);
          }
        }
      }
      return globalBounds;
    }

    public static Bounds GetBounds( GameObject go )
    {
      Renderer goRenderer = go.GetComponent<Renderer>();
      if (goRenderer != null)
      {
        return goRenderer.bounds;
      }
      return new Bounds();
    }


    public static bool IsInScene(this Object unityObject)
    {
      PrefabType prefabType = PrefabUtility.GetPrefabType(unityObject);

      return ( prefabType == PrefabType.None || prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance ||
               prefabType == PrefabType.ModelPrefabInstance || prefabType == PrefabType.DisconnectedModelPrefabInstance );
    }

    public static GameObject[] GetAllGameObjectsInScene()
    {
      List<GameObject> listGameObject = new List<GameObject>();

      GameObject[] arrGameObject = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));

      foreach (GameObject go in arrGameObject)
      {

        if ( (go.hideFlags & HideFlags.HideInHierarchy) == HideFlags.HideInHierarchy ||
             (go.hideFlags & HideFlags.HideAndDontSave) == HideFlags.HideAndDontSave ||
             !go.IsInScene() )
        {
          continue;
        }

        listGameObject.Add(go);
      }

      return listGameObject.ToArray();
    }

    public static GameObject[] GetAllGameObjectsInSceneWithGeometry()
    {
      List<GameObject> listGameObject = new List<GameObject>();

      GameObject[] arrGameObject = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));

      foreach (GameObject go in arrGameObject)
      {

        if ( (go.hideFlags & HideFlags.HideInHierarchy) == HideFlags.HideInHierarchy ||
             (go.hideFlags & HideFlags.HideAndDontSave) == HideFlags.HideAndDontSave ||
             !go.IsInScene() ||
             !go.HasMesh() )
        {
          continue;
        }

        listGameObject.Add(go);
      }

      return listGameObject.ToArray();
    }

    public static bool CheckIfAllSceneGameObjects(Object[] arrObjectReference)
    {
      if (arrObjectReference == null) return false;
      int arrObjectReference_size = arrObjectReference.Length;
      if (arrObjectReference_size == 0) return false;

      for (int i = 0; i < arrObjectReference_size; i++)
      {
        GameObject go = arrObjectReference[i] as GameObject;
        if (go == null)
        {
          return false;
        }
        else
        {
          if (go.IsInScene())
          {
            return false;
          }
        }
      }
      return true;
    }

    public static bool CheckIfAnySceneGameObjects(Object[] arrObjectReference)
    {
      if (arrObjectReference == null) return false;
      int arrObjectReference_size = arrObjectReference.Length;
      if (arrObjectReference_size == 0) return false;

      for (int i = 0; i < arrObjectReference_size; i++)
      {
        GameObject go = arrObjectReference[i] as GameObject;
        if (go != null)
        {
          if (go.IsInScene())
          {
            return true;
          }
        }
      }
      return false;
    }

    public static bool CheckIfAnySceneObjectHasComponent<T>(Object[] arrObjectReference)
        where T : UnityEngine.Component
    {
      if (arrObjectReference == null) return false;
      int arrObjectReference_size = arrObjectReference.Length;
      if (arrObjectReference_size == 0) return false;

      for (int i = 0; i < arrObjectReference_size; i++)
      {
        GameObject go = arrObjectReference[i] as GameObject;
        if (go != null)
        {
          if (go.IsInScene())
          {
            if (go.GetComponent<T>() != null)
            {
              return true;
            }
            GameObject[] arrGameObject = GetAllChildObjects(go, false);
            foreach (GameObject childGo in arrGameObject)
            {
              if (childGo.GetComponent<T>() != null)
              {
                return true;
              }
            }
          }
        }
      }
      return false;
    }

    public static bool CheckIfAnySceneObjectHasMesh(Object[] arrObjectReference)
    {
      if (arrObjectReference == null) return false;
      int arrObjectReference_size = arrObjectReference.Length;
      if (arrObjectReference_size == 0) return false;

      for (int i = 0; i < arrObjectReference_size; i++)
      {
        GameObject go = arrObjectReference[i] as GameObject;
        if (go != null)
        {
          if (go.IsInScene())
          {
            MeshFilter meshFilter = go.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
              return true;
            }
          }
        }
      }
      return false;
    }

    public static void GetCaronteFxGameObjects(Object[] arrObjectReference, out List<GameObject> listGameObject)
    {
      listGameObject = new List<GameObject>();
      int arrObjectReference_size = arrObjectReference.Length;

      for (int i = 0; i < arrObjectReference_size; i++)
      {
        GameObject go = arrObjectReference[i] as GameObject;
        if (go != null)
        {
          if (go.IsInScene())
          {
            if (go.GetComponent<Caronte_Fx>() != null)
            {
              listGameObject.Add(go);
            }
            GameObject[] arrGameObject = GetAllChildObjects(go, true);
            foreach (GameObject childGo in arrGameObject)
            {
              if (childGo.GetComponent<Caronte_Fx>() != null)
              {
                listGameObject.Add(childGo);
              }
            }
          }
        }
      }
    }
    /// <summary>
    /// Return a list with the relative fx and depth
    /// </summary>
    /// <param name="caronteFx"></param>
    /// <param name="listCaronteFx"></param>
    public static void GetCaronteFxsRelations(Caronte_Fx caronteFx, out List<Tuple2<Caronte_Fx, int>> listCaronteFx )
    {
      listCaronteFx = new List<Tuple2<Caronte_Fx, int>>();
      GameObject go = caronteFx.gameObject;
      if ( go.IsInScene() )
      {     
        Transform searchRoot = go.transform.parent;
        if (searchRoot != null)
        {
          GameObject[] arrChild = GetAllChildObjects(searchRoot.gameObject, true);
          AddRelations( go, arrChild, listCaronteFx );
        }
        else
        {
          GameObject[] arrChild = GetAllGameObjectsInScene();
          AddRelations( go, arrChild, listCaronteFx );
        }
      }
    }

    public static void AddRelations(GameObject parentFx, GameObject[] arrGameObject, List<Tuple2<Caronte_Fx, int>> listCaronteFx)
    {
      for (int i = 0; i < arrGameObject.Length; i++)
      {
        GameObject go = arrGameObject[i];
        Caronte_Fx fxChild = go.GetComponent<Caronte_Fx>();
        if (fxChild != null)
        {
          int depth = go.GetFxHierachyDepthFrom(parentFx);
          listCaronteFx.Add(Tuple2.New(fxChild, depth));
        }
      }
    }

    public static int GetFxHierachyDepthFrom(this GameObject go, GameObject otherGo)
    {
      Transform tr = go.transform;
      Transform otherTr = otherGo.transform;
      int depth = 0;
      while (tr != otherTr && tr != null && !tr.IsBrotherOf(otherTr) )
      {
        tr = tr.parent;
        depth++;
      }
      return depth;
    }

    public static bool IsBrotherOf(this GameObject go, GameObject otherGo )
    {
      Transform tr = go.transform;
      Transform otherTr = otherGo.transform;

      return IsBrotherOf( tr, otherTr );
    }

    public static bool IsBrotherOf(this Transform tr, Transform otherTr )
    {
      if (tr.parent == otherTr.parent)
      {
        return true;
      }
      return false;
    }

    public static void GetSceneGameObjects(Object[] arrObjectReference, List<GameObject> listGameObject)
    {
      int arrObjectReference_size = arrObjectReference.Length;

      for (int i = 0; i < arrObjectReference_size; i++)
      {
        GameObject go = arrObjectReference[i] as GameObject;
        if (go != null)
        {
          if (go.IsInScene())
          {     
            listGameObject.Add(go);
          }
        }
      }
    }

    public static void GetGeometryGameObjects(Object[] arrObjectReference, List<GameObject> listGameObject)
    {
      int arrObjectReference_size = arrObjectReference.Length;

      for (int i = 0; i < arrObjectReference_size; i++)
      {
        GameObject go = arrObjectReference[i] as GameObject;
        if (go != null)
        {
          if (go.IsInScene())
          {
            if (go.HasMesh() || AnyChildHasMesh(go) )
            {
              listGameObject.Add(go);
            }
          }
        }
      }
    }

    public static bool AnyChildHasMesh(GameObject go)
    {
      GameObject[] arrGameObject = GetAllChildObjects(go, true);
      foreach (GameObject childGO in arrGameObject)
      {
        if (childGO.HasMesh())
        {
          return true;
        }
      }
      return false;
    }

    public static void GetSceneGameObjectsWithComponent<T>(Object[] arrObjectReference, List<GameObject> listGameObject)
      where T: UnityEngine.Component
    {
      int arrObjectReference_size = arrObjectReference.Length;

      for (int i = 0; i < arrObjectReference_size; i++)
      {
        GameObject go = arrObjectReference[i] as GameObject;
        if (go != null)
        {
          if (go.IsInScene() )
          {
            if ( go.GetComponent<T>() != null)
            {
              listGameObject.Add(go);
            }
            else
            {
              GameObject[] arrGameObject = GetAllChildObjects(go, true);
              foreach (GameObject childGO in arrGameObject)
              {
                if (childGO.GetComponent<T>() != null)
                {
                  listGameObject.Add(childGO);
                }
              }
            }
          }
        }
      }
    }

    public static T[] GetAllSceneComponentsOfType<T>()
      where T: Component
    {
      List<T> listComponents = new List<T>();
      
      T[] components = Resources.FindObjectsOfTypeAll<T>();

      for (int i = 0; i < components.Length; i++)
      {
        T component = (T) components[i];
        if (IsInScene(component.gameObject))
        {
          listComponents.Add(component);
        }
      }

      return listComponents.ToArray();
    }

    public static void GetGameObjectPathId(this GameObject obj, List<GameObject> sceneRootObjects, out CRPathId goPathId)
    {
      goPathId = new CRPathId();
      Transform tr = obj.transform.parent;
      while (tr != null)
      {
        int childCount = tr.childCount;
        for (int i = 0; i < childCount; ++i)
        {
          GameObject go = tr.GetChild(i).gameObject;
          if (go == obj)
          {
            goPathId.AddPartialPath(go.name, i);
            break;
          }
        }
        obj = tr.gameObject;
        tr = obj.transform.parent;
      }

      int numRootObjects = sceneRootObjects.Count;
      for (int i = 0; i < numRootObjects; ++i)
      {
        GameObject rootObject = sceneRootObjects[i];
        if (rootObject == obj)
        {
          goPathId.AddPartialPath(obj.name, i);
        }
      }
    }

    public static void RenameGameObjects( string name, List<GameObject> listGOToRename)
    {
      int nGameObjectsToRename = listGOToRename.Count;
      for ( int i = 0; i < nGameObjectsToRename; i++ )
      {
        GameObject go = listGOToRename[i];
        go.name = name + i;
      }
    }

    public static void RenameGameObjectsAndMeshes( string name, List<GameObject> listGOToRename)
    {
      int nGameObjectsToRename = listGOToRename.Count;
      for ( int i = 0; i < nGameObjectsToRename; i++ )
      {
        GameObject go = listGOToRename[i];
        go.name = name + i;

        Mesh mesh = go.GetMesh();
        if (mesh != null)
        {
          mesh.name = go.name;
        }
      }
    }

    public static Animator GetFirstAnimatorInHierarchy(GameObject go)
    {
      Animator animator = go.GetComponent<Animator>();
      if (animator == null)
      {
        Transform tr = go.transform.parent;
        if (tr != null)
        {
          return ( GetFirstAnimatorInHierarchy( tr.gameObject ) );
        }
      }

      return animator;
    }

    public static void GetListBoundsFromListGO( List<GameObject> listGameObject, List<Bounds> listBounds )
    {
      listBounds.Clear();

      foreach( GameObject go in listGameObject )
      {
        if ( go != null )
        {
          Renderer rn = go.GetComponent<Renderer>();

          if (rn != null)
          {
            listBounds.Add(rn.bounds);
          }
        }
      }
    }

    public static bool GetBakedMesh(this GameObject gameObject, out Mesh mesh)
    {
      SkinnedMeshRenderer smRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
      if (smRenderer != null)
      {
        mesh = new Mesh();
        smRenderer.BakeMesh(mesh);
        if (mesh != null)
        {
          return true;
        }
      }
      MeshFilter meshfilter = gameObject.GetComponent<MeshFilter>();
      if (meshfilter != null)
      {
        mesh = meshfilter.sharedMesh;
        if (mesh != null)
        {
          return false;
        }
      }
      mesh = null;
      return false;
    }

    public static void AddMesh(this GameObject gameObject, Mesh mesh)
    {
      MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
      if (meshFilter == null)
      {
        meshFilter = gameObject.AddComponent<MeshFilter>();
      }

      Mesh unityMesh = meshFilter.sharedMesh;
      if (unityMesh == null)
      {
        meshFilter.sharedMesh = mesh;
      }

      MeshRenderer meshRender = gameObject.GetComponent<MeshRenderer>();
      if (meshRender == null)
      {
        meshRender = gameObject.AddComponent<MeshRenderer>();
      }
    }

    public static void ReplaceSkinnedMeshRenderer(this GameObject go)
    {
      SkinnedMeshRenderer smRenderer = go.GetComponent<SkinnedMeshRenderer>();

      Mesh skinnedMesh = null;
      Material[] materials = null;

      if (smRenderer != null)
      {
        skinnedMesh = smRenderer.sharedMesh;
        materials = smRenderer.sharedMaterials;
        Object.DestroyImmediate(smRenderer);
      }

      MeshRenderer mr = go.GetComponent<MeshRenderer>();
      if (mr == null)
      {
        mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterials = materials;
      }

      mr.enabled = true;

      MeshFilter meshFilter = go.GetComponent<MeshFilter>();
      if (meshFilter == null)
      {
        meshFilter = go.AddComponent<MeshFilter>();
        if (skinnedMesh != null)
        {
          meshFilter.sharedMesh = skinnedMesh;
        }
      }
    }

    public static Bounds GetWorldBounds(this GameObject gameObject)
    {
      Renderer renderer = gameObject.GetComponent<Renderer>();
      if (renderer != null)
      {
        return renderer.bounds;
      }
      return new Bounds();
    }

    public static void SetMesh(GameObject gameObject, Mesh mesh)
    {
      MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
      if (meshFilter != null)
      {
        meshFilter.sharedMesh = mesh;
        EditorUtility.SetDirty(meshFilter);
        return;
      }

      SkinnedMeshRenderer smRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
      if (smRenderer != null)
      {
        smRenderer.sharedMesh = mesh;
        EditorUtility.SetDirty(smRenderer);
      }
    }

    public static bool IsAnyUnsavedMeshInHierarchy(GameObject go)
    {
      bool isAnyUnsavedMesh = false;

      GameObject[] arrGameObjects = CREditorUtils.GetAllChildObjectsWithGeometry(go, true);
      int nGameObjects = arrGameObjects.Length;
      for( int i = 0 ; i < nGameObjects; i++ )
      {
        UnityEngine.GameObject gameObject = arrGameObjects[i];
        UnityEngine.Mesh mesh = gameObject.GetMesh();

        if (mesh != null && !AssetDatabase.Contains(mesh.GetInstanceID()) )
        {
          isAnyUnsavedMesh = true;
        }  
			}

      return isAnyUnsavedMesh;
    }

    public static void SaveAnyUnsavedMeshInHierarchy(GameObject go, bool saveAsCopy)
    {
      string path = EditorUtility.SaveFilePanelInProject("Save unsaved meshes into Assets...", go.name + ".prefab","prefab", "Please, enter a name where the unsaved meshes will be saved to" );
      if (path.Length != 0) 
      {      
        string meshesPrefabPath = AssetDatabase.GenerateUniqueAssetPath(path);
        Object meshesPrefab = PrefabUtility.CreateEmptyPrefab(meshesPrefabPath);
        GameObject[] arrGameObjects = CREditorUtils.GetAllChildObjectsWithGeometry(go, true);
        int nGameObjects = arrGameObjects.Length;
        for( int i = 0 ; i < nGameObjects; i++ )
        {
          UnityEngine.GameObject gameObject = arrGameObjects[i];
          UnityEngine.Mesh mesh = gameObject.GetMesh();
          if (mesh != null && !AssetDatabase.Contains(mesh.GetInstanceID()) )
          {
            if (saveAsCopy)
            {
              Mesh newMesh = UnityEngine.Object.Instantiate(mesh);
              newMesh.name = mesh.name;
              CREditorUtils.SetMesh(gameObject, newMesh);
              AssetDatabase.AddObjectToAsset(newMesh, meshesPrefab);
            }
            else
            {
              AssetDatabase.AddObjectToAsset(mesh, meshesPrefab);
            }

            EditorUtility.DisplayProgressBar("Saving scene meshes to prefab...", "Saving mesh " + i+1 + " of " + nGameObjects, (float)i+1 / (float)nGameObjects);
          }  
			  }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
      }
    }

  }

}