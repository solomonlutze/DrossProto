using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(WorldGridData))]
[CanEditMultipleObjects]
public class WorldGridDataEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();
    // if (GUILayout.Button("Rebuild World Grid Data"))
    // {
    //   WorldGridData worldGridData = target as WorldGridData;
    //   worldGridData.RebuildWorldGridData();
    // }
    if (GUILayout.Button("Clear placed game objects"))
    {
      WorldGridData worldGridData = target as WorldGridData;
      worldGridData.ClearExistingPlacedObjects();
    }
    if (GUILayout.Button("Rebuild placed objects"))
    {
      WorldGridData worldGridData = target as WorldGridData;
      worldGridData.RebuildPlacedObjects();
    }
    if (GUILayout.Button("Count placed objects"))
    {
      WorldGridData worldGridData = target as WorldGridData;
      worldGridData.CountExistingPlacedObjects();
    }
  }

}