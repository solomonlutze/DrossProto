using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;



public class WorldGridData : ScriptableObject
{

  [HideInInspector]
  public Vector2IntToStringDictionary chunkScenes;
  [HideInInspector]
  public FloorLayerToTileInfosDictionary worldGrid;

  public FloorLayerToEnvironmentTileDataDictionary environmentTileDataGrid;

  public int minXAcrossAllFloors = -5000;
  public int maxXAcrossAllFloors = 5000;
  public int minYAcrossAllFloors = -5000;
  public int maxYAcrossAllFloors = 5000;
  [Tooltip("Square size of a chunk, in tiles")]
  public int chunkSize = 10; // square size of a chunk, in tiles
  [HideInInspector]
  public Vector2Int chunksToLoad
  {
    get
    {
      if (Application.IsPlaying(GridManager.Instance.gameObject))
      {
        return chunksToLoad_runtime;
      }
      return chunksToLoad_editor;
    }
  }
  [Tooltip("how many chunks to load at once *in editor* on either side of current chunk. EG 1,1 = 3x3 grid")]
  public Vector2Int chunksToLoad_editor;
  [Tooltip("how many chunks to load at once *during runtime* on either side of current chunk. EG 1,1 = 3x3 grid")]
  public Vector2Int chunksToLoad_runtime;
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
    data.environmentTileDataGrid = new FloorLayerToEnvironmentTileDataDictionary();
    for (int i = Enum.GetValues(typeof(FloorLayer)).Length - 1; i >= 0; i--)
    {
      FloorLayer layer = (FloorLayer)i;
      data.worldGrid[layer] = new IntToEnvironmentTileInfoDictionary();
      data.environmentTileDataGrid[layer] = new IntToEnvironmentTileDataDictionary();
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
  }


  [MenuItem("CustomTools/SortTilesToCorrectTilemaps")]
  private static void SortTilesToCorrectTilemaps()
  {
    WorldGridData worldGridData = Resources.Load("Data/EnvironmentData/WorldGridData") as WorldGridData;
    Debug.Log("loading world grid data: " + worldGridData);
    int count = 0;
    foreach (LayerFloor layerFloor in GridManager.Instance.layerFloors.Values)
    {
      Dictionary<FloorTilemapType, Tilemap> tilemapDict = new Dictionary<FloorTilemapType, Tilemap>() {
        {FloorTilemapType.Ground, layerFloor.groundTilemap },
        {FloorTilemapType.Object, layerFloor.objectTilemap },
        {FloorTilemapType.Water, layerFloor.waterTilemap }
      };
      for (int x = worldGridData.minXAcrossAllFloors; x < worldGridData.maxXAcrossAllFloors; x++)
      {
        for (int y = worldGridData.maxYAcrossAllFloors; y > worldGridData.minYAcrossAllFloors; y--)
        {
          Vector3Int pos = new Vector3Int(x, y, 0);
          foreach (FloorTilemapType type in tilemapDict.Keys)
          {
            EnvironmentTile tile = (EnvironmentTile)tilemapDict[type].GetTile(pos);
            if (tile != null && tile.floorTilemapType != type)
            {
              count++;
              tilemapDict[type].SetTile(pos, null);
              tilemapDict[tile.floorTilemapType].SetTile(pos, tile);
            }
          }
          // check each tilemap, move a tile of the wrong type to the appropriate tilemap
        }
      }
    }
    Debug.Log("total misplaced tiles: " + count);
  }
  // [MenuItem("CustomTools/RepopulateWaterTilemaps")]
  // public static void RepopulateWaterTilemaps()
  // {
  //   foreach (LayerFloor layerFloor in GridManager.Instance.layerFloors.Values)
  //   {
  //     if (layerFloor.waterTilemap == null)
  //     {
  //       layerFloor.waterTilemap = Instantiate(GridManager.Instance.worldGridData.waterTilemapPrefab);
  //       layerFloor.waterTilemap.gameObject.transform.parent = layerFloor.groundTilemap.transform.parent;
  //       layerFloor.waterTilemap.name = LayerMask.LayerToName(layerFloor.gameObject.layer) + "_Water";
  //     }
  //     else
  //     {
  //       layerFloor.waterTilemap.transform.localPosition = Vector3.zero;
  //     }
  //   }
  // }

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

  // 0,0 is the default - floors extend nowhere, and wall objects take up the whole height. 
  // --we do not need to track this as height data, but we do need a wallObject if there's an objectTile at that location.
  // 0,1 is "empty" - floors do not extend, ceilings do not extend. 
  // --this is not the default, but it's also not a wallObject. We SHOULD track it, and should NOT have a wallObject. 

  public void SetEnvironmentTileData(TileLocation location, EnvironmentTileData tileData)
  {
    if (tileData.groundHeight != 0 || tileData.ceilingHeight != 1)
    {
      if (float.IsNaN(tileData.groundHeight) || float.IsNaN(tileData.ceilingHeight))
      {
        if (float.IsNaN(tileData.groundHeight))
        {
          Debug.Log("trying to set ground height to NAN at " + location.cellCenterPosition + ", value " + tileData.groundHeight);
          tileData.groundHeight = 0;
        }
        if (float.IsNaN(tileData.ceilingHeight))
        {
          Debug.Log("trying to set ceiling height to NAN at " + location.cellCenterPosition + ", value " + tileData.ceilingHeight);
          tileData.ceilingHeight = 1;
        }
      }
      environmentTileDataGrid[location.floorLayer][GridManager.Instance.CoordsToKey(location.tilemapCoordinates)] = tileData;
    }
    else
    {
      RemoveEnvironmentTileDataAtLocation(location.floorLayer, location.tilemapCoordinates);
    }
    EditorUtility.SetDirty(this);
    // AssetDatabase.SaveAssets();
  }

  public void ModifyFloorHeight(FloorLayer layer, Vector3Int location, EnvironmentTile tile, float height)
  {
    if (!environmentTileDataGrid.ContainsKey(layer)) { return; }
    TileLocation loc = new TileLocation(location.x, location.y, layer);
    bool ceiling = tile && tile.floorTilemapType == FloorTilemapType.Object;
    EnvironmentTileData tileData;
    if (environmentTileDataGrid[layer].TryGetValue(GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y)), out tileData))
    {
      if (float.IsNaN(tileData.groundHeight))
      {
        Debug.Log("trying to set NAN while modifying ground height, value " + tileData.groundHeight);
        tileData.groundHeight = 0;
      }
      if (float.IsNaN(tileData.ceilingHeight))
      {
        Debug.Log("trying to set NAN while modifying ceiling height, value " + tileData.ceilingHeight);
        tileData.ceilingHeight = 0;
      }
    }
    else
    {
      tileData = new EnvironmentTileData();
    }
    if (ceiling)
    {
      tileData.ceilingHeight = height;
    }
    else
    {
      tileData.groundHeight = height;
    }
    SetEnvironmentTileData(loc, tileData);
    AdjustWallObject(loc);
  }
  public void ClearWallObject(FloorLayer layer, Vector3Int location, EnvironmentTile tile)
  {
    Debug.Log("clear wall object");
    if (tile != null && tile.floorTilemapType == FloorTilemapType.Object)
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

#endif

  public void CreateTileParticleSystem(TileLocation tileLocation)
  {
    EnvironmentTileInfo eti = GridManager.Instance.GetTileAtLocation(tileLocation);
    TilePlacedObjects tilePlacedObjects = GetPlacedObjectsAtLocation(tileLocation, true);
    TileParticleSystem system = eti.GetTileParticleSystem();
    if (system != null)
    {
      tilePlacedObjects.tileParticleSystem = Instantiate(system);
      tilePlacedObjects.tileParticleSystem.Init(tileLocation, eti);
      tilePlacedObjects.tileParticleSystem.transform.position = tileLocation.cellCenterWorldPosition;
      tilePlacedObjects.tileParticleSystem.transform.SetParent(GridManager.Instance.wallObjectContainer, true);
    }
  }

  public void AdjustWallObject(TileLocation tileLocation)
  {
    EnvironmentTileInfo eti = GridManager.Instance.GetTileAtLocation(tileLocation);
    EnvironmentTileData tileData = GetEnvironmentTileData(tileLocation);
    if (tileData.IsEmpty() && eti.objectTileType == null)
    {
      DestroyPlacedObjectsAtLocation(tileLocation.floorLayer, tileLocation.tilemapCoordinates);
      return;
    }
    TilePlacedObjects tilePlacedObjects = GetPlacedObjectsAtLocation(tileLocation, true);
    // if (GridManager.Instance.placedGameObjects.ContainsKey(CoordsToKey(tileLocation)) && GridManager.Instance.placedGameObjects[CoordsToKey(tileLocation)] != null)
    // {
    //   wallObject = GridManager.Instance.placedGameObjects[CoordsToKey(tileLocation)].wallObject;
    // }
    if (tilePlacedObjects.wallObject == null)
    {
      tilePlacedObjects.wallObject = ObjectPoolManager.Instance.GetWallObjectPool().GetObject();
      tilePlacedObjects.wallObject.transform.position = tileLocation.cellCenterWorldPosition;
      tilePlacedObjects.wallObject.transform.SetParent(GridManager.Instance.wallObjectContainer, true);
    }
    if (eti.groundTileType != null)
    {
      Debug.Log("setting ground info to " + eti.groundTileType);
      tilePlacedObjects.wallObject.SetGroundInfo(eti.groundTileType, tileData.groundHeight);
    }
    if (eti.objectTileType != null)
    {
      tilePlacedObjects.wallObject.SetCeilingInfo(eti.objectTileType, tileData.ceilingHeight);
    }
    Debug.Log("should be init'ing wall object");
    tilePlacedObjects.wallObject.Init(tileLocation);
  }

  public void RemoveEnvironmentTileDataAtLocation(FloorLayer layer, Vector2Int location)
  {
    if (environmentTileDataGrid[layer].ContainsKey(GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y))))
    {
      environmentTileDataGrid[layer].Remove(GridManager.Instance.CoordsToKey(new Vector2Int(location.x, location.y)));
    }
  }

  public TilePlacedObjects GetPlacedObjectsAtLocation(TileLocation loc, bool newIfEmpty = false)
  {
    if (GridManager.Instance.placedGameObjects.ContainsKey(CoordsToKey(loc)) && GridManager.Instance.placedGameObjects[CoordsToKey(loc)] != null)
    {
      return GridManager.Instance.placedGameObjects[CoordsToKey(loc)];
    }
    if (newIfEmpty)
    {
      TilePlacedObjects newPlacedObjects = new TilePlacedObjects();
      GridManager.Instance.placedGameObjects[CoordsToKey(loc)] = newPlacedObjects;
      return newPlacedObjects;
    }
    return null;
  }

  public void DestroyPlacedObjectsAtLocation(FloorLayer layer, Vector2Int location)
  {
    TileLocation loc = new TileLocation(location.x, location.y, layer);
    TilePlacedObjects placedObject;
    int key = CoordsToKey(loc);
    if (GridManager.Instance.placedGameObjects.TryGetValue(key, out placedObject) && placedObject != null)
    {
      if (placedObject.wallObject != null)
      {
        ObjectPoolManager.Instance.wallObjectPool.Release(placedObject.wallObject);
      }
      if (placedObject.tileParticleSystem != null)
      {
        Destroy(placedObject.tileParticleSystem.gameObject);
      }
      GridManager.Instance.placedGameObjects.Remove(key);
    }
  }

  public float GetFloorHeight(TileLocation loc)
  {
    return GetFloorHeight(loc.floorLayer, loc.tilemapCoordinates);
  }

  public bool HasFloorHeightInfo(TileLocation loc)
  {
    return environmentTileDataGrid[loc.floorLayer].ContainsKey(GridManager.Instance.CoordsToKey(loc.tilemapCoordinates));
  }

  public EnvironmentTileData GetEnvironmentTileData(TileLocation loc)
  {
    EnvironmentTileData tileData;
    return environmentTileDataGrid[loc.floorLayer].TryGetValue(GridManager.Instance.CoordsToKey(loc.tilemapCoordinates), out tileData) ? tileData : new EnvironmentTileData();
  }

  public float GetFloorHeight(FloorLayer layer, Vector2Int location)
  {
    int locKey = GridManager.Instance.CoordsToKey(location);
    if (environmentTileDataGrid[layer].ContainsKey(locKey) && environmentTileDataGrid[layer][locKey] != null)
    {
      return environmentTileDataGrid[layer][locKey].groundHeight;
    }
    return 0;
  }

  // Height Grid should contain a value if the expected height isn't (0,1) - that is, floor at 0 and ceiling at 1.
  // Thus, any full-wall tile should contain a (0,0) entry.
  public void CleanAllEnvironmentTileDataValues()
  {
    FloorLayerToEnvironmentTileDataDictionary newEnvironmentTileDataGrid = new FloorLayerToEnvironmentTileDataDictionary();
    Debug.Log("cleaning");
    foreach (FloorLayer fl in (FloorLayer[])Enum.GetValues(typeof(FloorLayer)))
    {
      Debug.Log("cleaning layer " + fl);
      newEnvironmentTileDataGrid[fl] = new IntToEnvironmentTileDataDictionary();
      for (int x = minXAcrossAllFloors; x < maxXAcrossAllFloors; x++)
      {
        for (int y = maxYAcrossAllFloors; y > minYAcrossAllFloors; y--)
        {
          TileLocation loc = new TileLocation(new Vector2Int(x, y), fl);
          EnvironmentTileData tileData = new EnvironmentTileData();
          EnvironmentTileInfo info = GridManager.Instance.GetTileAtLocation(loc);
          if (info.objectTileType != null)
          {
            Debug.Log("has object!");
            tileData.ceilingHeight = 0;
          }
          if (environmentTileDataGrid.ContainsKey(loc.floorLayer) && environmentTileDataGrid[loc.floorLayer].ContainsKey(GridManager.Instance.CoordsToKey(loc.tilemapCoordinates)))
          {
            tileData = environmentTileDataGrid[loc.floorLayer][GridManager.Instance.CoordsToKey(loc.tilemapCoordinates)];
            if (tileData.groundHeight < 0)
            {
              tileData.groundHeight = 0;
            }
            if (tileData.groundHeight > 1)
            {
              tileData.groundHeight = 1;
            }
            if (tileData.ceilingHeight < 0)
            {
              tileData.ceilingHeight = 0;
            }
            if (tileData.ceilingHeight > 1)
            {
              tileData.ceilingHeight = 1;
            }
          }
          if (!tileData.IsEmpty())
          {
            newEnvironmentTileDataGrid[fl][GridManager.Instance.CoordsToKey(loc.tilemapCoordinates)] = tileData;
          }
        }
      }
    }

    Debug.Log("assigning new grid");
    environmentTileDataGrid = newEnvironmentTileDataGrid;
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
      GridManager.Instance.placedGameObjects = new Dictionary<int, TilePlacedObjects>();
    }
    foreach (TilePlacedObjects obj in GridManager.Instance.placedGameObjects.Values)
    {
      if (obj != null && obj.wallObject != null)
      {
        ObjectPoolManager.Instance.GetWallObjectPool().Release(obj.wallObject);
      }
    }
    GridManager.Instance.ClearLoadedChunks();
    GridManager.Instance.placedGameObjects.Clear();
    int childCount = GridManager.Instance.wallObjectContainer.transform.childCount;
    for (int i = childCount - 1; i >= 0; i--)
    {
      DestroyImmediate(GridManager.Instance.wallObjectContainer.transform.GetChild(i).gameObject);
    }
  }

  public void ClearExistingPlacedObjectsAndPool()
  {
    GridManager.Instance.ClearLoadedChunksAndResetPool();
  }

  public void CountExistingPlacedObjects()
  {
    Debug.Log("placed game objects count: " + GridManager.Instance.placedGameObjects.Count);
  }

  public void CountEnvironmentTileDatas()
  {
    foreach (FloorLayer fl in environmentTileDataGrid.Keys)
    {
      Debug.Log("count for " + fl + ": " + environmentTileDataGrid[fl].Count);
    }
  }

  public void AddWallsToEnvironmentTileData()
  {

    for (int i = Enum.GetValues(typeof(FloorLayer)).Length - 1; i >= 0; i--)
    {
      FloorLayer layer = (FloorLayer)i;
      if (!GridManager.Instance.layerFloors.ContainsKey(layer))
      {
        continue;
      }
      LayerFloor layerFloor = GridManager.Instance.layerFloors[layer];
      Tilemap objectTilemap = layerFloor.objectTilemap;
      for (int x = minXAcrossAllFloors; x < maxXAcrossAllFloors; x++)
      {
        for (int y = maxYAcrossAllFloors; y > minYAcrossAllFloors; y--)
        {
          TileLocation loc = new TileLocation(new Vector2Int(x, y), layer);
          EnvironmentTileData tileData = new EnvironmentTileData();
          if ((EnvironmentTile)objectTilemap.GetTile(loc.tilemapCoordinatesVector3) != null)
          {
            tileData.ceilingHeight = 0;
          }
          if (environmentTileDataGrid[loc.floorLayer].ContainsKey(GridManager.Instance.CoordsToKey(loc.tilemapCoordinates)))
          {
            tileData = environmentTileDataGrid[loc.floorLayer][GridManager.Instance.CoordsToKey(loc.tilemapCoordinates)];
          }
          if (!tileData.IsEmpty())
          {

          }
        }
      }
    }
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
          EnvironmentTileData tileData = new EnvironmentTileData();
          if ((EnvironmentTile)objectTilemap.GetTile(loc.tilemapCoordinatesVector3) != null)
          {
            tileData.ceilingHeight = 0;
          }
          if (environmentTileDataGrid[loc.floorLayer].ContainsKey(GridManager.Instance.CoordsToKey(loc.tilemapCoordinates)))
          {
            tileData = environmentTileDataGrid[loc.floorLayer][GridManager.Instance.CoordsToKey(loc.tilemapCoordinates)];
          }
          if (!tileData.IsEmpty())
          {
            AdjustWallObject(
              loc
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
        {
          DestroyPlacedObjectsAtLocation(layer, new Vector2Int(x, y));
        }
      }
    }
  }

  public void CreateAndPopulatePlacedObjectsForChunk(int xMin, int yMin)
  {
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
          EnvironmentTileData tileData = new EnvironmentTileData();
          if ((EnvironmentTile)objectTilemap.GetTile(loc.tilemapCoordinatesVector3) != null)
          {
            tileData.ceilingHeight = 0;
          }
          if (environmentTileDataGrid[loc.floorLayer].ContainsKey(GridManager.Instance.CoordsToKey(loc.tilemapCoordinates)))
          {
            tileData = environmentTileDataGrid[loc.floorLayer][GridManager.Instance.CoordsToKey(loc.tilemapCoordinates)];
          }
          EnvironmentTileInfo tileInfo = GridManager.Instance.GetTileAtLocation(loc);
          CreateTileParticleSystem(loc);
          if (!tileData.IsEmpty() && tileInfo.objectTileType != null)
          {
            AdjustWallObject(
              loc
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

}