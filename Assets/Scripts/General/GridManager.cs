﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TilemapDirection { None, Up, Down, Left, Right, Above, Below }

public class TileLocation {
	public Vector2Int position;
	public FloorLayer floorLayer;

	public TileLocation(Vector2Int pos, FloorLayer fl) {
		position = pos;
		floorLayer = fl;
	}
	public TileLocation(Vector3 pos, FloorLayer fl) {
		position = new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
		floorLayer = fl;
	}
	public static bool operator == (TileLocation t1, TileLocation t2)
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
		t1.position.Equals(t2.position) && t1.floorLayer.Equals(t2.floorLayer);
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
		return !t1.position.Equals(t2.position) || !t1.floorLayer.Equals(t2.floorLayer);
	}
	public override bool Equals(object obj) {
		bool rc = false;
        if (obj is TileLocation)
        {
            TileLocation tl2 = obj as TileLocation;
            rc = (this == tl2);
        }
        return rc;
	}

	public override int GetHashCode() {
		return position.GetHashCode() + floorLayer.GetHashCode();
	}
	public override string ToString() {
        return floorLayer.ToString() + ", " + position.ToString();
    }
}
//TODO: this should be a singleton
public class GridManager : Singleton<GridManager> {

	public Grid levelGrid;
	public Material semiTransparentMaterial;
	public Material fullyOpaqueMaterial;
	public LayerToLayerFloorDictionary layerFloors;
	public Dictionary<FloorLayer, Dictionary<Vector2Int, EnvironmentTileInfo>> worldGrid;


	public void Awake() {
		worldGrid = new Dictionary<FloorLayer, Dictionary<Vector2Int, EnvironmentTileInfo>>();
		Dictionary<Vector2, EnvironmentTileInfo> floor = new Dictionary<Vector2, EnvironmentTileInfo>();
		Tilemap groundTilemap;
		Tilemap objectTilemap;
		foreach (LayerFloor layerFloor in levelGrid.GetComponentsInChildren<LayerFloor>()) {
			FloorLayer fl = (FloorLayer) Enum.Parse(typeof (FloorLayer), layerFloor.gameObject.name);
			layerFloors[fl] = layerFloor;
		}

		int minXAcrossAllFloors = 5000;
		int maxXAcrossAllFloors = -5000;
		int minYAcrossAllFloors = 5000;
		int maxYAcrossAllFloors = -5000;
		foreach(LayerFloor lf in layerFloors.Values) {
			groundTilemap = lf.groundTilemap;
			minXAcrossAllFloors = Mathf.Min(minXAcrossAllFloors, groundTilemap.cellBounds.xMin);
			maxXAcrossAllFloors = Mathf.Max(maxXAcrossAllFloors, groundTilemap.cellBounds.xMax);
			minYAcrossAllFloors = Mathf.Min(minYAcrossAllFloors, groundTilemap.cellBounds.yMin);
			maxYAcrossAllFloors = Mathf.Max(maxYAcrossAllFloors, groundTilemap.cellBounds.yMax);
		}
		foreach (FloorLayer layer in Enum.GetValues(typeof(FloorLayer))) {
			floor.Clear();
			worldGrid[layer] = new Dictionary<Vector2Int, EnvironmentTileInfo>();
			if (!layerFloors.ContainsKey(layer)) {
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
					TileLocation loc = new TileLocation(new Vector2Int(x,y), layer);
					ConstructAndSetEnvironmentTileInfo(loc, groundTilemap, objectTilemap);

				}
			}
		}
	}

	public void ConstructAndSetEnvironmentTileInfo(TileLocation loc, Tilemap groundTilemap, Tilemap objectTilemap) {
		Vector3Int v3pos = new Vector3Int(loc.position.x, loc.position.y, 0);
		EnvironmentTileInfo info = new EnvironmentTileInfo();
		EnvironmentTile objectTile = objectTilemap.GetTile(v3pos) as EnvironmentTile;
		EnvironmentTile groundTile = groundTilemap.GetTile(v3pos) as EnvironmentTile;
		Vector3Int worldLoc = Vector3Int.RoundToInt(groundTilemap.CellToWorld(v3pos));
		info.Init(
			new TileLocation(new Vector2Int(worldLoc.x, worldLoc.y), loc.floorLayer),
			groundTile,
			objectTile
		);
		worldGrid[loc.floorLayer][loc.position] = info;
	}

	public EnvironmentTileInfo GetTileAtLocation(TileLocation loc) {
		return worldGrid[loc.floorLayer][loc.position];
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

	public void OnLayerChange(FloorLayer floor) {
		// Used to control floor layer transparency.
		if (layerFloors.ContainsKey(floor)) {
			LayerFloor newFloor = layerFloors[floor];
			newFloor.groundTilemap.GetComponent<TilemapRenderer>().material = fullyOpaqueMaterial;
			newFloor.objectTilemap.GetComponent<TilemapRenderer>().material = fullyOpaqueMaterial;

		}
		if (layerFloors.ContainsKey(floor + 1)) {
			LayerFloor nextFloorUp = layerFloors[floor + 1];
			nextFloorUp.groundTilemap.GetComponent<TilemapRenderer>().material = semiTransparentMaterial;
			nextFloorUp.objectTilemap.GetComponent<TilemapRenderer>().material = semiTransparentMaterial;
		}
	}
	public bool CanStickToAdjacentTile(Vector3 loc, FloorLayer floor) {
		foreach (TilemapDirection d in new TilemapDirection[] {
			TilemapDirection.Up,
			TilemapDirection.Right,
			TilemapDirection.Down,
			TilemapDirection.Left,
		}) {
			EnvironmentTileInfo et = GetAdjacentTile(loc, floor, d);
			if (et.CanBeStuckTo()) {
				return true;
			}
		}
		return false;
	}
	public bool CanClimbAdjacentTile(Vector3 loc, FloorLayer floor) {
		if (GetAdjacentTile(loc, floor, TilemapDirection.Above).IsEmpty()) {
			foreach (TilemapDirection d in new TilemapDirection[] {
				TilemapDirection.Up,
				TilemapDirection.Right,
				TilemapDirection.Down,
				TilemapDirection.Left,
			}) {
				EnvironmentTileInfo et = GetAdjacentTile(loc, floor, d);
				if (
					et.IsClimbable()
				) {
					// must be able to stick to the same tile above this one as well, obvs
					EnvironmentTileInfo tileAboveClimbableTile = GetAdjacentTile(loc, floor + 1, d);
					if (tileAboveClimbableTile.CanBeStuckTo()) {
						Debug.Log("tileAbove can be stuck to");
						return true;
					}
				}
			}
		}
		return false;
	}

	public EnvironmentTileInfo GetAdjacentTile(Vector3 loc, FloorLayer floor, TilemapDirection direction) {
		Vector2Int v2Loc = new Vector2Int(Mathf.FloorToInt(loc.x), Mathf.FloorToInt(loc.y));
		TileLocation tileLoc;
		switch (direction) {
			case TilemapDirection.Up:
				tileLoc = new TileLocation(v2Loc + new Vector2Int(0, 1), floor);
				return GetTileAtLocation(tileLoc);
			case TilemapDirection.Down:
				tileLoc = new TileLocation(v2Loc + new Vector2Int(0, -1), floor);
				return GetTileAtLocation(tileLoc);
			case TilemapDirection.Right:
				tileLoc = new TileLocation(v2Loc + new Vector2Int(1, 0), floor);
				return GetTileAtLocation(tileLoc);
			case TilemapDirection.Left:
				tileLoc = new TileLocation(v2Loc + new Vector2Int(-1, 0), floor);
				return GetTileAtLocation(tileLoc);
			case TilemapDirection.Above:
				tileLoc = new TileLocation(v2Loc, floor + 1);
				return GetTileAtLocation(tileLoc);
			case TilemapDirection.Below:
				tileLoc = new TileLocation(v2Loc, floor - 1);
				return GetTileAtLocation(tileLoc);
			case TilemapDirection.None:
			default:
				return GetTileAtLocation(new TileLocation(v2Loc, floor));
		}
	}
	public void ReplaceAdjacentTile(TileLocation loc, EnvironmentTile replacementTile, TilemapDirection direction) {
		TileLocation modifiedLoc = new TileLocation(loc.position, loc.floorLayer);
		switch (direction) {
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
	public void DestroyObjectTileAtLocation(TileLocation loc) {
		LayerFloor layerFloor = layerFloors[loc.floorLayer];
		if (layerFloor == null || layerFloor.groundTilemap == null || layerFloor.objectTilemap == null)
        {
            Debug.LogWarning("missing layerFloor info for "+loc.floorLayer.ToString());
            return;
        }
		Tilemap levelTilemap = layerFloor.objectTilemap;
		levelTilemap.SetTile(new Vector3Int(loc.position.x, loc.position.y, 0), null);
	}


	public void ReplaceTileAtLocation(TileLocation location, EnvironmentTile replacementTile) {
		LayerFloor layerFloor = layerFloors[location.floorLayer];
		if (layerFloor == null || layerFloor.groundTilemap == null || layerFloor.objectTilemap == null)
        {
            Debug.LogWarning("missing layerFloor info for "+location.floorLayer.ToString());
            return;
        }
		Tilemap levelTilemap = replacementTile != null && replacementTile.floorTilemapType == FloorTilemapType.Ground ? layerFloor.groundTilemap : layerFloor.objectTilemap;
		levelTilemap.SetTile(new Vector3Int(location.position.x, location.position.y, 0), replacementTile);
		ConstructAndSetEnvironmentTileInfo(location, layerFloor.groundTilemap, layerFloor.objectTilemap);
	}
}
