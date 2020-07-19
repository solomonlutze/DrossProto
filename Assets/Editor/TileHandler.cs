using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine.Tilemaps;
using UnityEditor.Tilemaps;

[ExecuteInEditMode]
public class TileHandler : MonoBehaviour
{

  void Update()
  {
    SelectAppropriateTilemapForBrushTile();
  }

  void SelectAppropriateTilemapForBrushTile()
  {
    if (EditorTools.activeToolType == typeof(EraseTool))
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
        else
        {
          desiredTilemap = selectedTilemap.transform.parent.GetComponent<LayerFloor>().objectTilemap;
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

}
