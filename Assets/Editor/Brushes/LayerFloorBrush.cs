using System;
using UnityEngine.Tilemaps;
using UnityEngine;
// using UnityEditor;
// using UnityEditor.Tilemaps;
// using UnityEditor.EditorTools;
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
          // Debug.Log("targetTilemap: " + targetTilemap);
          // Debug.Log("targetTilemap go: " + targetTilemap.gameObject);
          // Debug.Log("floorlayerofgameobject: " + WorldObject.GetFloorLayerOfGameObject(targetTilemap.gameObject));
          // Debug.Log("gridmanager instance: " + GridManager.Instance);
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
      }
      else
      {
        Reset();
        UpdateSizeAndPivot(new Vector3Int(position.size.x, position.size.y, 1), new Vector3Int(pickStart.x, pickStart.y, 0));
        if (brushTarget != null)
        {
          Tilemap groundTilemap = parentLayerFloor.groundTilemap;
          Tilemap objectTilemap = parentLayerFloor.objectTilemap;
          Tilemap tilemap;
          foreach (Vector3Int pos in position.allPositionsWithin)
          {
            Vector3Int brushPosition = new Vector3Int(pos.x - position.x, pos.y - position.y, 0);
            tilemap = groundTilemap;
            if (objectTilemap.GetTile(pos) != null)
            {
              tilemap = objectTilemap;
            }
            PickCell(pos, brushPosition, tilemap);
          }
        }
      }
      SelectAppropriateTilemapForBrushTileType();
    }

    public void SelectAppropriateTilemapForBrushTileType()
    {
      if (EditorTools.EditorTools.activeToolType == typeof(EraseTool))
      {
        return;
      }
      GridBrush brush = GridPaintingState.gridBrush as GridBrush;
      GridBrush.BrushCell cell = brush.cells.Length > 0 ? brush.cells[0] : null;
      GameObject tilemapToPaint = GridPaintingState.scenePaintTarget;
      Tilemap selectedTilemap = tilemapToPaint ? tilemapToPaint.GetComponent<Tilemap>() : null;
      if (cell != null && selectedTilemap != null)
      {
        EnvironmentTile selectedTile = cell.tile as EnvironmentTile;
        if (selectedTile != null)
        {
          // TilemapEditorTool.SetActiveEditorTool(typeof(EraseTool));
          Tilemap desiredTilemap;
          if (selectedTile.floorTilemapType == FloorTilemapType.Ground)
          {
            desiredTilemap = selectedTilemap.transform.parent.GetComponent<LayerFloor>().groundTilemap;
          }
          else if (selectedTile.floorTilemapType == FloorTilemapType.Ground)
          {
            desiredTilemap = selectedTilemap.transform.parent.GetComponent<LayerFloor>().objectTilemap;
          }
          else
          {
            desiredTilemap = selectedTilemap.transform.parent.GetComponent<LayerFloor>().visibilityTilemap;
          }
          if (selectedTilemap != desiredTilemap)
          {
            GameObject[] go = new GameObject[] { desiredTilemap.gameObject };
            Selection.objects = go;
          }
        }
        else
        {
          // Debug.Log("selected tile is null! " + selectedTile);
          // TilemapEditorTool.SetActiveEditorTool(typeof(EraseTool));
        }
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