using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[CustomPropertyDrawer(typeof(EquippedTraitsStringNames))]
public class EquippedTraitsPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);
        EditorGUILayout.PrefixLabel(label);
        SerializedProperty[] slotProperties = {
          property.FindPropertyRelative("head"),
          property.FindPropertyRelative("thorax"),
          property.FindPropertyRelative("abdomen"),
          property.FindPropertyRelative("legs"),
          property.FindPropertyRelative("wings"),
        };
        var labelwidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 70;
        foreach(SerializedProperty slot in slotProperties) {
          // Draw fields - passs GUIContent.none to each so they are drawn without labels
          string[] choices = PropertyDrawerHelpers.AllPassiveTraitNames(includeNone: true);
          int slotValIndex = Mathf.Max(Array.IndexOf(choices, slot.stringValue), 0);
          int idx = EditorGUILayout.Popup(slot.displayName, slotValIndex, PropertyDrawerHelpers.AllPassiveTraitNames(includeNone: true));
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
  SerializedProperty initialWeaponId;
  SerializedProperty initialActiveTrait1;
  SerializedProperty initialActiveTrait2;

  SerializedProperty equippedTraitsDictionary;
  SerializedProperty equippedTraits;
  void OnEnable()
  {
    initialWeaponId = serializedObject.FindProperty("initialWeaponId");
    equippedTraitsDictionary = serializedObject.FindProperty("equippedTraitsDictionary");
    initialActiveTrait1 = serializedObject.FindProperty("initialActiveTrait1");
    initialActiveTrait2 = serializedObject.FindProperty("initialActiveTrait2");
  }
  public override void OnInspectorGUI()
  {
      this.serializedObject.Update();
      DrawDefaultInspector();
      DrawWeaponSelect();
      DrawTraitSelect();
      this.serializedObject.ApplyModifiedProperties();
  }

    private void DrawPopup(string[] choices, SerializedProperty serializedProperty, string popupText) {
        int idx = Mathf.Max (0, Array.IndexOf (choices, serializedProperty.stringValue));
        idx = EditorGUILayout.Popup(popupText, idx, choices);
        if (idx < 0)
            idx = 0;

        serializedProperty.stringValue = (choices[idx] == "[None]") ? null : choices[idx];
    }

    private void DrawWeaponSelect() {
        DrawPopup(EditorHelpers.AllWeapons(includeNone: true), initialWeaponId, "Initial weapon ID");
    }

    private void DrawTraitSelect() {
        DrawPopup(PropertyDrawerHelpers.AllActiveTraitNames(includeNone: true), initialActiveTrait1, "Initial Active Trait 1");
        DrawPopup(PropertyDrawerHelpers.AllActiveTraitNames(includeNone: true), initialActiveTrait2, "Initial Active Trait 2");
    }

}