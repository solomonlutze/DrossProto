using UnityEngine;
using System.Collections;

[System.Serializable]
public class CharacterAttack : ScriptableObject
{

    //
    public Weapon weaponPrefab;
    public float windupDuration;
    public float attackDuration;
    public float range; // how far away from the owner the weapon starts
    public float distance; // how far from the spawn point the weapon travels
    public float cooldown;
    public int sweepRadiusInDegrees;
    public float ai_preferredMinAttackRange; // closer than this and we'd like to back up
    public float ai_preferredAttackRangeBuffer; // weapon effectiveRange minus range buffer = ideal attaack spot

    public DamageInfo baseDamage;

    public virtual void Init(Character owner)
    {
        owner.weaponInstance = Instantiate(weaponPrefab, owner.weaponPivot.position + GetInitialPosition(), owner.weaponPivot.rotation, owner.weaponPivot);
        Debug.Log("weapon instance: " + owner.weaponInstance);
        owner.weaponInstance.Init(this);
        owner.weaponInstance.gameObject.SetActive(false);
    }

    public Vector3 GetInitialPosition()
    {
        return new Vector3(range, 0, 0);
    }

    public virtual IEnumerator PerformAttackCycle(Character owner)
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
        owner.weaponInstance.gameObject.SetActive(true);
        InitializeHitboxes(owner);
    }

    public virtual void AfterAttack(Character owner)
    {
        owner.weaponInstance.gameObject.SetActive(false);
    }

    public virtual void InitializeHitboxes(Character owner)
    {
        Debug.Log("initializing hitboxes!");
        foreach (Hitbox hb in owner.weaponInstance.hitboxes)
        {
            Debug.Log("initializing hitbox: " + hb);
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

    public float GetInvulnerabilityWindow(Character character)
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
