using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class WorldGridData : ScriptableObject
{
  // public FloorLayerToTileInfosDictionary worldGrid;
  public FloorLaxyerToTileHeightInfosDictionary heightGrid;
  // public HashSet<EnvironmentTileInfo> lightSources;
  public int minXAcrossAllFloors;
  public int maxXAcrossAllFloors;
  public int minYAcrossAllFloors;
  public int maxYAcrossAllFloors;
  public static float floorHeightToPaint = 0.2f;

#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create an TraitItem Asset 
  [MenuItem("Assets/Create/Scene/WorldGrid")]
  public static void CreateWorldGridData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save WorldGrid", "WorldGrid", "Asset", "Save WorldGrid", "Assets/resources/Data/EnvironmentData");
    if (path == "")
      return;
    WorldGridData data = ScriptableObject.CreateInstance<WorldGridData>();
    // data.worldGrid = new FloorLayerToTileInfosDictionary();
    data.heightGrid = new FloorLaxyerToTileHeightInfosDictionary();
    for (int i = Enum.GetValues(typeof(FloorLayer)).Length - 1; i >= 0; i--)
    {
      FloorLayer layer = (FloorLayer)i;
      // data.worldGrid[layer] = new IntToEnvironmentTileInfoDictionary();
      data.heightGrid[layer] = new IntToFloatDictionary();
    }
    AssetDatabase.CreateAsset(data, path);
  }

  [MenuItem("CustomTools/RebuildWorldGrid")]
  private static void RebuildWorldGrid()
  {
    WorldGridData worldGridData = Resources.Load("Data/EnvironmentData/WorldGrid") as WorldGridData;
    Debug.Log("loading world grid data: " + worldGridData);
    worldGridData.RebuildWorldGridData();
  }

  public void SetFloorHeightToPaint(float height)
  {
    floorHeightToPaint = height;
  }

  public void PaintFloorHeight(FloorLayer layer, Vector3Int location)
  {
    Debug.Log("setting tile height at " + layer + " " + location + "with key " + GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y)) + " to " + floorHeightToPaint + "(previously " + GetFloorHeight(layer, new Vector2Int(location.x, location.y)) + ")");
    if (floorHeightToPaint == 0)
    {
      heightGrid[layer].Remove(GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y)));
    }
    else
    {
      heightGrid[layer][GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y))] = floorHeightToPaint;
    }
  }

  public float GetFloorHeight(TileLocation loc)
  {
    return GetFloorHeight(loc.floorLayer, loc.tilemapCoordinates);
  }
  public float GetFloorHeight(FloorLayer layer, Vector2Int location)
  {
    int locKey = GridManager.Instance.CoordsToKey(location);
    if (heightGrid[layer].ContainsKey(locKey))
    {
      Debug.Log("returning floor height value " + heightGrid[layer][GridManager.Instance.CoordsToKey(location)]);
      return heightGrid[layer][GridManager.Instance.CoordsToKey(location)];
    }
    return 0;
  }

  public void RebuildWorldGridData()
  {
    RecalculateBounds();
  }

  public void RecalculateBounds()
  {
    maxXAcrossAllFloors = -5000;
    minXAcrossAllFloors = 5000;
    minYAcrossAllFloors = 5000;
    maxYAcrossAllFloors = -5000;
    foreach (LayerFloor lf in GridManager.Instance.layerFloors.Values)
    {
      minXAcrossAllFloors = Mathf.Min(minXAcrossAllFloors, lf.groundTilemap.cellBounds.xMin);
      maxXAcrossAllFloors = Mathf.Max(maxXAcrossAllFloors, lf.groundTilemap.cellBounds.xMax);
      minYAcrossAllFloors = Mathf.Min(minYAcrossAllFloors, lf.groundTilemap.cellBounds.yMin);
      maxYAcrossAllFloors = Mathf.Max(maxYAcrossAllFloors, lf.groundTilemap.cellBounds.yMax);
    }
    if (minXAcrossAllFloors + maxXAcrossAllFloors % 2 != 0) { maxXAcrossAllFloors += 1; }
    if (minYAcrossAllFloors + maxYAcrossAllFloors % 2 != 0) { maxYAcrossAllFloors += 1; }
  }

  [MenuItem("CustomTools/World Grid/Painting/Increase Floor Height %#UP")]
  public static void IncreaseFloorHeightToPaint()
  {
    floorHeightToPaint += .2f;
    if (floorHeightToPaint > 1)
    {
      floorHeightToPaint = 1;
    }
    Debug.Log("new FloorHeightToPaint is " + floorHeightToPaint);
  }


  [MenuItem("CustomTools/World Grid/Painting/Decrease floor height %#DOWN")]
  public static void DecreaseFloorHeightToPaint()
  {
    floorHeightToPaint -= .2f;
    if (floorHeightToPaint < 0)
    {
      floorHeightToPaint = 0;
    }
    Debug.Log("new FloorHeightToPaint is " + floorHeightToPaint);
  }
#endif
}