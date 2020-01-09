using System;
using System.Collections;
using UnityEngine;

public class Hitbox_OLD : MonoBehaviour
{

  //TODO: Size things
  // - should be possible to set the shape: square, circle, or hitboxSpawn
  // - should be possible to set radius, xy, or neither (for hitboxSpawn)

  //how long this hitbox should last for
  public float duration;

  // what weapon does this hitbox belong to
  public Transform owningTransform;
  // damage we will apply on a hit
  private Damage_OLD damage;
  private CharacterAttackModifiers attackModifiers;
  public FloorLayer floor;

  // Use this for initialization
  void Start()
  {
  }
  // Characteristics of hitbox initialized by weapon.
  public void Init(Transform initializingTransform, Character owner, HitboxData info, CharacterAttackModifiers mods, float attackRange)
  {
    duration = info.duration;
    damage = new Damage_OLD(info.damageInfo, mods);
    // WARNING: duration of 0 means a hitbox needs to be cleaned up manually
    if (duration > 0)
    {
      StartCoroutine(CleanUpSelf());
    }
    attackModifiers = mods;
    owningTransform = initializingTransform;

    Debug.Log("attackRange: " + attackRange);
    switch (info.hitboxShape)
    {
      case HitboxShape.Box:
        transform.localScale = new Vector2(
            attackRange,
            info.size.y + Character.GetAttackValueModifier(attackModifiers.attackValueModifiers, CharacterAttackValue.HitboxSize)
        );
        break;
      case HitboxShape.Circle:
        Destroy(GetComponent<BoxCollider2D>());
        CircleCollider2D circleCol = gameObject.AddComponent<CircleCollider2D>();
        circleCol.radius = info.radius;
        break;
      case HitboxShape.Spawn:
        break;
    }
    if (info.followInitializingTransform && initializingTransform != null)
    {
      transform.parent = initializingTransform;
    }
    if (info.hitboxShape == HitboxShape.Spawn)
    {
      transform.localScale = Vector2.one;
    }
    floor = owner.currentFloor;
    WorldObject.ChangeLayersRecursively(transform, owner.currentFloor);
  }

  // Characteristics of hitbox initialized by weapon.

  // Destroys this hitbox. Waits until end of frame so it doesn't get cleaned up same frame; not sure
  // if that's necessary or not
  IEnumerator CleanUpSelf()
  {
    yield return new WaitForEndOfFrame();
    yield return new WaitForSeconds(duration);
    Destroy(gameObject);
  }

  void Update()
  {
    HandleTile();
  }

  void HandleTile()
  {
    // TODO: this should work for other collider shapes, not just boxes.
    // It should also work for bigger box colliders, which this doesn't.
    BoxCollider2D b = GetComponent<BoxCollider2D>();
    if (b == null) { return; }
    Vector3[] corners = {
            transform.TransformPoint(b.offset + new Vector2(b.size.x, b.size.y)*0.5f),
            transform.TransformPoint(b.offset + new Vector2(-b.size.x, b.size.y)*0.5f),
            transform.TransformPoint(b.offset + new Vector2(b.size.x, -b.size.y)*0.5f),
            transform.TransformPoint(b.offset + new Vector2(-b.size.x, -b.size.y)*0.5f)
        };
    foreach (Vector3 cornerPosition in corners)
    {
      EnvironmentTileInfo tile = GridManager.Instance.GetTileAtLocation(
          new TileLocation(
              new Vector2Int(Mathf.FloorToInt(cornerPosition.x),
              Mathf.FloorToInt(cornerPosition.y)),
              floor
          )
      );
      tile.TakeDamage(damage);
    }
  }

  // Anything we hit, try to damage! This can be applied to destructible environments later too.
  void OnTriggerStay2D(Collider2D col)
  {
    Debug.Log("hitbox collided with " + col.gameObject);
    col.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
  }
}
