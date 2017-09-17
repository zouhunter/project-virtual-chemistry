using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CaronteSharp;

namespace CaronteFX
{
  public class CRGOManager
  {
    /// <summary>
    /// GameObject - IdCaronte dictionaries
    /// </summary>
    CRBiDictionary<GameObject, uint> goToIdCaronte_    = new CRBiDictionary<GameObject, uint>(); //Bidirectional dictionary to translate GO - IDCaronte
    
    List<uint> listDeferredIdsToDelete_ = new List<uint>();
    List<int>  listObjectsIds_          = new List<int>();

    //-----------------------------------------------------------------------------------
    public void HierarchyChange()
    {
      RegisterUnityGameObjectInCaronte();
      ReleaseDeletedObjectsFromCaronte();
    }
    //-----------------------------------------------------------------------------------
    public void Clear()
    {
      goToIdCaronte_.Clear();
      listDeferredIdsToDelete_.Clear();
      listObjectsIds_.Clear();
      GOManager.unregisterAllGameObjects();    
    }
    //-----------------------------------------------------------------------------------
    public bool GetIdCaronteFromGO( GameObject go, out uint id )
    {
      return ( goToIdCaronte_.TryGetByFirst(go, out id ) );
    }
    //-----------------------------------------------------------------------------------
    public bool GetGOFromIdCaronte( uint id, out GameObject go )
    {
      return ( goToIdCaronte_.TryGetBySecond(id, out go ) );
    }
    //-----------------------------------------------------------------------------------
    private void RegisterUnityGameObjectInCaronte()
    {
      GameObject[] sceneObjects = CREditorUtils.GetAllGameObjectsInScene();
      int length = sceneObjects.Length;

      for (int i = 0; i < length; ++i)
      {
        GameObject go = sceneObjects[i];
        CREditorUtils.GetChildObjectsIds(go, listObjectsIds_);
        uint idCaronte;
        bool exists = goToIdCaronte_.TryGetByFirst(go, out idCaronte);
        if (!exists)
        {
          idCaronte = GOManager.RegisterGameObject( go.name, go.GetInstanceID(), listObjectsIds_.ToArray() );
          goToIdCaronte_.Add(go, idCaronte);
        }
        else
        {
          GOManager.ReregisterGameObject(idCaronte, go.name, go.GetInstanceID(), listObjectsIds_.ToArray() );
        }
      }
    }
    //-----------------------------------------------------------------------------------
    private void ReleaseDeletedObjectsFromCaronte()
    {
      foreach( var kvPair in goToIdCaronte_ )
      {
        GameObject go = kvPair.Key;
        uint id       = kvPair.Value;
        if (go == null)
        {
          listDeferredIdsToDelete_.Add(id);
        }
      }

      DeleteDeferredGameObjects();
    }
    //-----------------------------------------------------------------------------------
    private void DeleteDeferredGameObjects()
    {
      foreach( uint id in listDeferredIdsToDelete_ )
      {
        goToIdCaronte_.TryRemoveBySecond(id);
        GOManager.unregisterGameObject(id);      
      }

      listDeferredIdsToDelete_.Clear();
    }
    //-----------------------------------------------------------------------------------
  }
}