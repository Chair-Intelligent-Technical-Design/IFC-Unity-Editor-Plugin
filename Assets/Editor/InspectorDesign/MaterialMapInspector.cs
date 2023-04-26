#if (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Visualization of the material map in the inspector
/// </summary>
[CustomEditor(typeof(MaterialMap))]
public class MaterialMapInspector : Editor
{

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty listProperty = serializedObject.FindProperty(nameof(MaterialMap.Mapping));
        for (int idx = 0; idx < listProperty.arraySize; idx++)
        {
            SerializedProperty element = listProperty.GetArrayElementAtIndex(idx);
            EditorGUILayout.PropertyField(element);
        }

        if (GUILayout.Button("Apply Materials")) //button pressed
        {
            MaterialMap map = (MaterialMap)serializedObject.targetObject;
            map.ApplyMaterials();
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif