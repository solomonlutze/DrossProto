
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

[ExecuteAlways]
public class LayerFloor : MonoBehaviour {
	public Tilemap groundTilemap;
	public Tilemap objectTilemap;
    void Update()
    {
        if (!Application.IsPlaying(gameObject))
        {
            gameObject.layer = LayerMask.NameToLayer(gameObject.name);
            groundTilemap.gameObject.layer = LayerMask.NameToLayer(gameObject.name);
            objectTilemap.gameObject.layer = LayerMask.NameToLayer(gameObject.name);
            groundTilemap.gameObject.name = gameObject.name+"_Ground";
            groundTilemap.GetComponent<TilemapRenderer>().sortingLayerName = gameObject.name;
            objectTilemap.gameObject.name = gameObject.name+"_Object";
            objectTilemap.GetComponent<TilemapRenderer>().sortingLayerName = gameObject.name;
            objectTilemap.GetComponent<TilemapRenderer>().sortingOrder = 1;
        }
    }
}