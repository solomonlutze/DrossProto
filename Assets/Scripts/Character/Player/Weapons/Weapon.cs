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
  Character owner;
  SkillEffect owningEffect;
  List<Weapon> owningEffectActiveWeapons;
  Attack attack;
  float timeAlive = 0;
  float progress = 0;
  float previousProgress = 0;
  float increment = 0;
  int currentActionGroup = 0;

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
    defaultHitboxes = GetComponentsInChildren<Hitbox>();

    foreach (Hitbox hitbox in defaultHitboxes)
    {
      hitbox.Init(owner, attackSpawnData.damage);
    }
    WorldObject.ChangeLayersRecursively(gameObject.transform, owner.currentFloor);
  }

  void FixedUpdate()
  {
    timeAlive += Time.fixedDeltaTime;
    ExecuteWeaponActions(owner);
    if (timeAlive > attack.duration)
    {
      CleanUp();
    }
  }

  public void CleanUp()
  {
    owningEffectActiveWeapons.Remove(this);
    if (owner != null && attack.objectToSpawn != null && attack.objectToSpawn.attackData != null && attack.spawnObjectOnDestruction)
    {
      owningEffect.SpawnWeapon(attack.objectToSpawn, owner, owningEffectActiveWeapons);
    }
    Destroy(this.gameObject);
  }

  public void ExecuteWeaponActions(Character owner)
  {
    previousProgress = progress;

    float cappedTimeAlive = Mathf.Min(timeAlive, attack.duration);
    progress = cappedTimeAlive / attack.duration; // easing goes here if we have it
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
        MoveWeaponAction(action, owner, increment);
        break;
      case WeaponActionType.RotateRelative:
        RotateWeaponRelativeAction(action, owner, increment);
        break;
      case WeaponActionType.Wait:
        WaitWeaponAction(action, increment);
        break;
      case WeaponActionType.MarkDone:
        MarkDoneWeaponAction(action, increment);
        break;
    }
  }

  public void MoveWeaponAction(WeaponAction action, Character owner, float increment)
  {
    // rigidbody2D.MovePosition(transform.position + new Vector3(action.motion.EvaluateIncrement(owner, progress, previousProgress), 0, 0));
    transform.position += transform.rotation * new Vector3(action.motion.EvaluateIncrement(owner, progress, previousProgress), 0, 0);
  }

  // Rotate weapon relative to owner - think of a sword swing
  public void RotateWeaponRelativeAction(WeaponAction action, Character owner, float increment)
  {
    Vector3 direction = transform.position - owner.transform.position;
    // rigidbody2D.MoveRotation
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
    if (attack.objectToSpawn != null && attack.spawnObjectOnContact)
    {
      Quaternion rotationAngle = Quaternion.AngleAxis(transform.eulerAngles.z + attack.objectToSpawn.rotationOffset, Vector3.forward);
      GameObject go = Instantiate(attack.objectToSpawn.weaponObject.gameObject, transform.position + new Vector3(attack.objectToSpawn.range, 0, 0), rotationAngle);
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
