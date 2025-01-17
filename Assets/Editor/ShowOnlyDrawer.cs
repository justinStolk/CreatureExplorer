using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
public class ShowOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        string valueStr;

        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer:
                valueStr = prop.intValue.ToString();
                break;
            case SerializedPropertyType.Boolean:
                valueStr = prop.boolValue.ToString();
                break;
            case SerializedPropertyType.Float:
                string decimalString = "";
                for (int i = 0; i < (attribute as ShowOnlyAttribute).decimals; i++) 
                {
                    decimalString += "0";
                }
                valueStr = prop.floatValue.ToString("0." + decimalString);
                break;
            case SerializedPropertyType.String:
                valueStr = prop.stringValue;
                break;
            case SerializedPropertyType.Vector2:
                valueStr = prop.vector2Value.ToString();
                break;
            case SerializedPropertyType.Vector3:
                valueStr = prop.vector3Value.ToString();
                break; 
            case SerializedPropertyType.Enum: 
                valueStr = prop.enumNames[prop.enumValueIndex]; 
                break;
            case SerializedPropertyType.ObjectReference:
                try
                {
                    valueStr = prop.objectReferenceValue.ToString();
                }
                catch (NullReferenceException)
                {
                    valueStr = "None (Game Object)";
                }
                break;
            default:
                valueStr = "(not supported)";
                break;
        }

        float indent = (attribute as ShowOnlyAttribute).indent;
        if (indent >= 0)
        {
            EditorGUI.LabelField(position, label.text);

            Rect valuePosition = new Rect(position.x + position.width * indent, position.y, position.width * (1 - indent), position.height);
            EditorGUI.LabelField(valuePosition, valueStr);
        } 
        else 
            EditorGUI.LabelField(position, label.text, valueStr);
    }
}
