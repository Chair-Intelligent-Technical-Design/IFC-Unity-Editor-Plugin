#if (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(IfcPropertyData))]
public class IfcPropertyDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float width = 150;
        Rect materialPosition = new Rect(position.x, position.y, width, position.height);
        EditorGUI.PropertyField(materialPosition, property.FindPropertyRelative(nameof(IfcPropertyData.Name)), GUIContent.none);
        materialPosition = new Rect(position.x + width + 5, position.y, width, position.height);
        EditorGUI.PropertyField(materialPosition, property.FindPropertyRelative(nameof(IfcPropertyData.PropertyValue)), GUIContent.none);
    }
}
#endif