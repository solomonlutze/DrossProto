using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour {
	public float detectionRange = Constants.DEFAULT_DETECTION_RANGE; // default range at which this object can be sensed by AI (possibly also player?)
	public TileLocation GetTileLocation() {
		return new TileLocation(
			new Vector3(
				Mathf.FloorToInt(transform.position.x) + .5f,
				Mathf.FloorToInt(transform.position.y) + .5f,
				Mathf.FloorToInt(transform.position.z)
			),
			(Constants.FloorLayer) Enum.Parse(typeof (Constants.FloorLayer), LayerMask.LayerToName(gameObject.layer))
		);
	}
}
