using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

// this sucks
public static class GridConstants
{
  // below are world-scale distances between center of a hex and one in the adjacent column (x) or row (y)
  public const float X_SPACING = 0.87625f; // actual width of hexes
  public const float Y_SPACING = .75f; // y-distance between 2 hexes in adjacent rows
}
public enum TilemapDirection { None, UpperLeft, UpperRight, Left, Right, LowerLeft, LowerRight, Above, Below }
// public enum TilemapCorner { None, UpperLeft, UpperRight, LowerLeft, LowerRight }

public class TileLocation
{
  public Vector2Int tilemapCoordinates;
  public Vector3Int tilemapCoordinatesVector3
  {
    get
    {
      return new Vector3Int(
        tilemapCoordinates.x,
        tilemapCoordinates.y,
        // (int)GridManager.GetZOffsetForFloor(WorldObject.GetGameObjectLayerFromFloorLayer(floorLayer))
        0 // gfy
      );
    }
  }
  public Vector3 worldPosition;
  // use this for putting child objects on the tilemap? I guess?
  public Vector3 cellCenterPosition
  {
    get
    {
      int zOffSet = GridManager.GetZOffsetForGameObjectLayer(WorldObject.GetGameObjectLayerFromFloorLayer(floorLayer));
      Vector3 worldPos = GridManager.Instance.levelGrid.CellToWorld(new Vector3Int(
        tilemapCoordinates.x,
        tilemapCoordinates.y,
        zOffSet)
      );
      return new Vector3(worldPos.x, worldPos.y, zOffSet);
      // return GridManager.Instance.levelGrid.CellToWorld(new Vector3Int(
      //   tilemapCoordinates.x,
      //   tilemapCoordinates.y,
      //   GridManager.GetZOffsetForGameObjectLayer(WorldObject.GetGameObjectLayerFromFloorLayer(floorLayer)))
      // );
    }
  }

  public Vector3 cellCenterWorldPosition
  {
    get
    {
      return cellCenterPosition + new Vector3(0, 0, 0f);
    }
  }

  public Vector3 cubeCoords;
  public FloorLayer floorLayer;

  public TileLocation(int cellLocationX, int cellLocationY, FloorLayer fl)
  {
    Initialize(GridManager.Instance.levelGrid.CellToWorld(new Vector3Int(
      cellLocationX,
      cellLocationY,
      (int)GridManager.GetZOffsetForGameObjectLayer(WorldObject.GetGameObjectLayerFromFloorLayer(fl)))
    ), fl);

  }

  public TileLocation(Vector3 worldPos, FloorLayer fl) // NOTE: This will retain specific info on location within a cell in all float coords!
  {
    Initialize(worldPos, fl);
    // Vector2Int cellPos = GridManager.Instance.levelGrid.WorldToCell(worldPos);
    // worldPosition = GridManager.Instance.levelGrid.CellToWorld(cellPos); // rounds the position to something consistent
    // tilemapPosition = new Vector2Int(cellPos.x, cellPos.y);
    // floorLayer = fl;
  }
  public static TileLocation FromCubicCoords(Vector3 cubicCoords, FloorLayer fl)
  {
    float oddZ = (float)(Mathf.RoundToInt(cubicCoords.z) & 1);
    float gridX = cubicCoords.x + (cubicCoords.z - oddZ) / 2.0f;
    float gridY = cubicCoords.z * -1;
    if ((Mathf.RoundToInt(gridY) & 1) == 0)
    {
      gridX = gridX * GridConstants.X_SPACING;
    }
    else
    {
      gridX = (gridX + .5f) * GridConstants.X_SPACING;
    }
    gridY = gridY * GridConstants.Y_SPACING;
    return new TileLocation(
      new Vector3(
        gridX,
        gridY,
        (int)GridManager.GetZOffsetForGameObjectLayer(WorldObject.GetGameObjectLayerFromFloorLayer(fl))),
      fl
    );
  }
  public TileLocation(Vector2Int pos, FloorLayer fl) // NOTE: This will return the center of each tile in all float coords!;
  {
    Initialize(GridManager.Instance.levelGrid.CellToWorld(new Vector3Int(
      pos.x,
      pos.y,
      (int)GridManager.GetZOffsetForGameObjectLayer(WorldObject.GetGameObjectLayerFromFloorLayer(fl)))
    ), fl);
  }


  public void Initialize(Vector3 worldPos, FloorLayer fl)
  {
    tilemapCoordinates = (Vector2Int)GridManager.Instance.levelGrid.WorldToCell(worldPos);
    worldPosition = worldPos;
    // First to vector3Int
    cubeCoords = new Vector3();
    cubeCoords.y = -1 * worldPosition.y / GridConstants.Y_SPACING;
    // Debug.Log("cubeCoords.y: " + cubeCoords.y);
    if ((Mathf.RoundToInt(cubeCoords.y) & 1) == 0)
    {
      cubeCoords.x = worldPosition.x / GridConstants.X_SPACING;
    }
    else
    {
      cubeCoords.x = (worldPosition.x - .5f) / GridConstants.X_SPACING;
    }
    //then to cube coord
    float oddY = (float)(Mathf.RoundToInt(cubeCoords.y) & 1);
    cubeCoords.x = cubeCoords.x - (cubeCoords.y - oddY) / 2.0f;
    cubeCoords.z = cubeCoords.y;
    cubeCoords.y = -cubeCoords.x - cubeCoords.z;
    floorLayer = fl;
  }

  public Vector3Int CubeCoordsInt()
  {
    Vector3Int res = new Vector3Int();
    int inverseY = -tilemapCoordinates.y; // aefawefawefawefoianwefoihawefiohaweiofaweoifhaweihofawefhioaweihowefioh
    res.x = tilemapCoordinates.x - (inverseY - (inverseY & 1)) / 2;
    res.z = inverseY;
    res.y = -res.x - res.z;
    return res;
  }

  public int IntDistanceTo(TileLocation b) // NOTE: does not include floor height
  {
    Vector3Int ccA = CubeCoordsInt();
    Vector3Int ccB = b.CubeCoordsInt();
    return (Mathf.Abs(ccA.x - ccB.x) + Mathf.Abs(ccA.y - ccB.y) + Mathf.Abs(ccA.z - ccB.z)) / 2;
  }
  public float DistanceTo(TileLocation b)
  {
    return (Mathf.Abs(cubeCoords.x - b.cubeCoords.x) + Mathf.Abs(cubeCoords.y - b.cubeCoords.y) + Mathf.Abs(cubeCoords.z - b.cubeCoords.z)) / 2;
  }

  public static bool operator ==(TileLocation t1, TileLocation t2)
  {
    if (ReferenceEquals(t1, t2)) { return true; }
    if (ReferenceEquals(t1, null))
    {
      return false;
    }
    if (ReferenceEquals(t2, null))
    {
      return false;
    }
    return
    (t1 == null && t2 == null) ||
    t1.tilemapCoordinates.Equals(t2.tilemapCoordinates) && t1.floorLayer.Equals(t2.floorLayer);
  }

  public static bool operator !=(TileLocation t1, TileLocation t2)
  {
    if (ReferenceEquals(t1, t2)) { return false; }
    if (ReferenceEquals(t1, null))
    {
      return true;
    }
    if (ReferenceEquals(t2, null))
    {
      return true;
    }
    return !t1.tilemapCoordinates.Equals(t2.tilemapCoordinates) || !t1.floorLayer.Equals(t2.floorLayer);
  }
  public override bool Equals(object obj)
  {
    bool rc = false;
    if (obj is TileLocation)
    {
      TileLocation tl2 = obj as TileLocation;
      rc = (this == tl2);
    }
    return rc;
  }

  public override int GetHashCode()
  {
    return tilemapCoordinates.GetHashCode() + floorLayer.GetHashCode();
  }
  public override string ToString()
  {
    return floorLayer.ToString() + ", " + tilemapCoordinates.ToString();
  }
  public int x
  {
    get { return tilemapCoordinates.x; }
  }

  public int y
  {
    get { return tilemapCoordinates.y; }
  }
  public Vector3 position3D
  {
    get { return new Vector3(tilemapCoordinates.x, tilemapCoordinates.y, 0); }
  }
}

public class GridManager : Singleton<GridManager>
{

  public bool DEBUG_IgnoreLighting;
  public Grid levelGrid;
  public Material semiTransparentMaterial;
  public Material fullyOpaqueMaterial;
  public LayerToLayerFloorDictionary layerFloors;
  public Dictionary<FloorLayer, Dictionary<int, EnvironmentTileInfo>> worldGrid;

  private List<EnvironmentTileInfo> tilesToDestroyOnPlayerRespawn;

  private List<EnvironmentTileInfo> tilesToRestoreOnPlayerRespawn;
  public GameObject highlightTilePrefab;
  public HashSet<EnvironmentTileInfo> visibleTiles;
  public HashSet<EnvironmentTileInfo> recentlyVisibleTiles;
  public List<List<EnvironmentTileInfo>> tilesToMakeVisible;
  public List<List<EnvironmentTileInfo>> tilesToMakeObscured;

  public HashSet<EnvironmentTileInfo> lightSources;
  public HashSet<EnvironmentTileInfo> litTiles;
  public HashSet<EnvironmentTileInfo> tilesToRecalculateLightingFor;
  public EnvironmentTile visibilityTile;
  public Tilemap waterTilemapPrefab;
  public WallObject defaultWallObject;

  public Color nonVisibleTileColor;
  public LightSourceInfo sunlight;
  [Tooltip("Time it takes a tile to fade in, in seconds")]
  public float tileFadeTime = .25f;
  int interestObjectsCount = 0;
  public TileLocation currentPlayerLocation;

  int minXAcrossAllFloors;
  int maxXAcrossAllFloors;
  int minYAcrossAllFloors;
  int maxYAcrossAllFloors;
  public void Awake()
  {
    worldGrid = new Dictionary<FloorLayer, Dictionary<int, EnvironmentTileInfo>>();
    tilesToDestroyOnPlayerRespawn = new List<EnvironmentTileInfo>();
    tilesToRestoreOnPlayerRespawn = new List<EnvironmentTileInfo>();
    visibleTiles = new HashSet<EnvironmentTileInfo>();
    recentlyVisibleTiles = new HashSet<EnvironmentTileInfo>();
    tilesToMakeVisible = new List<List<EnvironmentTileInfo>>();
    tilesToMakeObscured = new List<List<EnvironmentTileInfo>>();
    lightSources = new HashSet<EnvironmentTileInfo>();
    tilesToRecalculateLightingFor = new HashSet<EnvironmentTileInfo>();
    Tilemap groundTilemap;
    Tilemap objectTilemap;
    Tilemap waterTilemap;
    Tilemap visibilityTilemap;
    Tilemap infoTilemap;
    maxXAcrossAllFloors = -5000;
    minXAcrossAllFloors = 5000;
    minYAcrossAllFloors = 5000;
    maxYAcrossAllFloors = -5000;
    foreach (LayerFloor lf in layerFloors.Values)
    {
      groundTilemap = lf.groundTilemap;
      lf.waterTilemap = Instantiate(waterTilemapPrefab, lf.gameObject.transform);
      lf.waterTilemap.gameObject.layer = lf.gameObject.layer;
      lf.waterTilemap.GetComponent<TilemapRenderer>().sortingLayerName = LayerMask.LayerToName(lf.gameObject.layer);
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
    HashSet<EnvironmentTileInfo> litTiles = new HashSet<EnvironmentTileInfo>();
    HashSet<EnvironmentTileInfo> currentTilesToLight = new HashSet<EnvironmentTileInfo>();
    HashSet<EnvironmentTileInfo> nextTilesToLight = new HashSet<EnvironmentTileInfo>();
    for (int i = Enum.GetValues(typeof(FloorLayer)).Length - 1; i >= 0; i--)
    {
      FloorLayer layer = (FloorLayer)i;
      worldGrid[layer] = new Dictionary<int, EnvironmentTileInfo>();
      if (!layerFloors.ContainsKey(layer))
      {
        continue;
      }
      LayerFloor layerFloor = layerFloors[layer];
      groundTilemap = layerFloor.groundTilemap;
      objectTilemap = layerFloor.objectTilemap;
      waterTilemap = layerFloor.waterTilemap;
      infoTilemap = layerFloor.infoTilemap;
      if (infoTilemap != null) { infoTilemap.gameObject.SetActive(false); }
      visibilityTilemap = layerFloor.visibilityTilemap;
      visibilityTilemap.gameObject.SetActive(false);
      for (int x = minXAcrossAllFloors; x < maxXAcrossAllFloors; x++)
      {
        // for (int y = minYAcrossAllFloors; y < maxYAcrossAllFloors; y++)
        for (int y = maxYAcrossAllFloors; y > minYAcrossAllFloors; y--)
        {
          //get both object and ground tile, build an environmentTileInfo out of them, and put it into our worldGrid
          TileLocation loc = new TileLocation(new Vector2Int(x, y), layer);
          ConstructAndSetEnvironmentTileInfo(loc, groundTilemap, objectTilemap, visibilityTilemap, infoTilemap, waterTilemap, litTiles, currentTilesToLight);
        }
      }
      // }
    }
    if (!DEBUG_IgnoreLighting)
    {
      InitializeLighting(litTiles, currentTilesToLight);

    }
    foreach (EnvironmentTileInfo source in lightSources)
    {
      AddIlluminationSourceToNeighbors(source);
    }
    Debug.Log("lightSources length: " + lightSources.Count);
  }

  float opacity = 0;
  int sign = 1;
  public void Update()
  {
    // tilesToRecalculateLightingFor.Clear();
    // HashSet<EnvironmentTileInfo> tempSet;
    // foreach (EnvironmentTileInfo lightSource in lightSources)
    // {
    //   tempSet = lightSource.IlluminateNeighbors();
    //   if (tempSet != null)
    //   {
    //     tilesToRecalculateLightingFor.UnionWith(tempSet);
    //   }
    // }
    // foreach (EnvironmentTileInfo litTile in tilesToRecalculateLightingFor)
    // {
    //   litTile.RecalculateIllumination();
    // }
    // if (tilesToMakeObscured.Count > 0)
    // {
    //   for (int i = tilesToMakeObscured[0].Count - 1; i >= 0; i--)
    //   {
    //     EnvironmentTileInfo tile = tilesToMakeObscured[0][i];
    //     if (tilesToRecalculateLightingFor.Contains(tile))
    //     {
    //       tilesToRecalculateLightingFor.Remove(tile);
    //     }
    //     if (visibleTiles.Contains(tile) || tile.IsEmpty())
    //     {
    //       tilesToMakeObscured[0].Remove(tile);
    //       continue;
    //     }
    //     Color c = tile.illuminationInfo.opaqueColor;
    //     c.a = layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.GetColor(tile.tileLocation.tilemapCoordinatesVector3).a;
    //     c.a += Time.deltaTime / tileFadeTime;
    //     if (c.a >= 1)
    //     {
    //       c.a = 1;
    //       tilesToMakeObscured[0].Remove(tile);
    //     }
    //     layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.SetColor(
    //       tile.tileLocation.tilemapCoordinatesVector3, c);
    //   }
    //   if (tilesToMakeObscured[0].Count == 0)
    //   {
    //     tilesToMakeObscured.RemoveAt(0);
    //   }
    // }
    // if (tilesToMakeVisible.Count > 0)
    // {
    //   for (int i = tilesToMakeVisible[0].Count - 1; i >= 0; i--)
    //   {
    //     EnvironmentTileInfo tile = tilesToMakeVisible[0][i];
    //     if (tilesToRecalculateLightingFor.Contains(tile))
    //     {
    //       tilesToRecalculateLightingFor.Remove(tile);
    //     }
    //     Color c = tile.illuminationInfo.visibleColor;
    //     c.a = layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.GetColor(tile.tileLocation.tilemapCoordinatesVector3).a;
    //     c.a -= Time.deltaTime / tileFadeTime;
    //     if (c.a <= tile.illuminationInfo.visibleColor.a)
    //     {
    //       c.a = tile.illuminationInfo.visibleColor.a;
    //       tilesToMakeVisible[0].Remove(tile);
    //     }
    //     layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.SetColor(
    //       tile.tileLocation.tilemapCoordinatesVector3, c);
    //   }
    //   if (tilesToMakeVisible[0].Count == 0)
    //   {
    //     tilesToMakeVisible.RemoveAt(0);
    //   }
    // }
    // foreach (EnvironmentTileInfo litTile in tilesToRecalculateLightingFor)
    // {
    //   // if (visibleTiles.Contains(litTile))
    //   // {
    //     layerFloors[litTile.tileLocation.floorLayer].visibilityTilemap.SetColor(litTile.tileLocation.tilemapCoordinatesVector3, litTile.illuminationInfo.visibleColor);
    //   // }
    //   // else
    //   // {
    //   //   layerFloors[litTile.tileLocation.floorLayer].visibilityTilemap.SetColor(litTile.tileLocation.tilemapCoordinatesVector3, litTile.illuminationInfo.opaqueColor);
    //   // }
    // }
  }

  public int CoordsToKey(Vector2Int coordinates)
  {
    return coordinates.x + ((maxXAcrossAllFloors - minXAcrossAllFloors + 1) * coordinates.y);
  }
  public EnvironmentTileInfo ConstructAndSetEnvironmentTileInfo(
    TileLocation loc,
    Tilemap groundTilemap,
    Tilemap objectTilemap,
    Tilemap visibilityTilemap,
    Tilemap infoTilemap,
    Tilemap waterTilemap,
    HashSet<EnvironmentTileInfo> litTiles = null,
    HashSet<EnvironmentTileInfo> currentTilesToLight = null
    )
  {
    Vector3Int v3pos = new Vector3Int(loc.tilemapCoordinates.x, loc.tilemapCoordinates.y, 0);
    EnvironmentTileInfo info = new EnvironmentTileInfo();
    visibilityTilemap.SetTile(v3pos, visibilityTile);
    EnvironmentTile objectTile = objectTilemap.GetTile(v3pos) as EnvironmentTile;
    EnvironmentTile groundTile = groundTilemap.GetTile(v3pos) as EnvironmentTile;
    InfoTile infoTile = infoTilemap.GetTile(v3pos) as InfoTile;

    info.Init(
      loc,
      groundTile,
      objectTile,
      infoTile
    );
    if (info.isLightSource)
    {
      foreach (LightRangeInfo i in info.lightSource.lightRangeInfos)
      {
        i.currentIntensity = i.defaultIntensity;
      }
      lightSources.Add(info);
    }
    if (loc.floorLayer == FloorLayer.F6 || GetAdjacentTile(loc, TilemapDirection.Above).IsEmptyAndSunlit())
    {
      info.AddIlluminatedBySource(sunlight, 0);
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
      info.wallObject = Instantiate(defaultWallObject);
      info.wallObject.transform.position = loc.cellCenterWorldPosition;
      info.wallObject.Init(loc, objectTile.sprite);
      // info.wallObject.transform.parent = groundTilemap.transform.parent;
    }
    if (info.HasTileTag(TileTag.Water))
    {
      waterTilemap.SetTile(v3pos, groundTilemap.GetTile(v3pos));
      groundTilemap.SetTile(v3pos, null);
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
    while (currentDistance < sunlight.lightRangeInfos.Length)
    {
      foreach (EnvironmentTileInfo tile in currentTilesToIlluminate)
      {
        if (currentDistance != 0)
        {
          tile.AddIlluminatedBySource(sunlight, currentDistance);
          litTiles.Add(tile);
        }
        if (!tile.IsEmpty())
        {
          layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.SetColor(tile.tileLocation.tilemapCoordinatesVector3, tile.illuminationInfo.visibleColor);
        }
        else
        {
          layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.SetColor(tile.tileLocation.tilemapCoordinatesVector3, Color.clear);
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
            if (dir == TilemapDirection.Above && (GetAdjacentTile(tile.tileLocation, dir) == null || !GetAdjacentTile(tile.tileLocation, dir).IsEmpty())) { continue; } // can't go up through a floor
            if (
              AdjacentTileIsValid(tile.tileLocation, dir)
              && GetAdjacentTile(tile.tileLocation, dir) != null
              && currentDistance != (sunlight.lightRangeInfos.Length - 1)
              && !litTiles.Contains(GetAdjacentTile(tile.tileLocation, dir)))
            {
              nextTilesToLight.Add(GetAdjacentTile(tile.tileLocation, dir));
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

  // Populate the world with small sprites that bridge gaps between areas.
  // NOTE: TO WORK CORRECTLY DURING WORLDMAP INITIALIZATION, THIS MUST BE CALLED ON THE TILE TO THE LEFT OF THE
  // TILE JUST ADDED.

  // DOUBLE NOTE: THIS PROBABLY WON'T WORK WITH HEX TILES 
  public void AddInterestObjects(TileLocation loc)
  {
    AddCornerInterestObjects(loc);
    foreach (TilemapDirection d in new TilemapDirection[] { TilemapDirection.Left, TilemapDirection.LowerLeft })
    {
      AddBorderInterestObjects(loc, d);
    }
  }

  public void AddBorderInterestObjects(TileLocation loc, TilemapDirection direction)
  {
    if (AdjacentTileIsValid(loc, direction))
    {
      EnvironmentTileInfo currentTile = GetTileAtLocation(loc);
      EnvironmentTileInfo adjacentTile = GetAdjacentTile(loc, direction);
      if (currentTile.groundTileType == adjacentTile.groundTileType || !currentTile.IsBorderClear(direction, adjacentTile)) { return; }
      {
        float interestPriority = currentTile.GetBorderInterestObjectPriority() - adjacentTile.GetBorderInterestObjectPriority() + UnityEngine.Random.value;
        if (interestPriority > .5f || !currentTile.AcceptsInterestObjects())
        {
          CreateAndPlaceInterestObject(currentTile, adjacentTile, direction);
        }
        else
        {
          // CreateAndPlaceInterestObject(adjacentTile, currentTile, GridManager.GetOppositeTilemapDirection(direction));
        }
      }
    }
  }

  // TODO: needs tidying for hex grid
  public void AddCornerInterestObjects(TileLocation loc)
  {
    // if (TileIsValid(loc) && GetTileAtLocation(loc).AcceptsInterestObjects())
    // {

    //   AddCornerInterestObject(GetAdjacentTileLocation(loc, TilemapDirection.Left), GetAdjacentTileLocation(loc, TilemapDirection.Down), loc, TilemapCorner.LowerLeft, 0);
    //   AddCornerInterestObject(GetAdjacentTileLocation(loc, TilemapDirection.Up), GetAdjacentTileLocation(loc, TilemapDirection.Left), loc, TilemapCorner.UpperLeft, 270);
    //   AddCornerInterestObject(GetAdjacentTileLocation(loc, TilemapDirection.Right), GetAdjacentTileLocation(loc, TilemapDirection.Up), loc, TilemapCorner.UpperRight, 180);
    //   AddCornerInterestObject(GetAdjacentTileLocation(loc, TilemapDirection.Down), GetAdjacentTileLocation(loc, TilemapDirection.Right), loc, TilemapCorner.LowerRight, 90);
    // }
  }

  // TODO: needs tidying for hex grid
  // public void AddCornerInterestObject(TileLocation loc1, TileLocation loc2, TileLocation destination, TilemapCorner corner, float rotation)
  // {
  //   if (TileIsValid(loc1) && TileIsValid(loc2))
  //   {
  //     GameObject obj = GetTileAtLocation(loc1).GetCornerInterestObject(GetTileAtLocation(loc2), GetTileAtLocation(destination));
  //     if (obj != null)
  //     {
  //       GetTileAtLocation(destination).cornerInterestObjects[corner] = InstantiateInterestObject(obj, GetTileAtLocation(destination), rotation);
  //     }
  //   }
  // }

  public void CreateAndPlaceInterestObject(EnvironmentTileInfo interestTile, EnvironmentTileInfo destinationTile, TilemapDirection direction)
  {
    GameObject obj = interestTile.GetBorderInterestObject();
    if (obj != null)
    {
      InstantiateInterestObject(obj, destinationTile, GetInterestRotationForDirection(direction));
    }
  }

  public GameObject InstantiateInterestObject(GameObject obj, EnvironmentTileInfo destinationTile, float rotation)
  {
    GameObject instance = Instantiate(obj);
    instance.isStatic = true;
    instance.transform.localPosition = new Vector3(destinationTile.tileLocation.cellCenterPosition.x, destinationTile.tileLocation.cellCenterPosition.y, 0);
    instance.transform.SetParent(layerFloors[destinationTile.tileLocation.floorLayer].interestObjects, false);
    instance.transform.eulerAngles = new Vector3(0, 0, rotation);
    instance.layer = LayerMask.NameToLayer(destinationTile.tileLocation.floorLayer.ToString());
    SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
    sr.sortingLayerName = destinationTile.tileLocation.floorLayer.ToString();
    sr.sortingOrder = 1;
    interestObjectsCount += 1;
    return instance;
  }
  public float GetInterestRotationForDirection(TilemapDirection direction)
  {
    // switch (direction)
    // {
    //   case TilemapDirection.Left:
    //     return 90f;
    //   case TilemapDirection.Down:
    //     return 180f;
    //   case TilemapDirection.Right:
    //     return 270f;
    //   case TilemapDirection.Up:
    //   default:
    return 0;
    // }
  }
  public EnvironmentTileInfo GetTileAtTilemapLocation(int x, int y, FloorLayer f)
  {
    return GetTileAtLocation(new TileLocation(x, y, f));
  }

  public EnvironmentTileInfo GetTileAtWorldPosition(Vector3 v, FloorLayer f)
  {
    return GetTileAtLocation(new TileLocation(v, f));
  }

  // WARNING: this'll blow up if you try to get an invalid tile, so, don't!
  public EnvironmentTileInfo GetTileAtLocation(TileLocation loc)
  {
    return worldGrid[loc.floorLayer][CoordsToKey(loc.tilemapCoordinates)];
  }

  public LayerFloor GetFloorLayerAbove(FloorLayer floorLayer)
  {
    FloorLayer fl = (FloorLayer)floorLayer + 1;
    if (layerFloors.ContainsKey(fl))
    {
      return layerFloors[fl];
    }
    return null;
  }

  public LayerFloor GetFloorLayerBelow(FloorLayer floorLayer)
  {
    FloorLayer fl = (FloorLayer)floorLayer - 1;
    if (layerFloors.ContainsKey(fl))
    {
      return layerFloors[fl];
    }
    return null;
  }
  // public EnvironmentTile GetTileAtLocation(Vector2 loc, FloorLayer floor, FloorTilemapType? floorTilemapType=null) {
  // 	Debug.Log("inside GetTileAtLocation?");
  // 	if (!layerFloors.ContainsKey(floor) || layerFloors[floor].groundTilemap == null || layerFloors[floor].objectTilemap == null)
  //     {
  //         Debug.LogWarning("missing layerFloor info for "+floor.ToString());
  //         return null;
  //     }
  //     LayerFloor layerFloor = layerFloors[floor];
  // 	Debug.Log("floor is "+floor);
  // 	EnvironmentTile tile = null;
  // 	if (floorTilemapType == null || floorTilemapType == FloorTilemapType.Object) {
  // 		Debug.Log("trying to select object tile");
  // 		tile = (EnvironmentTile) layerFloor.objectTilemap.GetTile(new Vector3Int(Mathf.FloorToInt(loc.x),Mathf.FloorToInt(loc.y),0));
  // 	}
  // 	if (tile == null || floorTilemapType == FloorTilemapType.Ground) {
  // 		Debug.Log("trying to select ground tile");
  // 		tile = (EnvironmentTile) layerFloor.groundTilemap.GetTile(new Vector3Int(Mathf.FloorToInt(loc.x),Mathf.FloorToInt(loc.y),0));
  // 	}
  // 	if (tile == null) { // Empty tile.
  // 		Debug.Log("tile is null");
  // 		return null; // TODO: Should maybe return some kind of placeholder "empty tile", rather than null
  // 	}
  // 	Debug.Log("getTileAtLocation: "+tile.gameObject.name + " "+tile.name);
  // 	return tile;
  // }

  public void OnLayerChange(FloorLayer floor)
  {
    // Used to control floor layer transparency.
    if (layerFloors.ContainsKey(floor))
    {
      LayerFloor newFloor = layerFloors[floor];
      newFloor.groundTilemap.GetComponent<TilemapRenderer>().material = fullyOpaqueMaterial;
      newFloor.objectTilemap.GetComponent<TilemapRenderer>().material = fullyOpaqueMaterial;

    }
    if (layerFloors.ContainsKey(floor + 1))
    {
      LayerFloor nextFloorUp = layerFloors[floor + 1];
      nextFloorUp.groundTilemap.GetComponent<TilemapRenderer>().material = semiTransparentMaterial;
      nextFloorUp.objectTilemap.GetComponent<TilemapRenderer>().material = semiTransparentMaterial;
    }
  }
  // public bool CanStickToAdjacentTile(Vector3 loc, FloorLayer floor)
  // {
  //   foreach (TilemapDirection d in new TilemapDirection[] {
  //     TilemapDirection.Up,
  //     TilemapDirection.Right,
  //     TilemapDirection.Down,
  //     TilemapDirection.Left,
  //   })
  //   {
  //     EnvironmentTileInfo et = GetAdjacentTile(loc, floor, d);
  //     if (et.CanBeStuckTo())
  //     {
  //       return true;
  //     }
  //   }
  //   return false;
  // }
  public bool CanBurrowOnCurrentTile(TileLocation tileLoc, Character c)
  {
    return GetTileAtLocation(tileLoc).groundTileTags.Contains(TileTag.Ground);
  }

  public bool CanDescendThroughCurrentTile(TileLocation location, Character c)
  {
    EnvironmentTileInfo tileBelow = GetAdjacentTile(location.position3D, location.floorLayer, TilemapDirection.Below);
    if (tileBelow != null && tileBelow.CharacterCanOccupyTile(c) && tileBelow.CharacterCanCrossTile(c))
    {
      return GetTileAtLocation(location).CharacterCanPassThroughFloorTile(c);
    }
    return false;
  }

  public bool CanAscendThroughTileAbove(TileLocation location, Character c)
  {
    EnvironmentTileInfo tileAbove = GetAdjacentTile(location.position3D, location.floorLayer, TilemapDirection.Above);
    if (tileAbove != null && tileAbove.CharacterCanOccupyTile(c) && tileAbove.CharacterCanCrossTile(c))
    {
      return GetTileAtLocation(tileAbove.tileLocation).CharacterCanPassThroughFloorTile(c);
    }
    return false;
  }

  // public bool CanClimbAdjacentTile(TileLocation tileLoc)
  // {
  //   if (GetAdjacentTile(tileLoc.position3D, tileLoc.floorLayer, TilemapDirection.Above).IsEmpty())
  //   {
  //     foreach (TilemapDirection d in new TilemapDirection[] {
  //       TilemapDirection.Up,
  //       TilemapDirection.Right,
  //       TilemapDirection.Down,
  //       TilemapDirection.Left,
  //     })
  //     {
  //       EnvironmentTileInfo et = GetAdjacentTile(tileLoc.position3D, tileLoc.floorLayer, d);
  //       if (
  //         et.IsClimbable()
  //       )
  //       {
  //         // must be able to stick to the same tile above this one as well, obvs
  //         EnvironmentTileInfo tileAboveClimbableTile = GetAdjacentTile(tileLoc.position3D, tileLoc.floorLayer + 1, d);
  //         if (tileAboveClimbableTile.CanBeStuckTo())
  //         {
  //           return true;
  //         }
  //       }
  //     }
  //   }
  //   return false;
  // }

  public bool AdjacentTileIsValid(TileLocation location, TilemapDirection direction)
  {
    if (direction == TilemapDirection.Above && (int)location.floorLayer == Constants.numberOfFloorLayers)
    {
      return false;
    }

    if (direction == TilemapDirection.Below && location.floorLayer == 0)
    {
      return false;
    }
    Vector2 possibleLocation = location.tilemapCoordinates + GetAdjacentTileOffset(location, direction);
    return
      possibleLocation.x < maxXAcrossAllFloors
      && possibleLocation.x > minXAcrossAllFloors
      && possibleLocation.y < maxYAcrossAllFloors
      && possibleLocation.y > minYAcrossAllFloors;

    // return TileIsValid(GetAdjacentTileLocation(location, direction));
    // switch (direction)
    // {
    //   case TilemapDirection.UpperLeft:
    //     return TileIsValid(new TileLocation(location.tilemapPosition + new Vector2Int(0, 1), location.floorLayer));
    //   case TilemapDirection.UpperRight:
    //     return TileIsValid(new TileLocation(location.tilemapPosition + new Vector2Int(1, 1), location.floorLayer));
    //   case TilemapDirection.LowerLeft:
    //     return TileIsValid(new TileLocation(location.tilemapPosition + new Vector2Int(0, 1), location.floorLayer));
    //   case TilemapDirection.LowerRight:
    //     return TileIsValid(new TileLocation(location.tilemapPosition + new Vector2Int(1, -1), location.floorLayer));
    //   case TilemapDirection.Right:
    //     return TileIsValid(new TileLocation(location.tilemapPosition + new Vector2Int(1, 0), location.floorLayer));
    //   case TilemapDirection.Left:
    //     return TileIsValid(new TileLocation(location.tilemapPosition + new Vector2Int(-1, 0), location.floorLayer));
    //   case TilemapDirection.Above:
    //     return TileIsValid(new TileLocation(location.tilemapPosition, location.floorLayer + 1));
    //   case TilemapDirection.Below:
    //     return TileIsValid(new TileLocation(location.tilemapPosition + new Vector2Int(0, 1), location.floorLayer - 1));
    //   case TilemapDirection.None:
    //   default:
    //     return TileIsValid(location);
    // }
  }

  // TODO: who is using this and why, it will probably explode?
  public bool TileIsValid(TileLocation loc)
  {
    if (!worldGrid.ContainsKey(loc.floorLayer))
    {
      return false;
    }
    if (!worldGrid[loc.floorLayer].ContainsKey(CoordsToKey(loc.tilemapCoordinates)))
    {
      return false;
    }
    return true;
  }

  public bool AdjacentTileIsValidAndEmpty(TileLocation location, TilemapDirection dir)
  {
    return AdjacentTileIsValid(location, dir) && GetAdjacentTile(location, dir).IsEmpty();
  }

  public bool TileIsValidAndEmpty(TileLocation location)
  {
    return TileIsValid(location) && GetTileAtLocation(location).IsEmpty();
  }

  public EnvironmentTileInfo GetAdjacentTile(TileLocation location, TilemapDirection dir)
  {
    // if (!AdjacentTileIsValid(location, dir)) { return null; }
    return GetAdjacentTile(location.position3D, location.floorLayer, dir);
  }

  public TileLocation GetAdjacentTileLocation(TileLocation location, TilemapDirection direction)
  {
    return GetAdjacentTileLocation(location.position3D, location.floorLayer, direction);
  }
  public TileLocation GetAdjacentTileLocation(Vector3 loc, FloorLayer floor, TilemapDirection direction)
  {
    Vector2Int v2Loc = new Vector2Int(Mathf.FloorToInt(loc.x), Mathf.FloorToInt(loc.y));
    int rightOffset = (int)loc.y & 1;
    int leftOffset = -((int)(loc.y + 1) & 1);
    switch (direction)
    {
      case TilemapDirection.UpperLeft:
        return new TileLocation(v2Loc + new Vector2Int(leftOffset, 1), floor);
      case TilemapDirection.UpperRight:
        return new TileLocation(v2Loc + new Vector2Int(rightOffset, 1), floor);
      case TilemapDirection.LowerLeft:
        return new TileLocation(v2Loc + new Vector2Int(leftOffset, -1), floor);
      case TilemapDirection.LowerRight:
        return new TileLocation(v2Loc + new Vector2Int(rightOffset, -1), floor);
      case TilemapDirection.Right:
        return new TileLocation(v2Loc + new Vector2Int(1, 0), floor);
      case TilemapDirection.Left:
        return new TileLocation(v2Loc + new Vector2Int(-1, 0), floor);
      case TilemapDirection.Above:
        return new TileLocation(v2Loc, floor + 1);
      case TilemapDirection.Below:
        return new TileLocation(v2Loc, floor - 1);
      case TilemapDirection.None:
      default:
        return new TileLocation(v2Loc, floor);
    }
  }

  public Vector2Int GetAdjacentTileCoords(TileLocation loc, TilemapDirection direction)
  {
    Vector2Int v2Loc = new Vector2Int(Mathf.FloorToInt(loc.x), Mathf.FloorToInt(loc.y));
    int rightOffset = (int)loc.y & 1;
    int leftOffset = -((int)(loc.y + 1) & 1);
    switch (direction)
    {
      case TilemapDirection.UpperLeft:
        return v2Loc + new Vector2Int(leftOffset, 1);
      case TilemapDirection.UpperRight:
        return v2Loc + new Vector2Int(rightOffset, 1);
      case TilemapDirection.LowerLeft:
        return v2Loc + new Vector2Int(leftOffset, -1);
      case TilemapDirection.LowerRight:
        return v2Loc + new Vector2Int(rightOffset, -1);
      case TilemapDirection.Right:
        return v2Loc + new Vector2Int(1, 0);
      case TilemapDirection.Left:
        return v2Loc + new Vector2Int(-1, 0);
      default:
        return v2Loc;
    }
  }

  public Vector2Int GetAdjacentTileOffset(TileLocation loc, TilemapDirection direction)
  {
    Vector2Int v2Loc = new Vector2Int(Mathf.FloorToInt(loc.x), Mathf.FloorToInt(loc.y));
    int rightOffset = (int)loc.y & 1;
    int leftOffset = -((int)(loc.y + 1) & 1);
    switch (direction)
    {
      case TilemapDirection.UpperLeft:
        return new Vector2Int(leftOffset, 1);
      case TilemapDirection.UpperRight:
        return new Vector2Int(rightOffset, 1);
      case TilemapDirection.LowerLeft:
        return new Vector2Int(leftOffset, -1);
      case TilemapDirection.LowerRight:
        return new Vector2Int(rightOffset, -1);
      case TilemapDirection.Right:
        return new Vector2Int(1, 0);
      case TilemapDirection.Left:
        return new Vector2Int(-1, 0);
      case TilemapDirection.Above:
      case TilemapDirection.Below:
      case TilemapDirection.None:
      default:
        return Vector2Int.zero;
    }
  }
  public EnvironmentTileInfo GetAdjacentTile(Vector3 loc, FloorLayer floor, TilemapDirection direction)
  {
    return GetTileAtLocation(GetAdjacentTileLocation(loc, floor, direction));
  }

  public void ReplaceAdjacentTile(TileLocation loc, EnvironmentTile replacementTile, TilemapDirection direction)
  {
    TileLocation modifiedLoc = new TileLocation(loc.tilemapCoordinates, loc.floorLayer);
    switch (direction)
    {
      case TilemapDirection.Above:
        modifiedLoc.floorLayer += 1;
        break;
      case TilemapDirection.Below:
        modifiedLoc.floorLayer -= 1;
        break;
      case TilemapDirection.None:
      default:
        break;
    }
    ReplaceTileAtLocation(modifiedLoc, replacementTile);
  }

  // Destroys a tile on the object layer.
  // TODO: generalize this probably? shrtug
  public void DestroyObjectTileAtLocation(TileLocation loc)
  {
    LayerFloor layerFloor = layerFloors[loc.floorLayer];
    if (layerFloor == null || layerFloor.groundTilemap == null || layerFloor.objectTilemap == null)
    {
      return;
    }
    Tilemap levelTilemap = layerFloor.objectTilemap;
    levelTilemap.SetTile(new Vector3Int(loc.tilemapCoordinates.x, loc.tilemapCoordinates.y, 0), null);
  }


  // TODO: Need to handle updating light when we replace a tile!
  public EnvironmentTileInfo ReplaceTileAtLocation(TileLocation location, EnvironmentTile replacementTile)
  {
    LayerFloor layerFloor = layerFloors[location.floorLayer];
    if (layerFloor == null || layerFloor.groundTilemap == null || layerFloor.objectTilemap == null)
    {
      Debug.LogWarning("missing layerFloor info for " + location.floorLayer.ToString());
      return null;
    }
    Tilemap levelTilemap = replacementTile != null && replacementTile.floorTilemapType == FloorTilemapType.Ground ? layerFloor.groundTilemap : layerFloor.objectTilemap;
    levelTilemap.SetTile(new Vector3Int(location.tilemapCoordinates.x, location.tilemapCoordinates.y, 0), replacementTile);
    return ConstructAndSetEnvironmentTileInfo(location, layerFloor.groundTilemap, layerFloor.objectTilemap, layerFloor.visibilityTilemap, layerFloor.waterTilemap, layerFloor.infoTilemap);
  }

  public void MarkTileToDestroyOnPlayerRespawn(EnvironmentTileInfo tile, EnvironmentTile replacementTile)
  {
    EnvironmentTileInfo newTileInfo = ReplaceTileAtLocation(tile.tileLocation, replacementTile);
    tilesToDestroyOnPlayerRespawn.Add(newTileInfo);
  }

  public void MarkTileToRestoreOnPlayerRespawn(EnvironmentTileInfo tile)
  {
    EnvironmentTileInfo oldTileInfo = tile;
    tilesToRestoreOnPlayerRespawn.Add(oldTileInfo);
  }

  public void DestroyTilesOnPlayerRespawn()
  {
    foreach (EnvironmentTileInfo tile in tilesToDestroyOnPlayerRespawn)
    {
      DestroyObjectTileAtLocation(tile.tileLocation);
    }
    tilesToDestroyOnPlayerRespawn.Clear();
  }

  public void RestoreTilesOnPlayerRespawn()
  {
    foreach (EnvironmentTileInfo tileInfo in tilesToRestoreOnPlayerRespawn)
    {
      ReplaceTileAtLocation(tileInfo.tileLocation, tileInfo.objectTileType);
    }
  }

  public void DEBUGHighlightTile(TileLocation tilePos, Color? color = null)
  {
    GameObject tileHighlight = Instantiate(highlightTilePrefab, new Vector3(tilePos.cellCenterPosition.x, tilePos.cellCenterPosition.y, layerFloors[tilePos.floorLayer].transform.position.z), Quaternion.identity, layerFloors[tilePos.floorLayer].transform);
    if (color != null)
    {
      tileHighlight.GetComponent<SpriteRenderer>().color = (Color)color;
    }
    // Debug.Log("highlighting tile at " + tileHighlight.transform.position);
    WorldObject.ChangeLayersRecursively(tileHighlight.transform, tilePos.floorLayer);
    StartCoroutine(DEBUGHighlightTileCleanup(tileHighlight));
  }

  public IEnumerator DEBUGHighlightTileCleanup(GameObject th)
  {
    yield return null;
    Destroy(th);
  }
  public static int GetZOffsetForGameObjectLayer(int floorLayer)
  {
    return (LayerMask.NameToLayer("B6") + Constants.numberOfFloorLayers) - floorLayer;
    // floor layers 9-20 (bottom to top)
    // we want them from 0-12, top to bottom
    // 20 = 0, 19 = 1, 18 = 2, 17 = 3
    // fl - (firstFloorLayerIndex + number of floors)?
    // int firstFloorLayerIndex = LayerMask.NameToLayer("B6"); // like... 15
    // return firstFloorLayerIndex - floorLayer;
    // int numberOfFloorLayers = Constants.numberOfFloorLayers; // 12?
  }

  public static TilemapDirection GetOppositeTilemapDirection(TilemapDirection d)
  {
    switch (d)
    {
      // case TilemapDirection.Up:
      //   return TilemapDirection.Down;
      // case TilemapDirection.Down:
      //   return TilemapDirection.Up;
      // case TilemapDirection.Left:
      //   return TilemapDirection.Right;
      // case TilemapDirection.Right:
      //   return TilemapDirection.Left;
      // case TilemapDirection.Above:
      //   return TilemapDirection.Below;
      // case TilemapDirection.Below:
      //   return TilemapDirection.Above;
      default:
        return d;
    }
  }

  // Returns floating-point location on tilemap, with axis units
  // equal to cell size.
  public Vector2 GetPositionOnTilemap(Vector3 worldPoint)
  {
    // transform world point to grid point
    // scale x and y by grid cell size
    Vector2 transformedPoint = (Vector2)levelGrid.transform.InverseTransformPoint(worldPoint);
    return new Vector2(transformedPoint.x / levelGrid.cellSize.x, transformedPoint.y / (levelGrid.cellSize.y * .75f)); // the 1.5 is for how hex cells overlap along y boundaries
  }

  public Color GetColorOfVisibilityTileAtLocation(TileLocation loc)
  {
    return layerFloors[loc.floorLayer].visibilityTilemap.GetColor(loc.tilemapCoordinatesVector3);
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
        layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.SetColor(tile.tileLocation.tilemapCoordinatesVector3, tile.illuminationInfo.visibleColor);
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
              && !totalTilesToIlluminate.Contains(GetAdjacentTile(tile.tileLocation, dir)))
            {
              nextTilesToIlluminate.Add(GetAdjacentTile(tile.tileLocation, dir));
            }
          }
        }
      }
      currentTilesToIlluminate = new HashSet<EnvironmentTileInfo>(nextTilesToIlluminate);
      nextTilesToIlluminate.Clear();
      currentDistance++;
    }
  }

  Coroutine _recalculateVisiblityCoroutine;

  public void PlayerChangedTile(TileLocation newPlayerTileLocation, int sightRange, DarkVisionInfo[] darkVisionInfos)
  {
    List<MusicStem> oldStems = new List<MusicStem>();
    if (currentPlayerLocation != null && GetTileAtLocation(currentPlayerLocation).infoTileType != null)
    {
      oldStems = GetTileAtLocation(currentPlayerLocation).infoTileType.musicStems;
    }
    List<MusicStem> newStems = new List<MusicStem>();
    if (GetTileAtLocation(newPlayerTileLocation).infoTileType != null)
    {
      newStems = GetTileAtLocation(newPlayerTileLocation).infoTileType.musicStems;
    }
    currentPlayerLocation = newPlayerTileLocation;
    foreach (MusicStem oldStem in oldStems)
    {
      if (!newStems.Contains(oldStem))
      {
        AkSoundEngine.PostEvent(oldStem.ToString() + "_FadeOut", gameObject);
      }
    }
    foreach (MusicStem newStem in newStems)
    {
      if (!oldStems.Contains(newStem))
      {
        AkSoundEngine.PostEvent(newStem.ToString() + "_FadeIn", gameObject);
      }
    }
  }

  void RecalculateVisibility(DarkVisionInfo[] darkVisionInfos)
  {
    // eh whatever do it later
  }
  List<TilemapDirection> nonEmptyTileDirections = new List<TilemapDirection>(){
    TilemapDirection.UpperLeft,
    TilemapDirection.Left,
    TilemapDirection.LowerLeft,
    TilemapDirection.UpperRight,
    TilemapDirection.Right,
    TilemapDirection.LowerRight,
  };

  List<TilemapDirection> emptyTileDirections = new List<TilemapDirection>(){
    TilemapDirection.UpperLeft,
    TilemapDirection.Left,
    TilemapDirection.LowerLeft,
    TilemapDirection.UpperRight,
    TilemapDirection.Right,
    TilemapDirection.LowerRight,
    TilemapDirection.Below,
  };

  void RecalculateVisibility(TileLocation newPlayerTileLocation, int sightRange, DarkVisionInfo[] darkVisionInfos)
  {
    System.Diagnostics.Stopwatch timeSpentThisFrame = new System.Diagnostics.Stopwatch();
    System.Diagnostics.Stopwatch timeSpentThisLoop = new System.Diagnostics.Stopwatch();
    System.Diagnostics.Stopwatch timeSpentRecalculatingVisibility = new System.Diagnostics.Stopwatch();
    timeSpentThisFrame.Start();
    timeSpentRecalculatingVisibility.Start();
    HashSet<EnvironmentTileInfo> totalVisibleTiles = new HashSet<EnvironmentTileInfo>();
    HashSet<EnvironmentTileInfo> nextVisibleTiles = new HashSet<EnvironmentTileInfo>();
    HashSet<int>[] consideredCoords = new HashSet<int>[] { new HashSet<int>(), new HashSet<int>(), new HashSet<int>() };
    List<List<EnvironmentTileInfo>> newTilesToMakeVisible = new List<List<EnvironmentTileInfo>>();
    tilesToMakeVisible.Clear();
    recentlyVisibleTiles.UnionWith(visibleTiles); // add all tiles visible BEFORE recalculating
                                                  // foreach (EnvironmentTileInfo tile in recentlyVisibleTiles)// if visibility seems to flicker we can move this down but it shouldn't
                                                  // {
                                                  //   tile.visibilityDistance += 1;
                                                  // }
    int sightRangeForFloor = sightRange; // who fucking knows!!
    for (int i = 0; i <= 0; i++)
    {
      timeSpentThisLoop.Restart();
      int currentDistance = 0;
      if (!Enum.IsDefined(typeof(FloorLayer), newPlayerTileLocation.floorLayer - i)) { continue; }
      EnvironmentTileInfo initialTile = GetTileAtLocation(new TileLocation(newPlayerTileLocation.x, newPlayerTileLocation.y, newPlayerTileLocation.floorLayer - i));
      HashSet<EnvironmentTileInfo> currentVisibleTiles = new HashSet<EnvironmentTileInfo>() { initialTile };
      int tileFloorOffset = 0;
      while (currentVisibleTiles.Count > 0)
      {
        List<EnvironmentTileInfo> tempTilesToMakeVisible = new List<EnvironmentTileInfo>();
        foreach (EnvironmentTileInfo tile in currentVisibleTiles)
        {
          tileFloorOffset = newPlayerTileLocation.floorLayer - tile.tileLocation.floorLayer;
          sightRangeForFloor = sightRange + Mathf.Abs(4 * tileFloorOffset); // who fucking knows!!  
          consideredCoords[tileFloorOffset].Add(CoordsToKey(tile.tileLocation.tilemapCoordinates));
          tile.effectiveVisibilityDistance = currentDistance;
          if (currentDistance <=
            DarkVisionAttributeData.GetVisibilityMultiplierForTile(darkVisionInfos, tile)
            * sightRangeForFloor)
          {
            totalVisibleTiles.Add(tile);
            if (GetColorOfVisibilityTileAtLocation(tile.tileLocation).a > tile.illuminationInfo.visibleColor.a)
            {
              tempTilesToMakeVisible.Add(tile);
            }
          }
          if (!tile.HasSolidObject() && currentDistance <= sightRangeForFloor) // can see over obstacles below us, I guess?
          {
            foreach (TilemapDirection dir in (tile.IsEmpty() && tileFloorOffset < 2 ? emptyTileDirections : nonEmptyTileDirections))
            {
              if (
                AdjacentTileIsValid(tile.tileLocation, dir)
                && !consideredCoords[tileFloorOffset + (dir == TilemapDirection.Below ? 1 : 0)].Contains(CoordsToKey(GetAdjacentTileCoords(tile.tileLocation, dir))))
              {
                nextVisibleTiles.Add(GetAdjacentTile(tile.tileLocation, dir));
              }
            }
          }
        }
        if (tempTilesToMakeVisible.Count > 0)
        {
          if (tilesToMakeVisible.Count > currentDistance)
          {
            newTilesToMakeVisible[currentDistance].AddRange(tempTilesToMakeVisible);
          }
          else
          {
            newTilesToMakeVisible.Add(tempTilesToMakeVisible);
          }
        }
        if (currentDistance != sightRangeForFloor)
        {
          currentVisibleTiles = new HashSet<EnvironmentTileInfo>(nextVisibleTiles);
          nextVisibleTiles.Clear();
        }
        currentDistance++;
      }
      // Debug.Log("Time spent calculating visibility for floor " + i + ": " + timeSpentThisLoop.ElapsedMilliseconds);
    }

    // This block finalizes everything. Visibility won't update until we have a chance to complete this loop.
    recentlyVisibleTiles.ExceptWith(totalVisibleTiles); // remove all tiles visible AFTER recalculating
    tilesToMakeObscured.Add(new List<EnvironmentTileInfo>(recentlyVisibleTiles));
    visibleTiles = new HashSet<EnvironmentTileInfo>(totalVisibleTiles); // replace contents of visibleTiles with totalVisibleTiles
    tilesToMakeVisible = newTilesToMakeVisible;
    // if (currentPlayerLocation == newPlayerTileLocation)
    // { // player position hasn't changed since we started
    _recalculateVisiblityCoroutine = null;
    timeSpentRecalculatingVisibility.Stop();
    // Debug.Log("time spent recalculating visibility: " + timeSpentRecalculatingVisibility.ElapsedMilliseconds);
    // }
    // else
    // { // player is on a new tile and we should recalculate
    // _recalculateVisiblityCoroutine = StartCoroutine(RecalculateVisibility(currentPlayerLocation));
    // }

  }

#if UNITY_EDITOR
  [MenuItem("CustomTools/ToggleInfoTilemaps")]
  public static void ToggleInfoTilemaps()
  {

    foreach (LayerFloor layerFloor in GridManager.Instance.layerFloors.Values)
    {
      if (layerFloor.infoTilemap != null && layerFloor.infoTilemap.GetComponent<TilemapRenderer>() != null)
      {
        layerFloor.infoTilemap.gameObject.SetActive(!layerFloor.infoTilemap.gameObject.activeSelf);
      }
    }
  }
#endif
}
