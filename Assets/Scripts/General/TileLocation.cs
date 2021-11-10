
using UnityEngine;
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
    }
  }

  public Vector2Int zeroIndexedTilemapCoordinates
  {
    get
    {
      return tilemapCoordinates - new Vector2Int(GridManager.Instance.worldGridData.minXAcrossAllFloors, GridManager.Instance.worldGridData.minYAcrossAllFloors);
    }
  }
  public Vector2Int chunkCoordinates
  {
    get
    {
      return new Vector2Int(zeroIndexedTilemapCoordinates.x / GridManager.Instance.worldGridData.chunkSize, zeroIndexedTilemapCoordinates.y / GridManager.Instance.worldGridData.chunkSize);
    }
  }
  public Vector3 cellCenterWorldPosition
  {
    get
    {
      return cellCenterPosition + new Vector3(0, 0, 0f);
    }
  }
  public TileLocation WithOffset(Vector2Int offset)
  {
    return new TileLocation(tilemapCoordinates.x + offset.x, tilemapCoordinates.y + offset.y, floorLayer);
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

  public TileLocation(Vector3 pos)
  { // this initializer assumes a correct z position! beware!
    Initialize(pos, GridManager.GetFloorLayerFromZPosition(pos.z));
  }

  public void Initialize(Vector3 worldPos, FloorLayer fl)
  {
    tilemapCoordinates = (Vector2Int)GridManager.Instance.levelGrid.WorldToCell(worldPos);
    worldPosition = worldPos;
    // First to vector3Int
    cubeCoords = new Vector3();
    cubeCoords.y = -1 * worldPosition.y / GridConstants.Y_SPACING;
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
