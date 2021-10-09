
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

[SelectionBase]
[ExecuteAlways]
public class LayerFloor : MonoBehaviour
{
  public Tilemap groundTilemap;
  public Tilemap objectTilemap;
  public Tilemap visibilityTilemap;
  public Tilemap waterTilemap; // Transient, only exists during play
  public Tilemap infoTilemap;
  public Transform interestObjects;
  public void Validate()
  {
    if (!Application.IsPlaying(gameObject))
    {
      if (transform.position.z != GridManager.GetZOffsetForGameObjectLayer(gameObject.layer))
      {
        transform.position = new Vector3(0, 0, GridManager.GetZOffsetForGameObjectLayer(gameObject.layer));
      }
      if (gameObject.name == "LayerFloor") { return; }
      gameObject.layer = LayerMask.NameToLayer(gameObject.name);
      if (groundTilemap == null)
      {
        if (transform.Find(gameObject.name + "_Ground") != null)
        {
          groundTilemap = transform.Find(gameObject.name + "_Ground").GetComponent<Tilemap>();
        }
      }
      else
      {
        groundTilemap.gameObject.layer = LayerMask.NameToLayer(gameObject.name);
        groundTilemap.gameObject.name = gameObject.name + "_Ground";
        groundTilemap.GetComponent<TilemapRenderer>().sortingLayerName = gameObject.name;
      }
      if (objectTilemap == null)
      {
        if (transform.Find(gameObject.name + "_Object") != null)
        {
          objectTilemap = transform.Find(gameObject.name + "_Object").GetComponent<Tilemap>();
        }
      }
      else
      {
        objectTilemap.gameObject.layer = LayerMask.NameToLayer(gameObject.name);
        objectTilemap.gameObject.name = gameObject.name + "_Object";
        objectTilemap.GetComponent<TilemapRenderer>().sortingLayerName = gameObject.name;
        objectTilemap.GetComponent<TilemapRenderer>().sortingOrder = 1;
      }
      if (visibilityTilemap == null)
      {
        if (transform.Find(gameObject.name + "_Visibility") != null)
        {
          objectTilemap.gameObject.layer = LayerMask.NameToLayer(gameObject.name);
          visibilityTilemap = transform.Find(gameObject.name + "_Visibility").GetComponent<Tilemap>();
        }
      }
      else
      {
        visibilityTilemap.gameObject.layer = LayerMask.NameToLayer(gameObject.name);
        visibilityTilemap.gameObject.name = gameObject.name + "_Visibility";
        if (visibilityTilemap.GetComponent<TilemapRenderer>() != null)
        {
          visibilityTilemap.GetComponent<TilemapRenderer>().sortingLayerName = gameObject.name;
          visibilityTilemap.GetComponent<TilemapRenderer>().sortingOrder = 100;
        }
      }
      if (infoTilemap == null)
      {
        if (transform.Find(gameObject.name + "_Info") != null)
        {
          infoTilemap = transform.Find(gameObject.name + "_Info").GetComponent<Tilemap>();
          infoTilemap.gameObject.layer = LayerMask.NameToLayer(gameObject.name);
        }
      }
      else
      {
        infoTilemap.gameObject.layer = LayerMask.NameToLayer(gameObject.name);
        infoTilemap.gameObject.name = gameObject.name + "_Info";
        infoTilemap.GetComponent<TilemapRenderer>().sortingLayerName = gameObject.name;
        infoTilemap.GetComponent<TilemapRenderer>().sortingOrder = 1;
      }
    }
  }
#if UNITY_EDITOR
  [MenuItem("CustomTools/ValidateLayerFloors")]
  public static void ValidateLayerFloors()
  {

    foreach (LayerFloor layerFloor in GridManager.Instance.layerFloors.Values)
    {
      layerFloor.Validate();
      // if (layerFloor.infoTilemap != null && layerFloor.infoTilemap.GetComponent<TilemapRenderer>() != null)
      // {
      //   layerFloor.infoTilemap.gameObject.SetActive(!layerFloor.infoTilemap.gameObject.activeSelf);
      // }
    }
  }
#endif
}