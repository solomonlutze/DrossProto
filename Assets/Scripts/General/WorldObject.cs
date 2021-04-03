using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour
{
  public float detectableRange = Constants.DEFAULT_DETECTION_RANGE; // default range at which this object can be sensed by AI (possibly also player?)

  public FloorLayer currentFloor;
  public static Dictionary<FloorLayer, int> floorLayerToGameObjectLayer = new Dictionary<FloorLayer, int>() {
    {FloorLayer.B6, 9},
    {FloorLayer.B5, 10},
    {FloorLayer.B4, 11},
    {FloorLayer.B3, 12},
    {FloorLayer.B2, 13},
    {FloorLayer.B1, 14},
    {FloorLayer.F1, 15},
    {FloorLayer.F2, 16},
    {FloorLayer.F3, 17},
    {FloorLayer.F4, 18},
    {FloorLayer.F5, 19},
    {FloorLayer.F6, 20},

  };
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
      transform.position,
      GetFloorLayer()
    );
  }

  public FloorLayer GetFloorLayer()
  {
    return (FloorLayer)Enum.Parse(typeof(FloorLayer), LayerMask.LayerToName(gameObject.layer));
  }

  public static void ChangeLayersRecursively(Transform t, FloorLayer layerName)
  {
    // if (t.position.z != GridManager.GetZOffsetForFloor(GetGameObjectLayerFromFloorLayer(layerName)))
    // {
    //   t.position = new Vector3(t.position.x, t.position.y, GridManager.GetZOffsetForFloor(GetGameObjectLayerFromFloorLayer(layerName)));
    // }
    ChangeLayersRecursively(t, layerName.ToString());
  }

  public static void ChangeLayersRecursively(Transform trans, string layerName)
  {
    trans.gameObject.layer = LayerMask.NameToLayer(layerName);
    SpriteRenderer r = trans.gameObject.GetComponent<SpriteRenderer>();
    if (r != null)
    {
      r.sortingLayerName = layerName;
    }
    TrailRenderer t = trans.gameObject.GetComponent<TrailRenderer>();
    if (t != null)
    {
      t.sortingLayerName = layerName;
    }
    Canvas c = trans.gameObject.GetComponent<Canvas>();
    if (c != null)
    {
      c.sortingLayerName = layerName;
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
    return floorLayerToGameObjectLayer[floorLayer];
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
