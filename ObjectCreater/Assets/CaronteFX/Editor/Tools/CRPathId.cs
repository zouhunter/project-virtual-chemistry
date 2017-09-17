using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CaronteFX
{
  [System.Serializable]
  public class CRPathId
  {
    [SerializeField]
    List<string> names_ = new List<string>();

    [SerializeField]
    List<int> indexes_ = new List<int>();

    public void AddPartialPath(string name, int index)
    {
      names_.Add(name);
      indexes_.Add(index);
    }

    public GameObject GetGameObjectReference(List<GameObject> rootGameObjects)
    {
      int numIndexes = indexes_.Count;
      int it         = numIndexes - 1;

      GameObject[] arrGameObjects = rootGameObjects.ToArray();


      return ( SearchReference(arrGameObjects, it) );

    }

    private GameObject SearchReference(GameObject[] arrGameObject, int it )
    {
      int currentIndex = indexes_[it];
      string currentName = names_[it];
      int numChilds = arrGameObject.Length;
      if (currentIndex > numChilds) return null;

      GameObject gameObject = arrGameObject[currentIndex];
      if (gameObject.name != currentName) return null;

      arrGameObject = gameObject.GetChildObjects();
      if (it == 0)
      {
        return gameObject;
      }
      else
      {
        SearchReference(arrGameObject, --it);
      }

      return null;
    }

  }
}