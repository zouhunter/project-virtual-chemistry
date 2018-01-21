using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Sprites;
using UnityEngine.Scripting;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Assertions.Must;
using UnityEngine.Assertions.Comparers;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
    public static class Utility
    {
        public const string commandParentName = "Triggers";
        public const string pickupParentName = "Elements";

        internal static void SurchSystem(this Transform trans, ref ActionGroup system)
        {
            if (system == null)
            {
                system = trans.GetComponentInParent<ActionGroup>();
            }
        }
        public static void RetriveCommand(Transform trans, UnityAction<ActionCommand> onRetive)
        {
            var coms = trans.GetComponents<ActionCommand>();
            if (coms != null && coms.Length > 0)
            {
                foreach (var com in coms)
                {
                    onRetive(com);
                }
                return;
            }
            else
            {
                foreach (Transform child in trans)
                {
                    RetriveCommand(child, onRetive);
                }
            }

        }

        public static void RetivePickElement(Transform trans, UnityAction<PickUpAbleElement> onRetive)
        {
            if (!trans.gameObject.activeSelf) return;
            var com = trans.GetComponent<PickUpAbleElement>();
            if (com)
            {
                onRetive(com);
                return;
            }
            else
            {
                foreach (Transform child in trans)
                {
                    RetivePickElement(child, onRetive);
                }
            }

        }

        public static void CreateRunTimeObjects(Transform transform, List<ActionPrefabItem> prefabList)
        {
           
            foreach (var item in prefabList)
            {
                if (item.ignore) continue;

                var parent = GetParent(transform, item.containsCommand, item.containsPickup);

                var created = CreateRunTimeObject(item.prefab, parent);

                if (item.rematrix)
                {
                    TransUtil.LoadmatrixInfo(item.matrix, created.transform);
                }
            }
        }
        public static Transform GetParent(Transform transform, bool containsCommand, bool containsPickup)
        {
            Transform parent = transform;
            if (containsCommand)
            {
                var commandParent = transform == null ?null: transform.Find(commandParentName);
                if (commandParent == null)
                {
                    commandParent = new GameObject(commandParentName).transform;
                    commandParent.SetParent(transform, false);
                }
                parent = commandParent;
            }
            else if (containsPickup)
            {
                var pickupParent = transform == null ? null : transform.Find(pickupParentName);
                if (pickupParent == null)
                {
                    pickupParent = new GameObject(pickupParentName).transform;
                    pickupParent.SetParent(transform, false);
                }
                parent = pickupParent;
            }
            return parent;
        }
        private static GameObject CreateRunTimeObject(GameObject prefab,Transform parent)
        {
            prefab.gameObject.SetActive(true);
            var created = GameObject.Instantiate(prefab);
            created.name = prefab.name;
            created.transform.SetParent(parent, false);
            return created;
        }
    }
}
