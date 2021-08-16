using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Condition))]
public class ConditionDrawer : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    EditorGUI.BeginProperty(position, label, property);

    var indent = EditorGUI.indentLevel;
    EditorGUI.indentLevel = 0;

    var conditionRect = new Rect(position.x, position.y, 150, EditorGUIUtility.singleLineHeight);
    var conditionValueRect = new Rect(position.x + 155, position.y, 200, EditorGUIUtility.singleLineHeight);

    var labelWidth = EditorGUIUtility.labelWidth;
    EditorGUIUtility.labelWidth = 30;
    EditorGUI.PropertyField(conditionRect, property.FindPropertyRelative("conditionType"), new GUIContent("if"));
    switch ((ConditionType)property.FindPropertyRelative("conditionType").enumValueIndex)
    {
      case ConditionType.TileType:
        EditorGUI.PropertyField(conditionValueRect, property.FindPropertyRelative("_tileType"), new GUIContent("is"));
        break;
      case ConditionType.TouchingTileWithTag:
        EditorGUI.PropertyField(conditionValueRect, property.FindPropertyRelative("_touchingTileType"), new GUIContent("is"));
        break;
      case ConditionType.ChargeLevel:
        EditorGUI.PropertyField(conditionValueRect, property.FindPropertyRelative("_chargeLevel"), new GUIContent("is"));
        break;
    }

    EditorGUIUtility.labelWidth = labelWidth;
    EditorGUI.indentLevel = indent;

    EditorGUI.EndProperty();
  }
  public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
  {
    return EditorGUIUtility.singleLineHeight;
  }
}

[CustomPropertyDrawer(typeof(Conditional<>))]
public class ConditionalDrawer : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    EditorGUI.BeginProperty(position, label, property);
    var indent = EditorGUI.indentLevel;
    EditorGUI.indentLevel = 0;
    var labelWidth = EditorGUIUtility.labelWidth;
    EditorGUIUtility.labelWidth = 40;
    var valueRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
    var conditionsRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("value"), new GUIContent("Value"));
    EditorGUIUtility.labelWidth = labelWidth;
    EditorGUI.PropertyField(conditionsRect, property.FindPropertyRelative("conditions"), GUIContent.none, true);
    EditorGUI.indentLevel = indent;

    EditorGUI.EndProperty();
  }
  public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
  {
    float height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("value"));
    SerializedProperty conditions = property.FindPropertyRelative("conditions");
    height += EditorGUI.GetPropertyHeight(conditions);
    return height;
  }
}