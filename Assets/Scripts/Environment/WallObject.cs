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

  public float floorHeight;
  public Collider2D wallCollider;

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


  void OnTriggerStay2D(Collider2D col)
  {
    // remember: "up" is a _negative_ z value, that's why this math is fucky!
    // e.g. if the floor is at z = 7, and the floor height is .4, then collision occurs between 7 and 6.6.
    bool enableCollision = col.transform.position.z >= (transform.position.z - floorHeight) && col.transform.position.z <= (transform.position.z);
    Debug.Log("stay: " + col.name + ", collision enabled: " + enableCollision);
    Physics2D.IgnoreCollision(col, wallCollider, !enableCollision);
  }
}
