
using UnityEngine;
using UnityEngine.Tilemaps;

[SelectionBase]
[ExecuteAlways]
public class LayerFloor : MonoBehaviour
{
  public Tilemap groundTilemap;
  public Tilemap objectTilemap;
  public Tilemap visibilityTilemap;
  public Transform interestObjects;
  void Update()
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
        visibilityTilemap.GetComponent<TilemapRenderer>().sortingLayerName = gameObject.name;
        visibilityTilemap.GetComponent<TilemapRenderer>().sortingOrder = 100;
      }
    }
  }
}