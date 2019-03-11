using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(Character))]
[CanEditMultipleObjects]
public class CharacterEditor : Editor 
{
  SerializedProperty initialWeaponId;
  SerializedProperty initialPassiveTrait1;
  SerializedProperty initialPassiveTrait2;
  SerializedProperty initialActiveTrait1;
  SerializedProperty initialActiveTrait2;
  void OnEnable()
  {
    initialWeaponId = serializedObject.FindProperty("initialWeaponId");
    initialPassiveTrait1 = serializedObject.FindProperty("initialPassiveTrait1");
    initialPassiveTrait2 = serializedObject.FindProperty("initialPassiveTrait2");
    initialActiveTrait1 = serializedObject.FindProperty("initialActiveTrait1");
    initialActiveTrait2 = serializedObject.FindProperty("initialActiveTrait2");
  }
  public override void OnInspectorGUI() 
  { 
      this.serializedObject.Update();
      DrawDefaultInspector ();
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
        DrawPopup(PropertyDrawerHelpers.AllPassiveTraitNames(includeNone: true), initialPassiveTrait1, "Initial Passive Trait 1");
        DrawPopup(PropertyDrawerHelpers.AllPassiveTraitNames(includeNone: true), initialPassiveTrait2, "Initial Passive Trait 2");
        DrawPopup(PropertyDrawerHelpers.AllActiveTraitNames(includeNone: true), initialActiveTrait1, "Initial Active Trait 1");
        DrawPopup(PropertyDrawerHelpers.AllActiveTraitNames(includeNone: true), initialActiveTrait2, "Initial Active Trait 2");
    }

}