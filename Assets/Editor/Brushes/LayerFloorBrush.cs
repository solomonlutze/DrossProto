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
      EnvironmentTile tile = null;
      if (cells.Length > 0)
      {
        tile = cells[0].tile as EnvironmentTile;
      }
      if (tile != null && tile.autoPlaceTileOnPaint_Above != null)
      {
        Debug.Log("grid manager instance??? " + GridManager.Instance);
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
      Tilemap targetTilemap = brushTarget.GetComponent<Tilemap>();
      if (targetTilemap != null)
      {
        LayerFloor parentLayerFloor = targetTilemap.GetComponentInParent<LayerFloor>();
        if (parentLayerFloor != null && parentLayerFloor.groundTilemap == targetTilemap)
        {
          LayerFloor layerFloorBelow = GridManager.Instance.GetFloorLayerBelow(WorldObject.GetFloorLayerOfGameObject(targetTilemap.gameObject));
          if (layerFloorBelow != null)
          {
            Tilemap objectTilemapBelow = layerFloorBelow.objectTilemap;
            foreach (Vector3Int location in position.allPositionsWithin)
            {
              EnvironmentTile originalTileToErase = (EnvironmentTile)targetTilemap.GetTile(location);
              if (originalTileToErase == null) { continue; }
              EnvironmentTile objectTileBelow = (EnvironmentTile)objectTilemapBelow.GetTile(location);
              if (objectTileBelow != null && objectTileBelow.colliderType != Tile.ColliderType.None)
              {
                // WARNING: possibly not performant! use SetTilesBlock instead if this gets ugly
                objectTilemapBelow.SetTile(location, null);
              }
            }
          }
        }
      }
      base.BoxErase(gridLayout, brushTarget, position);
    }
    public override void Pick(GridLayout gridLayout, GameObject brushTarget, BoundsInt position, Vector3Int pickStart)
    {
      LayerFloor parentLayerFloor = brushTarget.GetComponentInParent<LayerFloor>();
      if (parentLayerFloor == null)
      {
        base.Pick(gridLayout, brushTarget, position, pickStart);
        return;
      }
      Reset();
      UpdateSizeAndPivot(new Vector3Int(position.size.x, position.size.y, 1), new Vector3Int(pickStart.x, pickStart.y, 0));
      if (brushTarget == null)
      {
        return;
      }
      Tilemap groundTilemap = parentLayerFloor.groundTilemap;
      Tilemap objectTilemap = parentLayerFloor.objectTilemap;
      Tilemap tilemap;
      foreach (Vector3Int pos in position.allPositionsWithin)
      {
        Debug.Log("objectTilemap: " + objectTilemap);
        Vector3Int brushPosition = new Vector3Int(pos.x - position.x, pos.y - position.y, 0);
        tilemap = groundTilemap;
        Debug.Log("ground tile: " + groundTilemap.GetTile(pos));
        Debug.Log("object tile: " + objectTilemap.GetTile(pos));
        if (objectTilemap.GetTile(pos) != null)
        {
          tilemap = objectTilemap;
          Debug.Log("should be grabbing object tile!");
        }
        PickCell(pos, brushPosition, tilemap);
      }
    }

    private void PickCell(Vector3Int position, Vector3Int brushPosition, Tilemap tilemap)
    {
      if (tilemap == null)
        return;

      SetTile(brushPosition, tilemap.GetTile(position));
      SetMatrix(brushPosition, tilemap.GetTransformMatrix(position));
      SetColor(brushPosition, tilemap.GetColor(position));
    }
  }


}