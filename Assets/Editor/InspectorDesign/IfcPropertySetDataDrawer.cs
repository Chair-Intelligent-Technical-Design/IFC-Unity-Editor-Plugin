#if (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IfcPropertySetData))]
public class IfcPropertySetDataDrawer : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty groupNameProperty = serializedObject.FindProperty(nameof(IfcPropertySetData.PropertySetName));
        EditorGUILayout.LabelField(groupNameProperty.stringValue, EditorStyles.boldLabel);

        SerializedProperty labelProperty = serializedObject.FindProperty(nameof(IfcPropertySetData.Label));
        EditorGUILayout.PropertyField(labelProperty);

        SerializedProperty idProperty = serializedObject.FindProperty(nameof(IfcPropertySetData.Id));
        EditorGUILayout.PropertyField(idProperty);

        SerializedProperty listProperty = serializedObject.FindProperty(nameof(IfcPropertySetData.Properties));
        for (int idx = 0; idx < listProperty.arraySize; idx++)
        {
            SerializedProperty element = listProperty.GetArrayElementAtIndex(idx);
            EditorGUILayout.PropertyField(element);
        }
        serializedObject.ApplyModifiedProperties();
    }
}

#endif