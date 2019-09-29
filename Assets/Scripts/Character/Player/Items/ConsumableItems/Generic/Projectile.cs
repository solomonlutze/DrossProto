using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : CustomPhysicsController
{

  public Character owner;
  public float maxRange; // how far can it travel?
  public Vector3 heading; // what direction is it heading?
  public float travelSpeed; // how fast does it travel?
  public HitboxData hitboxData; // what damage does it deal on impact?
  public GameObject impactEffect; // what thing does it create on impact?

  public void Init(Character o)
  {
    owner = o;
    heading = Camera.main.ScreenToWorldPoint(Input.mousePosition) - o.transform.position;
    heading = new Vector3(heading.x, heading.y, 0);
    ApplyImpulseForce(heading.normalized * travelSpeed);
  }

  // TODO: If we want projectile items we need to come up with mods for them.
  public void OnCollisionEnter2D(Collision2D col)
  {
    // Instantiate(impactEffect, transform.position, transform.rotation).GetComponent<Hitbox>().Init(null, owner, hitboxData);
    Destroy(gameObject);
  }
}
