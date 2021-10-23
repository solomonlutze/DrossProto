using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallObject : MonoBehaviour
{
  public Sprite wallSprite;
  public GameObject[] wallPieces;
  public GameObject wallPieceObject;
  public int numberOfPieces;
  // public float spriteFrequency = 1f / 15f;
  public int orderInLayer;
  public float groundHeight; // floor distance from own layer
  public float ceilingHeight; // ceiling distance from above layer
  bool forCeiling;
  public Collider2D wallCollider;

  // wall objects are either "for ceiling" (object-layer, extends from ceiling down towards floor)
  // or "for floor" (floor layer, extends from floor up towards ceiling)
  public void Init(TileLocation location, EnvironmentTile tile, float height, bool ceiling = true)
  {
    if (wallPieces == null || wallPieces.Length == 0)
    {
      wallPieces = new GameObject[numberOfPieces];
      Debug.Log("new wallpieces!");
    }
    Debug.Log("wallpieces length " + wallPieces.Length);
    wallSprite = tile.sprite;
    forCeiling = ceiling;
    // if (wallPieces != null)
    // {
    //   foreach (GameObject piece in wallPieces)
    //   {
    //     DestroyImmediate(piece);
    //   }
    // }
    string sortingLayer = location.floorLayer.ToString();
    SpriteRenderer sr;
    Vector3 locScale;
    float progress = 0;
    float prevHeight = groundHeight;
    groundHeight = height;
    float targetHeight = Mathf.Max(height, prevHeight);
    // height = ceiling ? 1 : GridManager.Instance.worldGridData.GetFloorHeight(location);
    for (int i = 0; i <= targetHeight * numberOfPieces; i++)
    {
      progress += (1f / numberOfPieces);
      if (wallPieces[i] != null)
      {
        DestroyImmediate(wallPieces[i]);
      }
      if (progress < groundHeight)
      {
        wallPieces[i] = Instantiate(wallPieceObject, transform.position, Quaternion.identity);
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
    }
    WorldObject.ChangeLayersRecursively(transform, location.floorLayer);
  }

  void OnTriggerStay2D(Collider2D col)
  {
    // remember: "up" is a _negative_ z value, that's why this math is fucky!
    // e.g. if the floor is at z = 7, and the floor height is .4, then collision occurs between 7 and 6.6.
    float offset = .0001f;
    bool enableCollision = col.transform.position.z > (transform.position.z - groundHeight + offset) && col.transform.position.z <= (transform.position.z);
    Physics2D.IgnoreCollision(col, wallCollider, !enableCollision);
  }
  void OnCollisionStay2D(Collision2D col)
  {
    col.gameObject.SendMessage("OnWallObjectCollisionStay", this, SendMessageOptions.DontRequireReceiver);
  }
}
