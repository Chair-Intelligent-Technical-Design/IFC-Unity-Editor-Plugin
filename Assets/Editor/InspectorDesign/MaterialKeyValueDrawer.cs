#if (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// property drawer for the material mappings
/// </summary>
[CustomPropertyDrawer(typeof(MaterialMapKeyValue))]
public class MaterialKeyValueDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float width = 150;
        Rect materialPosition = new Rect(position.x, position.y, width, position.height);
        EditorGUI.PropertyField(materialPosition, property.FindPropertyRelative(nameof(MaterialMapKeyValue.IfcMaterialName)), GUIContent.none);
        materialPosition = new Rect(position.x + width + 5, position.y, width, position.height);
        EditorGUI.PropertyField(materialPosition, property.FindPropertyRelative(nameof(MaterialMapKeyValue.CorrespondingMaterial)), GUIContent.none);
    }
}

#endif