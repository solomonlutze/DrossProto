using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Overrideable<>))]
public class OverrideableDrawer : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    EditorGUI.BeginProperty(position, label, property);

    // EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(label));

    SerializedProperty overrides = property.FindPropertyRelative("overrides");
    var indent = EditorGUI.indentLevel;
    EditorGUI.indentLevel = indent + 1;

    bool hasOverrides = overrides.arraySize > 0;
    var defaultRect = new Rect(position.x, position.y, position.width - (hasOverrides ? 0 : 25), EditorGUIUtility.singleLineHeight);
    EditorGUI.PropertyField(defaultRect, property.FindPropertyRelative("defaultValue"), label.text == "Value" ? GUIContent.none : new GUIContent(label));
    if (!hasOverrides)
    {
      var addOverrideButtonRect = new Rect(position.x + position.width - 25, position.y, 25, EditorGUIUtility.singleLineHeight);
      if (GUI.Button(addOverrideButtonRect, "+"))
      {
        property.FindPropertyRelative("overrides");
        overrides.arraySize = 1;
      }
    }
    else
    {
      var overridesRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width - (hasOverrides ? 0 : 50), EditorGUIUtility.singleLineHeight);
      EditorGUI.PropertyField(overridesRect, overrides, GUIContent.none, true);
    }

    EditorGUI.indentLevel = indent;

    EditorGUI.EndProperty();
  }
  public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
  {
    float height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("defaultValue"));
    SerializedProperty overrides = property.FindPropertyRelative("overrides");
    if (overrides.arraySize > 0)
    {
      height += EditorGUI.GetPropertyHeight(overrides);
    }
    return height;
  }
}
