using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour {
	public float detectableRange = Constants.DEFAULT_DETECTION_RANGE; // default range at which this object can be sensed by AI (possibly also player?)
	private int prevLayer;
	public TileLocation GetTileLocation() {
		return new TileLocation(
			new Vector2Int(
				Mathf.FloorToInt(transform.position.x),
				Mathf.FloorToInt(transform.position.y)
			),
			(FloorLayer) Enum.Parse(typeof (FloorLayer), LayerMask.LayerToName(gameObject.layer))
		);
	}

	public static void ChangeLayersRecursively(Transform trans, string layerName)
	{
		trans.gameObject.layer = LayerMask.NameToLayer(layerName);
		SpriteRenderer r = trans.gameObject.GetComponent<SpriteRenderer>();
		if (r != null) {
			r.sortingLayerName = layerName;
		}
		foreach(Transform child in trans)
		{
			ChangeLayersRecursively(child, layerName);
		}
 	}
  private void OnValidate() {
	if (gameObject.layer != prevLayer) {
		prevLayer = gameObject.layer;
		ChangeLayersRecursively(transform, LayerMask.LayerToName(gameObject.layer));
	}
  }

}
