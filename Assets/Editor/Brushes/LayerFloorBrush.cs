using System;
using UnityEngine.Tilemaps;
using UnityEngine;
using System.Collections;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEditor.Tilemaps
{
  [CustomGridBrush(true, false, true, "Layer Floor Brush")]
  public class LayerFloorBrush : GridBrush
  {
    public override void BoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
    {
      Debug.Log("box filling");
      EnvironmentTile tile = null;
      if (cells.Length > 0)
      {
        tile = cells[0].tile as EnvironmentTile;
      }
      if (tile != null && tile.autoPlaceTileOnPaint_Above != null)
      {
        LayerFloor layerFloorAbove = GridManager.Instance.GetFloorLayerAbove(WorldObject.GetFloorLayerOfGameObject(brushTarget));
        if (layerFloorAbove != null)
        {
          Tilemap groundTilemapAbove = layerFloorAbove.groundTilemap;
          foreach (Vector3Int location in position.allPositionsWithin)
          {
            if (groundTilemapAbove.GetTile(location) == null)
            {
              groundTilemapAbove.SetTile(location, tile.autoPlaceTileOnPaint_Above);
            }
          }
        }
      }
      base.BoxFill(gridLayout, brushTarget, position);
    }
    public override void BoxErase(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
    {
      Debug.Log("erasing at position " + position);
      Tilemap targetTilemap = brushTarget.GetComponent<Tilemap>();
      if (targetTilemap != null)
      {
        Debug.Log("target tilemap: " + targetTilemap);
        Debug.Log("target tilemap GO: " + targetTilemap.gameObject);
        LayerFloor parentLayerFloor = targetTilemap.GetComponentInParent<LayerFloor>();
        if (parentLayerFloor != null && parentLayerFloor.groundTilemap == targetTilemap)
        {
          LayerFloor layerFloorBelow = GridManager.Instance.GetFloorLayerBelow(WorldObject.GetFloorLayerOfGameObject(targetTilemap.gameObject));
          if (layerFloorBelow != null)
          {
            Tilemap objectTilemapBelow = layerFloorBelow.objectTilemap;
            foreach (Vector3Int location in position.allPositionsWithin)
            {
              EnvironmentTile objectTileBelow = (EnvironmentTile)objectTilemapBelow.GetTile(location);
              if (objectTilemapBelow.GetTile(location) != null)
              {
                Debug.Log("Getting tile, it's: " + objectTilemapBelow.GetTile(location));
                // Debug.Log("Location + Pivot: " + (location + pivot));
                //   // WARNING: possibly not performant! use SetTilesBlock instead if this gets ugly
                objectTilemapBelow.SetTile(location, null);
              }
            }
          }
        }
      }
      base.BoxErase(gridLayout, brushTarget, position);
    }
  }

}