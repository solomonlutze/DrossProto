using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TraitEffect))]
public class TraitEffectPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        float res = EditorGUIUtility.singleLineHeight * 2;
        TraitEffectType traitEffectType = (TraitEffectType) property.FindPropertyRelative("effectType").enumValueIndex;
        switch (traitEffectType)
        {
            case TraitEffectType.Resistance:
                res += EditorGUIUtility.singleLineHeight;
                break;
            case TraitEffectType.AnimationInput:
                res += EditorGUIUtility.singleLineHeight * 2;
                break;
            case TraitEffectType.CharacterStat:
                res += EditorGUIUtility.singleLineHeight;
                break;
            case TraitEffectType.CharacterMovementAbility:
                res += EditorGUIUtility.singleLineHeight;
                break;
        }
        return res;

    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);;

        // set indentation
        int indent = EditorGUI.indentLevel;
        float labelWidth = EditorGUIUtility.labelWidth;
        EditorGUI.indentLevel = 0;
        EditorGUIUtility.labelWidth = 120;
        Rect effectTypeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(effectTypeRect, property.FindPropertyRelative("effectType"), new GUIContent("Effect Type: "));
        TraitEffectType traitEffectType = (TraitEffectType) property.FindPropertyRelative("effectType").enumValueIndex;
        switch (traitEffectType)
        {
            case TraitEffectType.Resistance:
                Rect resistanceRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, (position.width / 3 * 2), EditorGUIUtility.singleLineHeight);
                Rect resistanceMagnitudeRect = new Rect(position.x+5+(position.width/3 * 2), position.y + EditorGUIUtility.singleLineHeight, 50, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(resistanceRect, property.FindPropertyRelative("damageType"), new GUIContent("Damage Type: "));
                EditorGUI.PropertyField(resistanceMagnitudeRect, property.FindPropertyRelative("magnitude"), GUIContent.none);
                break;

            case TraitEffectType.DetectionRange:
                Rect detectionRangeMagnitudeRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, (position.width), EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(detectionRangeMagnitudeRect, property.FindPropertyRelative("magnitude"),  new GUIContent("Magnitude: "));
                break;
            case TraitEffectType.AnimationInput:
                Rect animationInputRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
                Rect blocksMovementRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(animationInputRect, property.FindPropertyRelative("animationInput"), new GUIContent("Animation Input: "));
                EditorGUI.PropertyField(blocksMovementRect, property.FindPropertyRelative("blocksMovement"), new GUIContent("Blocks Movement: "));
                break;
            case TraitEffectType.CharacterStat:
                Rect statRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, (position.width / 3 * 2), EditorGUIUtility.singleLineHeight);
                Rect statMagnitudeRect = new Rect(position.x+5+(position.width/3 * 2), position.y + EditorGUIUtility.singleLineHeight, 50, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(statRect, property.FindPropertyRelative("stat"), new GUIContent("Stat: "));
                EditorGUI.PropertyField(statMagnitudeRect, property.FindPropertyRelative("magnitude"), GUIContent.none);
                break;
            case TraitEffectType.CharacterMovementAbility:
                Rect movementAbilityRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(movementAbilityRect, property.FindPropertyRelative("movementAbility"), new GUIContent("Movement Ability: "));
                break;
            case TraitEffectType.CharacterAttack:
                Rect attackRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, (position.width / 3 * 2), EditorGUIUtility.singleLineHeight);
                Rect attackMagnitudeRect = new Rect(position.x+5+(position.width/3 * 2), position.y + EditorGUIUtility.singleLineHeight, 50, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(attackRect, property.FindPropertyRelative("attackValue"), new GUIContent("Attack Value: "));
                EditorGUI.PropertyField(attackMagnitudeRect, property.FindPropertyRelative("magnitude"), GUIContent.none);
                break;

        }
        // reset indent level
        EditorGUI.indentLevel = indent;
        EditorGUIUtility.labelWidth = labelWidth;
        EditorGUI.EndProperty();
    }
}