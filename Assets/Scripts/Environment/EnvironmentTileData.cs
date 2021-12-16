using System.Collections.Generic;
using UnityEngine;

// Used to store persistent tile data required to construct its full tileInfo.
// Nothing utility-based or derived should go here; all that should live in EnvironmentTileInfo
[System.Serializable]
public class EnvironmentTileData
{
  public float groundHeight;
  public float ceilingHeight;

  public EnvironmentTileData()
  {
    groundHeight = 0;
    ceilingHeight = 1;
  }

  public EnvironmentTileData(Vector2 heightInfo)
  {
    groundHeight = heightInfo.x;
    ceilingHeight = heightInfo.y;
  }

  public bool IsEmpty()
  {
    return groundHeight == 0 && ceilingHeight == 1;
  }
}