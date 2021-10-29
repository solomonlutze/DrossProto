using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallObject : MonoBehaviour
{
  public EnvironmentTile groundTile;
  public EnvironmentTile ceilingTile;
  public GameObject[] wallPieces;
  public GameObject wallPieceObject;
  public int numberOfPieces;
  // public float spriteFrequency = 1f / 15f;
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
    }
    floorLayer = location.floorLayer;
    string sortingLayer = floorLayer.ToString();
    for (int i = 0; i < numberOfPieces; i++)
    {
      float progress = 1f / numberOfPieces * (i + 1);
      if (wallPieces[i] != null)
      {
        DestroyImmediate(wallPieces[i]);
      }
      if (progress > 1 - groundHeight && groundTile != null)
      {
        // Debug.Log("progress==" + progress + ", create ground tile");
        CreateWallPiece(groundTile, i);
      }
      else if (progress < 1 - ceilingHeight && ceilingTile != null)
      {
        // Debug.Log("progress==" + progress + ", create ceiling tile");
        CreateWallPiece(ceilingTile, i);
      }
      else
      {
        // Debug.Log("progress==" + progress + ", do nothing");
      }
    }
    WorldObject.ChangeLayersRecursively(transform, floorLayer);
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
    wallPieces[i] = Instantiate(wallPieceObject, transform.position, Quaternion.identity);
    SpriteRenderer sr = wallPieces[i].GetComponent<SpriteRenderer>();
    sr.sprite = tile.sprite;
    sr.sortingOrder = orderInLayer;
    Vector3 locScale = wallPieces[i].transform.localScale;
    if (tile.wallSizeCurve.length > 0)
    {
      wallPieces[i].transform.localScale = locScale * tile.wallSizeCurve.Evaluate(1f / numberOfPieces * i);
    }
    wallPieces[i].transform.parent = transform;
    wallPieces[i].transform.localPosition = new Vector3(0, 0, (1f / numberOfPieces * (i + 1)) - 1);
  }

  void OnTriggerStay2D(Collider2D col)
  {
    // remember: "up" is a _negative_ z value, that's why this math is fucky!
    // e.g. if the floor is at z = 7, and the floor height is .4, then collision occurs between 7 and 6.6.
    float offset = .001f;
    // if (col.GetComponent<Character>())
    // {
    //   Debug.Log("ceiling check! col: " + col.gameObject.name + ", z: " + col.transform.position.z + ", ceiling location " + (transform.position.z - ceilingHeight + offset) + ", top of tile " + (transform.position.z - 1));
    // }

    bool enableCollision =
      (col.transform.position.z <= transform.position.z // between bottom of tile area...
      && col.transform.position.z > (transform.position.z - groundHeight + offset)) //...and top of ground
      || (ceilingTile != null && // or, ceiling tile exists, and we're
      (col.transform.position.z < (transform.position.z - ceilingHeight - offset)//...between bottom of ceiling...
      && col.transform.position.z >= (transform.position.z - 1))); //... and top of tile area
    //  || (col.transform.position.z < (transform.position.z - ceilingHeight + offset) && col.transform.position.z > (transform.position.z - 1))
    Physics2D.IgnoreCollision(col, wallCollider, !enableCollision);
  }

  public bool ShouldHaveCollisionWith(Vector3 other)
  {
    return false;
  }
  void OnCollisionStay2D(Collision2D col)
  {
    col.gameObject.SendMessage("OnWallObjectCollisionStay", transform.position, SendMessageOptions.DontRequireReceiver);
  }
}
