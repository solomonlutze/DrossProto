using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CustomPhysicsController : MonoBehaviour
{

  private Rigidbody2D rb;
  protected ContactFilter2D contactFilter;

  // normalized axis inputs from player controller or AI or whatever
  protected Vector2 movementInput;

  // non-normalized inputs from animation.
  protected Vector2 animationInput;
  // actual, calculated velocity. accounts for collisions.
  public Vector2 velocity;

  // the max number of units the object can move each second
  public float maxVelocity;

  // acceleration applied per frame of input. Defined by controller object, if attached.
  private float moveAcceleration;

  private Character owningCharacter;

  private Transform orientation;

  // Use to allow objects to have OnCollisionEnter events without stopping motion
  public bool ignoreCollisionPhysics;
  // the amount to decrease speed by per second (0 is none; 1 is 100%).
  // if >1, reduces us to 0 in less than a second (e.g. 2 stops you in 1/2 a second)
  public float drag;

  // if velocity is less than this number, set it to 0.
  public float velocityMin = .001f;

  public float skinWidth = .01f;

  private FloorLayer currentFloor;
  // Use this for initialization
  void Awake()
  {
    velocity = new Vector2(0, 0);
    contactFilter.useTriggers = false;
    contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
    if (gameObject.GetComponent<Character>() != null)
    {
      currentFloor = gameObject.GetComponent<Character>().currentFloor;
    }
    contactFilter.useLayerMask = true;
    rb = GetComponent<Rigidbody2D>();

  }

  void Start()
  {
    owningCharacter = GetComponent<Character>();
    if (owningCharacter != null)
    {
      // moveAcceleration = owningCharacter.GetStat(CharacterStat.MoveAcceleration);
      orientation = owningCharacter.orientation;
    }
  }

  void Update()
  {
    if (owningCharacter != null)
    {
      if (owningCharacter.dashing)
      {
        moveAcceleration = owningCharacter.GetStat(CharacterStat.DashAcceleration);
      }
      else
      {
        moveAcceleration = owningCharacter.GetStat(CharacterStat.MoveAcceleration);
      }
    }
  }
  public void OnLayerChange()
  {
    contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
    if (gameObject.GetComponent<Character>() != null)
    {
      currentFloor = gameObject.GetComponent<Character>().currentFloor;
    }
  }

  // TODO: if we use this for other projects, we should implement CalculateMovementWithGravity
  void FixedUpdate()
  {
    CalculateMovementTopDown(); // Y is a regular axis input; there is no gravity
                                // CalculateMovementWithGravity(); // Y input is for jumping; gravity applies.
  }

  // Called by character class to indicate desired movement.
  public void SetMovementInput(Vector2 newMovementInput)
  {
    movementInput = newMovementInput;
  }

  // WARNING: Adjusting velocity should always be done by applying force.
  // This function is only for forcing velocity to zero for particular effects
  // such as respawn. Use sparinigly!
  public void HardSetVelocityToZero()
  {
    velocity = Vector2.zero;
  }

  // Called by animation to indicate desired animation movement.
  // Applied exactly the same as movement input.
  public void SetAnimationInput(Vector2 newAnimationInput)
  {
    animationInput = newAnimationInput;
  }

  //POSSIBLY DEPRECATED
  // TODO: This is ugly bc AnimationEvents don't let you pass in 2 params, or a Vector2.
  // Look into adding an AnimationMovement object we can pass in, instead
  // of having two different functions.

  public void SetAnimationInputX(float newAnimationInputX)
  {
    animationInput = new Vector2(newAnimationInputX, animationInput.y);
  }
  public void SetAnimationInputY(float newAnimationInputY)
  {
    animationInput = new Vector2(animationInput.x, newAnimationInputY);
  }
  //END POSSIBLY DEPRECATED

  // how physics works:

  // calculate desired change to velocity this frame based on input, move acceleration, and drag
  // determine collision along each axis individually;
  // for each of x/y:
  //   - attempt to apply movement in that direction
  //   - if we would hit something, reduce our movement in that direction to hitDistance - skinwidth
  //   - move us distance amount in that direction

  // TODO: DRY
  // TODO: drag seems a little fishy here? it works, but should it be included in desiredMovement?
  // may be worth separating it out for clarity.

  void CalculateMovementTopDown()
  {
    Vector2 orientedAnimationInput = Vector2.zero;
    if (orientation != null) { orientedAnimationInput = orientation.rotation * animationInput; }
    maxVelocity = moveAcceleration - drag * Time.deltaTime;
    Vector2 desiredMovement = ((movementInput.normalized * moveAcceleration + orientedAnimationInput) - (drag * velocity)) * Time.deltaTime;
    Vector2 xMove = new Vector2(velocity.x + desiredMovement.x, 0);
    if (!ignoreCollisionPhysics)
    {
      xMove = CalculateCollisionForAxis(xMove);
    }
    velocity.x = xMove.x;
    Vector2 yMove = new Vector2(0, velocity.y + desiredMovement.y);
    if (!ignoreCollisionPhysics)
    {
      yMove = CalculateCollisionForAxis(yMove);
    }
    velocity.y = yMove.y;
    if (velocity.magnitude < velocityMin) { velocity = Vector2.zero; }
    transform.position += new Vector3(velocity.x, velocity.y, 0);
  }

  // Returns the distance we're allowed to move along an axis, x or y.
  // Tries to move us as far as we want to move, but if a collision would happen,
  // we reduce our movement in that direction to hitDistance - skinwidth
  Vector2 CalculateCollisionForAxis(Vector2 movement)
  {
    Vector2 originalMovement = movement;
    RaycastHit2D[] results;
    int hits = GetHitsFor2DCast(movement, new List<GameObject>() { }, out results);
    for (int i = 0; i < hits; i++)
    {
      RaycastHit2D hit = results[i];
      movement = movement.normalized * (hit.distance - skinWidth);
    }
    return movement;
  }

  public bool GetPathOpen(Vector2 castVector, List<GameObject> objectsToIgnore)
  {
    RaycastHit2D[] results;
    int hits = GetHitsFor2DCast(castVector, objectsToIgnore, out results);
    return hits <= 0;
  }
  // Below's a hack; rigidbody.cast also casts triggers, which is Bad. This prevents that.
  // This is weird and makes weird assumptions about the number of hits we'll receive on each
  // collider, so we should replace it with rb.cast ASAP.
  public int GetHitsFor2DCast(Vector2 castVector, List<GameObject> objectsToIgnore, out RaycastHit2D[] results)
  {
    results = new RaycastHit2D[20];
    Collider2D[] cols = GetComponentsInChildren<Collider2D>();
    int hits = 0;
    foreach (Collider2D col in cols)
    {
      if (!col.isTrigger)
      {
        RaycastHit2D[] res = new RaycastHit2D[4];
        int myHits = col.Cast(castVector, contactFilter, res, castVector.magnitude, true);
        int nonTriggerHits = 0;
        for (int i = 0; i < 20 - hits && i < myHits; i++)
        {
          CustomPhysicsController otherPhysics = res[i].collider.gameObject.GetComponent<CustomPhysicsController>();
          bool otherIgnoresCollisions = false;
          if (otherPhysics != null)
          {
            otherIgnoresCollisions = otherPhysics.ignoreCollisionPhysics;
            if (objectsToIgnore.Contains(otherPhysics.gameObject)) { continue; }
          }
          if (res[i].collider.GetComponentInChildren<Tilemap>() != null)
          { // TODO: this is... probably not great
            Vector3 hitPos = Vector3.zero;
            hitPos.x = res[i].point.x - 0.01f * res[i].normal.x;
            hitPos.y = res[i].point.y - 0.01f * res[i].normal.y;
            Vector3 offset = Vector3.zero;
            EnvironmentTileInfo tile1;
            EnvironmentTileInfo tile2;
            if (hitPos.x - Mathf.Floor(hitPos.x) <= 0.0001)
            {
              offset.x += .0001f;
            }
            if (hitPos.y - Mathf.Floor(hitPos.y) <= 0.0001)
            {
              offset.y += .0001f;
            }
            tile1 = GridManager.Instance.GetTileAtLocation(new TileLocation(hitPos + offset, currentFloor));
            tile2 = GridManager.Instance.GetTileAtLocation(new TileLocation(hitPos - offset, currentFloor));
            if (owningCharacter != null)
            {
              owningCharacter.HandleTileCollision(tile1);
              if (tile1 != tile2)
              {
                owningCharacter.HandleTileCollision(tile2);
              }
            }
          }
          if (!res[i].collider.isTrigger && !otherIgnoresCollisions && !Physics2D.GetIgnoreCollision(res[i].collider, col))
          {
            results[hits + nonTriggerHits] = res[i];
            nonTriggerHits++;
          }
        }
        hits += nonTriggerHits;
      }
    }
    return hits;
  }

  // adds a one-time force to our velocity. Used for e.g. weapon knockback.
  public void ApplyImpulseForce(Vector3 impulse)
  {
    velocity += new Vector2(impulse.x, impulse.y);
  }

  public static float GetMinimumDistanceBetweenObjects(GameObject a, GameObject b)
  {
    if (a.GetComponentInChildren<Collider2D>() == null || b.GetComponentInChildren<Collider2D>() == null)
    {
      // Debug.LogWarning("tried to find min distance between " + a.name + " and " + b.name + ", but both do not have colliders");
      return 0;
    }
    return a.GetComponentInChildren<Collider2D>().Distance(b.GetComponentInChildren<Collider2D>()).distance;
  }
}
