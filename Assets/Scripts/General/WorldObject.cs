using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WorldObject : MonoBehaviour
{

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

  void Awake()
  {
    transform.position = new Vector3(transform.position.x, transform.position.y, GridManager.GetZOffsetForGameObjectLayer(gameObject.layer));
  }
  public Collider2D col
  {
    get
    {
      return GetComponentInChildren<Collider2D>();
    }
  }

  private FloorLayer _prevFloor;
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

  public static void ChangeLayersRecursively(Transform t, int gameObjectLayer)
  {
    // if (t.position.z != GridManager.GetZOffsetForFloor(GetGameObjectLayerFromFloorLayer(layerName)))
    // {
    //   t.position = new Vector3(t.position.x, t.position.y, GridManager.GetZOffsetForFloor(GetGameObjectLayerFromFloorLayer(layerName)));
    // }
    ChangeLayersRecursively(t, (FloorLayer)Enum.Parse(typeof(FloorLayer), LayerMask.LayerToName(gameObjectLayer)));
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
    SpriteRenderer sr = trans.gameObject.GetComponent<SpriteRenderer>();
    if (sr != null)
    {
      sr.sortingLayerName = layerName;
    }
    Renderer r = trans.gameObject.GetComponent<Renderer>();
    if (r != null)
    {
      r.sortingLayerName = layerName;
      r.sortingOrder = 20;
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
    if (_prevFloor != currentFloor)
    {
      _prevFloor = currentFloor;
      ChangeLayersRecursively(transform, currentFloor);
    }
    transform.position = new Vector3(transform.position.x, transform.position.y, GridManager.GetZOffsetForGameObjectLayer(gameObject.layer));
    // int targetLayer = gameObject.layer;
    // if (transform.parent != null && LayerMask.LayerToName(transform.parent.gameObject.layer) != "Default")
    // {
    //   targetLayer = transform.parent.gameObject.layer;
    //   Debug.Log("target layer for " + gameObject + "should be " + LayerMask.LayerToName(targetLayer));
    // }
    // if (targetLayer != prevLayer)
    // {
    //   Debug.Log("changing target layer for " + gameObject + "to " + LayerMask.LayerToName(targetLayer));
    //   prevLayer = targetLayer;
    //   ChangeLayersRecursively(transform, LayerMask.LayerToName(targetLayer));
    // }
    transform.position = new Vector3(transform.position.x, transform.position.y, GridManager.GetZOffsetForGameObjectLayer(gameObject.layer));
  }
  [MenuItem("CustomTools/ChangeLayersRecursively")]
  public static void ChangeLayersRecursivelyForSelected()
  {

    GameObject selectedObject = Selection.objects.Length > 0 ? Selection.objects[0] as GameObject : null;
    if (selectedObject != null)
    {
      WorldObject.ChangeLayersRecursively(selectedObject.transform, selectedObject.layer);
    }
  }
#endif

}
