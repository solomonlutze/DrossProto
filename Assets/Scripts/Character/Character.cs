using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
// TODO: Character can probably extend CustomPhysicsController, which simplifies movement code a bit.
public class AnimationInfoObject
{
  public Vector2 animationInput;
}

public enum CharacterVital
{
  CurrentHealth,
  CurrentEnvironmentalDamageCooldown,
  CurrentDashCooldown
}
public enum CharacterStat
{
  MaxHealth,
  DetectableRange,
  MaxEnvironmentalDamageCooldown,
  MoveAcceleration,
  DashAcceleration,
  DashDuration,
  RotationSpeed,
  MaxDashCooldown,
  DashRange
}

public enum CharacterAttackValue
{
  Damage,
  Range,
  HitboxSize,
  Knockback,
  Stun,
  AttackSpeed,
  Cooldown,
  DurabilityDamage,
  Venom,
  AcidDamage,
}

// Special modes of character movement.
// Possibly unnecessary!!
public enum CharacterMovementAbility
{
  Burrow,
  FastFeet,
  Halteres,
  Hover,
  StickyFeet,
  WaterStride
}

public enum CharacterPerceptionAbility
{
  SensitiveAntennae
}

[System.Serializable]
public class CharacterStatModification
{
  public CharacterStat statToModify;

  public int magnitude;

  public float applicationDuration;
  public float duration;
  public float delay;

  public string source;

  public CharacterStatModification(CharacterStat s, int m, float dur, float del, string src)
  {
    statToModify = s;
    magnitude = m;
    duration = dur;
    delay = del;
    source = src;
  }
}

[System.Serializable]
public class ActiveStatModification
{
  public int magnitude;
  public string source;

  public ActiveStatModification(int m, string s)
  {
    magnitude = m;
    source = s;
  }
}

[System.Serializable]
public class CharacterAttackModifiers
{
  public CharacterAttackValueToIntDictionary attackValueModifiers;
  public bool forcesLymphDrop;

  public CharacterAttackModifiers()
  {
    attackValueModifiers = new CharacterAttackValueToIntDictionary();
  }

}

[System.Serializable]
public class TraitsLoadout
{
  public string head;
  public string thorax;
  public string abdomen;
  public string legs;
  public string wings;

  public TraitSlotToTraitDictionary EquippedTraits()
  {
    TraitSlotToTraitDictionary d = new TraitSlotToTraitDictionary();
    d[TraitSlot.Head] = Resources.Load("Data/TraitData/PassiveTraits/" + head) as PassiveTrait;
    d[TraitSlot.Thorax] = Resources.Load("Data/TraitData/PassiveTraits/" + thorax) as PassiveTrait;
    d[TraitSlot.Abdomen] = Resources.Load("Data/TraitData/PassiveTraits/" + abdomen) as PassiveTrait;
    d[TraitSlot.Legs] = Resources.Load("Data/TraitData/PassiveTraits/" + legs) as PassiveTrait;
    d[TraitSlot.Wings] = Resources.Load("Data/TraitData/PassiveTraits/" + wings) as PassiveTrait;
    return d;
  }

  public string[] AllPopulatedTraitNames
  {
    get
    {
      List<string> ret = new List<string>();
      foreach (string traitName in new string[] { head, thorax, abdomen, legs, wings })
      {
        if (traitName != null)
        {
          ret.Add(traitName);
        }
      }
      return ret.ToArray();
    }
  }
}

// TODO: Character should probably extend CustomPhysicsController, which should extend WorldObject
public class Character : WorldObject
{
  [Header("Stats and Vitals")]
  public CharacterVitalToFloatDictionary vitals;
  public StatToActiveStatModificationsDictionary statModifications;
  public DamageTypeToFloatDictionary damageTypeResistances;

  [Header("Attack Info")]
  public CharacterAttack characterAttack;
  public CharacterAttackModifiers attackModifiers;
  public Hitbox hitboxPrefab;
  private bool dashAttackEnabled = true;
  public CharacterAttack dashCharacterAttack;
  public CharacterAttackModifiers dashAttackModifiers;
  public Hitbox dashAttackHitboxPrefab;

  [Header("Trait Info")]
  public TraitSlotToTraitDictionary equippedTraits;
  public List<TraitEffect> conditionallyActivatedTraitEffects;
  public List<TraitEffect> activeConditionallyActivatedTraitEffects;
  public List<CharacterMovementAbility> activeMovementAbilities;

  [Header("Child Components")]
  public Transform orientation;
  public Transform crosshair;
  // our physics object
  public CustomPhysicsController po;
  // animation object
  protected Animator animator;
  public SpriteRenderer mainRenderer;

  public CircleCollider2D col;

  [Header("Game State Info")]
  public Color damageFlashColor = Color.red;
  public Color attackColor = Color.grey;
  public float damageFlashSpeed = 1.0f;
  public bool attacking = false;
  public bool dashing = false;
  protected bool attackCooldown = false;
  protected bool stunned = false;
  public bool animationPreventsMoving = false;
  public bool sticking = false;
  public Coroutine attackCoroutine;
  public FloorLayer currentFloor;
  protected Vector2 movementInput;
  // point in space we would like to face
  public Vector3 orientTowards;
  protected TileLocation currentTileLocation;
  protected EnvironmentTileInfo currentTile;
  protected float timeStandingStill = 0;
  protected float timeMoving = 0;
  protected Dictionary<string, GameObject> traitSpawnedGameObjects;
  protected List<string> sourceInvulnerabilities;

  [Header("Default Info")]
  public CharacterData defaultCharacterData;
  public TraitsLoadout initialEquippedTraits;

  private Coroutine damageFlashCoroutine;

  public Color damagedColor;

  [Header("Prefabs")]
  public MoltCasting moltCasingPrefab;

  protected virtual void Awake()
  {
    orientation = transform.Find("Orientation");
    col = GetComponent<CircleCollider2D>();
    if (orientation == null)
    {
      Debug.LogError("No object named 'Orientation' on Character object: " + gameObject.name);
    }
  }

  protected virtual void Start()
  {
    movementInput = new Vector2(0, 0);
    animator = GetComponent<Animator>();
    if (po == null)
    {
      Debug.LogError("No physics controller component on Character object: " + gameObject.name);
    }
    WorldObject.ChangeLayersRecursively(transform, currentFloor.ToString());
  }

  protected virtual void Init()
  {
    sourceInvulnerabilities = new List<string>();
    conditionallyActivatedTraitEffects = new List<TraitEffect>();
    traitSpawnedGameObjects = new Dictionary<string, GameObject>();
    InitializeFromCharacterData();
  }

  private void InitializeFromCharacterData()
  {
    if (defaultCharacterData != null)
    {
      CharacterData dataInstance = (CharacterData)ScriptableObject.Instantiate(defaultCharacterData);
      attackModifiers = dataInstance.attackModifiers;
      dashAttackModifiers = dataInstance.dashAttackModifiers;
      damageTypeResistances = dataInstance.damageTypeResistances;
      activeMovementAbilities.AddRange(dataInstance.movementAbilities);
    }
    vitals = new CharacterVitalToFloatDictionary();
    statModifications = new StatToActiveStatModificationsDictionary();
    vitals[CharacterVital.CurrentHealth] = GetStat(CharacterStat.MaxHealth);
    vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] = GetStat(CharacterStat.MaxEnvironmentalDamageCooldown);
    vitals[CharacterVital.CurrentDashCooldown] = GetStat(CharacterStat.MaxDashCooldown);
  }

  // non-physics biz
  protected virtual void Update()
  {
    HandleHealth();
    HandleFacingDirection();
    HandleTile();
    HandleCooldowns();
    HandleConditionallyActivatedTraits();
  }

  // physics biz
  void FixedUpdate()
  {
    CalculateMovement();
  }

  // WIP: COMBOS

  // How combos should work

  // if you're not attacking, Attack calls BeginAttack and queues your first attack
  // if you ARE attacking, and haven't queued an attack,
  //   Attack calls QueueNextAttack and queues a subsequent attack
  // if you are attacking and HAVE queued an attack,
  //   Attack resets the coroutine
  // at the end of an attack, (Weapon.FinishAttack) if a next attack is queued and present, it fires
  // if an attack is NOT queued, the combo is reset, and attacking is reset to false (Weapon.FinishCombo)

  // called via play input or npc AI
  protected void Attack()
  {
    if (characterAttack != null)
    {
      if (!attackCooldown)
      {
        attackCoroutine = StartCoroutine(DoAttack());
      }
    }
  }

  public IEnumerator DoAttack()
  {
    attacking = true;
    yield return new WaitForSeconds(characterAttack.attackSpeed);
    CreateAttackHitbox();
    attacking = false;
    yield return new WaitForSeconds(characterAttack.cooldown);
    attackCooldown = false;
    attackCoroutine = null;
  }
  public static float GetAttackValueModifier(CharacterAttackValueToIntDictionary attackModifiers, CharacterAttackValue value)
  {
    if (attackModifiers == null || !attackModifiers.ContainsKey(value))
    {
      return 0;
    }
    return attackModifiers[value] * Constants.CharacterAttackAdjustmentIncrements[value];
  }

  // TODO: Refactor attack info so that it all lives on a single object (...maybe)
  public void CreateAttackHitbox()
  {
    CreateHitbox(characterAttack, attackModifiers);
  }

  public void CreateDashAttackHitbox()
  {
    CreateHitbox(dashCharacterAttack, dashAttackModifiers);
  }

  public void CreateHitbox(CharacterAttack atk, CharacterAttackModifiers mods)
  {
    HitboxData hbi = atk.hitboxData;
    if (hbi == null) { Debug.LogError("no attack object defined for " + gameObject.name); }
    Vector3 pos = orientation.TransformPoint(Vector3.right * (atk.range + Character.GetAttackValueModifier(mods.attackValueModifiers, CharacterAttackValue.Range)));
    Hitbox hb = GameObject.Instantiate(hitboxPrefab, pos, orientation.rotation) as Hitbox;
    hb.gameObject.layer = LayerMask.NameToLayer(currentFloor.ToString());
    hb.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = currentFloor.ToString();
    hb.Init(orientation, this, hbi, mods);
  }

  public void ApplyAttackModifier(CharacterAttackModifiers mods, bool forDashAttack)
  {
    dashAttackEnabled |= forDashAttack;
    CharacterAttackModifiers modsToAdjust = forDashAttack ? dashAttackModifiers : attackModifiers;
    modsToAdjust.forcesLymphDrop |= mods.forcesLymphDrop;
    foreach (CharacterAttackValue val in mods.attackValueModifiers.Keys)
    {
      int existingVal = modsToAdjust.attackValueModifiers.ContainsKey(val) ? modsToAdjust.attackValueModifiers[val] : 0;
      modsToAdjust.attackValueModifiers[val] = existingVal + mods.attackValueModifiers[val];
    }
  }
  // public void ApplyDashAttackModifier(CharacterAttackValue attackValue, int magnitude)
  // {
  //   dashAttackEnabled = true;
  //   Debug.Log("applying dash attack modifier: " + attackValue + "," + magnitude);
  //   dashAttackModifiers[attackValue] += magnitude;
  // }

  public void SetAnimationInput(Vector2 newAnimationInput)
  {
    po.SetAnimationInput(newAnimationInput);
  }

  public void SetAnimationPreventsMoving(bool newAnimationPreventsMoving)
  {
    animationPreventsMoving = newAnimationPreventsMoving;
  }
  // Point character towards a rotation target.
  void HandleFacingDirection()
  {
    if (
      (attacking || animationPreventsMoving || stunned)
      && !activeMovementAbilities.Contains(CharacterMovementAbility.Halteres)
    )
    {
      return;
    }
    Quaternion targetDirection = GetTargetDirection();
    orientation.rotation = Quaternion.Slerp(orientation.rotation, targetDirection, GetStat(CharacterStat.RotationSpeed) * Time.deltaTime);
  }

  // Rotate character smoothly towards a particular orientation around the Z axis.
  // Warning: I don't understand this math. If character rotation seems buggy, this is a
  // potential culprit.
  Quaternion GetTargetDirection()
  {
    Vector3 target = orientTowards - transform.position;
    float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
    return Quaternion.AngleAxis(angle, Vector3.forward);
  }

  // used to calculate how far our facing direction is from our target facing direction.
  // Useful for e.g. deciding if an enemy is facing a player enough for attacking to be a good idea.
  public float GetAngleToTarget()
  {
    return Quaternion.Angle(GetTargetDirection(), orientation.rotation);
  }

  // add input to our velocity, if necessary/possible.
  protected void CalculateMovement()
  {
    if (CanMove())
    {
      if (movementInput == Vector2.zero)
      { // should be an approximate equals
        timeStandingStill += Time.deltaTime;
        timeMoving = 0;
      }
      else
      {
        timeMoving += Time.deltaTime;
        timeStandingStill = 0f;
      }
      po.SetMovementInput(new Vector2(movementInput.x, movementInput.y));
    }
    else
    {
      po.SetMovementInput(Vector2.zero);
    }
  }

  protected void Molt()
  {
    Instantiate(moltCasingPrefab, orientation.transform.position, orientation.transform.rotation);
    moltCasingPrefab.Init(mainRenderer.color, this);
    ApplyStatMod(new CharacterStatModification(
      CharacterStat.MaxHealth,
      -1,
      0,
      0,
      Guid.NewGuid().ToString("N").Substring(0, 15)
    ));
    AdjustHealth(5);
  }
  // Traits activated on dash are handled in HandleConditionallyActivatedTraits()
  protected void Dash()
  {
    if (vitals[CharacterVital.CurrentDashCooldown] > 0) { return; }
    DoDashAttack();
    StartCoroutine(ApplyDashInput());
    vitals[CharacterVital.CurrentDashCooldown] = GetStat(CharacterStat.MaxDashCooldown);
  }

  protected IEnumerator ApplyDashInput()
  {
    float t = 0;
    dashing = true;
    while (t < GetStat(CharacterStat.DashDuration))
    {
      po.SetMovementInput(orientation.rotation * new Vector3(1, 0, 0));
      t += Time.deltaTime;
      yield return null;
    }
    dashing = false;
  }

  protected void DoDashAttack()
  {
    if (dashAttackEnabled)
    {
      CreateDashAttackHitbox();
    }
  }
  // determines if input-based movement is allowed
  protected virtual bool CanMove()
  {
    if (!activeMovementAbilities.Contains(CharacterMovementAbility.Halteres))
    {
      if (attacking)
      {
        return false;
      }
      if (animationPreventsMoving)
      {
        return false;
      }
    }
    if (stunned)
    {
      return false;
    }
    return true;
  }

  // DAMAGE FUNCTIONS

  // Called from Hitbox's OnTriggerEnter. Calls other functions to determine outcome of getting hit.
  protected float GetDamageAfterResistance(float damage, DamageType damageType)
  {
    return ((100 - damageTypeResistances[damageType]) / 100) * damage;
  }
  protected virtual void TakeDamage(Damage damage)
  {
    if (
      sourceInvulnerabilities.Contains(damage.sourceString)
      && !damage.IgnoresInvulnerability())
    { return; }
    if (damage.owningCharacter == this) { return; }
    float damageAfterResistances = GetDamageAfterResistance(damage.GetDamage(), damage.GetDamageType());
    if (
      damageAfterResistances <= 0
      && damage.GetDamage() > 0
    // && hb.characterStatModifications.Count == 0
    ) { return; }
    InterruptAnimation();
    AdjustHealth(Mathf.Floor(-damageAfterResistances));
    CalculateAndApplyStun(damage.GetStun());
    // foreach (CharacterStatModification mod in damageObj.characterStatModifications)
    // {
    //   ApplyStatMod(mod);
    // }
    StartCoroutine(ApplyInvulnerability(damage));
    // if (damageFlashCoroutine != null) {
    //   StopCoroutine(damageFlashCoroutine);
    // }
    // StartCoroutine(ApplyDamageFlash(damageObj));
    Vector3 knockback = damage.CalculateAndReturnKnockback();
    if (knockback != Vector3.zero)
    {
      po.ApplyImpulseForce(knockback);
    }
  }

  private IEnumerator ApplyDamageFlash(DamageData damageObj)
  {
    // Todo: might wanna change this!
    Color baseColor = Color.white;
    mainRenderer.color = damageFlashColor;
    yield return new WaitForSeconds(damageFlashSpeed / 3);
    mainRenderer.color = baseColor;
    yield return new WaitForSeconds(damageFlashSpeed / 3);
    mainRenderer.color = damageFlashColor;
    yield return new WaitForSeconds(damageFlashSpeed / 3);
    mainRenderer.color = baseColor;
  }
  public virtual void Die()
  {
    Destroy(gameObject);
  }

  public virtual void AssignTraitsForNextLife(TraitSlotToUpcomingTraitDictionary nextLifeTraits)
  {

  }

  // Make us invulnerable, then un-make-us invulnerable. Damage is ignorned while invulnerable.
  void CalculateAndApplyInvulnerability(Damage damage)
  {
    if (damage.GetInvulnerabilityWindow() > 0)
    {
      StartCoroutine(ApplyInvulnerability(damage));
    }
  }

  IEnumerator ApplyInvulnerability(Damage damage)
  {
    string src = damage.sourceString;
    sourceInvulnerabilities.Add(src);
    yield return new WaitForSeconds(damage.GetInvulnerabilityWindow());
    if (sourceInvulnerabilities.Contains(src))
    {
      sourceInvulnerabilities.Remove(src);
    }
  }

  // Determines if we should be stunned, and stuns us if so.
  protected void CalculateAndApplyStun(float stunDuration, bool overrideStunResistance = false)
  {
    if (!overrideStunResistance)
    {
      // TODO: Stun resistance/reduction should happen here
    }
    if (stunDuration > 0)
    {
      StartCoroutine(ApplyStun(stunDuration));
    }
  }

  // Make us stunned, then un-make-us stunned. Cannot move or attack while stunned.

  IEnumerator ApplyStun(float stunDuration)
  {
    stunned = true;
    yield return new WaitForSeconds(stunDuration);
    stunned = false;
  }

  // End current attack/attack animation/combo and reset us to idle.
  // Used to keep us from finishing our attack after getting knocked across the screen.
  // TODO: it should be possible to "tank" some attacks and finish attacking
  void InterruptAnimation()
  {
    if (animator != null)
    {
      animator.SetTrigger("transitionToIdle");
    }
  }
  // END DAMAGE FUNCTIONS

  public float GetStat(CharacterStat statToGet)
  {
    Debug.Log("Stat to get: " + statToGet);
    Debug.Log("StatModifications: " + statToGet);
    StringToIntDictionary statMods = statModifications[statToGet];
    int modValue = 0;
    float returnValue = defaultCharacterData.defaultStats[statToGet];
    foreach (int modMagnitude in statMods.Values)
    {
      modValue += modMagnitude;
    }
    modValue = Mathf.Clamp(-12, modValue, 12);
    if (modValue >= 0)
    {
      returnValue *= ((3 + modValue) / 3);
    }
    else
    {
      returnValue *= (3f / (3 + Mathf.Abs(modValue)));
    }
    return returnValue;
  }


  public void ApplyStatMod(CharacterStatModification mod)
  {
    if (mod.duration > 0)
    {
      StartCoroutine(ApplyTemporaryStatMod(mod));
    }
    else
    {
      AddStatMod(mod);
    }
  }

  public IEnumerator ApplyTemporaryStatMod(CharacterStatModification mod)
  {
    AddStatMod(mod);
    yield return new WaitForSeconds(mod.duration);
    RemoveStatMod(mod.source);
  }

  public void AddStatMod(CharacterStatModification mod)
  {
    AddStatMod(mod.statToModify, mod.magnitude, mod.source);
  }

  public void AddStatMod(CharacterStat statToMod, int magnitude, string source)
  {
    statModifications[statToMod][source] = magnitude;
  }

  public void RemoveStatMod(string source)
  {
    foreach (CharacterStat stat in (CharacterStat[])Enum.GetValues(typeof(CharacterStat)))
    {
      StringToIntDictionary statMods = statModifications[stat];
      if (statMods.ContainsKey(source))
      {
        statMods.Remove(source);
      }
    }
  }

  public void AddMovementAbility(CharacterMovementAbility movementAbility)
  {
    activeMovementAbilities.Add(movementAbility);
  }

  public void RemoveMovementAbility(CharacterMovementAbility movementAbility)
  {
    activeMovementAbilities.Remove(movementAbility);
  }


  //TODO: SHOULD PROBS BE DEPRECATED
  public void AdjustHealth(float adjustment)
  {
    vitals[CharacterVital.CurrentHealth] =
      Mathf.Clamp(vitals[CharacterVital.CurrentHealth] + adjustment, 0, GetStat(CharacterStat.MaxHealth));
  }

  public virtual void SetCurrentFloor(FloorLayer newFloorLayer)
  {
    currentFloor = newFloorLayer;
    currentTileLocation = CalculateCurrentTileLocation();
    ChangeLayersRecursively(transform, newFloorLayer.ToString());
    po.OnLayerChange();
  }

  public void UseTile()
  {
    EnvironmentTileInfo et = GridManager.Instance.GetTileAtLocation(currentTileLocation);
    if (et.ChangesFloorLayer())
    {
      SetCurrentFloor(et.GetTargetFloorLayer(currentFloor));
    }
  }


  //TODO: this could potentially cause offset issues
  public TileLocation CalculateCurrentTileLocation()
  {
    return new TileLocation(
      new Vector2Int(
        Mathf.FloorToInt(transform.position.x),
        Mathf.FloorToInt(transform.position.y)
      ),
      currentFloor
    );
  }
  protected virtual void HandleHealth()
  {
    if (vitals[CharacterVital.CurrentHealth] <= 0)
    {
      Die();
    }
    mainRenderer.color = Color.Lerp(
      damagedColor,
      Color.white,
      vitals[CharacterVital.CurrentHealth] / GetStat(CharacterStat.MaxHealth)
    );
  }
  protected virtual void HandleTile()
  {
    EnvironmentTileInfo tile = GridManager.Instance.GetTileAtLocation(CalculateCurrentTileLocation());
    if (tile == null)
    {
      Debug.LogError("WARNING: no tile found at " + CalculateCurrentTileLocation().ToString());
      return;
    }
    if (tile != currentTile)
    {
      currentTile = tile;
    }
    TileLocation nowTileLocation = CalculateCurrentTileLocation();
    if (currentTileLocation != nowTileLocation)
    {
      currentTileLocation = nowTileLocation;
    }
    if (tile.objectTileType == null && tile.groundTileType == null)
    {
      bool canstick = GridManager.Instance.CanStickToAdjacentTile(transform.position, currentFloor);
      if (
        (activeMovementAbilities.Contains(CharacterMovementAbility.StickyFeet)
        && canstick)
        || sticking
      )
      {
        // do nothing. no fall plz.
      }
      else
      {
        transform.position = new Vector3(
          nowTileLocation.position.x + .5f,
          nowTileLocation.position.y + .5f,
          0f
        );
        SetCurrentFloor(currentFloor - 1);
      }
      return;
    }
    if (tile.dealsDamage && tile.environmentalDamage != null && vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] <= 0)
    {
      TakeEnvironmentalDamage(tile.environmentalDamage);
      vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] = GetStat(CharacterStat.MaxEnvironmentalDamageCooldown);
    }
  }

  public void TakeEnvironmentalDamage(DamageData damage)
  {

    float damageAfterResistances = GetDamageAfterResistance(damage.damageAmount, damage.damageType);
    if (
      damageAfterResistances <= 0
      && damage.damageAmount > 0
    // && hb.characterStatModifications.Count == 0
    ) { return; }
    AdjustHealth(Mathf.Floor(-damageAfterResistances));
  }
  public virtual void HandleTileCollision(EnvironmentTileInfo tile)
  {
    if (tile.GetColliderType() == Tile.ColliderType.None)
    {
      return;
    }
    else
    {
      // Debug.Log("collided with "+tile);
    }
  }

  public virtual void HandleConditionallyActivatedTraits()
  {
    foreach (TraitEffect trait in conditionallyActivatedTraitEffects)
    {
      switch (trait.activatingCondition)
      {
        case ConditionallyActivatedTraitCondition.Dashing:
          if (dashing)
          {
            trait.Apply(this);
          }
          else
          {
            trait.Expire(this);
          }
          break;
        case ConditionallyActivatedTraitCondition.NotMoving:
          if (timeStandingStill > trait.activatingConditionRequiredDuration)
          {
            trait.Apply(this);
          }
          else
          {
            trait.Expire(this);
          }
          break;
        case ConditionallyActivatedTraitCondition.Moving:
          if (timeMoving > trait.activatingConditionRequiredDuration)
          {
            trait.Apply(this);
          }
          else
          {
            trait.Expire(this);
          }
          break;
      }
    }
  }

  protected void HandleCooldowns()
  {
    if (vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] > 0)
    {
      vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] -= Time.deltaTime;
    }
    if (vitals[CharacterVital.CurrentDashCooldown] > 0)
    {
      vitals[CharacterVital.CurrentDashCooldown] -= Time.deltaTime;
    }
  }

  public virtual void AddAura(TraitEffect effect)
  {
    GameObject go = GameObject.Instantiate(effect.auraPrefab, transform.position, transform.rotation);
    go.layer = gameObject.layer;
    go.GetComponent<Renderer>().sortingLayerName = LayerMask.LayerToName(go.layer);
    go.transform.parent = transform;
    traitSpawnedGameObjects.Add(effect.sourceString, go);
  }

  public virtual void RemoveAura(TraitEffect effect)
  {
    if (traitSpawnedGameObjects.ContainsKey(effect.sourceString))
    {
      Destroy(traitSpawnedGameObjects[effect.sourceString]);
      traitSpawnedGameObjects.Remove(effect.sourceString);
    }
  }
}
