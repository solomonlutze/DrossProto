using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallObject : MonoBehaviour
{
  public Sprite wallSprite;
  public List<GameObject> wallPieces;
  public GameObject wallPieceObject;
  public int numberOfPieces;
  public float spriteFrequency = 1f / 15f;
  public int orderInLayer;
  public float height;
  bool forCeiling;
  public Collider2D wallCollider;

  // wall objects are either "for ceiling" (object-layer, extends from ceiling down towards floor)
  // or "for floor" (floor layer, extends from floor up towards ceiling)
  public void Init(TileLocation location, EnvironmentTile tile, bool ceiling = true)
  {
    wallSprite = tile.sprite;
    forCeiling = ceiling;
    wallPieces = new List<GameObject>();
    string sortingLayer = location.floorLayer.ToString();
    SpriteRenderer sr;
    Vector3 locScale;
    float progress = 0;
    // height = ceiling ? tile.ceilingHeight : tile.groundHeight;
    height = GridManager.Instance.worldGridData.GetFloorHeight(location);
    for (int i = 0; i <= height / spriteFrequency; i++)
    {
      progress += spriteFrequency;
      wallPieces.Add(Instantiate(wallPieceObject, transform.position, Quaternion.identity));
      sr = wallPieces[i].GetComponent<SpriteRenderer>();
      sr.sprite = wallSprite;
      sr.sortingOrder = orderInLayer;
      locScale = wallPieces[i].transform.localScale;
      if (tile.wallSizeCurve.length > 0)
      {
        wallPieces[i].transform.localScale = locScale * tile.wallSizeCurve.Evaluate(progress);
      }
      wallPieces[i].transform.parent = transform;
      wallPieces[i].transform.localPosition = new Vector3(0, 0, ceiling ? progress - 1 : -progress);
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
    float offset = 0001;
    bool enableCollision = col.transform.position.z > (transform.position.z - height + .0001) && col.transform.position.z <= (transform.position.z);
    if (enableCollision)
    {
      Debug.Log("col z: " + col.transform.position.z + ", wall height: " + (transform.position.z - height + offset));
    }
    Physics2D.IgnoreCollision(col, wallCollider, !enableCollision);
  }
}
