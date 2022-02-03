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
      if (tile != null)
      {
        // get tile type
        // validate that we're painting on the appropriate floor tilemap; switch to the appropriate one otherwise
        Transform brushTargetParent = brushTarget.transform.parent;
        string brushTargetName = "";
        switch (tile.floorTilemapType)
        {
          case (FloorTilemapType.Ground):
            brushTargetName = brushTargetParent.gameObject.name + "_Ground";
            break;
          case (FloorTilemapType.Object):
            brushTargetName = brushTargetParent.gameObject.name + "_Object";
            break;
          case (FloorTilemapType.Water):
            brushTargetName = brushTargetParent.gameObject.name + "_Water";
            break;
          case (FloorTilemapType.Info):
            brushTargetName = brushTargetParent.gameObject.name + "_Info";
            break;
          default:
            Debug.LogError("tilemap type not one of ground, object, water, info??");
            break;
        }
        Debug.Log("brushTargetName: " + brushTargetName + ", brushTarget.name " + brushTarget.name);
        if (brushTargetName != brushTarget.name)
        {
          brushTarget = brushTargetParent.Find(brushTargetName).gameObject;
          SelectAppropriateTilemapForBrushTileType();
        }
        if (tile.autoPlaceTileOnPaint_Above != null)
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
        // set height
        base.BoxFill(gridLayout, brushTarget, position);
        // this section insures a given tile ONLY has water OR ground, not both
        GameObject tilemapToEraseOn = null;
        if (tile.floorTilemapType == FloorTilemapType.Ground)
        {
          tilemapToEraseOn = brushTargetParent.Find(brushTargetParent.gameObject.name + "_Water").gameObject;
        }
        else if (tile.floorTilemapType == FloorTilemapType.Water)
        {
          tilemapToEraseOn = brushTargetParent.Find(brushTargetParent.gameObject.name + "_Ground").gameObject;
        }
        if (tilemapToEraseOn != null)
        {
          base.BoxErase(gridLayout, tilemapToEraseOn, position);
        }
        foreach (Vector3Int location in position.allPositionsWithin)
        {
          GridManager.Instance.worldGridData.PaintFloorHeight(WorldObject.GetFloorLayerOfGameObject(brushTarget), location, tile);
        }
      }
    }

    public override void BoxErase(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
    {
      Tilemap targetTilemap = brushTarget.GetComponent<Tilemap>();
      EnvironmentTile tile = null;
      if (cells.Length > 0)
      {
        tile = cells[0].tile as EnvironmentTile;
      }
      if (targetTilemap != null)
      {

        foreach (Vector3Int location in position.allPositionsWithin)
        {
          GridManager.Instance.worldGridData.ClearWallObject(WorldObject.GetFloorLayerOfGameObject(brushTarget), location, tile);
        }
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
                GridManager.Instance.worldGridData.ClearWallObject(WorldObject.GetFloorLayerOfGameObject(layerFloorBelow.gameObject), location, tile);
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
      SceneView sceneView = SceneView.lastActiveSceneView;
      int cameraZ = Mathf.RoundToInt(sceneView.camera.transform.position.z);
      FloorLayer targetLayer = (FloorLayer)(-(cameraZ - 10)); // 12 - 2 = 10, to account for camera distance
      Debug.Log("target layer: " + targetLayer);
      parentLayerFloor = GridManager.Instance.layerFloors[targetLayer];
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
          Tilemap waterTilemap = parentLayerFloor.waterTilemap;
          Tilemap infoTilemap = parentLayerFloor.infoTilemap;
          Tilemap tilemap;
          foreach (Vector3Int pos in position.allPositionsWithin)
          {
            Vector3Int brushPosition = new Vector3Int(pos.x - position.x, pos.y - position.y, 0);
            tilemap = waterTilemap;
            if (infoTilemap.gameObject.activeSelf && infoTilemap.GetTile(pos) != null)
            {
              tilemap = infoTilemap;
            }
            else if (objectTilemap.GetTile(pos) != null)
            {
              tilemap = objectTilemap;
            }
            else if (groundTilemap.GetTile(pos) != null)
            {
              tilemap = groundTilemap;
            }
            PickCell(pos, brushPosition, tilemap);
          }
        }
      }
      SelectAppropriateTilemapForBrushTileType(parentLayerFloor);
    }

    public void SelectAppropriateTilemapForBrushTileType(LayerFloor targetLayerFloor = null)
    {
      if (UnityEditor.EditorTools.ToolManager.activeToolType == typeof(EraseTool))
      {
        return;
      }
      GridBrush brush = GridPaintingState.gridBrush as GridBrush;
      GridBrush.BrushCell cell = brush.cells.Length > 0 ? brush.cells[0] : null;
      GameObject tilemapToPaint = GridPaintingState.scenePaintTarget;
      Tilemap selectedTilemap = tilemapToPaint ? tilemapToPaint.GetComponent<Tilemap>() : null;
      LayerFloor desiredLayerFloor = targetLayerFloor;
      if (desiredLayerFloor == null && selectedTilemap != null)
      {
        desiredLayerFloor = selectedTilemap.transform.parent.GetComponent<LayerFloor>();
      }
      if (cell != null)
      {
        FloorTilemapType floorTilemapType = (cell.tile as EnvironmentTile)?.floorTilemapType ?? (cell.tile as InfoTile)?.floorTilemapType ?? FloorTilemapType.Ground;

        // TilemapEditorTool.SetActiveEditorTool(typeof(EraseTool));
        Tilemap desiredTilemap;
        if (floorTilemapType == FloorTilemapType.Ground)
        {
          desiredTilemap = desiredLayerFloor.groundTilemap;
        }
        else if (floorTilemapType == FloorTilemapType.Object)
        {
          desiredTilemap = desiredLayerFloor.objectTilemap;
        }
        else if (floorTilemapType == FloorTilemapType.Info)
        {
          desiredTilemap = desiredLayerFloor.infoTilemap;
        }
        else if (floorTilemapType == FloorTilemapType.Water)
        {
          desiredTilemap = desiredLayerFloor.waterTilemap;
        }
        else
        {
          desiredTilemap = desiredLayerFloor.visibilityTilemap;
        }
        if (selectedTilemap != desiredTilemap)
        {
          GameObject[] go = new GameObject[] { desiredTilemap.gameObject };
          Selection.objects = go;
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