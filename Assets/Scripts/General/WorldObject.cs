using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour {
	public float DetectableRange = Constants.DEFAULT_DETECTION_RANGE; // default range at which this object can be sensed by AI (possibly also player?)
	public TileLocation GetTileLocation() {
		return new TileLocation(
			new Vector2Int(
				Mathf.FloorToInt(transform.position.x),
				Mathf.FloorToInt(transform.position.y)
			),
			(FloorLayer) Enum.Parse(typeof (FloorLayer), LayerMask.LayerToName(gameObject.layer))
		);
	}
}
