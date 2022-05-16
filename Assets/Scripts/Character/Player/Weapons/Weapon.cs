using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// in-world physical weapon object. This is what controls weapon movement during attacks.
public class Weapon : MonoBehaviour
{
  [Tooltip("Used to determine weapon range. Use the primary transform of the weapon with the largest X size.")]
  public Transform weaponBody;
  public Rigidbody2D rigidbody2D;
  public Hitbox[] defaultHitboxes;
  // public AwarenessTrigger awarenessTrigger;
  Character owner;
  SkillEffect owningEffect;
  List<Weapon> owningEffectActiveWeapons;
  public Attack attack;
  float timeAlive = 0;
  float progress = 0;
  float previousProgress = 0;
  float increment = 0;
  int currentActionGroup = 0;
  public bool attachToOwner;

  public void Update()
  {
    if (owner == null)
    {
      CleanUp();
    }
  }
  public void Init(AttackSpawn attackSpawnData, SkillEffect owningEffect, Character c, List<Weapon> activeWeaponObjects)
  {
    attack = attackSpawnData.attackData;
    owner = c;
    this.owningEffect = owningEffect;
    owningEffectActiveWeapons = activeWeaponObjects;
    owningEffectActiveWeapons.Add(this);
    attachToOwner = attackSpawnData.attachToOwner;
    defaultHitboxes = GetComponentsInChildren<Hitbox>();
    foreach (Hitbox hitbox in defaultHitboxes)
    {
      hitbox.Init(owner, attackSpawnData.damage, this);
    }
    WorldObject.ChangeLayersRecursively(gameObject.transform, owner.currentFloor);
  }

  void FixedUpdate()
  {
    timeAlive += Time.fixedDeltaTime;
    ExecuteWeaponActions(owner);
    if (attack.duration.Resolve(owner) > 0 && timeAlive > attack.duration.Resolve(owner))
    {
      CleanUp();
    }
  }

  public void CleanUp()
  {
    owningEffectActiveWeapons.Remove(this);
    if (owner != null && attack.objectToSpawn != null && attack.objectToSpawn.attackData != null && attack.spawnObjectOnDestruction)
    {
      // attack.objectToSpawn.owningWeaponDataWeapon.name);
      owningEffect.SpawnWeapon(attack.objectToSpawn, owner, owningEffectActiveWeapons, transform);
    }
    Destroy(this.gameObject);
  }

  public void ExecuteWeaponActions(Character owner)
  {
    previousProgress = progress;

    float cappedTimeAlive = Mathf.Min(timeAlive, attack.duration.Resolve(owner));
    progress = cappedTimeAlive / attack.duration.Resolve(owner);
    increment = progress - previousProgress;
    foreach (WeaponAction action in attack.weaponActions)
    {
      ExecuteWeaponAction(action, owner, increment);
    }
  }

  public void ExecuteWeaponAction(WeaponAction action, Character owner, float increment)
  {
    switch (action.type)
    {
      case WeaponActionType.Move:
        MoveWeaponAction(action, owner);
        break;
      case WeaponActionType.RotateRelative:
        RotateWeaponRelativeAction(action, owner, increment);
        break;
      case WeaponActionType.Homing:
        HomingWeaponAction(action, owner, increment);
        break;
      case WeaponActionType.Scale:
        ScaleWeaponAction(action, owner);
        break;
      case WeaponActionType.Wait:
        WaitWeaponAction(action, increment);
        break;
      case WeaponActionType.MarkDone:
        MarkDoneWeaponAction(action, increment);
        break;
    }
  }

  Character GetNearestTarget()
  {
    Collider2D[] hitColliders = new Collider2D[5];
    int numColliders = Physics2D.OverlapCircleNonAlloc(transform.position, attack.homing.homingRange, hitColliders, 1 << LayerMask.NameToLayer("Character"));
    float maxDistance = 10000;
    Character nearestEnemy = null;
    for (int i = 0; i < numColliders; i++)
    {
      // Debug.Log("hit: " + hitColliders[i].gameObject.name);
      Character c = hitColliders[i].GetComponentInParent<Character>();
      if (c == null || owner == null) { continue; }
      float distance = (transform.position - c.transform.position).sqrMagnitude;
      if (c != owner && c.gameObject.layer == owner.gameObject.layer && distance < maxDistance)
      {
        maxDistance = distance;
        nearestEnemy = c;
      }
    }
    // Character homingTarget = awarenessTrigger.NearestCharacter(attack.homing.maxAngleToTarget);
    // if (homingTarget == null) { return; }
    return nearestEnemy;
  }
  public void HomingWeaponAction(WeaponAction action, Character owner, float increment)
  {
    Character nearestTarget = GetNearestTarget();
    if (nearestTarget == null) { return; }
    Vector3 targetPosition = nearestTarget.transform.position;
    if ((targetPosition - transform.position).sqrMagnitude > attack.homing.homingRange * attack.homing.homingRange)
    {
      return;
    }
    Quaternion targetDirection = Utils.GetDirectionAngle(targetPosition - transform.position);
    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetDirection, action.motion.EvaluateIncrement(owner, progress, previousProgress));
  }

  public void MoveWeaponAction(WeaponAction action, Character owner)
  {
    transform.position += transform.rotation * new Vector3(action.motion.EvaluateIncrement(owner, progress, previousProgress), 0, 0);
  }

  public void ScaleWeaponAction(WeaponAction action, Character owner)
  {
    float increment = action.motion.EvaluateIncrement(owner, progress, previousProgress);
    transform.localScale += new Vector3(increment, increment, 0);
  }

  // Rotate weapon relative to owner - think of a sword swing
  public void RotateWeaponRelativeAction(WeaponAction action, Character owner, float increment)
  {
    Vector3 direction = transform.position - owner.transform.position;
    transform.RotateAround(owner.transform.position, Vector3.forward, action.motion.EvaluateIncrement(owner, progress, previousProgress));
  }
  public void WaitWeaponAction(WeaponAction action, float increment)
  {
    // do nothing
  }

  public void MarkDoneWeaponAction(WeaponAction action, float increment)
  {
    owningEffectActiveWeapons.Remove(this); // stops this weapon from marking the attack complete
  }

  public void OnContact(Collider2D col)
  {
    Character colCharacter = col.GetComponentInParent<Character>();
    if (!colCharacter || owner == colCharacter) { return; }
    if (attack.objectToSpawn != null && attack.spawnObjectOnContact)
    {
      Quaternion rotationAngle = Quaternion.AngleAxis(transform.eulerAngles.z + attack.objectToSpawn.rotationOffset.get(owner), Vector3.forward);
      GameObject go = Instantiate(attack.objectToSpawn.weaponObject.gameObject, transform.position + new Vector3(attack.objectToSpawn.range.get(owner), 0, 0), rotationAngle);
      Weapon weapon = go.GetComponent<Weapon>();
      if (weapon != null)
      {
        go.GetComponent<Weapon>().Init(attack.objectToSpawn, owningEffect, owner, owningEffectActiveWeapons);
      }
    }
    if (attack.destroyOnContact)
    {
      CleanUp();
    }
  }
}
