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

[System.Serializable]
public class Attack
{
    public bool destroyOnContact;
    public AttackSpawn objectToSpawn;
    public bool spawnObjectOnContact;
    public bool spawnObjectOnDestruction;
    public WeaponActionGroup[] weaponActionGroups;

    // public void Init(AttackSkillEffect effect, List<Attack> activeAttacks)
    // {
    //     owner = effect.owner;
    //     owningEffect = effect;
    //     owningEffectActiveAttacks = activeAttacks;
    //     owningEffectActiveAttacks.Add(this);
    //     defaultHitboxes = GetComponentsInChildren<Hitbox>();

    //     foreach (Hitbox hitbox in defaultHitboxes)
    //     {
    //         hitbox.Init(effect.owner, effect.baseDamage);
    //     }
    //     WorldObject.ChangeLayersRecursively(gameObject.transform, effect.owner.currentFloor);
    // }
    // public IEnumerator PerformWeaponActions()
    // {
    //     for (int i = 0; i < weaponActionGroups.Length; i++)
    //     {
    //         yield return ExecuteWeaponActionGroup(weaponActionGroups[i], i == weaponActionGroups.Length - 1);
    //     }
    //     CleanUp();
    // }

    // public void CleanUp()
    // {
    //     owningEffectActiveAttacks.Remove(this);
    //     if (objectToSpawn != null && objectToSpawn.weaponPrefab != null && spawnObjectOnDestruction)
    //     {
    //         owner.StartCoroutine(owningEffect.SpawnWeapon(objectToSpawn, transform.position, transform.eulerAngles));
    //     }
    //     Destroy(this.gameObject);
    // }

    // public IEnumerator ExecuteWeaponActionGroup(WeaponActionGroup actionGroup, bool isLast)
    // {
    //     float t = 0;
    //     float easedProgress = 0;
    //     float previousEasedProgress = 0;
    //     float increment = 0;
    //     while (t <= actionGroup.duration)
    //     {
    //         t += Time.deltaTime;
    //         previousEasedProgress = easedProgress;
    //         easedProgress = isLast ? Easing.Quadratic.Out(t / actionGroup.duration) : Easing.Linear(t / actionGroup.duration); // ease out on last group only
    //         increment = easedProgress - previousEasedProgress;
    //         foreach (WeaponAction action in actionGroup.weaponActions)
    //         {
    //             ExecuteWeaponAction(action, increment);
    //         }
    //         yield return null;
    //     }
    // }
    // public void ExecuteWeaponAction(WeaponAction action, float increment)
    // {
    //     switch (action.type)
    //     {
    //         case WeaponActionType.Move:
    //             MoveWeaponAction(action, increment);
    //             break;
    //         case WeaponActionType.RotateRelative:
    //             RotateWeaponRelativeAction(action, increment);
    //             break;
    //         case WeaponActionType.Wait:
    //             WaitWeaponAction(action, increment);
    //             break;
    //         case WeaponActionType.MarkDone:
    //             MarkDoneWeaponAction(action, increment);
    //             break;
    //     }
    // }

    // public void MoveWeaponAction(WeaponAction action, float increment)
    // {
    //     transform.position += transform.rotation * new Vector3(action.magnitude * increment, 0, 0);
    // }

    // // Rotate weapon relative to owner - think of a sword swing
    // public void RotateWeaponRelativeAction(WeaponAction action, float increment)
    // {
    //     Vector3 direction = transform.position - owner.transform.position;
    //     transform.RotateAround(owner.transform.position, Vector3.forward, action.magnitude * increment);
    // }
    // public void WaitWeaponAction(WeaponAction action, float increment)
    // {
    //     // do nothing
    // }

    // public void MarkDoneWeaponAction(WeaponAction action, float increment)
    // {
    //     owningEffectActiveAttacks.Remove(this); // stops this weapon from marking the attack complete
    // }

    // // Return the overall effective weapon range for this weapon and any of its spawned objects
    // // do you ever write code and just go "hm actually testing this will be awful so let's just...
    // // ...hope it works"
    // public float GetCumulativeEffectiveWeaponRange(float rangeSoFar = 0)
    // {
    //     float ownRange = rangeSoFar;
    //     float cur = rangeSoFar;
    //     foreach (WeaponActionGroup actionGroup in weaponActionGroups)
    //     {
    //         foreach (WeaponAction action in actionGroup.weaponActions)
    //         {
    //             if (action.type == WeaponActionType.Move)
    //             {
    //                 cur += action.magnitude;
    //                 ownRange = Mathf.Max(cur, ownRange);
    //             }
    //         }
    //     }
    //     float childRange = 0;
    //     if (objectToSpawn != null && objectToSpawn.weaponPrefab != null && spawnObjectOnDestruction)
    //     {
    //         childRange = objectToSpawn.weaponPrefab.GetCumulativeEffectiveWeaponRange();
    //     }
    //     return ownRange + childRange;
    // }

    // public void OnContact(Collider2D col)
    // {
    //     if (objectToSpawn != null && spawnObjectOnContact)
    //     {
    //         Quaternion rotationAngle = Quaternion.AngleAxis(transform.eulerAngles.z + objectToSpawn.rotationOffset, Vector3.forward);
    //         GameObject go = Instantiate(objectToSpawn.weaponPrefab.gameObject, transform.position + new Vector3(objectToSpawn.range, 0, 0), rotationAngle);
    //         Attack weapon = go.GetComponent<Attack>();
    //         if (weapon != null)
    //         {
    //             go.GetComponent<Attack>().Init(owningEffect, owningEffectActiveAttacks);
    //         }
    //     }
    //     if (destroyOnContact)
    //     {
    //         CleanUp();
    //     }
    // }
    public float GetCumulativeEffectiveWeaponRange()
    {
        float ownRange = 0;
        float cur = ownRange;
        foreach (WeaponActionGroup actionGroup in weaponActionGroups)
        {
            foreach (WeaponAction action in actionGroup.weaponActions)
            {
                if (action.type == WeaponActionType.Move)
                {
                    Debug.Log("found move with magnitude: " + action.magnitude);
                    cur += action.magnitude;
                    ownRange = Mathf.Max(cur, ownRange);
                }
            }
        }
        float childRange = 0;
        if (objectToSpawn != null && objectToSpawn.attackData != null && spawnObjectOnDestruction)
        {
            childRange = objectToSpawn.weaponSize + objectToSpawn.attackData.GetCumulativeEffectiveWeaponRange();
        }
        Debug.Log("updated ownRange: " + ownRange);
        return ownRange + childRange;
    }
}
