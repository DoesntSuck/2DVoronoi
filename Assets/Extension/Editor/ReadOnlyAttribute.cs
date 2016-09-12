using System;
using UnityEditor;

namespace UnityEngine
{
    /// <summary>
    /// Uneditable inspector property. Code courtesy of Lev-Lukomskyi: 
    /// http://answers.unity3d.com/questions/489942/how-to-make-a-readonly-property-in-inspector.html
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute
    {

    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string valueStr;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    valueStr = property.intValue.ToString();
                    break;
                case SerializedPropertyType.Boolean:
                    valueStr = property.boolValue.ToString();
                    break;
                case SerializedPropertyType.Float:
                    valueStr = property.floatValue.ToString("0.00000");
                    break;
                case SerializedPropertyType.String:
                    valueStr = property.stringValue;
                    break;
                default:
                    throw new ArgumentException("Type not supported");
            }

            EditorGUI.LabelField(position, label.text, valueStr);
        }
    }
}