using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallObject : MonoBehaviour
{
  public Sprite wallSprite;
  public List<GameObject> wallPieces;
  public GameObject wallPieceObject;
  public int numberOfPieces;
  public int orderInLayer;

  // void Start()
  // {
  //   wallPieces = new List<GameObject>();
  //   string sortingLayer = LayerMask.LayerToName(gameObject.layer);
  //   for (int i = 0; i < numberOfPieces; i++)
  //   {
  //     wallPieces.Add(Instantiate(wallPieceObject, transform.position, Quaternion.identity));
  //     SpriteRenderer sr = wallPieces[i].GetComponent<SpriteRenderer>();
  //     sr.sprite = wallSprite;
  //     sr.sortingLayerName = sortingLayer;
  //     sr.sortingOrder = 20;
  //     wallPieces[i].transform.position -= new Vector3(0, 0, i * 1.0f / numberOfPieces);
  //   }
  // }

  public void Init(TileLocation location, Sprite sprite)
  {
    wallSprite = sprite;
    wallPieces = new List<GameObject>();
    string sortingLayer = location.floorLayer.ToString();
    for (int i = 0; i < numberOfPieces; i++)
    {
      wallPieces.Add(Instantiate(wallPieceObject, transform.position, Quaternion.identity));
      SpriteRenderer sr = wallPieces[i].GetComponent<SpriteRenderer>();
      sr.sprite = wallSprite;
      sr.sortingOrder = orderInLayer;
      wallPieces[i].transform.parent = transform;
      wallPieces[i].transform.localPosition = new Vector3(0, 0, -i * 1.0f / numberOfPieces);
    }
    WorldObject.ChangeLayersRecursively(transform, location.floorLayer);
  }

  public void ChangeColor(Color newColor)
  {
    for (int i = 0; i < numberOfPieces; i++)
    {
      wallPieces[i].GetComponent<SpriteRenderer>().color = newColor;
    }
  }
}
