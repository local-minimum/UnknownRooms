using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ProcRoom.FloatRange), true)]
public class FloatRangeProperty : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);
        Rect rect = EditorGUI.PrefixLabel(position, label);
        rect.width *= 0.3f;
        EditorGUI.indentLevel = 0;
        EditorGUIUtility.labelWidth = 14f;
        EditorGUI.BeginChangeCheck();
        var min = EditorGUI.FloatField(rect, property.FindPropertyRelative("_min").floatValue);
        if (EditorGUI.EndChangeCheck())
        {
            property.FindPropertyRelative("_min").floatValue = Mathf.Min(min, property.FindPropertyRelative("_max").floatValue);
        }
        rect.x += rect.width + 3;
        EditorGUI.LabelField(rect, "-");
        rect.x += 14 + 3;
        EditorGUI.BeginChangeCheck();
        var max = EditorGUI.FloatField(rect, property.FindPropertyRelative("_max").floatValue);
        if (EditorGUI.EndChangeCheck())
        {
            property.FindPropertyRelative("_max").floatValue = Mathf.Max(max, property.FindPropertyRelative("_min").floatValue);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 20f;
    }
}
