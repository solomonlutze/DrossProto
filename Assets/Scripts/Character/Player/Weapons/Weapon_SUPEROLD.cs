using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
	In-game representation of a weapon.
	Should handle rendering/animating the weapon and spawning hitboxes.
	ANY AND ALL DATA should live on the WeaponData scriptable object - not here.
*/
public class Weapon_SUPEROLD : MonoBehaviour
{

  public Character owner;
  public WeaponData weaponData;
  public Animator animator;

  private IEnumerator attackQueueCoroutine;

  // TODO: this should probably be an enum
  private string currentAttack;

  private int currentComboLength = 0;
  private Renderer[] renderers;

  public Transform spawnTransform;
  // Set properties of the base damage object this weapon uses.
  // Hitboxes will modify these damage objects.
  public void Init()
  {
    renderers = GetComponentsInChildren<Renderer>();
    SetWeaponVisible(false);
    if (spawnTransform == null)
    {
      spawnTransform = transform;
    }
  }

  // Set attacking on the animator, and specify which attack animation to use
  // with queueAttack_<animationName>

  public void BeginAttack()
  {
    if (owner.usingSkill) { return; }
    owner.usingSkill = true;
    QueueNextAttack();
    SetWeaponVisible(true);
    // currentAttack = attackAnimations[currentComboLength].attackAnimation;
    // animator.SetBool("queueAttack_"+currentAttack, true);
  }

  // Spawns a specified hitbox at HitboxSpawn object's location (or weapon location if no spawn exists).
  // The attack animation will specify which prefab to instantiate.
  public void CreateHitbox(string hitboxId)
  {
    HitboxData hbi = weaponData.attackAnimations[currentComboLength - 1].hitboxes.Find(x => x.id == hitboxId);
    if (hbi == null) { Debug.LogError("Animation " + weaponData.attackAnimations[currentComboLength - 1].attackAnimation + " has no hitbox with id " + hitboxId); }
    Hitbox_OLD hb = GameObject.Instantiate(weaponData.hitboxPrefab, spawnTransform.position, spawnTransform.rotation) as Hitbox_OLD;
    // hb.Init(spawnTransform, owner, hbi);
  }

  // Clears the animation boolean for the current attack, marking it as ended
  public void FinishAttack()
  {
    if (owner.usingSkill)
    {
      animator.SetBool("queueAttack_" + currentAttack, false);
    }
  }

  // allows us to stop the coroutine if it's already running so we can start it again
  public void QueueNextAttack()
  {
    if (currentComboLength < weaponData.attackAnimations.Length)
    {
      if (attackQueueCoroutine != null)
      {
        StopCoroutine(attackQueueCoroutine);
      }
      attackQueueCoroutine = QueueNextAttackCoroutine();
      StartCoroutine(attackQueueCoroutine);
    }
  }

  // Fired by user action.
  // Queues up the next attack, then un-queues it if attackQueueWindow expires.
  // In practice, this means that the next attack will only be happen if the button is pressed within attackQueueWindow
  // seconds of the end of the attack.
  // TODO: currentComboLength should be incremented if/when the attack actually begins.
  private IEnumerator QueueNextAttackCoroutine()
  {
    string attackToQueue = weaponData.attackAnimations[currentComboLength].attackAnimation; // save this bc currentComboLength may change during the yield
    animator.SetBool("queueAttack_" + attackToQueue, true);
    yield return new WaitForSeconds(weaponData.attackQueueWindow);
    animator.SetBool("queueAttack_" + attackToQueue, false);
  }

  // Called from animation when it begins.
  public void OnEnterAttackAnimation()
  {
    AttackAnimation currentAttackAnimation = weaponData.attackAnimations[currentComboLength];
    animator.SetBool("queueAttack_" + currentAttackAnimation.attackAnimation, false);
    animator.SetFloat("windupSpeed", 1f / currentAttackAnimation.windup.phaseDuration);
    animator.SetFloat("attackSpeed", 1f / currentAttackAnimation.attack.phaseDuration);
    animator.SetFloat("recoverySpeed", 1f / currentAttackAnimation.recovery.phaseDuration);
    IncrementCurrentComboLength();
  }
  public void IncrementCurrentComboLength()
  {
    currentComboLength++;
  }

  // Reset currentComboLength and set attacking to false.
  public void FinishCombo()
  {
    SetWeaponVisible(false);
    owner.usingSkill = false;
    currentComboLength = 0;
  }
  // END WIP

  // Currently calling this when attacks start and stop
  // so we're very likely to have weapon visibility issues at some point.
  // in that case, render conditionally in update based on whether attacking == true probably
  public void SetWeaponVisible(bool flag)
  {
    foreach (Renderer r in renderers)
    {
      r.enabled = flag;
    }

  }
}
