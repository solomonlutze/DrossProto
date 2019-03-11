using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TilemapDirection { None, Up, Down, Left, Right, Above, Below }
public struct TileLocation {
	public Vector3 position;
	public Constants.FloorLayer floorLayer;

	public TileLocation(Vector3 pos, Constants.FloorLayer fl) {
		position = pos;
		floorLayer = fl;
	}
	public static bool operator ==(TileLocation t1, TileLocation t2)
	{
		return t1.position.Equals(t2.position) && t1.floorLayer.Equals(t2.floorLayer);
	}
		public static bool operator !=(TileLocation t1, TileLocation t2)
	{
		return !t1.position.Equals(t2.position) || !t1.floorLayer.Equals(t2.floorLayer);
	}
}

public class GridManager : MonoBehaviour {

	public Grid levelGrid;
	public LayerToLayerFloorDictionary layerFloors;
	// public EnvironmentTile GetTileAtLocation(Vector3  loc) {
	// 	EnvironmentTile tile = (EnvironmentTile) levelTilemap.GetTile(levelGrid.WorldToCell(loc));
	// 	if (tile == null) {
	// 		Debug.LogError("Found a non-EnvironmentTile at location "+loc);
	// 	}
	// 	return tile;
	// }

	public Vector3Int GetCellFromWorldLocation(Vector3 loc) {
        return levelGrid.WorldToCell(loc);
	}
	public EnvironmentTile GetTileAtLocation(Vector3 loc, Constants.FloorLayer floor) {
		if (!layerFloors.ContainsKey(floor) || layerFloors[floor].groundTilemap == null || layerFloors[floor].objectTilemap == null)
        {
            Debug.LogWarning("missing layerFloor info for "+floor.ToString());
            return null;
        }
        LayerFloor layerFloor = layerFloors[floor];
        EnvironmentTile tile = (EnvironmentTile) layerFloor.objectTilemap.GetTile(new Vector3Int(Mathf.FloorToInt(loc.x),Mathf.FloorToInt(loc.y),Mathf.FloorToInt(loc.z)));
		if (tile == null) {
			tile = (EnvironmentTile) layerFloor.groundTilemap.GetTile(new Vector3Int(Mathf.FloorToInt(loc.x),Mathf.FloorToInt(loc.y),Mathf.FloorToInt(loc.z)));
		}
		if (tile == null) {
			Debug.LogError("Found a non-EnvironmentTile at location "+loc);
		}
		return tile;
	}

	public EnvironmentTile GetAdjacentTile(Vector3 loc, Constants.FloorLayer floor, TilemapDirection direction) {
		switch (direction) {
			case TilemapDirection.Above:
				return GetTileAtLocation(loc, (Constants.FloorLayer) ((int) floor + 1));
			case TilemapDirection.Below:
				return GetTileAtLocation(loc, (Constants.FloorLayer) ((int) floor + 1));
			case TilemapDirection.None:
			default:
				return GetTileAtLocation(loc, floor);
		}
	}
	public void ReplaceAdjacentTile(Vector3 loc, Constants.FloorLayer floor, EnvironmentTile replacementTile, TilemapDirection direction) {
		switch (direction) {
			case TilemapDirection.Above:
				ReplaceTileAtLocation(loc, (Constants.FloorLayer) ((int) floor + 1), replacementTile);
				break;
			case TilemapDirection.Below:
				ReplaceTileAtLocation(loc, (Constants.FloorLayer) ((int) floor - 1), replacementTile);
				break;
			case TilemapDirection.None:
			default:
				ReplaceTileAtLocation(loc, floor, replacementTile);
				break;
		}
	}

	// Destroys a tile on the object layer.
	// TODO: generalize this probably? shrtug
	public void DestroyObjectTileAtLocation(Vector3 loc, Constants.FloorLayer floor) {
		LayerFloor layerFloor = layerFloors[floor];
		if (layerFloor == null || layerFloor.groundTilemap == null || layerFloor.objectTilemap == null)
        {
            Debug.LogWarning("missing layerFloor info for "+floor.ToString());
            return;
        }
		Tilemap levelTilemap = layerFloor.objectTilemap;
		levelTilemap.SetTile(new Vector3Int(Mathf.FloorToInt(loc.x),Mathf.FloorToInt(loc.y),Mathf.FloorToInt(loc.z)), null);
	}

	public void ReplaceTileAtLocation(Vector3 loc, Constants.FloorLayer floor, EnvironmentTile replacementTile) {
		LayerFloor layerFloor = layerFloors[floor];
		if (layerFloor == null || layerFloor.groundTilemap == null || layerFloor.objectTilemap == null)
        {
            Debug.LogWarning("missing layerFloor info for "+floor.ToString());
            return;
        }
		Tilemap levelTilemap = replacementTile.floorTilemapType == FloorTilemapType.Ground ? layerFloor.groundTilemap : layerFloor.objectTilemap;
		levelTilemap.SetTile(new Vector3Int(Mathf.FloorToInt(loc.x),Mathf.FloorToInt(loc.y),Mathf.FloorToInt(loc.z)), replacementTile);
	}
}
