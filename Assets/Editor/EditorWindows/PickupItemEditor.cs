using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(PickupItem))]
[CanEditMultipleObjects]
public class PickupItemEditor : Editor 
{
  SerializedProperty itemId;
  SerializedProperty itemType;
  private int idx;
  void OnEnable()
  {
    itemId = serializedObject.FindProperty("itemId");
    itemType = serializedObject.FindProperty("itemType");
  }
  public override void OnInspectorGUI() 
  {
    
      serializedObject.Update();
      DrawDefaultInspector ();
      string[] choices = EditorHelpers.AllItemsOfType(((InventoryItemType) itemType.intValue).ToString());
      idx = Mathf.Max (0, Array.IndexOf (choices, itemId.stringValue));
      idx = EditorGUILayout.Popup("Item Id", idx, choices);
      if (idx < 0)
          idx = 0;
      itemId.stringValue = choices[idx];
      serializedObject.ApplyModifiedProperties();
  }

}