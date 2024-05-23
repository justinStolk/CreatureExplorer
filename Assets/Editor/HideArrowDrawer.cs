using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(HideArrowAttribute))]
public class HideArrowDrawer : PropertyDrawer
{
    static readonly GUIContent DropdownIcon = EditorGUIUtility.IconContent("icon dropdown");


    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    { 
        if (prop.propertyType == SerializedPropertyType.Boolean)
        {
            prop.boolValue = EditorGUI.Foldout(position, prop.boolValue, label.text, EditorStyles.foldoutHeader);
        } else
        {
            Debug.LogWarning("Cannot draw dropdown arrow for non boolean field: " + prop.displayName);
        }
    }
}
#endif
