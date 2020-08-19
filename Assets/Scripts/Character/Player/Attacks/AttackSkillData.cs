using UnityEngine;
using System.Collections;

[System.Serializable]
public class AttackSkillData : CharacterSkillData
{
  public Weapon weaponPrefab; // actual weapon prefab. instantiate this on init
  // public Weapon weaponInstance; // reference to actual weapon gameobject instance, instantiated from the prefab
  public float windupDuration;
  public float attackDuration;
  public float range; // how far away from the owner the weapon starts
  public float distance; // how far from the spawn point the weapon travels
  public float cooldown;
  public int sweepRadiusInDegrees;

  public DamageInfo baseDamage;

  public override void Init(Character owner)
  {
    Weapon wi = Instantiate(weaponPrefab, owner.weaponPivot.position + GetInitialPosition(), owner.weaponPivot.rotation, owner.weaponPivot);
    owner.weaponInstances.Add(name, wi);
    wi.Init(this);
    wi.gameObject.SetActive(false);
  }

  public Vector3 GetInitialPosition()
  {
    return new Vector3(range, 0, 0);
  }

  public override IEnumerator UseSkill(Character owner)
  {
    yield return PerformSkillCycle(owner);
  }

  public override IEnumerator PerformSkillCycle(Character owner)
  {
    yield return owner.StartCoroutine(DoWindup());
    BeforeAttack(owner);
    yield return owner.StartCoroutine(DoAttack(owner));
    AfterAttack(owner);
    yield return owner.StartCoroutine(DoCooldown());
  }

  public virtual IEnumerator DoWindup()
  {
    yield return new WaitForSeconds(windupDuration);
  }

  public virtual void BeforeAttack(Character owner)
  {
    owner.weaponInstances[name].gameObject.SetActive(true);
    InitializeHitboxes(owner);
  }

  public virtual void AfterAttack(Character owner)
  {
    owner.weaponInstances[name].gameObject.SetActive(false);
  }

  public virtual void InitializeHitboxes(Character owner)
  {
    foreach (Hitbox hb in owner.weaponInstances[name].hitboxes)
    {
      hb.Init(this, owner);
    }
  }

  public virtual IEnumerator DoAttack(Character owner)
  {
    return null;
  }

  public virtual IEnumerator DoCooldown()
  {
    yield return new WaitForSeconds(cooldown);
  }

  public virtual void InterruptAttack(Character owner)
  {
    return;
  }

  // DAMAGE INFO ACCESSORS

  public int GetDamageAmount(Character character)
  {
    return baseDamage.damageAmount;
  }
  public DamageType GetDamageType(Character character)
  {
    return baseDamage.damageType;
  }

  public float GetStun(Character character)
  {
    return baseDamage.stun;
  }

  public bool IgnoresInvulnerability()
  {
    return false;
  }

  public float GetInvulnerabilityWindow()
  {
    return baseDamage.invulnerabilityWindow;
  }

  public Vector3 GetKnockback(Character character, Hitbox hitbox)
  {
    return baseDamage.knockback
      * (hitbox.transform.position - character.transform.position);
  }

  public bool ForcesItemDrop()
  {
    return baseDamage.forcesItemDrop;
  }
}
