using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour
{
    public float detectableRange = Constants.DEFAULT_DETECTION_RANGE; // default range at which this object can be sensed by AI (possibly also player?)

    public FloorLayer currentFloor;
    public Collider2D col
    {
        get
        {
            return GetComponentInChildren<Collider2D>();
        }
    }

    private int prevLayer;
    public TileLocation GetTileLocation()
    {
        return new TileLocation(
          new Vector2Int(
            Mathf.FloorToInt(transform.position.x),
            Mathf.FloorToInt(transform.position.y)
          ),
          GetFloorLayer()
        );
    }

    public FloorLayer GetFloorLayer()
    {
        return (FloorLayer)Enum.Parse(typeof(FloorLayer), LayerMask.LayerToName(gameObject.layer));
    }

    public static void ChangeLayersRecursively(Transform transform, FloorLayer layerName)
    {
        ChangeLayersRecursively(transform, layerName.ToString());
    }

    public static void ChangeLayersRecursively(Transform trans, string layerName)
    {
        trans.gameObject.layer = LayerMask.NameToLayer(layerName);
        SpriteRenderer r = trans.gameObject.GetComponent<SpriteRenderer>();
        if (r != null)
        {
            r.sortingLayerName = layerName;
        }
        foreach (Transform child in trans)
        {
            ChangeLayersRecursively(child, layerName);
        }
    }

    public static FloorLayer GetFloorLayerOfGameObject(GameObject go)
    {
        return GetFloorLayerFromGameObjectLayer(go.layer);
    }
    public static FloorLayer GetFloorLayerFromGameObjectLayer(int gameObjectLayer)
    {
        int firstFloorLayerIndex = LayerMask.NameToLayer("B6");
        return (FloorLayer)(gameObjectLayer - firstFloorLayerIndex);
    }

    public static int GetGameObjectLayerFromFloorLayer(FloorLayer floorLayer)
    {
        return LayerMask.NameToLayer(floorLayer.ToString());
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += _OnValidate;
    }

    private void _OnValidate()
    {
        if (this == null || this.gameObject == null) { return; }
        if (gameObject.layer != prevLayer)
        {
            prevLayer = gameObject.layer;
            ChangeLayersRecursively(transform, LayerMask.LayerToName(gameObject.layer));
        }
    }
#endif

}
