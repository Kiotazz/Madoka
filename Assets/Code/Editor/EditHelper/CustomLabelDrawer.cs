using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CustomLabelAttribute))]
public class CustomLabelDrawer : PropertyDrawer
{
    private GUIContent _label = null;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (null == _label) _label = new GUIContent((attribute as CustomLabelAttribute).name);
        EditorGUI.PropertyField(position, property, _label);
    }
}
