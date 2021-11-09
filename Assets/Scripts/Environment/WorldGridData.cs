using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;



public class WorldGridData : ScriptableObject
{

  [HideInInspector]
  public Vector2IntToStringDictionary chunkScenes;
  [HideInInspector]
  public FloorLayerToTileInfosDictionary worldGrid;
  [HideInInspector]
  public FloorLayerToTileHeightInfosDictionary heightGrid; // X is floor height; y is ceiling height
  public int minXAcrossAllFloors = -5000;
  public int maxXAcrossAllFloors = 5000;
  public int minYAcrossAllFloors = -5000;
  public int maxYAcrossAllFloors = 5000;
  [Tooltip("Square size of a chunk, in tiles")]
  public int chunkSize = 10; // square size of a chunk, in tiles
  [Tooltip("how many chunks to load at once on either side of current chunk. EG 1,1 = 3x3 grid")]
  public Vector2Int chunksToLoad;
  public SceneTemplateAsset chunkSceneTemplate;
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
      if (float.IsNaN(heightValue.x) || float.IsNaN(heightValue.y))
      {
        Debug.Log("trying to set NAN, value " + heightValue);
        heightValue = new Vector2(0, 1);
      }
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
  //       if (GridManager.Instance.placedGameObjects.ContainsKey(CoordsToKey(loc)) && GridManager.Instance.placedGameObjects[CoordsToKey(loc)] != null)
  //       {
  //         Debug.Log("replacing existing object");
  //         wallObject = GridManager.Instance.placedGameObjects[CoordsToKey(loc)].GetComponent<WallObject>();
  //       }
  //       else
  //       {
  //         Debug.Log("instantiating new object");
  //         wallObject = Instantiate(defaultWallObjectPrefab);
  //         wallObject.transform.position = loc.cellCenterWorldPosition;
  //       }
  //       wallObject.Init(loc, tile, heightToPaint, ceiling);
  //       GridManager.Instance.placedGameObjects[CoordsToKey(loc)] = wallObject.gameObject;
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
  //       if (GridManager.Instance.placedGameObjects.ContainsKey(CoordsToKey(loc)) && GridManager.Instance.placedGameObjects[CoordsToKey(loc)] != null)
  //       {
  //         Debug.Log("replacing existing object");
  //         wallObject = GridManager.Instance.placedGameObjects[CoordsToKey(loc)].GetComponent<WallObject>();
  //       }
  //       else
  //       {
  //         Debug.Log("instantiating new object");
  //         wallObject = Instantiate(defaultWallObjectPrefab);
  //         wallObject.transform.position = loc.cellCenterWorldPosition;
  //       }
  //       wallObject.Init(loc, tile, heightToPaint, !ceiling);
  //       GridManager.Instance.placedGameObjects[CoordsToKey(loc)] = wallObject.gameObject;
  //     }
  //   }
  //   if (heightGrid[layer][GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y))] == Vector2.zero)
  //   {
  //     heightGrid[layer].Remove(GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y)));
  //     if (GridManager.Instance.placedGameObjects.ContainsKey(CoordsToKey(loc)) && GridManager.Instance.placedGameObjects[CoordsToKey(loc)] != null)
  //     {
  //       GameObject.DestroyImmediate(GridManager.Instance.placedGameObjects[CoordsToKey(loc)]);
  //       GridManager.Instance.placedGameObjects.Remove(CoordsToKey(loc));
  //     }
  //   }}
  // }

  // 0,0 is the default - floors extend nowhere, and wall objects take up the whole height. 
  // --we do not need to track this as height data, but we do need a wallObject if there's an objectTile at that location.
  // 0,1 is "empty" - floors do not extend, ceilings do not extend. 
  // --this is not the default, but it's also not a wallObject. We SHOULD track it, and should NOT have a wallObject. 

  public void SetHeightValue(TileLocation location, Vector2 heightValue)
  {
    if (float.IsNaN(heightValue.x) || float.IsNaN(heightValue.y))
    {
      Debug.Log("trying to set NAN at " + location.cellCenterPosition + ", value " + heightValue);
    }
    else
    {
      heightGrid[location.floorLayer][GridManager.Instance.CoordsToKey(location.tilemapCoordinates)] = heightValue;
    }
  }
  public void AdjustWallObject(TileLocation tileLocation, Vector2 heightValue, EnvironmentTile groundTile, EnvironmentTile objectTile)
  {
    if (heightValue != Vector2.zero)
    {
      SetHeightValue(tileLocation, heightValue);
    }
    else
    {
      RemoveHeightDataAtLocation(tileLocation.floorLayer, tileLocation.tilemapCoordinates);
      // no return!! gotta make a wall tile still maybe!!
    }
    if ((heightValue.x == 0 && heightValue.y == 1) || (heightValue == Vector2.zero && objectTile == null))
    {
      DestroyWallObjectAtLocation(tileLocation.floorLayer, tileLocation.tilemapCoordinates);
      return;
    }
    WallObject wallObject;
    if (GridManager.Instance.placedGameObjects.ContainsKey(CoordsToKey(tileLocation)) && GridManager.Instance.placedGameObjects[CoordsToKey(tileLocation)] != null)
    {
      wallObject = GridManager.Instance.placedGameObjects[CoordsToKey(tileLocation)].GetComponent<WallObject>();
    }
    else
    {
      wallObject = ObjectPoolManager.Instance.GetWallObjectPool().GetObject();
      wallObject.transform.position = tileLocation.cellCenterWorldPosition;
    }
    GridManager.Instance.placedGameObjects[CoordsToKey(tileLocation)] = wallObject.gameObject;
    if (groundTile != null)
    {
      wallObject.SetGroundInfo(groundTile, heightValue.x);
    }
    if (objectTile != null)
    {
      wallObject.SetCeilingInfo(objectTile, heightValue.y);
    }
    wallObject.Init(tileLocation);
  }

  public void RemoveHeightDataAtLocation(FloorLayer layer, Vector2Int location)
  {
    if (heightGrid[layer].ContainsKey(GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y))))
    {
      heightGrid[layer].Remove(GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y)));
    }
  }

  public GameObject GetPlacedObjectAtLocation(TileLocation loc)
  {
    if (GridManager.Instance.placedGameObjects.ContainsKey(CoordsToKey(loc)) && GridManager.Instance.placedGameObjects[CoordsToKey(loc)] != null)
    {
      return GridManager.Instance.placedGameObjects[CoordsToKey(loc)];
    }
    return null;
  }
  public void DestroyWallObjectAtLocation(FloorLayer layer, Vector2Int location)
  {
    TileLocation loc = new TileLocation(location.x, location.y, layer);
    GameObject placedObject;
    int key = CoordsToKey(loc);
    if (GridManager.Instance.placedGameObjects.TryGetValue(key, out placedObject) && placedObject != null)
    {
      WallObject wallObject = placedObject.GetComponent<WallObject>();
      ObjectPoolManager.Instance.wallObjectPool.Release(wallObject);
      GridManager.Instance.placedGameObjects.Remove(key);
    }
  }

  public float GetFloorHeight(TileLocation loc)
  {
    return GetFloorHeight(loc.floorLayer, loc.tilemapCoordinates);
  }

  public bool HasFloorHeightInfo(TileLocation loc)
  {
    return heightGrid[loc.floorLayer].ContainsKey(GridManager.Instance.CoordsToKey(loc.tilemapCoordinates));
  }

  public float GetFloorHeight(FloorLayer layer, Vector2Int location)
  {
    int locKey = GridManager.Instance.CoordsToKey(location);
    if (heightGrid[layer].ContainsKey(locKey) && heightGrid[layer][locKey] != null)
    {
      return heightGrid[layer][locKey].x;
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
    if (GridManager.Instance.placedGameObjects == null)
    {
      GridManager.Instance.placedGameObjects = new CoordsToGameObjectDictionary();
    }
    foreach (GameObject obj in GridManager.Instance.placedGameObjects.Values)
    {
      if (obj != null)
      {
        obj.SetActive(false);
      }
    }
    GridManager.Instance.ClearLoadedChunks();
    GridManager.Instance.placedGameObjects.Clear();
  }

  public void ClearExistingPlacedObjectsAndPool()
  {
    GridManager.Instance.ClearLoadedChunksAndResetPool();
  }

  public void CountExistingPlacedObjects()
  {
    Debug.Log("placed game objects count: " + GridManager.Instance.placedGameObjects.Count);
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
          Vector2 heightValue = Vector2.up;
          if ((EnvironmentTile)objectTilemap.GetTile(loc.tilemapCoordinatesVector3) != null)
          {
            heightValue = Vector2.zero;
          }
          if (heightGrid[loc.floorLayer].ContainsKey(GridManager.Instance.CoordsToKey(loc.tilemapCoordinates)))
          {
            heightValue = heightGrid[loc.floorLayer][GridManager.Instance.CoordsToKey(loc.tilemapCoordinates)];
          }
          if (heightValue != Vector2.up)
          {
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


  public void CreateAndPopulatePlacedObjectsForChunk(Vector2Int chunkCoords)
  {
    CreateAndPopulatePlacedObjectsForChunk(chunkCoords.x * chunkSize + minXAcrossAllFloors, chunkCoords.y * chunkSize + minYAcrossAllFloors);
  }

  public void UnloadPlacedObjectsForChunk(Vector2Int chunkCoords)
  {
    UnloadPlacedObjectsForChunk(chunkCoords.x * chunkSize + minXAcrossAllFloors, chunkCoords.y * chunkSize + minYAcrossAllFloors);
  }

  public void UnloadPlacedObjectsForChunk(int xMin, int yMin)
  {

    for (int i = Enum.GetValues(typeof(FloorLayer)).Length - 1; i >= 0; i--)
    {
      FloorLayer layer = (FloorLayer)i;
      for (int x = xMin; x < xMin + chunkSize; x++)
      {
        for (int y = yMin; y < yMin + chunkSize; y++)
        { DestroyWallObjectAtLocation(layer, new Vector2Int(x, y)); }
      }
    }
  }

  public void CreateAndPopulatePlacedObjectsForChunk(int xMin, int yMin)
  {
    int count = 0;
    for (int i = Enum.GetValues(typeof(FloorLayer)).Length - 1; i >= 0; i--)
    {
      FloorLayer layer = (FloorLayer)i;
      LayerFloor layerFloor = GridManager.Instance.layerFloors[layer];
      Tilemap groundTilemap = layerFloor.groundTilemap;
      Tilemap objectTilemap = layerFloor.objectTilemap;
      Tilemap waterTilemap = layerFloor.waterTilemap;
      Tilemap infoTilemap = layerFloor.infoTilemap;
      for (int x = xMin; x < xMin + chunkSize; x++)
      {
        for (int y = yMin; y < yMin + chunkSize; y++)
        {
          TileLocation loc = new TileLocation(new Vector2Int(x, y), layer);
          Vector2 heightValue = Vector2.up;
          if ((EnvironmentTile)objectTilemap.GetTile(loc.tilemapCoordinatesVector3) != null)
          {
            heightValue = Vector2.zero;
          }
          if (heightGrid[loc.floorLayer].ContainsKey(GridManager.Instance.CoordsToKey(loc.tilemapCoordinates)))
          {
            heightValue = heightGrid[loc.floorLayer][GridManager.Instance.CoordsToKey(loc.tilemapCoordinates)];
          }
          if (heightValue != Vector2.up)
          {
            count++;
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
    // Debug.Log("object count at " + xMin + "," + yMin + ": " + count);
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

  public void LoadChunk(TileLocation loc)
  {
    LoadChunk(loc.chunkCoordinates);
  }
  public void LoadChunk(Vector2Int chunkCoords)
  {
    CreateAndPopulatePlacedObjectsForChunk(chunkCoords);
  }

  public void UnloadChunk(Vector2Int chunkCoords)
  {
    UnloadPlacedObjectsForChunk(chunkCoords);
  }

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