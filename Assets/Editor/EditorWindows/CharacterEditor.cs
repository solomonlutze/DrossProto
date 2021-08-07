using UnityEditor;
using UnityEngine;
using System;

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
  // public override void OnInspectorGUI()
  // {
  //   this.serializedObject.Update();
  //   DrawDefaultInspector();
  //   // DrawTraitSelect();
  //   this.serializedObject.ApplyModifiedProperties();
  // }

  private void DrawPopup(string[] choices, SerializedProperty serializedProperty, string popupText)
  {
    int idx = Mathf.Max(0, Array.IndexOf(choices, serializedProperty.stringValue));
    idx = EditorGUILayout.Popup(popupText, idx, choices);
    if (idx < 0)
      idx = 0;

    serializedProperty.stringValue = (choices[idx] == "[None]") ? null : choices[idx];
  }

}