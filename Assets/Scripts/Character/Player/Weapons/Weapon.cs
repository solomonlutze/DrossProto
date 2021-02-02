using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// in-world physical weapon object. This is what controls weapon movement during attacks.
public class Weapon : MonoBehaviour
{
  [Tooltip("Used to determine weapon range. Use the primary transform of the weapon with the largest X size.")]
  public Transform weaponBody;
  public Hitbox[] defaultHitboxes;
  Character owner;
  AttackSkillEffect owningEffect;
  List<Weapon> owningEffectActiveWeapons;
  Attack attack;
  float timeInCurrentActionGroup = 0;
  float easedProgress = 0;
  float previousEasedProgress = 0;
  float increment = 0;
  int currentActionGroup = 0;

  public void Update()
  {
    if (owner == null)
    {
      CleanUp();
    }
  }
  public void Init(Attack atk, AttackSkillEffect effect, Character c, List<Weapon> activeWeaponObjects)
  {
    attack = atk;
    owner = c;
    owningEffect = effect;
    owningEffectActiveWeapons = activeWeaponObjects;
    owningEffectActiveWeapons.Add(this);
    defaultHitboxes = GetComponentsInChildren<Hitbox>();

    foreach (Hitbox hitbox in defaultHitboxes)
    {
      hitbox.Init(owner, effect.baseDamage);
    }
    WorldObject.ChangeLayersRecursively(gameObject.transform, owner.currentFloor);
  }

  void FixedUpdate()
  {
    if (currentActionGroup < attack.weaponActionGroups.Length)
    {
      timeInCurrentActionGroup += Time.fixedDeltaTime;
      if (timeInCurrentActionGroup >= attack.weaponActionGroups[currentActionGroup].duration)
      {
        ExecuteWeaponActionGroup(
          attack.weaponActionGroups[currentActionGroup].duration, // don't allow overstepping of duration
          attack.weaponActionGroups[currentActionGroup],
          currentActionGroup == attack.weaponActionGroups.Length - 1
        );
        currentActionGroup++;
        timeInCurrentActionGroup = 0;
        easedProgress = 0;
        previousEasedProgress = 0;
      }
      else
      {
        ExecuteWeaponActionGroup(timeInCurrentActionGroup, attack.weaponActionGroups[currentActionGroup], currentActionGroup == attack.weaponActionGroups.Length - 1);
      }
    }
    else
    {
      CleanUp();
    }
  }

  // public IEnumerator PerformWeaponActions()
  // {
  //   for (int i = 0; i < attack.weaponActionGroups.Length; i++)
  //   {
  //     yield return null;
  //     // yield return ExecuteWeaponActionGroup(attack.weaponActionGroups[i], i == attack.weaponActionGroups.Length - 1);
  //   }
  //   CleanUp();
  // }

  public void CleanUp()
  {
    owningEffectActiveWeapons.Remove(this);
    if (owner != null && attack.objectToSpawn != null && attack.objectToSpawn.attackData != null && attack.spawnObjectOnDestruction)
    {
      owner.StartCoroutine(owningEffect.SpawnWeapon(attack.objectToSpawn, owner, owningEffectActiveWeapons));
    }
    Destroy(this.gameObject);
  }

  public void ExecuteWeaponActionGroup(float t, WeaponActionGroup actionGroup, bool isLast)
  {
    previousEasedProgress = easedProgress;
    easedProgress = isLast ? Easing.Quadratic.Out(t / actionGroup.duration) : Easing.Linear(t / actionGroup.duration); // ease out on last group only
    increment = easedProgress - previousEasedProgress;
    foreach (WeaponAction action in actionGroup.weaponActions)
    {
      ExecuteWeaponAction(action, increment);
    }
  }

  // public IEnumerator ExecuteWeaponActionGroup(WeaponActionGroup actionGroup, bool isLast)
  // {
  //   float t = 0;
  //   float easedProgress = 0;
  //   float previousEasedProgress = 0;
  //   float increment = 0;
  //   while (t <= actionGroup.duration)
  //   {
  //     t += Time.deltaTime;
  //     previousEasedProgress = easedProgress;
  //     easedProgress = isLast ? Easing.Quadratic.Out(t / actionGroup.duration) : Easing.Linear(t / actionGroup.duration); // ease out on last group only
  //     increment = easedProgress - previousEasedProgress;
  //     foreach (WeaponAction action in actionGroup.weaponActions)
  //     {
  //       ExecuteWeaponAction(action, increment);
  //     }
  //     yield return null;
  //   }
  // }
  public void ExecuteWeaponAction(WeaponAction action, float increment)
  {
    switch (action.type)
    {
      case WeaponActionType.Move:
        MoveWeaponAction(action, increment);
        break;
      case WeaponActionType.RotateRelative:
        RotateWeaponRelativeAction(action, increment);
        break;
      case WeaponActionType.Wait:
        WaitWeaponAction(action, increment);
        break;
      case WeaponActionType.MarkDone:
        MarkDoneWeaponAction(action, increment);
        break;
    }
  }

  public void MoveWeaponAction(WeaponAction action, float increment)
  {
    transform.position += transform.rotation * new Vector3(action.magnitude * increment, 0, 0);
  }

  // Rotate weapon relative to owner - think of a sword swing
  public void RotateWeaponRelativeAction(WeaponAction action, float increment)
  {
    Vector3 direction = transform.position - owner.transform.position;
    transform.RotateAround(owner.transform.position, Vector3.forward, action.magnitude * increment);
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
        go.GetComponent<Weapon>().Init(attack.objectToSpawn.attackData, owningEffect, owner, owningEffectActiveWeapons);
      }
    }
    if (attack.destroyOnContact)
    {
      CleanUp();
    }
  }
}
