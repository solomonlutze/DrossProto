using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
  Physical,
  Heat,
  Fungal,
  Cold,
  Acid

}

public enum HitboxShape { Box, Circle, Spawn };

// TODO: Currently only used by trait-spawned attacks. Should also be used by weapon-spawned attacks.
public enum HitboxSpawnPoint { CenteredOnPlayer, CenteredOnCrosshair }

[System.Serializable]
public class DamageInfo
{

  // amount of damage we do
  public float damage;
  // amount of impulse force applied on hit

  public float knockback;
  // Duration in seconds of stun. TODO: should this value represent something else?
  public float stun;

  public Transform hitboxTransform;

  public Transform attackerTransform;
  // amount of invulnerability this attack imparts. TODO: separate windows per damage source?
  public float invulnerabilityWindow;
  // whether this damage object should respect invulnerability from OTHER targets
  public bool ignoreInvulnerability;
  // whether this damage object should corrode certain tiles
  public bool corrosive = false;

  // whether this damage object should force hit enemies to drop their items
  public bool forcesItemDrop = false;

  public TileDurability durabilityDamageLevel = TileDurability.Delicate;

  public DamageType damageType;
  public List<CharacterStatModification> characterStatModifications;
  private string _sourceString = "";
  public string sourceString
  {
    get
    {
      if (_sourceString == "")
      {
        _sourceString = Guid.NewGuid().ToString("N").Substring(0, 15);
      }
      return _sourceString;
    }
  }
  // public DamageObject() {}
  public DamageInfo() { }
  public DamageInfo(float d, float k, float s, float i, DamageType dt, bool c, bool f, TileDurability durabilityDamage, Transform hitboxT, Transform attackerT)
  {
    damage = d;
    knockback = k;
    stun = s;
    invulnerabilityWindow = i;
    durabilityDamageLevel = durabilityDamage;
    hitboxTransform = hitboxT;
    damageType = dt;
    corrosive = c;
    forcesItemDrop = f;
    attackerTransform = attackerT;
    characterStatModifications = new List<CharacterStatModification>();
  }
}

// Damage-dealing hitbox object.
[System.Serializable]
public class HitboxInfo
{
  public string id;
  public string hitboxDescription;
  // should this hitbox follow our initializingTransform; if false, it just hangs out in worldspace
  public bool followInitializingTransform;
  public float duration;
  public HitboxShape hitboxShape;
  // used only for circle shape.
  public float radius;

  // Used only for box shape.
  public Vector2 size;
  public DamageInfo damageInfo;

}
public class Hitbox : MonoBehaviour
{

  //TODO: Size things
  // - should be possible to set the shape: square, circle, or hitboxSpawn
  // - should be possible to set radius, xy, or neither (for hitboxSpawn)

  //how long this hitbox should last for
  public float duration;

  // what weapon does this hitbox belong to
  public Transform owningTransform;
  // damage we will apply on a hit
  public DamageInfo damageObj;
  public FloorLayer floor;

  // Use this for initialization
  void Start()
  {
  }
  // Characteristics of hitbox initialized by weapon.
  public void Init(Transform initializingTransform, Character owner, HitboxInfo info, DamageInfo damageObject = null, CharacterAttackModifiers attackModifiers = null)
  {
    duration = info.duration;
    // WARNING: duration of 0 means a hitbox needs to be cleaned up manually
    if (duration > 0)
    {
      StartCoroutine(CleanUpSelf());
    }
    // TODO: clean this up, wny tf is it like this
    damageObj = new DamageInfo(
        info.damageInfo.damage,
        info.damageInfo.knockback,
        info.damageInfo.stun,
        info.damageInfo.invulnerabilityWindow,
        info.damageInfo.damageType,
        info.damageInfo.corrosive,
        info.damageInfo.forcesItemDrop,
        info.damageInfo.durabilityDamageLevel,
        transform,
        owner ? owner.transform : initializingTransform
    );// TODO: THIS IS A PROBLEM
    if (attackModifiers != null)
    {
      PopulateDamageObjectWithAttackModifiers(attackModifiers, info);
    }
    owningTransform = initializingTransform;
    floor = owner.currentFloor;
    switch (info.hitboxShape)
    {
      case HitboxShape.Box:
        transform.localScale = new Vector2(
            info.size.x + Character.GetAttackValueModifier(attackModifiers.attackValueModifiers, CharacterAttackValue.HitboxSize),
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
    WorldObject.ChangeLayersRecursively(transform, owner.currentFloor);
  }
  // TODO: Refactor attack info so that it all lives on a single object (...maybe)
  void PopulateDamageObjectWithAttackModifiers(CharacterAttackModifiers attackModifiers, HitboxInfo hbi)
  {
    if (hbi == null) { return; }
    damageObj.damage += Character.GetAttackValueModifier(attackModifiers.attackValueModifiers, CharacterAttackValue.Damage);
    damageObj.knockback += Character.GetAttackValueModifier(attackModifiers.attackValueModifiers, CharacterAttackValue.Knockback);
    damageObj.stun += Character.GetAttackValueModifier(attackModifiers.attackValueModifiers, CharacterAttackValue.Stun);
    float rawNewDurability = Mathf.Max((float)damageObj.durabilityDamageLevel, Character.GetAttackValueModifier(attackModifiers.attackValueModifiers, CharacterAttackValue.DurabilityDamage));
    damageObj.durabilityDamageLevel = (TileDurability)rawNewDurability;
    damageObj.corrosive = Character.GetAttackValueModifier(attackModifiers.attackValueModifiers, CharacterAttackValue.AcidDamage) > 0;
    damageObj.forcesItemDrop = attackModifiers.forcesLymphDrop;
    // TODO: Venom should do something!
    // if (attackModifiers[CharacterAttackValue.Venom] > 0) {
    //     damageObj.characterStatModifications.Add(new CharacterStatModification(
    //         CharacterStat.CurrentHealth,
    //         Character.GetAttackValueModifier(attackModifiers, CharacterAttackValue.Venom),
    //         5.0f,
    //         0f,
    //         Guid.NewGuid().ToString()
    //     ));
    // }
  }
  // Characteristics of hitbox initialized by weapon.
  public void OLD__Init(Transform initializingTransform, Character owner, HitboxInfo info)
  {
    duration = info.duration;
    // WARNING: duration of 0 means a hitbox needs to be cleaned up manually
    if (duration > 0)
    {
      StartCoroutine(CleanUpSelf());
    }
    owningTransform = initializingTransform;
    floor = owner.currentFloor;
    switch (info.hitboxShape)
    {
      case HitboxShape.Box:
        transform.localScale = info.size;
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
  }

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
      tile.TakeDamage(damageObj);
    }

  }

  // Anything we hit, try to damage! This can be applied to destructible environments later too.
  void OnTriggerStay2D(Collider2D col)
  {
    Debug.Log("hitbox collided with " + col.gameObject);
    col.SendMessage("TakeDamage", damageObj, SendMessageOptions.DontRequireReceiver);
  }
}
