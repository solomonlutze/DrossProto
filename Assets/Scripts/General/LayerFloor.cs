
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

[SelectionBase]
[ExecuteAlways]
public class LayerFloor : MonoBehaviour
{
  public Tilemap groundTilemap;
  public Tilemap objectTilemap;
  public Transform interestObjects;
  void Update()
  {
    if (!Application.IsPlaying(gameObject))
    {
      // groundTilemap.GetComponent<TilemapRenderer>().sortingLayerName = "Background";
      // objectTilemap.GetComponent<TilemapRenderer>().sortingLayerName = "Background";
      // groundTilemap.GetComponent<TilemapRenderer>().sortingOrder = 0;
      // objectTilemap.GetComponent<TilemapRenderer>().sortingOrder = 0;
      // objectTilemap.transform.localPosition = new Vector3(0, 0, 0f);
      if (transform.position.z != GridManager.GetZOffsetForFloor(gameObject.layer))
      {
        transform.position = new Vector3(0, 0, GridManager.GetZOffsetForFloor(gameObject.layer));
      }
      if (gameObject.name == "LayerFloor") { return; }
      gameObject.layer = LayerMask.NameToLayer(gameObject.name);
      if (groundTilemap == null && transform.Find(gameObject.name + "_Ground") != null)
      {
        groundTilemap = transform.Find(gameObject.name + "_Ground").GetComponent<Tilemap>();
      }
      else
      {
        groundTilemap.gameObject.layer = LayerMask.NameToLayer(gameObject.name);
        groundTilemap.gameObject.name = gameObject.name + "_Ground";
        groundTilemap.GetComponent<TilemapRenderer>().sortingLayerName = gameObject.name;
      }
      if (objectTilemap == null && transform.Find(gameObject.name + "_Object") != null)
      {
        objectTilemap = transform.Find(gameObject.name + "_Object").GetComponent<Tilemap>();
      }
      else
      {
        objectTilemap.gameObject.layer = LayerMask.NameToLayer(gameObject.name);
        objectTilemap.gameObject.name = gameObject.name + "_Object";
        objectTilemap.GetComponent<TilemapRenderer>().sortingLayerName = gameObject.name;
        objectTilemap.GetComponent<TilemapRenderer>().sortingOrder = 1;
      }
    }
  }
}