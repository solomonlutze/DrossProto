using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
	Behavior:
	-Swarmling hovers around a target.
	-Pick a spot at random around our target and move towards it.
	-Once (????), pick a new spot and move towards that.
	-Variables include:
		target
		speed
		max velocity change
		randomness (orbit distance from center)
	Separate behaviors should influence setting target, etc.


 */
public class Swarmling : MonoBehaviour
{

  // Thing we're following
  public Transform target;

  public float acceleration = 1;

  public float randomness;

  // target offset will be recalculated at random between these two numbers
  public float minTimeForTargetCalc;
  public float maxTimeForTargetCalc;

  // what we're actually moving towards, as an offset from the target
  private Vector3 targetOffset;
  private CustomPhysicsController po;
  public GameObject tilemapGameObject;

  Tilemap tilemap;


  // Use this for initialization
  void Start()
  {
    po = GetComponent<CustomPhysicsController>();
    // po.TRASHY_INIT(acceleration);
    if (tilemapGameObject != null)
    {
      tilemap = tilemapGameObject.GetComponent<Tilemap>();
    }
    StartCoroutine(PickNewTarget());
  }

  // Update is called once per frame
  void Update()
  {
    Vector3 dest = target.position + targetOffset;
    Vector3 force = (dest - transform.position).normalized;
    po.SetMovementInput(new Vector2(force.x, force.y));
  }

  IEnumerator PickNewTarget()
  {
    while (true)
    {
      targetOffset = new Vector3(Random.Range(-1 * randomness, randomness), Random.Range(-1 * randomness, randomness), 0);
      yield return new WaitForSeconds(Random.Range(minTimeForTargetCalc, maxTimeForTargetCalc));
    }
  }

  // TODO: Replace this with our custom physics
  void OnCollisionEnter2D(Collision2D collision)
  {
    Vector3 hitPosition = Vector3.zero;
    Debug.Log("onCol enter");
    if (tilemap != null && tilemapGameObject == collision.gameObject)
    {
      foreach (ContactPoint2D hit in collision.contacts)
      {
        hitPosition.x = hit.point.x - 0.01f * hit.normal.x;
        hitPosition.y = hit.point.y - 0.01f * hit.normal.y;
        Debug.Log("destroying...");
        TileInteractionHandler ih = tilemap.GetComponent<TileInteractionHandler>();
        ih.DamageDirtTile(hitPosition);
        // TestTile testTile = tilemap.GetTile(tilemap.WorldToCell(hitPosition)) as TestTile;
        // if (testTile != null) {
        // 	// testTile.Boop();
        // 	// tilemap.RefreshTile(tilemap.WorldToCell(hitPosition));
        // }
      }
    }
  }
}
