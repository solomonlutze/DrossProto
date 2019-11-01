
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

[ExecuteAlways]
public class LayerFloor : MonoBehaviour
{
  public Tilemap groundTilemap;
  public Tilemap objectTilemap;
  void Update()
  {
    if (!Application.IsPlaying(gameObject))
    {
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