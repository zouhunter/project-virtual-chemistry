using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using UnityEditorInternal;
namespace WorldActionSystem
{
    public partial class ActionEditorUtility
    {
        public static void LoadmatrixInfo(SerializedProperty matrixProp, Transform transform)
        {
            var materix = Matrix4x4.identity;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    materix[i, j] = matrixProp.FindPropertyRelative("e" + i + "" + j).floatValue;
                }
            }
            transform.localPosition = materix.GetColumn(0);
            transform.localEulerAngles = materix.GetColumn(1);
            transform.localScale = materix.GetColumn(2);
        }
        public static void SaveMatrixInfo(SerializedProperty matrixProp, Transform transform)
        {
            var materix = Matrix4x4.identity;
            materix.SetColumn(0, transform.localPosition);
            materix.SetColumn(1, transform.localEulerAngles);
            materix.SetColumn(2, transform.localScale);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    matrixProp.FindPropertyRelative("e" + i + "" + j).floatValue = materix[i, j];
                }
            }
        }
        public static void ApplyPrefab(GameObject gitem)
        {
            var instanceRoot = PrefabUtility.FindValidUploadPrefabInstanceRoot(gitem);
            var prefab = PrefabUtility.GetPrefabParent(instanceRoot);
            if (prefab != null)
            {
                if (prefab.name == gitem.name)
                {
                    PrefabUtility.ReplacePrefab(gitem, prefab, ReplacePrefabOptions.ConnectToPrefab);
                }
            }
        }

        internal static void LoadPrefab(SerializedProperty prefabProp,SerializedProperty ct_commandProp,SerializedProperty ct_pickProp, SerializedProperty instanceIDProp, SerializedProperty rematrixProp, SerializedProperty matrixProp)
        {
            if (prefabProp.objectReferenceValue == null)
            {
                return;
            }

            if (instanceIDProp.intValue != 0)
            {
                var gitem = EditorUtility.InstanceIDToObject(instanceIDProp.intValue);

                if (gitem != null)
                {
                    return;
                }
            }

            GameObject gopfb = prefabProp.objectReferenceValue as GameObject;
            if (gopfb != null)
            {
                var actionSystem = GameObject.FindObjectOfType<ActionGroup>();
                var parent = actionSystem == null ? null : actionSystem.transform;
                parent = Utility.GetParent(parent, ct_commandProp.boolValue, ct_pickProp.boolValue);
                GameObject go = PrefabUtility.InstantiatePrefab(gopfb) as GameObject;

                instanceIDProp.intValue = go.GetInstanceID();

               
                go.transform.SetParent(parent, false);

                if (rematrixProp.boolValue)
                {
                    LoadmatrixInfo(matrixProp, go.transform);
                }
            }
        }

        internal static void SavePrefab(SerializedProperty instanceIDProp, SerializedProperty rematrixProp, SerializedProperty matrixProp)
        {
            var gitem = EditorUtility.InstanceIDToObject(instanceIDProp.intValue);
            if (gitem != null)
            {
                if (rematrixProp.boolValue)
                {
                    ActionEditorUtility.SaveMatrixInfo(matrixProp, (gitem as GameObject).transform);
                }
                ActionEditorUtility.ApplyPrefab(gitem as GameObject);
                GameObject.DestroyImmediate(gitem);
            }
            instanceIDProp.intValue = 0;
        }

        internal static void InsertItem(SerializedProperty prefabProp, UnityEngine.Object obj)
        {
            var prefab = PrefabUtility.GetPrefabParent(obj);
            if (prefab != null)
            {
                prefabProp.objectReferenceValue = PrefabUtility.FindPrefabRoot(prefab as GameObject);
            }
            else
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path))
                {
                    prefabProp.objectReferenceValue = obj;
                }
            }
        }
        public static void ResetValue(SerializedProperty property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = 0;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = false;
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = 0f;
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = "";
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = Color.black;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = null;
                    break;
                case SerializedPropertyType.LayerMask:
                    property.intValue = 0;
                    break;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = 0;
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = default(Vector2);
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = default(Vector3);
                    break;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = default(Vector4);
                    break;
                case SerializedPropertyType.Rect:
                    property.rectValue = default(Rect);
                    break;
                case SerializedPropertyType.ArraySize:
                    property.intValue = 0;
                    break;
                case SerializedPropertyType.Character:
                    property.intValue = 0;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                    break;
                case SerializedPropertyType.Bounds:
                    property.boundsValue = default(Bounds);
                    break;
                case SerializedPropertyType.Gradient:
                    //!TODO: Amend when Unity add a public API for setting the gradient.
                    break;
            }

            if (property.isArray)
            {
                property.arraySize = 0;
            }

            ResetChildPropertyValues(property);
        }

        private static void ResetChildPropertyValues(SerializedProperty element)
        {
            if (!element.hasChildren)
                return;

            var childProperty = element.Copy();
            int elementPropertyDepth = element.depth;
            bool enterChildren = true;

            while (childProperty.Next(enterChildren) && childProperty.depth > elementPropertyDepth)
            {
                enterChildren = false;
                ResetValue(childProperty);
            }
        }

        /// <summary>
        /// Copies value of <paramref name="sourceProperty"/> into <pararef name="destProperty"/>.
        /// </summary>
        /// <param name="destProperty">Destination property.</param>
        /// <param name="sourceProperty">Source property.</param>
        public static void CopyPropertyValue(SerializedProperty destProperty, SerializedProperty sourceProperty)
        {
            if (destProperty == null)
                throw new ArgumentNullException("destProperty");
            if (sourceProperty == null)
                throw new ArgumentNullException("sourceProperty");

            sourceProperty = sourceProperty.Copy();
            destProperty = destProperty.Copy();

            CopyPropertyValueSingular(destProperty, sourceProperty);

            if (sourceProperty.hasChildren)
            {
                int elementPropertyDepth = sourceProperty.depth;
                while (sourceProperty.Next(true) && destProperty.Next(true) && sourceProperty.depth > elementPropertyDepth)
                    CopyPropertyValueSingular(destProperty, sourceProperty);
            }
        }

        private static void CopyPropertyValueSingular(SerializedProperty destProperty, SerializedProperty sourceProperty)
        {
            switch (destProperty.propertyType)
            {
                case SerializedPropertyType.Integer:
                    destProperty.intValue = sourceProperty.intValue;
                    break;
                case SerializedPropertyType.Boolean:
                    destProperty.boolValue = sourceProperty.boolValue;
                    break;
                case SerializedPropertyType.Float:
                    destProperty.floatValue = sourceProperty.floatValue;
                    break;
                case SerializedPropertyType.String:
                    destProperty.stringValue = sourceProperty.stringValue;
                    break;
                case SerializedPropertyType.Color:
                    destProperty.colorValue = sourceProperty.colorValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    destProperty.objectReferenceValue = sourceProperty.objectReferenceValue;
                    break;
                case SerializedPropertyType.LayerMask:
                    destProperty.intValue = sourceProperty.intValue;
                    break;
                case SerializedPropertyType.Enum:
                    destProperty.enumValueIndex = sourceProperty.enumValueIndex;
                    break;
                case SerializedPropertyType.Vector2:
                    destProperty.vector2Value = sourceProperty.vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    destProperty.vector3Value = sourceProperty.vector3Value;
                    break;
                case SerializedPropertyType.Vector4:
                    destProperty.vector4Value = sourceProperty.vector4Value;
                    break;
                case SerializedPropertyType.Rect:
                    destProperty.rectValue = sourceProperty.rectValue;
                    break;
                case SerializedPropertyType.ArraySize:
                    destProperty.intValue = sourceProperty.intValue;
                    break;
                case SerializedPropertyType.Character:
                    destProperty.intValue = sourceProperty.intValue;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    destProperty.animationCurveValue = sourceProperty.animationCurveValue;
                    break;
                case SerializedPropertyType.Bounds:
                    destProperty.boundsValue = sourceProperty.boundsValue;
                    break;
                case SerializedPropertyType.Gradient:
                    //!TODO: Amend when Unity add a public API for setting the gradient.
                    break;
            }
        }

    }
}