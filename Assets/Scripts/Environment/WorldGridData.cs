using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class WorldGridData : ScriptableObject
{

  [HideInInspector]
  public CoordsToGameObjectDictionary placedGameObjects;
  [HideInInspector]
  public FloorLayerToTileInfosDictionary worldGrid;
  [HideInInspector]
  public FloorLayerToTileHeightInfosDictionary heightGrid; // X is floor height; y is ceiling height
  // public HashSet<EnvironmentTileInfo> lightSources;
  public int minXAcrossAllFloors = -5000;
  public int maxXAcrossAllFloors = 5000;
  public int minYAcrossAllFloors = -5000;
  public int maxYAcrossAllFloors = 5000;
  public Tilemap waterTilemapPrefab;
  public WallObject defaultWallObjectPrefab;
  public static float heightToPaint = 0.0f;

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
    data.heightGrid = new FloorLayerToTileHeightInfosDictionary();
    for (int i = Enum.GetValues(typeof(FloorLayer)).Length - 1; i >= 0; i--)
    {
      FloorLayer layer = (FloorLayer)i;
      data.worldGrid[layer] = new IntToEnvironmentTileInfoDictionary();
      data.heightGrid[layer] = new IntToVector2Dictionary();
    }
    AssetDatabase.CreateAsset(data, path);
  }

  [MenuItem("CustomTools/RebuildWorldGrid")]
  private static void RebuildWorldGrid()
  {
    WorldGridData worldGridData = Resources.Load("Data/EnvironmentData/WorldGridData") as WorldGridData;
    Debug.Log("loading world grid data: " + worldGridData);
    worldGridData.RebuildPlacedObjects();
  }

  [MenuItem("CustomTools/ClearWorldGrid")]
  private static void ClearWorldGrid()
  {
    WorldGridData worldGridData = Resources.Load("Data/EnvironmentData/WorldGridData") as WorldGridData;
    Debug.Log("loading world grid data: " + worldGridData);
    // worldGridData.ClearExistingGridInfo();
  }
  public void SetFloorHeightToPaint(float height)
  {
    heightToPaint = height;
    if (Mathf.RoundToInt(heightToPaint * 10) == 0)
    {
      heightToPaint = 0;
    }
    if (Mathf.RoundToInt(heightToPaint * 10) == 10)
    {
      heightToPaint = 1;
    }
  }

  public void ClearWallObject(FloorLayer layer, Vector3Int location, EnvironmentTile tile)
  {
    Debug.Log("clear wall object");
    if (tile.floorTilemapType == FloorTilemapType.Object)
    {
      ModifyFloorHeight(layer, location, tile, 1);
    }
    else
    {
      ModifyFloorHeight(layer, location, tile, 0);
    }
  }

  public void PaintFloorHeight(FloorLayer layer, Vector3Int location, EnvironmentTile tile)
  {
    ModifyFloorHeight(layer, location, tile, heightToPaint);
  }

  public void ModifyFloorHeight(FloorLayer layer, Vector3Int location, EnvironmentTile tile, float height)
  {
    TileLocation loc = new TileLocation(location.x, location.y, layer);
    bool ceiling = tile.floorTilemapType == FloorTilemapType.Object;
    Vector2 heightValue;
    if (heightGrid[layer].ContainsKey(GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y))))
    {
      heightValue = heightGrid[layer][GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y))];
    }
    else
    {
      heightValue = new Vector2(0, 1);
    }
    if (ceiling)
    {
      heightValue = new Vector2(heightValue.x, height);
    }
    else
    {
      heightValue = new Vector2(height, heightValue.y);
    }
    AdjustWallObject(loc, heightValue, ceiling ? null : tile, ceiling ? tile : null);
  }
  //     {
  //       heightGrid[layer][GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y))] = new Vector2(heightToPaint, heightGrid[layer][GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y))].y);
  //       WallObject wallObject;
  //       if (placedGameObjects.ContainsKey(CoordsToKey(loc)) && placedGameObjects[CoordsToKey(loc)] != null)
  //       {
  //         Debug.Log("replacing existing object");
  //         wallObject = placedGameObjects[CoordsToKey(loc)].GetComponent<WallObject>();
  //       }
  //       else
  //       {
  //         Debug.Log("instantiating new object");
  //         wallObject = Instantiate(defaultWallObjectPrefab);
  //         wallObject.transform.position = loc.cellCenterWorldPosition;
  //       }
  //       wallObject.Init(loc, tile, heightToPaint, ceiling);
  //       placedGameObjects[CoordsToKey(loc)] = wallObject.gameObject;
  //     }
  //   }
  //   else
  //   {
  //     if (heightToPaint == 0 && heightValue.y == 1) // this location should have no object tile
  //     {
  //       RemoveHeightDataAtLocation(layer, location);
  //     }
  //     else
  //     {
  //       heightGrid[layer][GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y))] = new Vector2(heightToPaint, heightGrid[layer][GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y))].y);
  //       WallObject wallObject;
  //       if (placedGameObjects.ContainsKey(CoordsToKey(loc)) && placedGameObjects[CoordsToKey(loc)] != null)
  //       {
  //         Debug.Log("replacing existing object");
  //         wallObject = placedGameObjects[CoordsToKey(loc)].GetComponent<WallObject>();
  //       }
  //       else
  //       {
  //         Debug.Log("instantiating new object");
  //         wallObject = Instantiate(defaultWallObjectPrefab);
  //         wallObject.transform.position = loc.cellCenterWorldPosition;
  //       }
  //       wallObject.Init(loc, tile, heightToPaint, !ceiling);
  //       placedGameObjects[CoordsToKey(loc)] = wallObject.gameObject;
  //     }
  //   }
  //   if (heightGrid[layer][GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y))] == Vector2.zero)
  //   {
  //     heightGrid[layer].Remove(GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y)));
  //     if (placedGameObjects.ContainsKey(CoordsToKey(loc)) && placedGameObjects[CoordsToKey(loc)] != null)
  //     {
  //       GameObject.DestroyImmediate(placedGameObjects[CoordsToKey(loc)]);
  //       placedGameObjects.Remove(CoordsToKey(loc));
  //     }
  //   }}
  // }

  public void AdjustWallObject(TileLocation tileLocation, Vector2 heightValue, EnvironmentTile groundTile, EnvironmentTile objectTile)
  {
    if (heightValue.x == 0 && heightValue.y == 1)
    {
      RemoveHeightDataAtLocation(tileLocation.floorLayer, tileLocation.tilemapCoordinates);
      return;
    }
    heightGrid[tileLocation.floorLayer][GridManager.Instance.CoordsToKey(tileLocation.tilemapCoordinates)] = heightValue;
    Debug.Log("setting value to " + heightGrid[tileLocation.floorLayer][GridManager.Instance.CoordsToKey(tileLocation.tilemapCoordinates)]);
    WallObject wallObject;
    if (placedGameObjects.ContainsKey(CoordsToKey(tileLocation)) && placedGameObjects[CoordsToKey(tileLocation)] != null)
    {
      Debug.Log("replacing existing object");
      wallObject = placedGameObjects[CoordsToKey(tileLocation)].GetComponent<WallObject>();
    }
    else
    {
      Debug.Log("instantiating new object " + defaultWallObjectPrefab);
      wallObject = Instantiate(defaultWallObjectPrefab);
      wallObject.transform.position = tileLocation.cellCenterWorldPosition;
    }
    if (groundTile != null)
    {
      wallObject.SetGroundInfo(groundTile, heightValue.x);
    }
    if (objectTile != null)
    {
      wallObject.SetCeilingInfo(objectTile, heightValue.y);
    }
    wallObject.Init(tileLocation); // WARNING: expects ceiling/wall tile info to already be set! provide those, maybe?
    placedGameObjects[CoordsToKey(tileLocation)] = wallObject.gameObject;
  }
  public void RemoveHeightDataAtLocation(FloorLayer layer, Vector2Int location)
  {
    TileLocation loc = new TileLocation(location.x, location.y, layer);
    if (heightGrid[layer].ContainsKey(GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y))))
    {
      Debug.Log("contains key " + GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y)) + ", contents " + heightGrid[layer][GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y))]);
      heightGrid[layer].Remove(GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y)));
    }
    if (placedGameObjects.ContainsKey(CoordsToKey(loc)) && placedGameObjects[CoordsToKey(loc)] != null)
    {
      Debug.Log("set height to 0, should destroy");
      GameObject.DestroyImmediate(placedGameObjects[CoordsToKey(loc)]);
      placedGameObjects.Remove(CoordsToKey(loc));
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
      return heightGrid[layer][GridManager.Instance.CoordsToKey(location)].x;
    }
    return 0;
  }

  public void RebuildPlacedObjects()
  {
    // CreateNewWorldGrid();
    Debug.Log("clearing");
    ClearExistingPlacedObjects();
    Debug.Log("cleared");
    CreateAndPopulatePlacedObjects();
    // RecalculateBounds();
    // ReapplyHeightInfo();
  }

  // public void CreateNewWorldGrid()
  // {
  //   for (int i = Enum.GetValues(typeof(FloorLayer)).Length - 1; i >= 0; i--)
  //   {
  //     FloorLayer layer = (FloorLayer)i;
  //     worldGrid[layer] = new IntToEnvironmentTileInfoDictionary();
  //   }
  // }

  // Delete any physical objects associated with the grid
  public void ClearExistingPlacedObjects()
  {
    foreach (GameObject obj in placedGameObjects.Values)
    {
      GameObject.DestroyImmediate(obj);
    }
    placedGameObjects.Clear();

  }

  public void CreateAndPopulatePlacedObjects()
  {
    for (int i = Enum.GetValues(typeof(FloorLayer)).Length - 1; i >= 0; i--)
    {
      FloorLayer layer = (FloorLayer)i;
      worldGrid[layer] = new IntToEnvironmentTileInfoDictionary();
      if (!GridManager.Instance.layerFloors.ContainsKey(layer))
      {
        continue;
      }
      LayerFloor layerFloor = GridManager.Instance.layerFloors[layer];
      Tilemap groundTilemap = layerFloor.groundTilemap;
      Tilemap objectTilemap = layerFloor.objectTilemap;
      Tilemap waterTilemap = layerFloor.waterTilemap;
      Tilemap infoTilemap = layerFloor.infoTilemap;
      for (int x = minXAcrossAllFloors; x < maxXAcrossAllFloors; x++)
      {
        for (int y = maxYAcrossAllFloors; y > minYAcrossAllFloors; y--)
        {
          TileLocation loc = new TileLocation(new Vector2Int(x, y), layer);
          if (heightGrid[loc.floorLayer].ContainsKey(GridManager.Instance.CoordsToKey(loc.tilemapCoordinates)))
          {
            Vector2 heightValue = heightGrid[loc.floorLayer][GridManager.Instance.CoordsToKey(loc.tilemapCoordinates)];
            AdjustWallObject(
              loc,
              heightValue,
              (EnvironmentTile)groundTilemap.GetTile(loc.tilemapCoordinatesVector3),
              (EnvironmentTile)objectTilemap.GetTile(loc.tilemapCoordinatesVector3)
            );
          }
        }
      }
    }
  }

  public void CreateAndPopulateNewWorldGrid()
  {
    Debug.Log("create and populate");
    Tilemap groundTilemap;
    Tilemap objectTilemap;
    Tilemap waterTilemap;
    Tilemap infoTilemap;
    worldGrid = new FloorLayerToTileInfosDictionary();
    foreach (LayerFloor lf in GridManager.Instance.layerFloors.Values)
    {
      Debug.Log("populating " + lf);
      groundTilemap = lf.groundTilemap;
      groundTilemap.GetComponent<TilemapRenderer>().sortingLayerName = "Default";
      groundTilemap.GetComponent<TilemapRenderer>().sortingOrder = 0;

      objectTilemap = lf.objectTilemap;
      objectTilemap.GetComponent<TilemapRenderer>().sortingLayerName = "Default";
      groundTilemap.GetComponent<TilemapRenderer>().sortingOrder = 0;
      if (lf.waterTilemap == null)
      {
        lf.waterTilemap = Instantiate(waterTilemapPrefab, lf.gameObject.transform);
        lf.waterTilemap.gameObject.layer = lf.gameObject.layer;
        lf.waterTilemap.GetComponent<TilemapRenderer>().sortingLayerName = "Default";
        lf.waterTilemap.GetComponent<TilemapRenderer>().sortingOrder = 0;
      }
    }
    TileLocation location;
    for (int i = Enum.GetValues(typeof(FloorLayer)).Length - 1; i >= 0; i--)
    {
      FloorLayer layer = (FloorLayer)i;
      worldGrid[layer] = new IntToEnvironmentTileInfoDictionary();
      if (!GridManager.Instance.layerFloors.ContainsKey(layer))
      {
        continue;
      }
      LayerFloor layerFloor = GridManager.Instance.layerFloors[layer];
      groundTilemap = layerFloor.groundTilemap;
      objectTilemap = layerFloor.objectTilemap;
      waterTilemap = layerFloor.waterTilemap;
      infoTilemap = layerFloor.infoTilemap;
      for (int x = minXAcrossAllFloors; x < maxXAcrossAllFloors; x++)
      {
        for (int y = maxYAcrossAllFloors; y > minYAcrossAllFloors; y--)
        {
          //get both object and ground tile, build an environmentTileInfo out of them, and put it into our worldGrid
          location = new TileLocation(new Vector2Int(x, y), layer);
          ConstructAndSetEnvironmentTileInfo(location, groundTilemap, objectTilemap, infoTilemap, waterTilemap);
        }
      }
    }
  }

  public EnvironmentTileInfo ConstructAndSetEnvironmentTileInfo(
  TileLocation loc,
  Tilemap groundTilemap,
  Tilemap objectTilemap,
  Tilemap infoTilemap,
  Tilemap waterTilemap
  )
  {
    Vector3Int v3pos = new Vector3Int(loc.tilemapCoordinates.x, loc.tilemapCoordinates.y, 0);
    EnvironmentTileInfo info = new EnvironmentTileInfo();
    EnvironmentTile objectTile = objectTilemap.GetTile(v3pos) as EnvironmentTile;
    EnvironmentTile groundTile = groundTilemap.GetTile(v3pos) as EnvironmentTile;
    InfoTile infoTile = infoTilemap.GetTile(v3pos) as InfoTile;

    // info.Init(
    //   loc,
    //   groundTile,
    //   objectTile,
    //   infoTile
    // );
    // if (GetFloorHeight(loc) > 0)
    // {
    //   info.groundHeight = GetFloorHeight(loc);
    //   Debug.Log("info.groundHeight");
    //   info.groundObject = Instantiate(defaultWallObjectPrefab);
    //   info.groundObject.transform.position = loc.cellCenterWorldPosition;
    //   info.groundObject.Init(loc, groundTile, false);
    // }
    // if (info.HasSolidObject())
    // {
    //   info.wallObject = Instantiate(defaultWallObjectPrefab);
    //   info.wallObject.transform.position = loc.cellCenterWorldPosition;
    //   info.wallObject.Init(loc, objectTile, true);
    // }
    if (info.HasTileTag(TileTag.Water))
    {
      waterTilemap.SetTile(v3pos, groundTilemap.GetTile(v3pos));
      groundTilemap.SetTile(v3pos, null);
    }
    worldGrid[loc.floorLayer][CoordsTo2DKey(loc.tilemapCoordinates)] = info;
    return info;
  }

  public int CoordsTo2DKey(Vector2Int coordinates)
  {
    return coordinates.x + ((maxXAcrossAllFloors - minXAcrossAllFloors + 1) * coordinates.y);
  }


  public int CoordsToKey(TileLocation coordinates)
  {
    return coordinates.x +
    ((maxXAcrossAllFloors - minXAcrossAllFloors + 1) * coordinates.y)
    + ((maxXAcrossAllFloors - minXAcrossAllFloors + 1) * (maxYAcrossAllFloors - minYAcrossAllFloors + 1) * (int)coordinates.floorLayer);
  }

  // public void RecalculateBounds()
  // {
  //   maxXAcrossAllFloors = 5000;
  //   minXAcrossAllFloors = -5000;
  //   minYAcrossAllFloors = -5000;
  //   maxYAcrossAllFloors = 5000;
  //   foreach (LayerFloor lf in GridManager.Instance.layerFloors.Values)
  //   {
  //     minXAcrossAllFloors = Mathf.Min(minXAcrossAllFloors, lf.groundTilemap.cellBounds.xMin);
  //     maxXAcrossAllFloors = Mathf.Max(maxXAcrossAllFloors, lf.groundTilemap.cellBounds.xMax);
  //     minYAcrossAllFloors = Mathf.Min(minYAcrossAllFloors, lf.groundTilemap.cellBounds.yMin);
  //     maxYAcrossAllFloors = Mathf.Max(maxYAcrossAllFloors, lf.groundTilemap.cellBounds.yMax);
  //   }
  //   if (minXAcrossAllFloors + maxXAcrossAllFloors % 2 != 0) { maxXAcrossAllFloors += 1; }
  //   if (minYAcrossAllFloors + maxYAcrossAllFloors % 2 != 0) { maxYAcrossAllFloors += 1; }
  // }

  [MenuItem("CustomTools/World Grid/Painting/Increase Floor Height %#UP")]
  public static void IncreaseFloorHeightToPaint()
  {
    heightToPaint += .2f;
    if (heightToPaint > 1)
    {
      heightToPaint = 1;
    }
    Debug.Log("new FloorHeightToPaint is " + heightToPaint);
  }


  [MenuItem("CustomTools/World Grid/Painting/Decrease floor height %#DOWN")]
  public static void DecreaseFloorHeightToPaint()
  {
    heightToPaint -= .2f;
    if (heightToPaint < 0)
    {
      heightToPaint = 0;
    }
    Debug.Log("new FloorHeightToPaint is " + heightToPaint);
  }
#endif
}