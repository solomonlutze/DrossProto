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



  public void Init(TileLocation location, EnvironmentTile objectTile)
  {
    wallSprite = objectTile.sprite;
    wallPieces = new List<GameObject>();
    string sortingLayer = location.floorLayer.ToString();
    SpriteRenderer sr;
    Vector3 locScale;
    float progress;
    for (int i = 0; i < numberOfPieces; i++)
    {
      progress = i * 1.0f / numberOfPieces;
      wallPieces.Add(Instantiate(wallPieceObject, transform.position, Quaternion.identity));
      sr = wallPieces[i].GetComponent<SpriteRenderer>();
      sr.sprite = wallSprite;
      sr.sortingOrder = orderInLayer;
      locScale = wallPieces[i].transform.localScale;
      if (objectTile.wallSizeCurve.length > 0)
      {
        wallPieces[i].transform.localScale = locScale * objectTile.wallSizeCurve.Evaluate(progress);
      }
      wallPieces[i].transform.parent = transform;
      wallPieces[i].transform.localPosition = new Vector3(0, 0, -progress);
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
