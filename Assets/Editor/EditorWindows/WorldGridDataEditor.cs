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
    if (GUILayout.Button("Clear placed game objects"))
    {
      WorldGridData worldGridData = target as WorldGridData;
      worldGridData.ClearExistingPlacedObjects();
    }
    if (GUILayout.Button("Count placed objects"))
    {
      WorldGridData worldGridData = target as WorldGridData;
      worldGridData.CountExistingPlacedObjects();
    }
    if (GUILayout.Button("Count environment tile datas"))
    {
      WorldGridData worldGridData = target as WorldGridData;
      worldGridData.CountEnvironmentTileDatas();
    }
    if (GUILayout.Button("Normalize height grid"))
    {
      WorldGridData worldGridData = target as WorldGridData;
      worldGridData.CleanAllEnvironmentTileDataValues();
    }
    // WARNING: this blows away all height data! probably never use it!!
    if (GUILayout.Button("Replace environment tile grid data with new"))
    {
      WorldGridData worldGridData = target as WorldGridData;
      worldGridData.environmentTileDataGrid = new FloorLayerToEnvironmentTileDataDictionary();

      foreach (FloorLayer fl in (FloorLayer[])Enum.GetValues(typeof(FloorLayer)))
      {
        worldGridData.environmentTileDataGrid.Add(fl, new IntToEnvironmentTileDataDictionary());
      }
    }
  }

}