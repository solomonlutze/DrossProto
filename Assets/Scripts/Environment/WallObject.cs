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
  public float ceilingHeight = 1; // ceiling distance from above layer
  public Collider2D wallCollider;

  // wall objects are either "for ceiling" (object-layer, extends from ceiling down towards floor)
  // or "for floor" (floor layer, extends from floor up towards ceiling)
  public void Init(TileLocation location, EnvironmentTile tile, float height, bool ceiling = true)
  {
    if (wallPieces == null || wallPieces.Length == 0)
    {
      wallPieces = new GameObject[numberOfPieces];
    }
    string sortingLayer = location.floorLayer.ToString();
    SpriteRenderer sr;
    Vector3 locScale;
    Debug.Log("wall pieces count " + wallPieces.Length);
    if (ceiling)
    {
      ceilingTile = tile;
      ceilingHeight = height;
    }
    else
    {
      groundTile = tile;
      groundHeight = height;
    }
    Debug.Log("ceiling height " + ceilingHeight);
    Debug.Log("groundHeight " + groundHeight);
    for (int i = 0; i < numberOfPieces; i++)
    {
      float progress = 1f / numberOfPieces * i;
      if (wallPieces[i] != null)
      {
        DestroyImmediate(wallPieces[i]);
      }
      // ceiling height = .6
      // floor height = .2
      // at progress = .1, place a floor tile
      // at progress = .3, do nothing
      // at progress = .7, place a ceiling tile
      if (progress > 1 - groundHeight && groundTile != null)
      {
        Debug.Log("progress==" + progress + ", create ground tile");
        CreateWallPiece(groundTile, i);
      }
      else if (progress < 1 - ceilingHeight && ceilingTile != null)
      {
        Debug.Log("progress==" + progress + ", create ceiling tile");
        CreateWallPiece(ceilingTile, i);
      }
      else
      {
        Debug.Log("progress==" + progress + ", do nothing");
      }
    }

    // if (ceiling) 
    // {
    //   float prevHeight = ceilingHeight;
    //   ceilingHeight = height;
    //   float startHeight = Mathf.Min(height, prevHeight);

    //   progress = (1f / numberOfPieces) * Mathf.CeilToInt(startHeight * numberOfPieces);
    //   for (int i = Mathf.CeilToInt(startHeight * numberOfPieces); i < numberOfPieces; i++)
    //   {
    //     Debug.Log("placing piece " + i);
    //     if (wallPieces[i] != null)
    //     {
    //       DestroyImmediate(wallPieces[i]);
    //     }
    //     if (progress > ceilingHeight)
    //     {
    //       wallPieces[i] = Instantiate(wallPieceObject, transform.position, Quaternion.identity);
    //       sr = wallPieces[i].GetComponent<SpriteRenderer>();
    //       sr.sprite = wallSprite;
    //       sr.sortingOrder = orderInLayer;
    //       locScale = wallPieces[i].transform.localScale;
    //       if (tile.wallSizeCurve.length > 0)
    //       {
    //         wallPieces[i].transform.localScale = locScale * tile.wallSizeCurve.Evaluate(progress);
    //       }
    //       wallPieces[i].transform.parent = transform;
    //       wallPieces[i].transform.localPosition = new Vector3(0, 0, (1f / numberOfPieces * i) - 1);
    //     }
    //     progress += (1f / numberOfPieces);
    //   }
    // }
    // else
    // {
    //   float prevHeight = groundHeight;
    //   groundHeight = height;
    //   float targetHeight = Mathf.Max(height, prevHeight);
    //   for (int i = 0; i <= targetHeight * numberOfPieces; i++)
    //   {
    //     progress += (1f / numberOfPieces);
    //     if (wallPieces[i] != null)
    //     {
    //       DestroyImmediate(wallPieces[i]);
    //     }
    //     if (progress < groundHeight)
    //     {
    //       wallPieces[i] = Instantiate(wallPieceObject, transform.position, Quaternion.identity);
    //       sr = wallPieces[i].GetComponent<SpriteRenderer>();
    //       sr.sprite = wallSprite;
    //       sr.sortingOrder = orderInLayer;
    //       locScale = wallPieces[i].transform.localScale;
    //       if (tile.wallSizeCurve.length > 0)
    //       {
    //         wallPieces[i].transform.localScale = locScale * tile.wallSizeCurve.Evaluate(progress);
    //       }
    //       wallPieces[i].transform.parent = transform;
    //       wallPieces[i].transform.localPosition = new Vector3(0, 0, (1f / numberOfPieces * i) - 1);
    //     }
    //   }
    // }

    WorldObject.ChangeLayersRecursively(transform, location.floorLayer);
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
    wallPieces[i].transform.localPosition = new Vector3(0, 0, (1f / numberOfPieces * i) - 1);
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
