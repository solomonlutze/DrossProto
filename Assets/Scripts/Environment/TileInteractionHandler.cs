using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileInteractionHandler : MonoBehaviour {

	public Tilemap tilemap;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// void OnCollisionEnter2D(Collision2D collision) {
	// 	Debug.Log(gameObject + "collided with " +collision.gameObject);
	// }

	void HandleTileInteraction(GameObject other) {
		Debug.Log(gameObject + " collided with "+other);
	}

	// TODO: At some point we should start using a single tilemap.
	// When we do, this should replace with a default tile, instead of destroying.
	public void DestroyTile(Vector3 tileLocation) {
		if (tilemap != null) {
			Debug.Log("setting to null?");
			tilemap.SetTile(tilemap.WorldToCell(tileLocation), null);
		}
	}

	public void DamageDirtTile(Vector3 tileLocation) {
		if (tilemap != null) {
			// TileWithAttributes tile = tilemap.GetTile(tilemap.WorldToCell(tileLocation)) as TileWithAttributes;
			// if (tile != null && tile.Dirt) {
			// 	tilemap.SetTile(tilemap.WorldToCell(tileLocation), null);
			// }
		}

	}
}
