// using UnityEditor;
// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;
// using System;

// [CustomEditor(typeof(TraitItem))]
// [CanEditMultipleObjects]
// public class TraitItemEditor : Editor 
// {
//   SerializedProperty activeTrait;
//   SerializedProperty passiveTrait;
//   private int idx;
//   private string oldItemType;
//   void OnEnable()
//   {
//     activeTrait = serializedObject.FindProperty("activeTrait");
//     passiveTrait = serializedObject.FindProperty("passiveTrait");
//   }
//   public override void OnInspectorGUI() 
//   {
    
//       serializedObject.Update();
//       DrawDefaultInspector ();
//       string[] choices = EditorHelpers.AllItemsOfType(((InventoryItemType) itemType.intValue).ToString());
//       idx = EditorGUILayout.Popup("Item Id", idx, choices);
//       if (idx < 0)
//           idx = 0;
//       itemId.stringValue = choices[idx];
//       serializedObject.ApplyModifiedProperties();
//   }

// }