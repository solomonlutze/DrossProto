using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class WorldGridData : ScriptableObject
{
  [HideInInspector]
  public FloorLayerToTileInfosDictionary worldGrid;
  // public EnvironmentTileInfo DEBUG_TILE;
  public HashSet<EnvironmentTileInfo> lightSources;
  int minXAcrossAllFloors;
  int maxXAcrossAllFloors;
  int minYAcrossAllFloors;
  int maxYAcrossAllFloors;

  public int CoordsToKey(Vector2Int coordinates)
  {
    return coordinates.x + ((maxXAcrossAllFloors - minXAcrossAllFloors + 1) * coordinates.y);
  }
#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create an TraitItem Asset
  [MenuItem("Assets/Create/Scene/WorldGrid")]
  public static void CreateWorldGridData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save WorldGrid", "WorldGrid", "Asset", "Save WorldGrid", "Assets/resources/Data/EnvironmentData");
    if (path == "")
      return;
    WorldGridData data = ScriptableObject.CreateInstance<WorldGridData>();
    data.worldGrid = new FloorLayerToTileInfosDictionary();
    for (int i = Enum.GetValues(typeof(FloorLayer)).Length - 1; i >= 0; i--)
    {
      FloorLayer layer = (FloorLayer)i;
      data.worldGrid[layer] = new IntToEnvironmentTileInfoDictionary();
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

  public void RebuildWorldGridData()
  {
    Debug.Log("Rebuilding world grid data");
    worldGrid = new FloorLayerToTileInfosDictionary();
    Tilemap groundTilemap;
    Tilemap objectTilemap;
    Tilemap visibilityTilemap;
    maxXAcrossAllFloors = -5000; // yes, they look backwards
    minXAcrossAllFloors = 5000;  // no, they're not backwards
    maxYAcrossAllFloors = -5000; // they're initialized to something way outside than the actual min/maxes
    minYAcrossAllFloors = 5000;
    lightSources = new HashSet<EnvironmentTileInfo>();
    LayerToLayerFloorDictionary layerFloors = GridManager.Instance.layerFloors;
    Debug.Log("iterating over layer floors");
    foreach (LayerFloor lf in layerFloors.Values)
    {
      Debug.Log("layer floor " + lf);
      groundTilemap = lf.groundTilemap;
      lf.interestObjects = new GameObject().transform;
      lf.interestObjects.parent = lf.transform;
      lf.interestObjects.position = lf.transform.position;
      lf.interestObjects.gameObject.layer = lf.gameObject.layer;
      minXAcrossAllFloors = Mathf.Min(minXAcrossAllFloors, groundTilemap.cellBounds.xMin);
      maxXAcrossAllFloors = Mathf.Max(maxXAcrossAllFloors, groundTilemap.cellBounds.xMax);
      minYAcrossAllFloors = Mathf.Min(minYAcrossAllFloors, groundTilemap.cellBounds.yMin);
      maxYAcrossAllFloors = Mathf.Max(maxYAcrossAllFloors, groundTilemap.cellBounds.yMax);
    }
    if (minXAcrossAllFloors + maxXAcrossAllFloors % 2 != 0) { maxXAcrossAllFloors += 1; }
    if (minYAcrossAllFloors + maxYAcrossAllFloors % 2 != 0) { maxYAcrossAllFloors += 1; }
    Debug.Log("minx: " + minXAcrossAllFloors);
    Debug.Log("maxX: " + maxXAcrossAllFloors);
    Debug.Log("minY: " + minYAcrossAllFloors);
    Debug.Log("maxY: " + maxYAcrossAllFloors);
    HashSet<EnvironmentTileInfo> litTiles = new HashSet<EnvironmentTileInfo>();
    HashSet<EnvironmentTileInfo> currentTilesToLight = new HashSet<EnvironmentTileInfo>();
    HashSet<EnvironmentTileInfo> nextTilesToLight = new HashSet<EnvironmentTileInfo>();
    for (int i = Enum.GetValues(typeof(FloorLayer)).Length - 1; i >= 0; i--)
    {
      FloorLayer layer = (FloorLayer)i;
      if (!layerFloors.ContainsKey(layer))
      {
        continue;
      }
      worldGrid[layer] = new IntToEnvironmentTileInfoDictionary();
      LayerFloor layerFloor = layerFloors[layer];
      groundTilemap = layerFloor.groundTilemap;
      objectTilemap = layerFloor.objectTilemap;
      visibilityTilemap = layerFloor.visibilityTilemap;
      for (int x = minXAcrossAllFloors; x < maxXAcrossAllFloors; x++)
      {
        // for (int y = minYAcrossAllFloors; y < maxYAcrossAllFloors; y++)
        for (int y = maxYAcrossAllFloors; y > minYAcrossAllFloors; y--)
        {
          //get both object and ground tile, build an environmentTileInfo out of them, and put it into our worldGrid
          TileLocation loc = new TileLocation(new Vector2Int(x, y), layer);
          ConstructAndSetEnvironmentTileInfo(loc, groundTilemap, objectTilemap, visibilityTilemap, litTiles, currentTilesToLight);
        }
      }
      // }
    }
    Debug.Log("Initializing lighting");
    InitializeLighting(litTiles, currentTilesToLight);
    Debug.Log("Adding illumination sources to neighbors");
    foreach (EnvironmentTileInfo source in lightSources)
    {
      AddIlluminationSourceToNeighbors(source);
    }
  }
  public EnvironmentTileInfo ConstructAndSetEnvironmentTileInfo(
  TileLocation loc,
  Tilemap groundTilemap,
  Tilemap objectTilemap,
  Tilemap visibilityTilemap,
  HashSet<EnvironmentTileInfo> litTiles = null,
  HashSet<EnvironmentTileInfo> currentTilesToLight = null
  )
  {
    Vector3Int v3pos = new Vector3Int(loc.tilemapCoordinates.x, loc.tilemapCoordinates.y, 0);
    // if (v3pos.x != 0 || v3pos.y != 0)
    // {
    //   if (GridManager.Instance.CoordsToKey(loc.tilemapCoordinates) == 0)
    //   {
    //     Debug.Log("got coords of zero for tilemap coordinates " + loc.tilemapCoordinates);
    //   }
    //   return null;
    // }
    EnvironmentTileInfo info = new EnvironmentTileInfo();
    // if (loc.floorLayer == FloorLayer.B1)
    // {
    //   DEBUG_TILE = info;
    // }
    visibilityTilemap.SetTile(v3pos, GridManager.Instance.visibilityTile);
    EnvironmentTile objectTile = objectTilemap.GetTile(v3pos) as EnvironmentTile;
    EnvironmentTile groundTile = groundTilemap.GetTile(v3pos) as EnvironmentTile;

    info.Init(
      loc,
      groundTile,
      objectTile
    );
    if (info.isLightSource)
    {
      foreach (LightRangeInfo i in info.lightSource.lightRangeInfos)
      {
        i.currentIntensity = i.defaultIntensity;
      }
      lightSources.Add(info);
    }
    if (loc.floorLayer == FloorLayer.F6 || GridManager.Instance.GetAdjacentTile(loc, TilemapDirection.Above).IsEmptyAndSunlit())
    {
      info.AddIlluminatedBySource(GridManager.Instance.sunlight, 0);
      if (litTiles != null)
      {
        litTiles.Add(info);
      }
      if (!info.IsEmpty() && currentTilesToLight != null)
      {
        currentTilesToLight.Add(info);
      }
    }
    if (!info.IsEmpty())
    {
      visibilityTilemap.SetColor(v3pos, info.illuminationInfo.visibleColor);
    }
    else
    {
      visibilityTilemap.SetColor(v3pos, Color.clear);
    }
    if (info.HasSolidObject())
    {
      info.wallObject = Instantiate(GridManager.Instance.defaultWallObject);
      info.wallObject.transform.position = loc.cellCenterWorldPosition;
      info.wallObject.Init(loc, objectTile.sprite);
    }
    worldGrid[loc.floorLayer][CoordsToKey(loc.tilemapCoordinates)] = info;
    return info;
  }

  public void InitializeLighting(
    HashSet<EnvironmentTileInfo> litTiles,
    HashSet<EnvironmentTileInfo> currentTilesToIlluminate)
  {
    HashSet<EnvironmentTileInfo> nextTilesToLight = new HashSet<EnvironmentTileInfo>();
    int currentDistance = 0;
    while (currentDistance < GridManager.Instance.sunlight.lightRangeInfos.Length)
    {
      foreach (EnvironmentTileInfo tile in currentTilesToIlluminate)
      {
        if (currentDistance != 0)
        {
          tile.AddIlluminatedBySource(GridManager.Instance.sunlight, currentDistance);
          litTiles.Add(tile);
        }
        if (!tile.IsEmpty())
        {
          GridManager.Instance.layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.SetColor(tile.tileLocation.tilemapCoordinatesVector3, tile.illuminationInfo.visibleColor);
        }
        else
        {
          GridManager.Instance.layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.SetColor(tile.tileLocation.tilemapCoordinatesVector3, Color.clear);
        }
        if (!tile.HasSolidObject())
        {
          foreach (TilemapDirection dir in new List<TilemapDirection>(){
            TilemapDirection.UpperLeft,
            TilemapDirection.Left,
            TilemapDirection.LowerLeft,
            TilemapDirection.UpperRight,
            TilemapDirection.Right,
            TilemapDirection.LowerRight,
            TilemapDirection.Above,
            TilemapDirection.Below
          })
          {
            if (dir == TilemapDirection.Below && !tile.IsEmpty()) { continue; } // can't go down through a floor
            if (dir == TilemapDirection.Above && (GridManager.Instance.GetAdjacentTile(tile.tileLocation, dir) == null || !GridManager.Instance.GetAdjacentTile(tile.tileLocation, dir).IsEmpty())) { continue; } // can't go up through a floor
            if (
              GridManager.Instance.AdjacentTileIsValid(tile.tileLocation, dir)
              && GridManager.Instance.GetAdjacentTile(tile.tileLocation, dir) != null
              && currentDistance != (GridManager.Instance.sunlight.lightRangeInfos.Length - 1)
              && !litTiles.Contains(GridManager.Instance.GetAdjacentTile(tile.tileLocation, dir)))
            {
              nextTilesToLight.Add(GridManager.Instance.GetAdjacentTile(tile.tileLocation, dir));
            }
          }
        }
      }
      currentTilesToIlluminate = new HashSet<EnvironmentTileInfo>(nextTilesToLight);
      nextTilesToLight.Clear();
      Debug.Log("current distance: " + currentDistance);
      currentDistance++;
    }

  }
  // TODO: move this method into EnvironmentTileInfo probably
  public void AddIlluminationSourceToNeighbors(EnvironmentTileInfo sourceTile)
  {
    int currentDistance = 0;
    HashSet<EnvironmentTileInfo> totalTilesToIlluminate = new HashSet<EnvironmentTileInfo>();
    HashSet<EnvironmentTileInfo> nextTilesToIlluminate = new HashSet<EnvironmentTileInfo>();
    HashSet<EnvironmentTileInfo> currentTilesToIlluminate = new HashSet<EnvironmentTileInfo>() { sourceTile };
    while (currentDistance < sourceTile.lightSource.lightRangeInfos.Length)
    {
      foreach (EnvironmentTileInfo tile in currentTilesToIlluminate)
      {
        tile.AddIlluminatedBySource(sourceTile.lightSource, currentDistance);
        sourceTile.illuminatedNeighbors.Add(tile);
        totalTilesToIlluminate.Add(tile);
        GridManager.Instance.layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.SetColor(tile.tileLocation.tilemapCoordinatesVector3, tile.illuminationInfo.visibleColor);
        if (!tile.HasSolidObject())
        {
          foreach (TilemapDirection dir in new List<TilemapDirection>(){
            TilemapDirection.UpperLeft,
            TilemapDirection.Left,
            TilemapDirection.LowerLeft,
            TilemapDirection.UpperRight,
            TilemapDirection.Right,
            TilemapDirection.LowerRight,
          })
          {
            if (
              currentDistance != (sourceTile.lightSource.lightRangeInfos.Length - 1)
              && !totalTilesToIlluminate.Contains(GridManager.Instance.GetAdjacentTile(tile.tileLocation, dir)))
            {
              nextTilesToIlluminate.Add(GridManager.Instance.GetAdjacentTile(tile.tileLocation, dir));
            }
          }
        }
      }
      currentTilesToIlluminate = new HashSet<EnvironmentTileInfo>(nextTilesToIlluminate);
      nextTilesToIlluminate.Clear();
      currentDistance++;
    }
  }

#endif
}