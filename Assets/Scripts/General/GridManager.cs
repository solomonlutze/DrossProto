using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TilemapDirection { None, UpperLeft, UpperRight, Left, Right, LowerLeft, LowerRight, Above, Below }
// public enum TilemapCorner { None, UpperLeft, UpperRight, LowerLeft, LowerRight }

public class TileLocation
{
  public Vector2Int tilemapPosition;
  public Vector3 worldPosition;
  public FloorLayer floorLayer;

  public TileLocation(int cellLocationX, int cellLocationY, FloorLayer fl)
  {
    Initialize(new Vector2Int(cellLocationX, cellLocationY), fl);

  }
  public TileLocation(Vector2Int pos, FloorLayer fl)
  {
    Initialize(pos, fl);
  }

  public TileLocation(Vector3 worldPos, FloorLayer fl)
  {
    Initialize((Vector2Int)GridManager.Instance.levelGrid.WorldToCell(worldPos), fl);
    // Vector2Int cellPos = GridManager.Instance.levelGrid.WorldToCell(worldPos);
    // worldPosition = GridManager.Instance.levelGrid.CellToWorld(cellPos); // rounds the position to something consistent
    // tilemapPosition = new Vector2Int(cellPos.x, cellPos.y);
    // floorLayer = fl;
  }

  public void Initialize(Vector2Int pos, FloorLayer fl)
  {
    tilemapPosition = pos;
    worldPosition = GridManager.Instance.levelGrid.CellToWorld(
      new Vector3Int(
        pos.x,
        pos.y,
        (int)GridManager.GetZOffsetForFloor(WorldObject.GetGameObjectLayerFromFloorLayer(fl)))
    );
    floorLayer = fl;
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
    t1.tilemapPosition.Equals(t2.tilemapPosition) && t1.floorLayer.Equals(t2.floorLayer);
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
    return !t1.tilemapPosition.Equals(t2.tilemapPosition) || !t1.floorLayer.Equals(t2.floorLayer);
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
    return tilemapPosition.GetHashCode() + floorLayer.GetHashCode();
  }
  public override string ToString()
  {
    return floorLayer.ToString() + ", " + tilemapPosition.ToString();
  }
  public int x
  {
    get { return tilemapPosition.x; }
  }

  public int y
  {
    get { return tilemapPosition.y; }
  }
  public Vector3 position3D
  {
    get { return new Vector3(tilemapPosition.x, tilemapPosition.y, 0); }
  }

  public Vector3 tileCenter
  {
    get { return new Vector3(tilemapPosition.x + .5f, tilemapPosition.y + .5f, 0); }
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

  int interestObjectsCount = 0;
  public void Awake()
  {
    worldGrid = new Dictionary<FloorLayer, Dictionary<Vector2Int, EnvironmentTileInfo>>();
    tilesToDestroyOnPlayerRespawn = new List<EnvironmentTileInfo>();
    tilesToRestoreOnPlayerRespawn = new List<EnvironmentTileInfo>();
    Dictionary<Vector2, EnvironmentTileInfo> floor = new Dictionary<Vector2, EnvironmentTileInfo>();
    Tilemap groundTilemap;
    Tilemap objectTilemap;
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
      for (int x = minXAcrossAllFloors; x < maxXAcrossAllFloors; x++)
      {
        for (int y = minYAcrossAllFloors; y < maxYAcrossAllFloors; y++)
        {
          //get both object and ground tile, build an environmentTileInfo out of them, and put it into our worldGrid
          TileLocation loc = new TileLocation(new Vector2Int(x, y), layer);
          ConstructAndSetEnvironmentTileInfo(loc, groundTilemap, objectTilemap);
        }
      }
      // }
    }
  }

  public EnvironmentTileInfo ConstructAndSetEnvironmentTileInfo(TileLocation loc, Tilemap groundTilemap, Tilemap objectTilemap)
  {
    Vector3Int v3pos = new Vector3Int(loc.tilemapPosition.x, loc.tilemapPosition.y, 0);
    EnvironmentTileInfo info = new EnvironmentTileInfo();
    EnvironmentTile objectTile = objectTilemap.GetTile(v3pos) as EnvironmentTile;
    EnvironmentTile groundTile = groundTilemap.GetTile(v3pos) as EnvironmentTile;
    Vector3Int worldLoc = Vector3Int.RoundToInt(groundTilemap.CellToWorld(v3pos));
    info.Init(
      new TileLocation(new Vector2Int(worldLoc.x, worldLoc.y), loc.floorLayer),
      groundTile,
      objectTile
    );
    worldGrid[loc.floorLayer][loc.tilemapPosition] = info;
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
    instance.transform.localPosition = new Vector3(destinationTile.tileLocation.tileCenter.x, destinationTile.tileLocation.tileCenter.y, 0);
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
      Debug.LogError("WARNING: Tried to find invalid tile at layer " + loc.floorLayer + ", coordinates " + loc.tilemapPosition);
      return null;
    }
    return worldGrid[loc.floorLayer][loc.tilemapPosition];
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
    if (!worldGrid[loc.floorLayer].ContainsKey(loc.tilemapPosition))
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
    switch (direction)
    {
      case TilemapDirection.UpperLeft:
        return new TileLocation(v2Loc + new Vector2Int(0, 1), floor);
      case TilemapDirection.UpperRight:
        return new TileLocation(v2Loc + new Vector2Int(1, 1), floor);
      case TilemapDirection.LowerLeft:
        return new TileLocation(v2Loc + new Vector2Int(0, -1), floor);
      case TilemapDirection.LowerRight:
        return new TileLocation(v2Loc + new Vector2Int(1, -1), floor);
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
    TileLocation modifiedLoc = new TileLocation(loc.tilemapPosition, loc.floorLayer);
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
    levelTilemap.SetTile(new Vector3Int(loc.tilemapPosition.x, loc.tilemapPosition.y, 0), null);
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
    levelTilemap.SetTile(new Vector3Int(location.tilemapPosition.x, location.tilemapPosition.y, 0), replacementTile);
    return ConstructAndSetEnvironmentTileInfo(location, layerFloor.groundTilemap, layerFloor.objectTilemap);
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

  public void DEBUGHighlightTile(TileLocation tilePos)
  {
    GameObject tileHighlight = Instantiate(highlightTilePrefab, tilePos.worldPosition + new Vector3(.5f, .5f, 0), Quaternion.identity);
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
}
