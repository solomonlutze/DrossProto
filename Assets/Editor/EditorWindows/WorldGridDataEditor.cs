using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(WorldGridData))]
[CanEditMultipleObjects]
public class WorldGridDataEditor : Editor
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

    WorldGridData worldGridData = target as WorldGridData;
    serializedObject.Update();
    DrawDefaultInspector();
    if (GUILayout.Button("Recalculate Bounds"))
    {
      worldGridData.RecalculateBounds();
    }
    serializedObject.ApplyModifiedProperties();
  }

}