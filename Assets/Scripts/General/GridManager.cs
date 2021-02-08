using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
      return GridManager.Instance.levelGrid.CellToWorld(new Vector3Int(
        tilemapCoordinates.x,
        tilemapCoordinates.y,
        (int)GridManager.GetZOffsetForFloor(WorldObject.GetGameObjectLayerFromFloorLayer(floorLayer)))
      );
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
      (int)GridManager.GetZOffsetForFloor(WorldObject.GetGameObjectLayerFromFloorLayer(fl)))
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
        (int)GridManager.GetZOffsetForFloor(WorldObject.GetGameObjectLayerFromFloorLayer(fl))),
      fl
    );
  }
  public TileLocation(Vector2Int pos, FloorLayer fl) // NOTE: This will return the center of each tile in all float coords!;
  {
    Initialize(GridManager.Instance.levelGrid.CellToWorld(new Vector3Int(
      pos.x,
      pos.y,
      (int)GridManager.GetZOffsetForFloor(WorldObject.GetGameObjectLayerFromFloorLayer(fl)))
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

  public Grid levelGrid;
  public Material semiTransparentMaterial;
  public Material fullyOpaqueMaterial;
  public LayerToLayerFloorDictionary layerFloors;
  public Dictionary<FloorLayer, Dictionary<Vector2Int, EnvironmentTileInfo>> worldGrid;

  private List<EnvironmentTileInfo> tilesToDestroyOnPlayerRespawn;

  private List<EnvironmentTileInfo> tilesToRestoreOnPlayerRespawn;
  public GameObject highlightTilePrefab;
  public HashSet<EnvironmentTileInfo> visibleTiles;
  public HashSet<EnvironmentTileInfo> recentlyVisibleTiles;
  public List<List<EnvironmentTileInfo>> tilesToMakeVisible;
  public List<List<EnvironmentTileInfo>> tilesToMakeObscured;

  public List<EnvironmentTileInfo> lightSources;
  public Color nonVisibleTileColor;
  int interestObjectsCount = 0;
  public void Awake()
  {
    worldGrid = new Dictionary<FloorLayer, Dictionary<Vector2Int, EnvironmentTileInfo>>();
    tilesToDestroyOnPlayerRespawn = new List<EnvironmentTileInfo>();
    tilesToRestoreOnPlayerRespawn = new List<EnvironmentTileInfo>();
    visibleTiles = new HashSet<EnvironmentTileInfo>();
    recentlyVisibleTiles = new HashSet<EnvironmentTileInfo>();
    tilesToMakeVisible = new List<List<EnvironmentTileInfo>>();
    tilesToMakeObscured = new List<List<EnvironmentTileInfo>>();
    lightSources = new List<EnvironmentTileInfo>();
    Dictionary<Vector2, EnvironmentTileInfo> floor = new Dictionary<Vector2, EnvironmentTileInfo>();
    Tilemap groundTilemap;
    Tilemap objectTilemap;
    Tilemap visibilityTilemap;
    int minXAcrossAllFloors = 5000;
    int maxXAcrossAllFloors = -5000;
    int minYAcrossAllFloors = 5000;
    int maxYAcrossAllFloors = -5000;
    foreach (LayerFloor lf in layerFloors.Values)
    {
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
    foreach (FloorLayer layer in Enum.GetValues(typeof(FloorLayer)))
    {
      floor.Clear();
      worldGrid[layer] = new Dictionary<Vector2Int, EnvironmentTileInfo>();
      if (!layerFloors.ContainsKey(layer))
      {
        continue;
      }
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
          ConstructAndSetEnvironmentTileInfo(loc, groundTilemap, objectTilemap, visibilityTilemap);
        }
      }
      // }
    }
    Debug.Log("lightSources length: " + lightSources.Count);
  }

  float opacity = 0;
  int sign = 1;
  public void Update()
  {
    // HashSet<EnvironmentTileInfo> tilesToRemove = new HashSet<EnvironmentTileInfo>();
    // foreach (EnvironmentTileInfo tile in recentlyVisibleTiles)
    // {
    //   Color c = layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.GetColor(tile.tileLocation.tilemapCoordinatesVector3);
    //   c.a += Time.deltaTime * 3;
    //   layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.SetColor(
    //     tile.tileLocation.tilemapCoordinatesVector3, c);
    //   if (c.a >= 1)
    //   {
    //     tilesToRemove.Add(tile);
    //   }
    // }
    // recentlyVisibleTiles.ExceptWith(tilesToRemove);
    if (tilesToMakeObscured.Count > 0)
    {
      for (int i = tilesToMakeObscured[0].Count - 1; i >= 0; i--)
      {
        EnvironmentTileInfo tile = tilesToMakeObscured[0][i];
        if (visibleTiles.Contains(tile))
        {
          tilesToMakeObscured[0].Remove(tile);
          continue;
        }
        Color c = layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.GetColor(tile.tileLocation.tilemapCoordinatesVector3);
        c.a += Time.deltaTime * 3;
        if (c.a >= 1)
        {
          c.a = 1;
          tilesToMakeObscured[0].Remove(tile);
        }
        layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.SetColor(
          tile.tileLocation.tilemapCoordinatesVector3, c);
      }
      if (tilesToMakeObscured[0].Count == 0)
      {
        tilesToMakeObscured.RemoveAt(0);
      }
    }
    if (tilesToMakeVisible.Count > 0)
    {
      for (int i = tilesToMakeVisible[0].Count - 1; i >= 0; i--)
      {
        EnvironmentTileInfo tile = tilesToMakeVisible[0][i];
        Color c = layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.GetColor(tile.tileLocation.tilemapCoordinatesVector3);
        c.a -= Time.deltaTime * 3;
        if (c.a <= 0)
        {
          c.a = 0;
          tilesToMakeVisible[0].Remove(tile);
        }
        layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.SetColor(
          tile.tileLocation.tilemapCoordinatesVector3, c);
      }
      if (tilesToMakeVisible[0].Count == 0)
      {
        tilesToMakeVisible.RemoveAt(0);
      }
    }

    // if (GameMaster.Instance.GetPlayerController() != null)
    // {
    //   TileLocation charLoc = GameMaster.Instance.GetPlayerController().GetTileLocation();
    //   LayerFloor layerFloor = layerFloors[charLoc.floorLayer];
    //   int z = (int)GridManager.GetZOffsetForFloor(WorldObject.GetGameObjectLayerFromFloorLayer(charLoc.floorLayer));

    //   for (int x = charLoc.tilemapCoordinates.x - 40; x <= charLoc.tilemapCoordinates.x + 40; x++)
    //   {
    //     for (int y = charLoc.tilemapCoordinates.y - 40; y <= charLoc.tilemapCoordinates.y + 40; y++)
    //     {
    //       // Debug.Log("x: " + x);
    //       // Debug.Log("y: " + y);
    //       // if (layerFloor.groundTilemap.GetTile(new Vector3Int(x, y, 0)).name != "waffle") { return; }
    //       Color c = layerFloor.visibilityTilemap.GetColor(new Vector3Int(x, y, 0));
    //       // c.a += opacity;
    //       c.a = Mathf.PingPong(Time.time, 1);
    //       layerFloor.visibilityTilemap.SetColor(new Vector3Int(x, y, 0), c);
    //       // opacity += .0001f * sign;
    //       // if (opacity > 1 || opacity < 0)
    //       // {
    //       //   sign *= -1;
    //       // }
    //     }
    //     // opacity = Mathf.PingPong(Time.time, 1f);
    //     // Debug.Log("opacity: " + opacity);
    //   }
    // }

    // get floor layer for player
    // determine center tile (player location)
    // player location within chunk at chunksize / 2
    // 
    // PlayerController player = GameMaster.Instance.GetPlayerController();
    // if (player != null)
    // {
    //   Tile[] tileArray = new Tile[chunkSize * chunkSize];
    //   TileLocation playerLoc = player.GetTileLocation();
    //   // BoundsInt bounds = new BoundsInt(playerLoc.x - chunkSize / 2, playerLoc.y - chunkSize / 2, 1, playerLoc.x + chunkSize / 2, playerLoc.y + chunkSize / 2, 1);
    //   BoundsInt bounds = new BoundsInt(-12, -12, 0, 24, 24, 1);
    //   TileBase[] tileBaseBlock = layerFloors[player.currentFloor].visibilityTilemap.GetTilesBlock(bounds);
    //   Tile[] tileBlock = new Tile[tileBaseBlock.Length];
    //   Debug.Log("tileblock length: " + tileBlock);
    //   HashSet<EnvironmentTileInfo> tilesToRemove = new HashSet<EnvironmentTileInfo>();
    //   // Debug.Log("bounds")
    //   for (int i = 0; i < tileBaseBlock.Length; i++)
    //   {
    //     Tile tile = (Tile)tileBaseBlock[i];
    //     Debug.Log("as tile: " + tile);
    //     if (tile != null)
    //     {
    //       Debug.Log("tile.color before " + tile.color);
    //       tile.flags = TileFlags.None;
    //       Color c = tile.color;
    //       c.a = Mathf.PingPong(Time.time, 1);
    //       tile.color = c;
    //       Debug.Log("tile.color after " + tile.color);
    //     }
    //     else
    //     {
    //       Debug.Log("couldn't cast tile at " + i);
    //     }
    //     tileBlock[i] = fillTile;
    //     // tile;
    //   }
    //   layerFloors[player.currentFloor].visibilityTilemap.SetTilesBlock(bounds, tileBlock);
    // foreach (EnvironmentTileInfo tile in recentlyVisibleTiles)
    // {
    //   Color c = layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.GetColor(tile.tileLocation.tilemapCoordinatesVector3);
    //   c.a += Time.deltaTime * 3;
    //   layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.SetColor(
    //     tile.tileLocation.tilemapCoordinatesVector3, c);
    //   if (c.a >= 1)
    //   {
    //     tilesToRemove.Add(tile);
    //   }
    // }
    // recentlyVisibleTiles.ExceptWith(tilesToRemove);
    // foreach (EnvironmentTileInfo tile in visibleTiles)
    // {
    //   Color c = layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.GetColor(tile.tileLocation.tilemapCoordinatesVector3);
    //   if (c.a <= 0) { continue; }
    //   c.a -= Time.deltaTime * 3;
    //   if (c.a < 0) { c.a = 0; }
    //   layerFloors[tile.tileLocation.floorLayer].visibilityTilemap.SetColor(
    //     tile.tileLocation.tilemapCoordinatesVector3, c);
    // }
    // }
  }

  public EnvironmentTileInfo ConstructAndSetEnvironmentTileInfo(TileLocation loc, Tilemap groundTilemap, Tilemap objectTilemap, Tilemap visibilityTilemap)
  {
    Vector3Int v3pos = new Vector3Int(loc.tilemapCoordinates.x, loc.tilemapCoordinates.y, 0);
    EnvironmentTileInfo info = new EnvironmentTileInfo();
    EnvironmentTile objectTile = objectTilemap.GetTile(v3pos) as EnvironmentTile;
    if (objectTile != null && objectTile.lightRangeInfo != null && objectTile.lightRangeInfo.Length > 0)
    {
      lightSources.Add(info);
    }
    visibilityTilemap.SetColor(v3pos, nonVisibleTileColor);
    EnvironmentTile groundTile = groundTilemap.GetTile(v3pos) as EnvironmentTile;
    info.Init(
      loc,
      groundTile,
      objectTile
    );
    // if (objectTile != null && objectTile.colliderType == Tile.ColliderType.Grid)
    // {
    //   Debug.Log("placing " + objectTile + " tile at tilemap position " + loc.tilemapCoordinates + ", world position " + loc.worldPosition);
    // }
    worldGrid[loc.floorLayer][loc.tilemapCoordinates] = info;
    // if (interestObjectsCount < 500)
    // {
    // AddInterestObjects(GetAdjacentTileLocation(loc, TilemapDirection.Left));
    // }
    return info;
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
    if (TileIsValid(GetAdjacentTileLocation(loc, direction)))
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

  public EnvironmentTileInfo GetTileAtLocation(TileLocation loc)
  {
    if (!TileIsValid(loc))
    {
      Debug.LogError("WARNING: Tried to find invalid tile at layer " + loc.floorLayer + ", coordinates " + loc.tilemapCoordinates);
      return null;
    }
    return worldGrid[loc.floorLayer][loc.tilemapCoordinates];
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
    return TileIsValid(GetAdjacentTileLocation(location, direction));
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

  public bool TileIsValid(TileLocation loc)
  {
    if (!worldGrid.ContainsKey(loc.floorLayer))
    {
      return false;
    }
    if (!worldGrid[loc.floorLayer].ContainsKey(loc.tilemapCoordinates))
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
    return ConstructAndSetEnvironmentTileInfo(location, layerFloor.groundTilemap, layerFloor.objectTilemap, layerFloor.visibilityTilemap);
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
  public static float GetZOffsetForFloor(int floorLayer)
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
  public void PlayerChangedTile(TileLocation newPlayerTileLocation)
  {
    int playerSightRange = 6;
    int currentDistance = 0;
    HashSet<EnvironmentTileInfo> totalVisibleTiles = new HashSet<EnvironmentTileInfo>();
    HashSet<EnvironmentTileInfo> nextVisibleTiles = new HashSet<EnvironmentTileInfo>();
    HashSet<EnvironmentTileInfo> currentVisibleTiles = new HashSet<EnvironmentTileInfo>() { GetTileAtLocation(newPlayerTileLocation) };
    // Debug.Log("recentlyVisibileTiles starts with " + recentlyVisibleTiles.Count);
    // Debug.Log("visibleTiles starts with " + visibleTiles.Count);
    tilesToMakeVisible.Clear();
    recentlyVisibleTiles.UnionWith(visibleTiles); // add all tiles visible BEFORE recalculating
    // Debug.Log("recentlyVisibileTiles after union with visibleTiles has " + recentlyVisibleTiles.Count);
    foreach (EnvironmentTileInfo tile in recentlyVisibleTiles)// if visibility seems to flicker we can move this down but it shouldn't
    {
      tile.visibilityDistance += 1;
    }
    while (currentDistance <= playerSightRange)
    {
      List<EnvironmentTileInfo> tempTilesToMakeVisible = new List<EnvironmentTileInfo>();
      foreach (EnvironmentTileInfo tile in currentVisibleTiles)
      {
        tile.visibilityDistance = currentDistance;
        totalVisibleTiles.Add(tile);
        if (GetColorOfVisibilityTileAtLocation(tile.tileLocation).a > 0)
        {
          tempTilesToMakeVisible.Add(tile);
        }
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
            currentDistance != playerSightRange
            && !totalVisibleTiles.Contains(GetAdjacentTile(tile.tileLocation, dir)))
          {
            nextVisibleTiles.Add(GetAdjacentTile(tile.tileLocation, dir));
          }
        }
      }
      if (tempTilesToMakeVisible.Count > 0)
      {
        tilesToMakeVisible.Add(tempTilesToMakeVisible);
      }
      if (currentDistance != playerSightRange)
      {
        currentVisibleTiles = new HashSet<EnvironmentTileInfo>(nextVisibleTiles);
        nextVisibleTiles.Clear();
      }
      currentDistance++;
    }
    recentlyVisibleTiles.ExceptWith(totalVisibleTiles); // remove all tiles visible AFTER recalculating
    tilesToMakeObscured.Add(new List<EnvironmentTileInfo>(recentlyVisibleTiles));
    // Debug.Log("recentlyVisibileTiles after exceptWith has " + recentlyVisibleTiles.Count);
    visibleTiles = new HashSet<EnvironmentTileInfo>(totalVisibleTiles); // replace contents of visibleTiles with totalVisibleTiles
    // Debug.Log("visibleTiles ends with " + visibleTiles.Count);
  }
}
