using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EditTilemapTrigger : MonoBehaviour {

    public bool targetIsActiveAtStart;
	public int someInt;
	public int someSliderInt;
	public Grid grid;
	public Tilemap targetGroundTilemap;
	public Tilemap targetObstacleTilemap;
	public Tilemap destinationGroundTilemap;
	public Tilemap destinationObstacleTilemap;

	//let's think about positioning
	//I think every
	void Start () {
		foreach (TilemapRenderer renderer in gameObject.GetComponentsInChildren<TilemapRenderer>()) {
			renderer.enabled = false;
		}
	}

	// Update is called once per frame
	void Update () {

	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.gameObject.tag == "Player") {
			ApplyTilemapChanges();
            Destroy(gameObject);
		}
	}

	void ApplyTilemapChanges() {
		Vector3Int tilePos;
		Vector3Int destinationPos;
		foreach (var pos in targetGroundTilemap.cellBounds.allPositionsWithin) {
			tilePos = Vector3Int.FloorToInt(pos);
			if (targetGroundTilemap.HasTile(tilePos)) {
				destinationPos = Vector3Int.FloorToInt(grid.LocalToWorld(pos));
				destinationGroundTilemap.SetTile(destinationPos, targetGroundTilemap.GetTile(tilePos));
				destinationObstacleTilemap.SetTile(destinationPos, null);
			}
		}
		foreach (var pos in targetObstacleTilemap.cellBounds.allPositionsWithin) {
			tilePos = Vector3Int.FloorToInt(pos);
			if (targetObstacleTilemap.HasTile(tilePos)) {
				destinationPos = Vector3Int.FloorToInt(grid.LocalToWorld(pos));
				destinationObstacleTilemap.SetTile(destinationPos, targetObstacleTilemap.GetTile(tilePos));
				destinationGroundTilemap.SetTile(destinationPos, null);
			}
		}
	}
}
