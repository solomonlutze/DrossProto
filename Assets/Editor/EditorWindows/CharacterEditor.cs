using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[CustomPropertyDrawer(typeof(TraitsLoadout_OLD))]
public class EquippedTraitsPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);
        EditorGUILayout.PrefixLabel(label);
        string[] allPassiveTraitNames = PropertyDrawerHelpers.AllPassiveTraitNames(includeNone: true);
        SerializedProperty[] slotProperties = {
          property.FindPropertyRelative("head"),
          property.FindPropertyRelative("thorax"),
          property.FindPropertyRelative("abdomen"),
          property.FindPropertyRelative("legs"),
          property.FindPropertyRelative("wings"),
        };
        var labelwidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 70;
        string[] choices = PropertyDrawerHelpers.AllPassiveTraitNames(includeNone: true);
        foreach (SerializedProperty slot in slotProperties)
        {
            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            int slotValIndex = Mathf.Max(Array.IndexOf(choices, slot.stringValue), 0);
            int idx = EditorGUILayout.Popup(slot.displayName, slotValIndex, allPassiveTraitNames);
            if (idx < 0) { idx = 0; }
            slot.stringValue = (choices[idx] == "[None]") ? null : choices[idx];
        }
        EditorGUIUtility.labelWidth = labelwidth;
        EditorGUI.EndProperty();
    }

}

[CustomEditor(typeof(Character))]
[CanEditMultipleObjects]
public class CharacterEditor : Editor
{
    SerializedProperty initialskill1;
    SerializedProperty initialskill2;

    SerializedProperty equippedTraitsDictionary;
    void OnEnable()
    {
        // equippedTraitsDictionary = serializedObject.FindProperty("equippedTraitsDictionary");
        // initialskill1 = serializedObject.FindProperty("initialskill1");
        // initialskill2 = serializedObject.FindProperty("initialskill2");
    }
    public override void OnInspectorGUI()
    {
        this.serializedObject.Update();
        DrawDefaultInspector();
        // DrawTraitSelect();
        this.serializedObject.ApplyModifiedProperties();
    }

    private void DrawPopup(string[] choices, SerializedProperty serializedProperty, string popupText)
    {
        int idx = Mathf.Max(0, Array.IndexOf(choices, serializedProperty.stringValue));
        idx = EditorGUILayout.Popup(popupText, idx, choices);
        if (idx < 0)
            idx = 0;

        serializedProperty.stringValue = (choices[idx] == "[None]") ? null : choices[idx];
    }

    // private void DrawTraitSelect()
    // {
    //   string[] allActiveTraitNames = PropertyDrawerHelpers.AllActiveTraitNames(includeNone: true);
    //   DrawPopup(allActiveTraitNames, initialskill1, "Initial Active Trait 1");
    //   DrawPopup(allActiveTraitNames, initialskill2, "Initial Active Trait 2");
    // }

}