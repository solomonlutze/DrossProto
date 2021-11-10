using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallObject : MonoBehaviour, IPoolable
{
  public EnvironmentTile groundTile;
  public EnvironmentTile ceilingTile;
  public GameObject[] wallPieces;
  public GameObject wallPieceObject;
  public int numberOfPieces;
  public int orderInLayer;
  public float groundHeight; // floor distance from own layer
  public float ceilingHeight = 0; // ceiling distance from above layer
  public FloorLayer floorLayer;
  public Collider2D wallCollider;


  // wall objects are built 1 of 2 ways:
  // as part of the terrain painting process,
  // or during a total rebuild
  public void Init(TileLocation location)
  {
    if (wallPieces == null || wallPieces.Length == 0)
    {
      wallPieces = new GameObject[numberOfPieces];
      for (int i = 0; i < numberOfPieces; i++)
      {
        wallPieces[i] = Instantiate(wallPieceObject, transform.position, Quaternion.identity);
        wallPieces[i].transform.parent = transform;
        wallPieces[i].transform.localPosition = new Vector3(0, 0, (1f / numberOfPieces * (i + 1)) - 1);
      }
    }
    floorLayer = location.floorLayer;
    string sortingLayer = floorLayer.ToString();
    for (int i = 0; i < numberOfPieces; i++)
    {
      float progress = 1f / numberOfPieces * (i + 1);
      if (wallPieces[i] != null)
      {
        wallPieces[i].SetActive(false);
      }
      if (progress > 1 - groundHeight && groundTile != null)
      {
        CreateWallPiece(groundTile, i);
      }
      else if (progress < 1 - ceilingHeight && ceilingTile != null)
      {
        CreateWallPiece(ceilingTile, i);
      }
    }
    WorldObject.ChangeLayersRecursively(transform, floorLayer);
  }

  public void Clear()
  {
    groundTile = null;
    ceilingTile = null;
    groundHeight = 0;
    ceilingHeight = 0;
  }
  public void SetCeilingInfo(EnvironmentTile tile, float height)
  {
    ceilingTile = tile;
    ceilingHeight = height;
  }


  public void SetGroundInfo(EnvironmentTile tile, float height)
  {
    groundTile = tile;
    groundHeight = height;
  }

  void CreateWallPiece(EnvironmentTile tile, int i)
  {
    GameObject wallPiece = wallPieces[i];
    wallPiece.SetActive(true);
    SpriteRenderer sr = wallPiece.GetComponent<SpriteRenderer>();
    sr.sprite = tile.sprite;
    sr.sortingOrder = orderInLayer;
    Vector3 locScale = Vector3.one;
    if (tile.wallSizeCurve != null && tile.wallSizeCurve.length > 0)
    {
      locScale = locScale * tile.wallSizeCurve.Evaluate(1f / numberOfPieces * i);
    }
    wallPiece.transform.localScale = locScale;
  }

  public void UpdateCollisions(Collider2D col)
  {
    // remember: "up" is a _negative_ z value, that's why this math is fucky!
    // e.g. if the floor is at z = 7, and the floor height is .4, then collision occurs between 7 and 6.6
    bool enableCollision = ShouldHaveCollisionWith(col.transform.position.z);
    Physics2D.IgnoreCollision(col, wallCollider, !enableCollision);
  }

  public bool ShouldHaveCollisionWith(float otherZ)
  {
    float offset = -.001f;
    return GroundHasCollisionWith(otherZ, offset) || CeilingHasCollisionWith(otherZ, offset);
  }

  public bool GroundHasCollisionWith(float otherZ, float offset = 0)
  {
    return otherZ <= transform.position.z // between bottom of tile area...
      && otherZ > (transform.position.z - groundHeight - offset); //...and top of ground
  }

  public bool CeilingHasCollisionWith(float otherZ, float offset = 0)
  {
    return ceilingTile != null && // ceiling tile exists, and we're ...
      (otherZ <= (transform.position.z - ceilingHeight - offset)//...between bottom of ceiling...
      && otherZ >= (transform.position.z - 1)); // and top of tile area
  }
  void OnCollisionStay2D(Collision2D col)
  {
    col.gameObject.SendMessage("OnWallObjectCollisionStay", transform.position, SendMessageOptions.DontRequireReceiver);
  }
}
