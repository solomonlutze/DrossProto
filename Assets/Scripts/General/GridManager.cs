using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using ScriptableObjectArchitecture;

// this sucks
public static class GridConstants
{
  // below are world-scale distances between center of a hex and one in the adjacent column (x) or row (y)
  public const float X_SPACING = 0.87625f; // actual width of hexes
  public const float Y_SPACING = .75f; // y-distance between 2 hexes in adjacent rows
}
public enum TilemapDirection { None, UpperLeft, UpperRight, Left, Right, LowerLeft, LowerRight, Above, Below }

public class TilePlacedObjects
{
  public WallObject wallObject;
  public TileParticleSystem tileParticleSystem;
}
// Supports TRANSIENT grid/level info
// See WorldGridData for persistent/serialized grid info
public class GridManager : Singleton<GridManager>
{

  public bool DEBUG_IgnoreLighting;
  public int initialWallObjectPoolSize;
  public HashSet<Vector2Int> loadedChunks;
  public HashSet<Vector2Int> desiredChunks;
  public Dictionary<int, TilePlacedObjects> placedGameObjects;
  public Transform wallObjectContainer;
  Coroutine chunkLoadCoroutine;
  public WorldGridData worldGridData;
  public Grid levelGrid;
  public Material semiTransparentMaterial;
  public Material fullyOpaqueMaterial;
  public LayerToLayerFloorDictionary layerFloors;
  public FloorLayerToTileInfosDictionary worldGrid;

  private List<EnvironmentTileInfo> tilesToDestroyOnPlayerRespawn;

  private List<EnvironmentTileInfo> tilesToRestoreOnPlayerRespawn;
  public GameObject highlightTilePrefab;

  public StringVariable areaName;
  public GameEvent areaNameChanged; int interestObjectsCount = 0;
  public TileLocation currentPlayerLocation;

  public void Awake()
  {
    worldGrid = worldGridData.worldGrid;
    tilesToDestroyOnPlayerRespawn = new List<EnvironmentTileInfo>();
    tilesToRestoreOnPlayerRespawn = new List<EnvironmentTileInfo>();
    placedGameObjects = new Dictionary<int, TilePlacedObjects>();
    loadedChunks = new HashSet<Vector2Int>();
    worldGridData.ClearExistingPlacedObjects();
    StartCoroutine(ObjectPoolManager.Instance.GetWallObjectPool().Populate(initialWallObjectPoolSize));
    StartLoadAndUnloadChunks(new TileLocation(Vector3.zero));
    worldGrid = new FloorLayerToTileInfosDictionary();
    return;
  }

  public int CoordsToKey(Vector2Int coordinates)
  {
    return worldGridData.CoordsTo2DKey(coordinates);
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
    EnvironmentTileInfo i = new EnvironmentTileInfo();
    InfoTile it = layerFloors[loc.floorLayer].infoTilemap.HasTile(loc.tilemapCoordinatesVector3)
      && layerFloors[loc.floorLayer].infoTilemap.GetTile(loc.tilemapCoordinatesVector3) as InfoTile != null ?
      (InfoTile)layerFloors[loc.floorLayer].infoTilemap.GetTile(loc.tilemapCoordinatesVector3) :
      null;
    i.Init(
      loc,
      (EnvironmentTile)layerFloors[loc.floorLayer].groundTilemap.GetTile(loc.tilemapCoordinatesVector3),
      (EnvironmentTile)layerFloors[loc.floorLayer].objectTilemap.GetTile(loc.tilemapCoordinatesVector3),
      (EnvironmentTile)layerFloors[loc.floorLayer].waterTilemap.GetTile(loc.tilemapCoordinatesVector3),
      it,
      worldGridData.GetEnvironmentTileData(loc));
    return i;
  }

  public WallObject GetWallObjectAtLocation(TileLocation loc)
  {
    TilePlacedObjects placedObjects = worldGridData.GetPlacedObjectsAtLocation(loc);
    return placedObjects == null ? null : placedObjects.wallObject;
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

  public bool ShouldHaveCollisionWith(EnvironmentTileInfo eti, Transform colliderTransform)
  {
    return ShouldHaveCollisionWith(eti, colliderTransform.position.z);
  }

  public bool ShouldHaveCollisionWith(EnvironmentTileInfo eti, float otherZ)
  {
    float offset = -.001f;
    return GroundHasCollisionWith(eti, otherZ, offset) || CeilingHasCollisionWith(eti, otherZ, offset);
  }
  public bool GroundHasCollisionWith(EnvironmentTileInfo eti, float otherZ, float offset = 0)
  {
    return otherZ <= eti.tileLocation.z // between bottom of tile area...
      && otherZ > (eti.tileLocation.z - eti.GroundHeight() - offset); //...and top of ground
  }

  public bool CeilingHasCollisionWith(EnvironmentTileInfo eti, float otherZ, float offset = 0)
  {
    return eti.objectTileType != null && // ceiling tile exists, and we're ...
      (otherZ <= (eti.tileLocation.z - eti.CeilingHeight() - offset)//...between bottom of ceiling...
      && otherZ >= (eti.tileLocation.z - 1)); // and top of tile area
  }

  public bool AdjacentTileIsValid(TileLocation location, TilemapDirection direction)
  {
    if (direction == TilemapDirection.Above && (int)location.floorLayer == DrossConstants.numberOfFloorLayers)
    {
      return false;
    }

    if (direction == TilemapDirection.Below && location.floorLayer == 0)
    {
      return false;
    }
    Vector2 possibleLocation = location.tilemapCoordinates + GetAdjacentTileOffset(location, direction);
    return
      possibleLocation.x < worldGridData.maxXAcrossAllFloors
      && possibleLocation.x > worldGridData.minXAcrossAllFloors
      && possibleLocation.y < worldGridData.maxYAcrossAllFloors
      && possibleLocation.y > worldGridData.minYAcrossAllFloors;
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
    return GetTileAtLocation(location);
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
    return (LayerMask.NameToLayer("B6") + DrossConstants.numberOfFloorLayers) - floorLayer;
  }

  public static FloorLayer GetFloorLayerFromZPosition(float zPos)
  {
    return (FloorLayer)(-(Mathf.CeilToInt(zPos) - 12));
  }

  public float GetFloorHeightForTileLocation(TileLocation loc)
  {
    return GetTileAtLocation(loc).GroundHeight();
  }
  public float GetFloorPositionForTileLocation(TileLocation loc)
  {
    return loc.z - GetTileAtLocation(loc).GroundHeight();
  }

  public float GetCeilingHeightForTileLocation(TileLocation loc)
  {
    return GetTileAtLocation(loc).CeilingHeight();
  }

  public static TilemapDirection GetOppositeTilemapDirection(TilemapDirection d)
  {
    switch (d)
    {
      case TilemapDirection.UpperRight:
        return TilemapDirection.LowerLeft;
      case TilemapDirection.Right:
        return TilemapDirection.Left;
      case TilemapDirection.LowerRight:
        return TilemapDirection.UpperLeft;
      case TilemapDirection.LowerLeft:
        return TilemapDirection.UpperRight;
      case TilemapDirection.Left:
        return TilemapDirection.Right;
      case TilemapDirection.UpperLeft:
        return TilemapDirection.LowerRight;
      case TilemapDirection.Above:
        return TilemapDirection.Below;
      case TilemapDirection.Below:
        return TilemapDirection.Above;
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

  public void PlayerChangedTile(TileLocation newPlayerTileLocation, int sightRange = 0, DarkVisionInfo[] darkVisionInfos = null)
  {
    InfoTile currentInfoTile = null;
    List<MusicStem> oldStems = new List<MusicStem>();
    if (currentPlayerLocation != null && GetTileAtLocation(currentPlayerLocation).infoTileType != null)
    {
      currentInfoTile = GetTileAtLocation(currentPlayerLocation).infoTileType;
      if (currentInfoTile != null)
      {
        oldStems = GetTileAtLocation(currentPlayerLocation).infoTileType.musicStems;
      }
    }
    InfoTile nextInfoTile = GetTileAtLocation(newPlayerTileLocation).infoTileType;
    List<MusicStem> newStems = new List<MusicStem>();
    if (nextInfoTile != null)
    {
      newStems = nextInfoTile.musicStems;
      if (areaName.Value != nextInfoTile.areaName)
      {
        areaName.Value = nextInfoTile.areaName;
        areaNameChanged.Raise();
      }
      if (currentInfoTile != nextInfoTile)
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
    if (currentPlayerLocation == null || currentPlayerLocation.chunkCoordinates != newPlayerTileLocation.chunkCoordinates)
    {
      PlayerChangedChunk(newPlayerTileLocation);
    }
    currentPlayerLocation = newPlayerTileLocation;
  }

  public void PlayerChangedChunk(TileLocation newPlayerTileLocation)
  {
    StartLoadAndUnloadChunks(newPlayerTileLocation);
  }

  public void StartLoadAndUnloadChunks(TileLocation centeredOnLocation)
  {
    HashSet<Vector2Int> chunksToLoad = new HashSet<Vector2Int>();
    if (loadedChunks == null || loadedChunks.Count == 0)
    {
      worldGridData.ClearExistingPlacedObjects();
      loadedChunks = new HashSet<Vector2Int>();
    }
    if (placedGameObjects == null)
    {
      Debug.LogWarning("making new placedGameObjects!!");
      placedGameObjects = new Dictionary<int, TilePlacedObjects>();
    }
    for (int x = -worldGridData.chunksToLoad.x; x <= worldGridData.chunksToLoad.x; x++)
    {
      for (int y = -worldGridData.chunksToLoad.y; y <= worldGridData.chunksToLoad.y; y++)
      {
        Vector2Int offset = new Vector2Int(x, y);
        chunksToLoad.Add(centeredOnLocation.chunkCoordinates + offset);
      }
    }
    // StartCoroutine(YieldForExistingLoadCoroutine(chunksToLoad));
    if (chunkLoadCoroutine != null)
    {
      StopCoroutine(chunkLoadCoroutine);
    }
    chunkLoadCoroutine = StartCoroutine(LoadAndUnloadChunksCoroutine(chunksToLoad));
  }
  public void LoadAndUnloadChunks(TileLocation centeredOnLocation)
  {
    List<Vector2Int> chunksToLoad = new List<Vector2Int>();
    if (loadedChunks == null)
    {
      loadedChunks = new HashSet<Vector2Int>();
    }
    for (int x = -worldGridData.chunksToLoad.x; x <= worldGridData.chunksToLoad.x; x++)
    {
      for (int y = -worldGridData.chunksToLoad.y; y <= worldGridData.chunksToLoad.y; y++)
      {
        Vector2Int offset = new Vector2Int(x, y);
        chunksToLoad.Add(centeredOnLocation.chunkCoordinates + offset);
      }
    }
    HashSet<Vector2Int> chunksToUnload = new HashSet<Vector2Int>();
    foreach (Vector2Int loadedChunk in loadedChunks)
    {
      if (!chunksToLoad.Contains(loadedChunk))
      {
        chunksToUnload.Add(loadedChunk);
        worldGridData.UnloadChunk(loadedChunk);
      }
    }
    foreach (Vector2Int chunk in chunksToUnload)
    {
      loadedChunks.Remove(chunk);
    }
  }

  bool waitingForLoad = false;
  // IEnumerator YieldForExistingLoadCoroutine(HashSet<Vector2Int> chunksToLoad)
  // {

  //   if (chunkLoadCoroutine != null)
  //   {
  //     waitingForLoad = true;
  //     // yield return chunkLoadCoroutine;
  //   }
  //   waitingForLoad = false;
  //   chunkLoadCoroutine = StartCoroutine(LoadAndUnloadChunksCoroutine(chunksToLoad));
  // }

  System.Diagnostics.Stopwatch timeSinceLastLoad;
  IEnumerator LoadAndUnloadChunksCoroutine(HashSet<Vector2Int> chunksToLoad)
  {
    timeSinceLastLoad ??= new System.Diagnostics.Stopwatch();
    // Debug.Log("CHUNKS TimeSinceLastLoad started: " + timeSinceLastLoad.ElapsedMilliseconds + "ms");
    timeSinceLastLoad.Restart();
    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
    watch.Start();
    // Debug.Log("CHUNKS Started loading!");
    yield return LoadChunksCoroutine(chunksToLoad);
    yield return UnloadChunksCoroutine(chunksToLoad);
    // Debug.Log("CHUNKS finished load/unload, took " + watch.ElapsedMilliseconds + "ms");
    // Debug.Log("CHUNKS Finished loading!");
    chunkLoadCoroutine = null;
  }

  public bool LoadingChunks()
  {
    return chunkLoadCoroutine != null;
  }

  IEnumerator LoadChunksCoroutine(HashSet<Vector2Int> desiredChunks)
  {
    // if (false) { yield return null; }
    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
    watch.Start();
    foreach (Vector2Int chunk in desiredChunks)
    {
      if (!loadedChunks.Contains(chunk))
      {
        worldGridData.LoadChunk(chunk);
        loadedChunks.Add(chunk);
        if (watch.ElapsedMilliseconds > 50 && !waitingForLoad)
        {
          yield return null;
          watch.Restart();
        }
      }
    }
  }

  IEnumerator UnloadChunksCoroutine(HashSet<Vector2Int> desiredChunks)
  {
    // if (false) { yield return null; }
    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
    HashSet<Vector2Int> chunksToUnload = new HashSet<Vector2Int>();
    watch.Start();
    foreach (Vector2Int loadedChunk in loadedChunks)
    {
      if (!desiredChunks.Contains(loadedChunk))
      {
        chunksToUnload.Add(loadedChunk);
        worldGridData.UnloadChunk(loadedChunk);
        if (watch.ElapsedMilliseconds > 50 && !waitingForLoad)
        {
          yield return null;
          watch.Restart();
        }
      }
    }
    foreach (Vector2Int chunk in chunksToUnload)
    {
      loadedChunks.Remove(chunk);
    }
  }

  public void UpdateWallObjectCollisionsForCharacter(Character c)
  {
    TileLocation location;
    TileLocation characterLocation = c.GetTileLocation();
    for (int x = -2; x <= 2; x++)
    {
      for (int y = -2; y <= 2; y++)
      {
        location = characterLocation.WithOffset(new Vector2Int(x, y));
        TilePlacedObjects placedObjects;
        placedGameObjects.TryGetValue(worldGridData.CoordsToKey(location), out placedObjects);
        if (placedObjects != null && placedObjects.wallObject != null)
        {
          placedObjects.wallObject.UpdateCollisions(c.physicsCollider);
        }
      }
    }
  }
  public void ClearLoadedChunksAndResetPool()
  {
    ObjectPoolManager.Instance.GetWallObjectPool().Clear();
    ClearLoadedChunks();
  }

  public void ClearLoadedChunks()
  {
    if (loadedChunks == null)
    {
      loadedChunks = new HashSet<Vector2Int>();
    }
    loadedChunks.Clear();
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
