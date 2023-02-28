using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Condition))]
public class ConditionDrawer : PropertyDrawer
{

  // NOTE: these must be in the same order as their Comparator counterparts or this doesn't work!
  string[] _comparatorChoices = new[] { "=", "<", "<=", ">", ">=", "!=" };
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    EditorGUI.BeginProperty(position, label, property);

    var indent = EditorGUI.indentLevel;
    EditorGUI.indentLevel = indent + 1;

    var labelWidth = EditorGUIUtility.labelWidth;
    EditorGUIUtility.labelWidth = 40;
    var conditionRect = new Rect(position.x, position.y, 150, EditorGUIUtility.singleLineHeight);
    var conditionValueRect = new Rect(position.x + 155, position.y, 200, EditorGUIUtility.singleLineHeight);
    var comparatorValueRect = new Rect(position.x + 155, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
    var conditionWithComparatorValueRect = new Rect(position.x + 155 + EditorGUIUtility.labelWidth, position.y, 200 - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

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
        {
          int _comparatorIndex = EditorGUI.Popup(comparatorValueRect, property.FindPropertyRelative("comparator").intValue, _comparatorChoices);
          property.FindPropertyRelative("comparator").intValue = _comparatorIndex;
          EditorGUI.PropertyField(conditionWithComparatorValueRect, property.FindPropertyRelative("_chargeLevel"), new GUIContent(""));
          break;
        }
      case ConditionType.CurrentFoodCount:
        {
          int _comparatorIndex = EditorGUI.Popup(comparatorValueRect, property.FindPropertyRelative("comparator").intValue, _comparatorChoices);
          property.FindPropertyRelative("comparator").intValue = _comparatorIndex;
          EditorGUI.PropertyField(conditionWithComparatorValueRect, property.FindPropertyRelative("_currentFoodCount"), new GUIContent(""));
          break;
        }
      case ConditionType.MoveInputNormalizedMagnitude:
        {
          int _comparatorIndex = EditorGUI.Popup(comparatorValueRect, property.FindPropertyRelative("comparator").intValue, _comparatorChoices);
          property.FindPropertyRelative("comparator").intValue = _comparatorIndex;
          EditorGUI.PropertyField(conditionWithComparatorValueRect, property.FindPropertyRelative("_moveInputMagnitude"), new GUIContent(""));
          break;
        }
      case ConditionType.VelocityNormalizedMagnitude:
        {
          int _comparatorIndex = EditorGUI.Popup(comparatorValueRect, property.FindPropertyRelative("comparator").intValue, _comparatorChoices);
          property.FindPropertyRelative("comparator").intValue = _comparatorIndex;
          EditorGUI.PropertyField(conditionWithComparatorValueRect, property.FindPropertyRelative("_velocityMagnitude"), new GUIContent(""));
          break;
        }
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
    EditorGUI.indentLevel = indent + 1;
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