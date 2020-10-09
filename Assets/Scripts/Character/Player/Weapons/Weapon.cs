using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WeaponAction
{
  public WeaponActionType type;
  public float magnitude;
}

[System.Serializable]
public class WeaponActionGroup
{
  public float duration;
  public WeaponAction[] weaponActions;
}

public enum WeaponActionType
{
  Move,
  Wait,
  RotateRelative,
  MarkDone
}

public class Weapon : MonoBehaviour
{
  public Character owner;
  public AttackSkillEffect owningEffect;
  public Hitbox[] hitboxes;
  public WeaponActionGroup[] weaponActionGroups;
  public List<Weapon> owningEffectWeaponInstances;
  public bool destroyOnContact;
  public WeaponSpawn objectToSpawn;
  public bool spawnObjectOnContact;
  public bool spawnObjectOnDestruction;

  public void Init(AttackSkillEffect effect, List<Weapon> weaponInstances)
  {
    owner = effect.owner;
    owningEffect = effect;
    owningEffectWeaponInstances = weaponInstances;
    owningEffectWeaponInstances.Add(this);
    hitboxes = GetComponentsInChildren<Hitbox>();

    foreach (Hitbox hitbox in hitboxes)
    {
      hitbox.Init(effect.owner, effect.baseDamage);
    }
    WorldObject.ChangeLayersRecursively(gameObject.transform, effect.owner.currentFloor);
  }
  public IEnumerator PerformWeaponActions()
  {
    for (int i = 0; i < weaponActionGroups.Length; i++)
    {
      yield return ExecuteWeaponActionGroup(weaponActionGroups[i], i == weaponActionGroups.Length - 1);
    }
    CleanUp();
  }

  public void CleanUp()
  {
    owningEffectWeaponInstances.Remove(this);
    if (objectToSpawn != null && objectToSpawn.weaponPrefab != null && spawnObjectOnDestruction)
    {
      owner.StartCoroutine(owningEffect.SpawnWeapon(objectToSpawn, transform.position, transform.eulerAngles));
    }
    Destroy(this.gameObject);
  }

  public IEnumerator ExecuteWeaponActionGroup(WeaponActionGroup actionGroup, bool isLast)
  {
    float t = 0;
    float easedProgress = 0;
    float previousEasedProgress = 0;
    float increment = 0;
    while (t <= actionGroup.duration)
    {
      t += Time.deltaTime;
      previousEasedProgress = easedProgress;
      easedProgress = isLast ? Easing.Quadratic.Out(t / actionGroup.duration) : Easing.Linear(t / actionGroup.duration); // ease out on last group only
      increment = easedProgress - previousEasedProgress;
      foreach (WeaponAction action in actionGroup.weaponActions)
      {
        ExecuteWeaponAction(action, increment);
      }
      yield return null;
    }
  }
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
    owningEffectWeaponInstances.Remove(this); // stops this weapon from marking the attack complete
  }

  // Return the overall effective weapon range for this weapon and any of its spawned objects
  // do you ever write code and just go "hm actually testing this will be awful so let's just...
  // ...hope it works"
  public float GetCumulativeEffectiveWeaponRange(float rangeSoFar = 0)
  {
    float ownRange = rangeSoFar;
    float cur = rangeSoFar;
    foreach (WeaponActionGroup actionGroup in weaponActionGroups)
    {
      foreach (WeaponAction action in actionGroup.weaponActions)
      {
        if (action.type == WeaponActionType.Move)
        {
          cur += action.magnitude;
          ownRange = Mathf.Max(cur, ownRange);
        }
      }
    }
    float childRange = 0;
    if (objectToSpawn != null && objectToSpawn.weaponPrefab != null && spawnObjectOnDestruction)
    {
      childRange = objectToSpawn.weaponPrefab.GetCumulativeEffectiveWeaponRange();
    }
    return ownRange + childRange;
  }

  public void OnContact(Collider2D col)
  {
    if (objectToSpawn != null && spawnObjectOnContact)
    {
      Quaternion rotationAngle = Quaternion.AngleAxis(transform.eulerAngles.z + objectToSpawn.rotationOffset, Vector3.forward);
      GameObject go = Instantiate(objectToSpawn.weaponPrefab.gameObject, transform.position + new Vector3(objectToSpawn.range, 0, 0), rotationAngle);
      Weapon weapon = go.GetComponent<Weapon>();
      if (weapon != null)
      {
        go.GetComponent<Weapon>().Init(owningEffect, owningEffectWeaponInstances);
      }
    }
    if (destroyOnContact)
    {
      CleanUp();
    }
  }
}
