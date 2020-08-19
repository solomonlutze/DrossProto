using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
// TODO: Character can probably extend CustomPhysicsController, which simplifies movement code a bit.
public class AnimationInfoObject
{
  public Vector2 animationInput;
}

public enum CharacterType
{
  Player,
  Enemy
}
// non-derivable values like health, current number of molts, etc
public enum CharacterVital
{
  CurrentHealth,
  CurrentEnvironmentalDamageCooldown,
  RemainingStamina,
  CurrentMoltCount
}

// values driving physical character behavior
// base values live in character data
// can include maximums, cooldown periods, etc
// not usually user facing, so can be pretty grody and granular if need be
public enum CharacterStat
{
  REMOVE_0 = 0,
  MaxHealthLostPerMolt = 1,
  DetectableRange = 2,
  MoveAcceleration = 3,
  FlightAcceleration = 4,
  Stamina = 5,
  DashAcceleration = 6,
  DashDuration = 7,
  DashRecoveryDuration = 8,
  RotationSpeed = 9
}

public enum CharacterAttribute
{
  REMOVE_0 = 0,
  REMOVE_1 = 1,
  REMOVE_2 = 2,
  Burrow = 3,
  Camouflage = 4,
  HazardResistance = 5,
  Resist_Fungal = 6,
  Resist_Heat = 7,
  Resist_Acid = 8,
  Resist_Physical = 9,
  WaterResistance = 10,
  Flight = 11,
  Dash = 12,
  Health = 13
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

public enum AscendingDescendingState
{ // negative is up and positive is down because the map is backwards, Do Not @ Me
  Ascending = -1,
  Descending = 1,
  None = 0
}

public enum CharacterPerceptionAbility
{
  SensitiveAntennae
}

[System.Serializable]
public class CharacterAttributeModification
{
  public CharacterAttribute statToModify;

  public int magnitude;

  public float applicationDuration;
  public float duration;
  public float delay;

  public string source;

  public CharacterAttributeModification(CharacterAttribute s, int m, float dur, float del, string src)
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
  public Trait head;
  public Trait thorax;
  public Trait abdomen;
  public Trait legs;
  public Trait wings;

  public bool AllTraitsEmpty()
  {
    return head == null
        && thorax == null
        && abdomen == null
        && legs == null
        && wings == null;
  }
}

[System.Serializable]
public class TraitsLoadout_OLD
{
  public string head;
  public string thorax;
  public string abdomen;
  public string legs;
  public string wings;

  public TraitSlotToTraitDictionary EquippedTraits()
  {
    TraitSlotToTraitDictionary d = new TraitSlotToTraitDictionary();
    // d[TraitSlot.Head] = Resources.Load("Data/TraitData/PassiveTraits/" + head) as PassiveTrait;
    // d[TraitSlot.Thorax] = Resources.Load("Data/TraitData/PassiveTraits/" + thorax) as PassiveTrait;
    // d[TraitSlot.Abdomen] = Resources.Load("Data/TraitData/PassiveTraits/" + abdomen) as PassiveTrait;
    // d[TraitSlot.Legs] = Resources.Load("Data/TraitData/PassiveTraits/" + legs) as PassiveTrait;
    // d[TraitSlot.Wings] = Resources.Load("Data/TraitData/PassiveTraits/" + wings) as PassiveTrait;
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
  public CharacterType characterType;
  public CharacterVitalToFloatDictionary vitals;
  public StatToActiveStatModificationsDictionary statModifications;
  public DamageTypeToFloatDictionary damageTypeResistances;

  [Header("Attack Info")]
  public List<CharacterSkillData> characterSkills;
  public List<CharacterSkillData> characterSpells;
  public Dictionary<string, Weapon> weaponInstances;
  // public Weapon weaponInstance;
  public CharacterAttackModifiers attackModifiers;
  public Hitbox_OLD hitboxPrefab;
  private bool dashAttackEnabled = true;
  public AttackSkillData dashCharacterAttack;
  public CharacterAttackModifiers dashAttackModifiers;
  public Hitbox_OLD dashAttackHitboxPrefab;

  [Header("Trait Info", order = 1)]
  public TraitSlotToTraitDictionary traits;
  public CharacterAttributeToIntDictionary attributes;
  public List<TraitEffect> conditionallyActivatedTraitEffects;
  public List<TraitEffect> activeConditionallyActivatedTraitEffects;
  public List<CharacterMovementAbility> activeMovementAbilities;

  [Header("Child Components")]
  public Transform orientation;

  public Transform weaponPivot;
  public Transform crosshair;
  // our physics object
  public CustomPhysicsController po;
  // animation object
  protected Animator animator;
  public SpriteRenderer[] renderers;

  public CircleCollider2D circleCollider;
  public BoxCollider2D boxCollider; // used for calculating collisions w/ tiles while changing floor layer
  public CharacterVisuals characterVisuals;

  [Header("Game State Info")]
  public Color damageFlashColor = Color.red;
  public Color attackColor = Color.grey;
  public float damageFlashSpeed = 1.0f;
  public bool usingSkill = false;
  public bool flying = false;
  public float dashTimer = 0.0f;
  public float dashRecoveryTimer = 0.0f;
  protected bool stunned = false;
  public bool animationPreventsMoving = false;
  public bool sticking = false;
  public Coroutine skillCoroutine;
  protected Coroutine flyCoroutine;
  protected Vector2 movementInput;
  // point in space we would like to face
  public Vector3 orientTowards;
  protected TileLocation currentTileLocation;
  protected EnvironmentTileInfo currentTile;
  protected TileLocation lastSafeTileLocation;
  protected float timeStandingStill = 0;
  protected float timeMoving = 0;
  protected Dictionary<string, GameObject> traitSpawnedGameObjects;
  public AscendingDescendingState ascendingDescendingState = AscendingDescendingState.None;
  public bool ascending
  {
    get { return ascendingDescendingState == AscendingDescendingState.Ascending; }
  }
  public bool descending
  {
    get { return ascendingDescendingState == AscendingDescendingState.Descending; }
  }
  public float ascendDescendSpeed = 1f;
  protected List<string> sourceInvulnerabilities;
  public float footstepCooldown = 0.0f;
  public float maxFootstepCooldown = 0.2f;

  [Header("Default Info")]
  public CharacterData defaultCharacterData;

  private Coroutine damageFlashCoroutine;

  public Color damagedColor;

  [Header("Prefabs")]
  public MoltCasting moltCasingPrefab;

  protected virtual void Awake()
  {
    orientation = transform.Find("Orientation");
    circleCollider = GetComponent<CircleCollider2D>();
    boxCollider = GetComponent<BoxCollider2D>();
    if (orientation == null)
    {
      Debug.LogError("No object named 'Orientation' on Character object: " + gameObject.name);
      return;
    }
    // weaponPivot = orientation.Find("WeaponPivot");
    // if (weaponPivot == null)
    // {
    //   Debug.LogError("No object named 'WeaponPivot' on Character object: " + gameObject.name);
    // }
  }

  protected virtual void Start()
  {
    movementInput = new Vector2(0, 0);
    animator = GetComponent<Animator>();
    if (po == null)
    {
      Debug.LogError("No physics controller component on Character object: " + gameObject.name);
    }
    WorldObject.ChangeLayersRecursively(transform, currentFloor);
    transform.position = new Vector3(transform.position.x, transform.position.y, GridManager.GetZOffsetForFloor(gameObject.layer));
  }

  protected virtual void Init()
  {
    sourceInvulnerabilities = new List<string>();
    conditionallyActivatedTraitEffects = new List<TraitEffect>();
    ascendingDescendingState = AscendingDescendingState.None;
    traitSpawnedGameObjects = new Dictionary<string, GameObject>();
    if (weaponInstances != null)
    {
      foreach (Weapon w in weaponInstances.Values)
      {
        Destroy(w.gameObject);
      }
      weaponInstances.Clear();
    }
    else
    {
      weaponInstances = new Dictionary<string, Weapon>();
    }
    characterSkills = CalculateSkills(traits);
    if (!HasAttackSkill(characterSkills))
    {
      characterSkills.Insert(0, defaultCharacterData.defaultCharacterAttack);
    }
    for (int i = 0; i < characterSkills.Count; i++)
    {
      characterSkills[i].Init(this);
    }
    attributes = CalculateAttributes(traits);
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
    // statModifications = new StatToActiveStatModificationsDictionary();
    vitals[CharacterVital.CurrentHealth] = GetCurrentMaxHealth();
    vitals[CharacterVital.RemainingStamina] = GetMaxStamina();
    // vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] = GetStat(CharacterStat.MaxEnvironmentalDamageCooldown);
    // vitals[CharacterVital.CurrentDashCooldown] = GetStat(CharacterStat.MaxDashCooldown);
  }

  protected virtual void InitializeAttributes()
  {
    foreach (Trait trait in traits.Values)
    {
      if (trait == null) { continue; }
      foreach (CharacterAttribute attribute in trait.attributeModifiers.Keys)
      {
        attributes[attribute] += trait.attributeModifiers[attribute];
      }
    }
  }

  public static CharacterAttributeToIntDictionary CalculateAttributes(TraitSlotToTraitDictionary traits)
  {

    CharacterAttributeToIntDictionary ret = new CharacterAttributeToIntDictionary(true);
    foreach (Trait trait in traits.Values)
    {
      if (trait == null) { continue; }
      foreach (CharacterAttribute attribute in trait.attributeModifiers.Keys)
      {
        ret[attribute] += trait.attributeModifiers[attribute];
      }
    }
    return ret;
  }

  public virtual CharacterSkillData GetSelectedCharacterSkill()
  {
    if (characterSkills.Count > 0)
    {
      return characterSkills[0];
    }
    return null;
  }

  public virtual Weapon GetSelectedWeaponInstance()
  {
    if (characterSkills.Count > 0)
    {
      string skillName = GetSkillNameFromIndex(0);
      return weaponInstances[skillName];
    }
    return null;
  }
  public static List<CharacterSkillData> CalculateSkills(TraitSlotToTraitDictionary traits)
  {
    List<CharacterSkillData> ret = new List<CharacterSkillData>();
    foreach (Trait trait in traits.Values)
    {
      if (trait == null) { continue; }
      if (trait.skillData != null)
      {
        ret.Add(trait.skillData);
      }
    }
    return ret;
  }

  public static bool HasAttackSkill(List<CharacterSkillData> skills)
  {
    foreach (CharacterSkillData skill in skills)
    {
      if ((AttackSkillData)skill != null)
      {
        return true;
      }
    }
    return false;
  }
  // non-physics biz
  protected virtual void Update()
  {
    HandleHealth();
    HandleFacingDirection();
    HandleFalling();
    HandleTile();
    HandleCooldowns();
    HandleConditionallyActivatedTraits();
    HandleAscendOrDescend();
  }

  // physics biz
  protected virtual void FixedUpdate()
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

  public void UseSkill(CharacterSkillData skill)
  {
    if (skill != null)
    {
      if (skillCoroutine == null)
      {
        skillCoroutine = StartCoroutine(DoSkill(skill));
      }
    }
  }

  public IEnumerator DoSkill(CharacterSkillData skill)
  {
    usingSkill = true;
    yield return StartCoroutine(skill.UseSkill(this));
    usingSkill = false;
    skillCoroutine = null;
  }

  public static float GetAttackValueModifier(CharacterAttackValueToIntDictionary attackModifiers, CharacterAttackValue value)
  {
    if (attackModifiers == null || !attackModifiers.ContainsKey(value))
    {
      return 0;
    }
    return attackModifiers[value] * Constants.CharacterAttackAdjustmentIncrements[value];
  }

  // // TODO: Refactor attack info so that it all lives on a single object (...maybe)
  // public void CreateAttackHitbox()
  // {
  //   CreateHitbox(defaultCharacterAttack, attackModifiers);
  // }

  public void CreateDashAttackHitbox()
  {
    CreateHitbox(dashCharacterAttack, dashAttackModifiers);
  }

  public string GetSkillNameFromIndex(int idx)
  {
    return characterSkills[idx].name;
  }
  public float GetAttackRange(int skillIdxForAttack = 0)
  {
    string skillName = GetSkillNameFromIndex(skillIdxForAttack);
    return weaponInstances[skillName].range;
    // return attack.range + Character.GetAttackValueModifier(mods.attackValueModifiers, CharacterAttackValue.Range);
  }

  public float GetEffectiveAttackRange(int skillIdxForAttack = 0)
  {
    string skillName = GetSkillNameFromIndex(skillIdxForAttack);
    return weaponInstances[skillName].effectiveRange;
  }

  public float GetRange(int skillIdxForAttack = 0)
  {
    string skillName = GetSkillNameFromIndex(skillIdxForAttack);
    return weaponInstances[skillName].range;
    // return attack.range + Character.GetAttackValueModifier(mods.attackValueModifiers, CharacterAttackValue.Range);
  }
  public int GetAttackRadiusInDegrees(int skillIdxForAttack)
  {
    string skillName = GetSkillNameFromIndex(skillIdxForAttack);
    return weaponInstances[skillName].sweepRadiusInDegrees;
  }
  public void CreateHitbox(AttackSkillData atk, CharacterAttackModifiers mods)
  {
    // HitboxData hbi = atk.hitboxData;
    // if (hbi == null) { Debug.LogError("no attack object defined for " + gameObject.name); }
    // Vector3 pos = orientation.TransformPoint(Vector3.right * GetAttackRange(atk, mods) / 2);
    // Hitbox_OLD hb = GameObject.Instantiate(hitboxPrefab, pos, orientation.rotation) as Hitbox_OLD;
    // hb.gameObject.layer = LayerMask.NameToLayer(currentFloor.ToString());
    // hb.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = currentFloor.ToString();
    // hb.Init(orientation, this, hbi, mods, GetAttackRange(atk, mods));
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
      (usingSkill || animationPreventsMoving || stunned)
      && !activeMovementAbilities.Contains(CharacterMovementAbility.Halteres)
    )
    {
      return;
    }
    Quaternion targetDirection = GetTargetDirection();
    orientation.rotation = Quaternion.Slerp(orientation.rotation, targetDirection, GetRotationSpeed() * Time.deltaTime);
  }

  // Rotate character smoothly towards a particular orientation around the Z axis.
  // Warning: I don't understand this math. If character rotation seems buggy, this is a
  // potential culprit.
  Quaternion GetTargetDirection()
  {
    return GetDirectionAngle(orientTowards);
  }

  Quaternion GetDirectionAngle(Vector3 targetPoint)
  {
    Vector2 target = targetPoint - transform.position;
    float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
    return Quaternion.AngleAxis(angle, Vector3.forward);
  }

  // used to calculate how far our facing direction is from our target facing direction.
  // Useful for e.g. deciding if an enemy is facing a player enough for attacking to be a good idea.
  public float GetAngleToTarget()
  {
    return Quaternion.Angle(GetTargetDirection(), orientation.rotation);
  }

  public float GetAngleToDirection(Vector3 targetPoint)
  {
    return Quaternion.Angle(GetDirectionAngle(targetPoint), orientation.rotation);
  }

  // add input to our velocity, if necessary/possible.
  protected void CalculateMovement()
  {
    if (CanMove())
    {
      if (!IsDashing() && !flying && (movementInput == Vector2.zero))
      { // should be an approximate equals
        timeStandingStill += Time.deltaTime;
        timeMoving = 0;
      }
      else
      {
        timeMoving += Time.deltaTime;
        timeStandingStill = 0f;
      }
      if (IsDashing())
      {
        po.SetMovementInput(orientation.rotation * new Vector3(1, 0, 0));
      }
      else
      {
        po.SetMovementInput(new Vector2(movementInput.x, movementInput.y));
      }
    }
    else
    {
      po.SetMovementInput(Vector2.zero);
    }
  }

  // Should mainly be used for ascending/descending.
  // Ideally, should only be used to avoid collision.
  protected void CenterCharacterOnCurrentTile()
  {
    transform.position = new Vector3(
      currentTileLocation.position.x + .5f,
      currentTileLocation.position.y + .5f,
      transform.position.z
    );
  }
  protected void Molt()
  {
    Instantiate(moltCasingPrefab, orientation.transform.position, orientation.transform.rotation);
    moltCasingPrefab.Init(Color.white, this);
    AdjustCurrentMoltCount(1);
    AdjustCurrentHealth(5);
  }
  // Traits activated on dash are handled in HandleConditionallyActivatedTraits()

  public bool IsDashing()
  {
    return dashTimer > 0;
  }

  public bool IsRecoveringFromDash()
  {
    return dashRecoveryTimer > 0;
  }

  protected bool CanDash()
  {
    return GetCharacterVital(CharacterVital.RemainingStamina) > 0 && dashTimer <= 0 && dashRecoveryTimer <= 0 && !flying && CanMove();
  }

  protected void Dash()
  {
    BeginDash();
  }

  protected void BeginDash()
  {
    vitals[CharacterVital.RemainingStamina] -= GetDashStaminaCost();
    dashTimer = GetStat(CharacterStat.DashDuration);
  }

  protected void EndDash()
  {
    dashRecoveryTimer = GetStat(CharacterStat.DashRecoveryDuration);
  }

  protected void Fly()
  {
    BeginFly();
  }

  protected void BeginFly()
  {
    flying = true;
    AscendOneFloor();
  }

  protected void EndFly()
  {
    flying = false;
  }

  protected void FlyUp()
  {
    // consume stamina here
    AscendOneFloor();
  }

  protected void FlyDown()
  {
    if (GridManager.Instance.TileIsValidAndEmpty(GetTileLocation()))
    {
      DescendOneFloor();
    }
    else
    {
      EndFly();
    }
  }

  protected void DoDashAttack()
  {
    if (dashAttackEnabled)
    {
      CreateDashAttackHitbox();
    }
  }

  protected void AscendOneFloor()
  {
    StartAscentOrDescent(AscendingDescendingState.Ascending);
  }

  protected void DescendOneFloor()
  {
    StartAscentOrDescent(AscendingDescendingState.Descending);
  }

  public void StartAscentOrDescent(AscendingDescendingState ascendOrDescend)
  {
    if (ascendingDescendingState != AscendingDescendingState.None) { return; }
    ascendingDescendingState = ascendOrDescend;
    Debug.Log("set ascendingDescendingState to " + ascendingDescendingState);
    if (descending)
    {
      SetCurrentFloor(currentFloor - 1);
    }
  }

  public void HandleAscendOrDescend()
  {
    if (ascending)
    {
      Debug.Log("should be ascending?");
      transform.position -= new Vector3(0, 0, 1 / ascendDescendSpeed * Time.deltaTime);
      if (transform.position.z - GridManager.GetZOffsetForFloor(gameObject.layer + 1) < .01)
      {
        transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Round(transform.position.z));
        ascendingDescendingState = AscendingDescendingState.None;
        SetCurrentFloor(currentFloor + 1);
      }
    }
    else if (descending)
    {
      transform.position += new Vector3(0, 0, 1 / ascendDescendSpeed * Time.deltaTime);
      // Debug.Log("descending distance: " + (GridManager.GetZOffsetForFloor(gameObject.layer) - transform.position.z));
      if (GridManager.GetZOffsetForFloor(gameObject.layer) - transform.position.z < .01)
      {
        transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Round(transform.position.z));
        ascendingDescendingState = AscendingDescendingState.None;
      }
    }
  }
  // determines if input-based movement is allowed
  protected virtual bool CanMove()
  {
    if (ascending && !flying)
    {
      return false;
    }
    if (!activeMovementAbilities.Contains(CharacterMovementAbility.Halteres))
    {
      if (usingSkill)
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
    if (IsRecoveringFromDash())
    {
      return false;
    }
    return true;
  }

  // DAMAGE FUNCTIONS
  protected virtual void TakeDamage(IDamageSource damageSource)
  {
    if (damageSource.IsOwnedBy(this)) { return; }
    if (damageSource.IsSameOwnerType(this)) { return; }
    if (
      sourceInvulnerabilities.Contains(damageSource.sourceString)
      && !damageSource.ignoresInvulnerability)
    { return; }
    float damageAfterResistances = damageSource.CalculateDamageAfterResistances(this);
    if (
      damageAfterResistances <= 0
      && damageSource.damageAmount > 0
    ) { return; }
    if (damageSource.movementAbilitiesWhichBypassDamage.Intersect(activeMovementAbilities).Any())
    {
      return;
    }
    InterruptAnimation();
    AdjustCurrentHealth(Mathf.Floor(-damageAfterResistances));
    CalculateAndApplyStun(damageSource.stunMagnitude);
    StartCoroutine(ApplyInvulnerability(damageSource));
    Vector3 knockback = damageSource.GetKnockbackForCharacter(this);
    if (knockback != Vector3.zero)
    {
      po.ApplyImpulseForce(knockback);
    }
  }

  public int GetDamageTypeResistanceLevel(DamageType type)
  {
    bool exists = Enum.TryParse("Resist_" + type.ToString(), out CharacterAttribute resistAttribute);
    if (!exists)
    {
      Debug.LogError("Could not find attribute Resist_" + type.ToString());
      return 0;
    }
    return GetAttribute(resistAttribute);
  }

  private IEnumerator ApplyDamageFlash(DamageData_OLD damageObj)
  {
    // Todo: might wanna change this!
    Color baseColor = Color.white;
    yield return null;
    // mainRenderer.color = damageFlashColor;
    // yield return new WaitForSeconds(damageFlashSpeed / 3);
    // mainRenderer.color = baseColor;
    // yield return new WaitForSeconds(damageFlashSpeed / 3);
    // mainRenderer.color = damageFlashColor;
    // yield return new WaitForSeconds(damageFlashSpeed / 3);
    // mainRenderer.color = baseColor;
  }
  public virtual void Die()
  {
    Destroy(gameObject);
  }

  IEnumerator ApplyInvulnerability(IDamageSource damageSource)
  {
    float invulnerabilityDuration = damageSource.invulnerabilityWindow;
    if (invulnerabilityDuration <= 0) { yield break; }
    string src = damageSource.sourceString;
    sourceInvulnerabilities.Add(src);
    yield return new WaitForSeconds(invulnerabilityDuration);
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


  public virtual void AssignTraitsForNextLife(TraitSlotToUpcomingTraitDictionary nextLifeTraits)
  {

  }

  // ATTRIBUTE/VITALS ACCESSORS
  public int GetAttribute(CharacterAttribute attributeToGet)
  {
    bool exists = attributes.TryGetValue(attributeToGet, out int val);
    if (!exists)
    {
      Debug.LogError("Tried to access non-existant attribute " + attributeToGet);
    }
    return val;
  }

  // STAT GETTERS
  public float GetCurrentMaxHealth()
  {
    return
        GetMaxHealth()
        - (GetMaxHealthLostPerMolt() * GetCharacterVital(CharacterVital.CurrentMoltCount));
  }

  public float GetMaxHealth()
  {
    return defaultCharacterData
      .GetHealthAttributeData()
      .GetMaxHealth(this);
  }
  public float GetMaxStamina()
  {
    return defaultCharacterData.defaultStats[CharacterStat.Stamina];
  }

  public float GetStaminaRecoverySpeed()
  {
    return 3;
  }
  public float GetRotationSpeed()
  {
    return defaultCharacterData.defaultStats[CharacterStat.RotationSpeed];
  }

  public float GetMaxHealthLostPerMolt()
  {
    return defaultCharacterData.defaultStats[CharacterStat.MaxHealthLostPerMolt];
  }

  public bool GetCanFlyUp()
  {
    return defaultCharacterData
      .GetFlightAttributeData()
      .GetAttributeTier(this)
      .canFlyUp;
  }

  public float GetMaxFlightDuration()
  {
    return defaultCharacterData
      .GetFlightAttributeData()
      .GetAttributeTier(this)
      .flightDuration;
  }

  public float GetDashStaminaCost()
  {
    return defaultCharacterData
      .GetDashAttributeData()
      .GetDashStaminaCost(this);
  }

  public float GetStat(CharacterStat statToGet)
  {
    return defaultCharacterData.defaultStats[statToGet];
    // StringToIntDictionary statMods = statModifications[statToGet];
    // int modValue = 0;
    // float returnValue = defaultCharacterData.defaultStats[statToGet];
    // foreach (int modMagnitude in statMods.Values)
    // {
    //   modValue += modMagnitude;
    // }
    // modValue = Mathf.Clamp(-12, modValue, 12);
    // if (modValue >= 0)
    // {
    //   returnValue *= ((3 + modValue) / 3);
    // }
    // else
    // {
    //   returnValue *= (3f / (3 + Mathf.Abs(modValue)));
    // }
    // return returnValue;
  }


  // public void ApplyStatMod(CharacterStatModification mod)
  // {
  //     if (mod.duration > 0)
  //     {
  //         StartCoroutine(ApplyTemporaryStatMod(mod));
  //     }
  //     else
  //     {
  //         AddStatMod(mod);
  //     }
  // }

  // public IEnumerator ApplyTemporaryStatMod(CharacterStatModification mod)
  // {
  //     AddStatMod(mod);
  //     yield return new WaitForSeconds(mod.duration);
  //     RemoveStatMod(mod.source);
  // }

  // public void AddStatMod(CharacterStatModification mod)
  // {
  //     AddStatMod(mod.statToModify, mod.magnitude, mod.source);
  // }

  // public void AddStatMod(CharacterStat statToMod, int magnitude, string source)
  // {
  //     statModifications[statToMod][source] = magnitude;
  // }

  // public void RemoveStatMod(string source)
  // {
  //     foreach (CharacterStat stat in (CharacterStat[])Enum.GetValues(typeof(CharacterStat)))
  //     {
  //         StringToIntDictionary statMods = statModifications[stat];
  //         if (statMods.ContainsKey(source))
  //         {
  //             statMods.Remove(source);
  //         }
  //     }
  // }

  public void AddMovementAbility(CharacterMovementAbility movementAbility)
  {
    activeMovementAbilities.Add(movementAbility);
  }

  public void RemoveMovementAbility(CharacterMovementAbility movementAbility)
  {
    activeMovementAbilities.Remove(movementAbility);
  }


  //VITALS GETTERS/SETTERS
  public void AdjustCurrentHealth(float adjustment)
  {
    vitals[CharacterVital.CurrentHealth] =
      Mathf.Clamp(vitals[CharacterVital.CurrentHealth] + adjustment, 0, GetCurrentMaxHealth());
  }

  public void AdjustCurrentMoltCount(float adjustment)
  {
    vitals[CharacterVital.CurrentMoltCount] += adjustment;
  }
  public float GetCharacterVital(CharacterVital vital)
  {
    return vitals[vital];
  }
  public virtual void SetCurrentFloor(FloorLayer newFloorLayer)
  {
    if (flying)
    {
      if (!AllTilesOnTargetFloorEmpty(newFloorLayer))
      {
        flying = false;
        Debug.Log("clearing flying flag");
        ascendingDescendingState = AscendingDescendingState.Descending;
        return;
      }
    }
    else if (!AllTilesOnTargetFloorClearOfObjects(newFloorLayer))
    {
      CenterCharacterOnCurrentTile();
    }
    currentFloor = newFloorLayer;
    currentTileLocation = CalculateCurrentTileLocation();
    ChangeLayersRecursively(transform, newFloorLayer);
    HandleTileCollision(GridManager.Instance.GetTileAtLocation(currentTileLocation));
    po.OnLayerChange();
  }


  public void UseTile()
  {
    Debug.Log("using tile");
    EnvironmentTileInfo et = GridManager.Instance.GetTileAtLocation(currentTileLocation);
    if (et.ChangesFloorLayer())
    {
      Debug.Log("starting ascent or descent? " + et.AscendsOrDescends());
      StartAscentOrDescent(et.AscendsOrDescends());
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
    foreach (SpriteRenderer renderer in renderers)
    {

      renderer.color = Color.Lerp(
        damagedColor,
        Color.white,
        vitals[CharacterVital.CurrentHealth] / GetMaxHealth()
      );
    }
  }

  protected HashSet<EnvironmentTileInfo> GetTouchingTiles(FloorLayer layerToConsider)
  {
    return new HashSet<EnvironmentTileInfo> {
      GridManager.Instance.GetTileAtLocation(transform.TransformPoint(boxCollider.bounds.extents.x, boxCollider.bounds.extents.y, transform.position.z), layerToConsider),
      GridManager.Instance.GetTileAtLocation(transform.TransformPoint(-boxCollider.bounds.extents.x, boxCollider.bounds.extents.y, transform.position.z), layerToConsider),
      GridManager.Instance.GetTileAtLocation(transform.TransformPoint(boxCollider.bounds.extents.x, -boxCollider.bounds.extents.y, transform.position.z), layerToConsider),
      GridManager.Instance.GetTileAtLocation(transform.TransformPoint(-boxCollider.bounds.extents.x, -boxCollider.bounds.extents.y, transform.position.z), layerToConsider)
    };
  }

  protected virtual bool AllTilesOnTargetFloorEmpty(FloorLayer targetFloor)
  {
    // if we're flying, every tile above us needs to be empty.

    HashSet<EnvironmentTileInfo> touchingTiles = GetTouchingTiles(targetFloor);
    foreach (EnvironmentTileInfo tile in touchingTiles)
    {

      if (!tile.IsEmpty())
      {
        Debug.Log("non-empty tile found at " + tile.tileLocation.position + ", floor " + targetFloor);
        Debug.Log("floor = " + tile.groundTileType);
        Debug.Log("object = " + tile.objectTileType);
        return false;
      }
    }
    return true;
  }

  protected virtual bool AllTilesOnTargetFloorClearOfObjects(FloorLayer targetFloor)
  {

    HashSet<EnvironmentTileInfo> touchingTiles = GetTouchingTiles(targetFloor);
    foreach (EnvironmentTileInfo tile in touchingTiles)
    {
      if (tile.HasSolidObject()) { return false; }
    }
    return true;
  }

  protected virtual void HandleFalling() // we have removed sticking!! make it stick yourself!!!
  {
    if (
        activeMovementAbilities.Contains(CharacterMovementAbility.Hover)
          || sticking
          || flying
          || ascending
          || descending
        )
    {
      return; // abilities prevent falling!
    }
    HashSet<EnvironmentTileInfo> touchingTiles = GetTouchingTiles(currentFloor);
    foreach (EnvironmentTileInfo tile in touchingTiles)
    {
      if (tile.objectTileType != null || tile.groundTileType != null)
      {
        return; // At least one corner is on a tile
      }
    }
    DescendOneFloor();
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
    if (ascending || descending) { return; }

    if (tile.dealsDamage/* && vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] <= 0*/)
    {
      foreach (EnvironmentalDamage envDamage in tile.environmentalDamageSources)
      {
        TakeDamage(envDamage);
      }
    }
    if (footstepCooldown <= 0 && timeMoving > 0)
    {
      footstepCooldown = tile.HandleFootstep(this);
    }
    if (tile.CanRespawnPlayer())
    {
      if (!tile.CharacterCanCrossTile(this))
      {
        RespawnCharacterAtLastSafeLocation();
      }
    }
    else if (tile.groundTileType != null && ascendingDescendingState != AscendingDescendingState.Ascending)
    {
      Debug.Log("not ascending, setting flying to false");
      flying = false;
      lastSafeTileLocation = currentTileLocation;
    }
  }

  protected void RespawnCharacterAtLastSafeLocation()
  {
    // skill1.CancelActiveEffects();
    // skill2.CancelActiveEffects();
    transform.position =
      new Vector3(lastSafeTileLocation.position.x + .5f, lastSafeTileLocation.position.y + .5f, GridManager.GetZOffsetForFloor(GetGameObjectLayerFromFloorLayer(lastSafeTileLocation.floorLayer)));
    if (currentFloor != lastSafeTileLocation.floorLayer)
    {
      SetCurrentFloor(lastSafeTileLocation.floorLayer);
    }
    CalculateAndApplyStun(.5f, true);
    po.HardSetVelocityToZero();
  }

  public virtual void HandleContactWithRespawnTile()
  {

  }

  public virtual void HandleTileCollision(EnvironmentTileInfo tile)
  {
    if (tile.GetColliderType() == Tile.ColliderType.None)
    {
      return;
    }
    else
    {
      if (tile.CharacterCanBurrowThroughObjectTile(this))
      {
        GridManager.Instance.MarkTileToRestoreOnPlayerRespawn(tile);
        tile.DestroyObjectTile();
      }
    }
  }

  public virtual void HandleConditionallyActivatedTraits()
  {
    foreach (TraitEffect trait in conditionallyActivatedTraitEffects)
    {
      switch (trait.activatingCondition)
      {
        case ConditionallyActivatedTraitCondition.Dashing:
          if (flying)
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
    if (flying)
    {
      vitals[CharacterVital.RemainingStamina] -= Time.deltaTime * GetMaxStamina() / 5;
      if (vitals[CharacterVital.RemainingStamina] <= 0)
      {
        EndFly();
      }
    }
    else if (IsDashing())
    {
      dashTimer -= Time.deltaTime;
      if (!IsDashing())
      {
        EndDash();
      }
    }
    else if (IsRecoveringFromDash())
    {
      dashRecoveryTimer -= Time.deltaTime;
    }
    else
    {
      vitals[CharacterVital.RemainingStamina]
        = Mathf.Min(vitals[CharacterVital.RemainingStamina] + (Time.deltaTime * GetMaxStamina() / GetStaminaRecoverySpeed()), GetMaxStamina());
    }
    if (footstepCooldown > 0)
    {
      footstepCooldown -= Time.deltaTime;
    }
    // if (vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] > 0)
    // {
    //     vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] -= Time.deltaTime;
    // }
    // if (vitals[CharacterVital.CurrentDashCooldown] > 0)
    // {
    //     vitals[CharacterVital.CurrentDashCooldown] -= Time.deltaTime;
    // }
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
